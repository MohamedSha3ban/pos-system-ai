import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

// Module-level (not per-injection) so every request sharing this interceptor instance sees
// the same in-flight-refresh state -- this is what dedupes N concurrent 401s into ONE
// refresh call instead of N races against the refresh-token rotation (which would revoke
// each other, since only one presented token per rotation is valid -- see SessionService).
let isRefreshing = false;
const refreshedAccessToken$ = new BehaviorSubject<string | null>(null);

// Endpoints that either don't need a token (login/register) or ARE the refresh mechanism
// itself -- retrying these through the 401-refresh flow would loop or make no sense.
const AUTH_EXCLUDED_PATHS = ['/auth/login', '/auth/register-tenant', '/auth/refresh', '/auth/logout'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getAccessToken();
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
  const isExcluded = AUTH_EXCLUDED_PATHS.some(path => req.url.includes(path));

  return next(authReq).pipe(
    catchError(error => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isExcluded) {
        return throwError(() => error);
      }
      return handleUnauthorized(req, next, auth, router);
    })
  );
};

function handleUnauthorized(req: Parameters<HttpInterceptorFn>[0], next: Parameters<HttpInterceptorFn>[1], auth: AuthService, router: Router) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshedAccessToken$.next(null);

    return auth.refresh().pipe(
      switchMap(result => {
        isRefreshing = false;
        if (!result) {
          router.navigate(['/login']);
          return throwError(() => new Error('Session expired -- please log in again.'));
        }
        refreshedAccessToken$.next(result.accessToken);
        return next(req.clone({ setHeaders: { Authorization: `Bearer ${result.accessToken}` } }));
      }),
      catchError(err => {
        isRefreshing = false;
        router.navigate(['/login']);
        return throwError(() => err);
      })
    );
  }

  // A refresh is already in flight (triggered by a different concurrent request) --
  // wait for it to finish, then retry this request with whatever token it produced.
  return refreshedAccessToken$.pipe(
    filter((token): token is string => token !== null),
    take(1),
    switchMap(token => next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })))
  );
}
