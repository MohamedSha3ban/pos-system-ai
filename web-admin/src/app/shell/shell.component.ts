import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet, Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  session = this.auth.getSession();

  hasPermission(code: string): boolean {
    return this.auth.hasPermission(code);
  }

  get isPlatformAdmin(): boolean {
    return this.auth.isPlatformAdmin();
  }

  logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/login']));
  }
}
