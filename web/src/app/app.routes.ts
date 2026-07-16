import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'pos', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'pos', loadComponent: () => import('./features/pos/pos.component').then(m => m.PosComponent), canActivate: [authGuard] },
  { path: 'products', loadComponent: () => import('./features/products/products.component').then(m => m.ProductsComponent), canActivate: [authGuard] }
];
