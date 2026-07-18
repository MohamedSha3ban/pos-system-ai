import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminProduct, Category, UpsertProductRequest } from '../../core/models/models';
import { ProductService } from '../../core/services/product.service';
import { CategoryService } from '../../core/services/category.service';

// TODO: replace with the real active-location id from the logged-in tenant context.
const DEFAULT_LOCATION_ID = '00000000-0000-0000-0000-000000000000';

const EMPTY_FORM: UpsertProductRequest = {
  name: '', description: '', sku: '', barcode: '', price: 0, costPrice: undefined, categoryId: undefined, isActive: true
};

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);

  products: AdminProduct[] = [];
  categories: Category[] = [];

  // product form state
  showProductForm = false;
  editingProductId: string | null = null;
  productForm: UpsertProductRequest = { ...EMPTY_FORM };
  initialQuantity = 0;

  // category form state
  showCategoryForm = false;
  newCategoryName = '';
  editingCategoryId: string | null = null;
  editingCategoryName = '';

  errorMessage = '';

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.productService.getCatalog(DEFAULT_LOCATION_ID).subscribe({
      next: p => this.products = p as AdminProduct[],
      error: () => this.errorMessage = 'Could not load products.'
    });
    this.categoryService.getAll().subscribe({
      next: c => this.categories = c,
      error: () => this.errorMessage = 'Could not load categories.'
    });
  }

  // --- Product CRUD ---
  openCreateForm(): void {
    this.editingProductId = null;
    this.productForm = { ...EMPTY_FORM };
    this.initialQuantity = 0;
    this.showProductForm = true;
  }

  openEditForm(p: AdminProduct): void {
    this.editingProductId = p.id;
    this.productForm = {
      name: p.name, description: p.description, sku: p.sku, barcode: p.barcode,
      price: p.price, costPrice: p.costPrice, categoryId: p.categoryId, isActive: p.isActive
    };
    this.showProductForm = true;
  }

  saveProduct(): void {
    this.errorMessage = '';
    if (this.editingProductId) {
      this.productService.update(this.editingProductId, this.productForm, DEFAULT_LOCATION_ID).subscribe({
        next: () => { this.showProductForm = false; this.reload(); },
        error: () => this.errorMessage = 'Could not save product.'
      });
    } else {
      this.productService.create(this.productForm, DEFAULT_LOCATION_ID, this.initialQuantity).subscribe({
        next: () => { this.showProductForm = false; this.reload(); },
        error: () => this.errorMessage = 'Could not create product.'
      });
    }
  }

  deleteProduct(p: AdminProduct): void {
    if (!confirm(`Delete "${p.name}"? This can't be undone.`)) return;
    this.productService.delete(p.id).subscribe({
      next: () => this.reload(),
      error: () => this.errorMessage = 'Could not delete product.'
    });
  }

  cancelProductForm(): void {
    this.showProductForm = false;
  }

  // --- Category CRUD ---
  createCategory(): void {
    if (!this.newCategoryName.trim()) return;
    this.categoryService.create(this.newCategoryName.trim()).subscribe({
      next: () => { this.newCategoryName = ''; this.reload(); },
      error: () => this.errorMessage = 'Could not create category.'
    });
  }

  startEditCategory(c: Category): void {
    this.editingCategoryId = c.id;
    this.editingCategoryName = c.name;
  }

  saveCategory(): void {
    if (!this.editingCategoryId) return;
    this.categoryService.update(this.editingCategoryId, this.editingCategoryName).subscribe({
      next: () => { this.editingCategoryId = null; this.reload(); },
      error: () => this.errorMessage = 'Could not update category.'
    });
  }

  deleteCategory(c: Category): void {
    if (!confirm(`Delete category "${c.name}"? Products keep their data but lose this category.`)) return;
    this.categoryService.delete(c.id).subscribe({
      next: () => this.reload(),
      error: () => this.errorMessage = 'Could not delete category.'
    });
  }
}
