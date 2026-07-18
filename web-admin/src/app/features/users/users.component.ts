import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminUser, Role, CreateUserRequest, UpdateUserRequest } from '../../core/models/models';
import { UserService } from '../../core/services/user.service';
import { RoleService } from '../../core/services/role.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users.component.html'
})
export class UsersComponent implements OnInit {
  private userService = inject(UserService);
  private roleService = inject(RoleService);

  users: AdminUser[] = [];
  roles: Role[] = [];
  errorMessage = '';

  showForm = false;
  editingUserId: string | null = null;
  form = { fullName: '', email: '', password: '', isActive: true, roleIds: [] as string[] };

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.userService.getAll().subscribe({ next: u => this.users = u, error: () => this.errorMessage = 'Could not load users.' });
    this.roleService.getAll().subscribe({ next: r => this.roles = r });
  }

  openCreateForm(): void {
    this.editingUserId = null;
    this.form = { fullName: '', email: '', password: '', isActive: true, roleIds: [] };
    this.showForm = true;
  }

  openEditForm(u: AdminUser): void {
    this.editingUserId = u.id;
    this.form = { fullName: u.fullName, email: u.email, password: '', isActive: u.isActive, roleIds: u.roles.map(r => r.id) };
    this.showForm = true;
  }

  toggleRole(roleId: string, checked: boolean): void {
    this.form.roleIds = checked ? [...this.form.roleIds, roleId] : this.form.roleIds.filter(id => id !== roleId);
  }

  save(): void {
    this.errorMessage = '';
    if (this.editingUserId) {
      const req: UpdateUserRequest = { fullName: this.form.fullName, isActive: this.form.isActive, roleIds: this.form.roleIds };
      this.userService.update(this.editingUserId, req).subscribe({
        next: () => { this.showForm = false; this.reload(); },
        error: () => this.errorMessage = 'Could not save user.'
      });
    } else {
      const req: CreateUserRequest = { fullName: this.form.fullName, email: this.form.email, password: this.form.password, roleIds: this.form.roleIds };
      this.userService.create(req).subscribe({
        next: () => { this.showForm = false; this.reload(); },
        error: () => this.errorMessage = 'Could not create user.'
      });
    }
  }

  deactivate(u: AdminUser): void {
    if (!confirm(`Deactivate ${u.fullName}? They won't be able to log in.`)) return;
    this.userService.deactivate(u.id).subscribe({ next: () => this.reload(), error: () => this.errorMessage = 'Could not deactivate user.' });
  }

  cancel(): void {
    this.showForm = false;
  }
}
