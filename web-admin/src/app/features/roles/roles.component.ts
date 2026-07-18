import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Role, UpsertRoleRequest } from '../../core/models/models';
import { RoleService } from '../../core/services/role.service';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './roles.component.html'
})
export class RolesComponent implements OnInit {
  private roleService = inject(RoleService);

  roles: Role[] = [];
  availablePermissions: string[] = [];
  errorMessage = '';

  showForm = false;
  editingRoleId: string | null = null;
  form: UpsertRoleRequest = { name: '', permissions: [] };

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.roleService.getAll().subscribe({ next: r => this.roles = r, error: () => this.errorMessage = 'Could not load roles.' });
    this.roleService.getAvailablePermissions().subscribe({ next: p => this.availablePermissions = p });
  }

  openCreateForm(): void {
    this.editingRoleId = null;
    this.form = { name: '', permissions: [] };
    this.showForm = true;
  }

  openEditForm(r: Role): void {
    this.editingRoleId = r.id;
    this.form = { name: r.name, permissions: [...r.permissions] };
    this.showForm = true;
  }

  togglePermission(code: string, checked: boolean): void {
    this.form.permissions = checked ? [...this.form.permissions, code] : this.form.permissions.filter(p => p !== code);
  }

  save(): void {
    this.errorMessage = '';
    const request$ = this.editingRoleId
      ? this.roleService.update(this.editingRoleId, this.form)
      : this.roleService.create(this.form);

    request$.subscribe({
      next: () => { this.showForm = false; this.reload(); },
      error: () => this.errorMessage = 'Could not save role.'
    });
  }

  delete(r: Role): void {
    if (r.isSystemRole) return;
    if (!confirm(`Delete role "${r.name}"?`)) return;
    this.roleService.delete(r.id).subscribe({
      next: () => this.reload(),
      error: (err) => this.errorMessage = err?.error?.error || 'Could not delete role.'
    });
  }

  cancel(): void {
    this.showForm = false;
  }
}
