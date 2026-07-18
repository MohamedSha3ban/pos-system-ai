import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html'
})
export class CartComponent {
  cart = inject(CartService);
  private router = inject(Router);

  updateQuantity(productId: string, value: string): void {
    const qty = parseInt(value, 10);
    this.cart.updateQuantity(productId, isNaN(qty) ? 0 : qty);
  }

  goToCheckout(): void {
    this.router.navigate(['/checkout']);
  }
}
