import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountAuthService } from '../services/account-auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AccountAuthService).getToken();
  if (!token) return next(req);
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
