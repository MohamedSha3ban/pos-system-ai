import { Injectable, signal, computed } from '@angular/core';
import { CartLine, Product } from '../models/models';

/// <summary>In-memory cart state, shared across the app via DI (providedIn: 'root').
/// A real deployment would likely persist this to window.storage or a backend cart
/// so it survives a refresh -- kept simple/in-memory for this starter.</summary>
@Injectable({ providedIn: 'root' })
export class CartService {
  private linesSignal = signal<CartLine[]>([]);
  lines = this.linesSignal.asReadonly();

  subtotal = computed(() => this.linesSignal().reduce((sum, l) => sum + l.product.price * l.quantity, 0));

  itemCount(): number {
    return this.linesSignal().reduce((sum, l) => sum + l.quantity, 0);
  }

  add(product: Product): void {
    const lines = this.linesSignal();
    const existing = lines.find(l => l.product.id === product.id);
    if (existing) {
      this.linesSignal.set(lines.map(l => l.product.id === product.id ? { ...l, quantity: l.quantity + 1 } : l));
    } else {
      this.linesSignal.set([...lines, { product, quantity: 1 }]);
    }
  }

  updateQuantity(productId: string, quantity: number): void {
    if (quantity <= 0) {
      this.remove(productId);
      return;
    }
    this.linesSignal.set(this.linesSignal().map(l => l.product.id === productId ? { ...l, quantity } : l));
  }

  remove(productId: string): void {
    this.linesSignal.set(this.linesSignal().filter(l => l.product.id !== productId));
  }

  clear(): void {
    this.linesSignal.set([]);
  }
}
