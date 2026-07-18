import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { AccountAuthService } from '../../core/services/account-auth.service';
import { OrderService } from '../../core/services/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.component.html'
})
export class CheckoutComponent {
  cart = inject(CartService);
  account = inject(AccountAuthService);
  private orderService = inject(OrderService);
  private router = inject(Router);

  // inline login/register (checkout requires a customer account)
  authMode: 'login' | 'register' = 'login';
  authForm = { fullName: '', email: '', password: '', phone: '' };
  authError = '';
  authLoading = false;

  paymentMethod = 'CardPresent';
  paymentMethods = [
    { value: 'CardPresent', label: 'Card' },
    { value: 'ApplePay', label: 'Apple Pay' },
    { value: 'GooglePay', label: 'Google Pay' }
  ];

  placingOrder = false;
  orderError = '';
  completedOrderId: string | null = null;

  submitAuth(): void {
    this.authError = '';
    this.authLoading = true;
    const obs = this.authMode === 'login'
      ? this.account.login(this.authForm.email, this.authForm.password)
      : this.account.register(this.authForm.fullName, this.authForm.email, this.authForm.password, this.authForm.phone);

    obs.subscribe({
      next: () => this.authLoading = false,
      error: () => { this.authError = this.authMode === 'login' ? 'Invalid email or password.' : 'Could not create account (email may already be in use).'; this.authLoading = false; }
    });
  }

  placeOrder(): void {
    this.orderError = '';
    this.placingOrder = true;

    // NOTE: this demo checkout doesn't collect real card details -- see README
    // "Next steps" for wiring up Stripe Elements to produce a real paymentToken here.
    const tenders = [{ method: this.paymentMethod, amount: this.cart.subtotal() }];

    this.orderService.checkout(this.cart.lines(), tenders).subscribe({
      next: order => {
        this.completedOrderId = order.id;
        this.cart.clear();
        this.placingOrder = false;
      },
      error: () => { this.orderError = 'Checkout failed. Please try again.'; this.placingOrder = false; }
    });
  }
}
