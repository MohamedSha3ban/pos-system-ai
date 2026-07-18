import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Product, CartLine } from '../../core/models/models';
import { ProductService } from '../../core/services/product.service';
import { OrderService, Tender } from '../../core/services/order.service';

// TODO: replace with the real active-location id from the logged-in tenant context.
const DEFAULT_LOCATION_ID = '00000000-0000-0000-0000-000000000000';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './pos.component.html'
})
export class PosComponent implements OnInit {
  private productService = inject(ProductService);
  private orderService = inject(OrderService);

  products: Product[] = [];
  cart: CartLine[] = [];
  paymentMethod = 'Cash';
  paymentMethods = ['Cash', 'CardPresent', 'ApplePay', 'GooglePay', 'QrBankTransfer'];
  lastOrderTotal: number | null = null;
  errorMessage = '';

  ngOnInit(): void {
    this.productService.getCatalog(DEFAULT_LOCATION_ID).subscribe({
      next: products => this.products = products,
      error: () => this.errorMessage = 'Could not load catalog.'
    });
  }

  addToCart(product: Product): void {
    const existing = this.cart.find(c => c.product.id === product.id);
    if (existing) existing.quantity++;
    else this.cart.push({ product, quantity: 1 });
  }

  removeLine(line: CartLine): void {
    this.cart = this.cart.filter(c => c !== line);
  }

  get subtotal(): number {
    return this.cart.reduce((sum, l) => sum + l.product.price * l.quantity, 0);
  }

  checkout(): void {
    this.errorMessage = '';
    if (this.cart.length === 0) return;

    const tenders: Tender[] = [{ method: this.paymentMethod, amount: this.subtotal }];
    this.orderService.checkout(DEFAULT_LOCATION_ID, this.cart, tenders).subscribe({
      next: order => {
        this.lastOrderTotal = order.grandTotal;
        this.cart = [];
      },
      error: () => this.errorMessage = 'Checkout failed. Please try again.'
    });
  }
}
