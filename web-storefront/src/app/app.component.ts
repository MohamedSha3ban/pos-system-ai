import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CartService } from './core/services/cart.service';
import { AccountAuthService } from './core/services/account-auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  template: `
    <div class="storefront">
      <header class="topbar">
        <a routerLink="/" class="brand">The Shop</a>
        <nav>
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Shop</a>
          <a routerLink="/cart" routerLinkActive="active">Cart ({{ cart.itemCount() }})</a>
          <a routerLink="/account" routerLinkActive="active">{{ account.isLoggedIn() ? 'Account' : 'Log in' }}</a>
        </nav>
      </header>
      <main>
        <router-outlet></router-outlet>
      </main>
    </div>
  `
})
export class AppComponent {
  cart = inject(CartService);
  account = inject(AccountAuthService);
}
