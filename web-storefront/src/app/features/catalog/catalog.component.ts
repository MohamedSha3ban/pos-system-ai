import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product } from '../../core/models/models';
import { CatalogService } from '../../core/services/catalog.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './catalog.component.html'
})
export class CatalogComponent implements OnInit {
  private catalogService = inject(CatalogService);
  cart = inject(CartService);

  products: Product[] = [];
  loading = true;
  errorMessage = '';
  justAdded: string | null = null;

  ngOnInit(): void {
    this.catalogService.getProducts().subscribe({
      next: p => { this.products = p; this.loading = false; },
      error: () => { this.errorMessage = 'Could not load products.'; this.loading = false; }
    });
  }

  addToCart(product: Product): void {
    this.cart.add(product);
    this.justAdded = product.id;
    setTimeout(() => { if (this.justAdded === product.id) this.justAdded = null; }, 1200);
  }
}
