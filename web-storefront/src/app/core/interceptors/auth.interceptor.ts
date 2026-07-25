import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AccountAuthService } from '../services/account-auth.service';

// Same dedupe-concurrent-refreshes pattern as web-admin's interceptor -- see its comments
// for why this needs to be module-level state.
let isRefreshing = false;
const refreshedAccessToken$ = new BehaviorSubject<string | null>(null);

const AUTH_EXCLUDED_PATHS = ['/auth/login', '/auth/register', '/auth/refresh', '/auth/logout'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AccountAuthService);

  const token = auth.getAccessToken();
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
  const isExcluded = AUTH_EXCLUDED_PATHS.some(path => req.url.includes(path));

  return next(authReq).pipe(
    catchError(error => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isExcluded || !token) {
        // No token on the request in the first place (anonymous catalog browsing) --
        // a 401 there isn't a session problem, just pass it through.
        return throwError(() => error);
      }
      return handleUnauthorized(req, next, auth);
    })
  );
};

function handleUnauthorized(req: Parameters<HttpInterceptorFn>[0], next: Parameters<HttpInterceptorFn>[1], auth: AccountAuthService) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshedAccessToken$.next(null);

    return auth.refresh().pipe(
      switchMap(result => {
        isRefreshing = false;
        if (!result) {
          // Refresh token is dead too -- let the failure surface normally; the checkout/
          // account screens already handle "not logged in" by showing the login form again.
          return throwError(() => new Error('Session expired -- please log in again.'));
        }
        refreshedAccessToken$.next(result.accessToken);
        return next(req.clone({ setHeaders: { Authorization: `Bearer ${result.accessToken}` } }));
      }),
      catchError(err => {
        isRefreshing = false;
        return throwError(() => err);
      })
    );
  }

  return refreshedAccessToken$.pipe(
    filter((token): token is string => token !== null),
    take(1),
    switchMap(token => next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })))
  );
}
