import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AccountAuthService } from '../../core/services/account-auth.service';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './account.component.html'
})
export class AccountComponent {
  account = inject(AccountAuthService);
  private router = inject(Router);

  mode: 'login' | 'register' = 'login';
  form = { fullName: '', email: '', password: '', phone: '' };
  error = '';
  loading = false;

  submit(): void {
    this.error = '';
    this.loading = true;
    const obs = this.mode === 'login'
      ? this.account.login(this.form.email, this.form.password)
      : this.account.register(this.form.fullName, this.form.email, this.form.password, this.form.phone);

    obs.subscribe({
      next: () => { this.loading = false; this.router.navigate(['/']); },
      error: () => { this.error = this.mode === 'login' ? 'Invalid email or password.' : 'Could not create account.'; this.loading = false; }
    });
  }

  logout(): void {
    this.account.logout().subscribe();
  }
}
