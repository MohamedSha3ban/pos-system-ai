import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryItem } from '../../core/models/models';
import { InventoryService } from '../../core/services/inventory.service';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory.component.html'
})
export class InventoryComponent implements OnInit {
  private inventoryService = inject(InventoryService);

  items: InventoryItem[] = [];
  errorMessage = '';
  editingId: string | null = null;
  editValue = 0;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.inventoryService.getAll().subscribe({ next: i => this.items = i, error: () => this.errorMessage = 'Could not load inventory.' });
  }

  startEdit(item: InventoryItem): void {
    this.editingId = item.id;
    this.editValue = item.quantityOnHand;
  }

  save(item: InventoryItem): void {
    this.inventoryService.adjust(item.id, { quantityOnHand: this.editValue }).subscribe({
      next: () => { this.editingId = null; this.reload(); },
      error: () => this.errorMessage = 'Could not adjust stock.'
    });
  }

  cancel(): void {
    this.editingId = null;
  }
}
