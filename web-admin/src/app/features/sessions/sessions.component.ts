import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SessionInfo } from '../../core/models/models';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-sessions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sessions.component.html'
})
export class SessionsComponent implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);

  sessions: SessionInfo[] = [];
  errorMessage = '';
  loggingOutAll = false;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.auth.getSessions().subscribe({
      next: s => this.sessions = s,
      error: () => this.errorMessage = 'Could not load sessions.'
    });
  }

  logoutAllDevices(): void {
    if (!confirm('This will sign you out on every device, including this one. Continue?')) return;
    this.loggingOutAll = true;
    this.auth.logoutAllDevices().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => { this.errorMessage = 'Could not log out all devices.'; this.loggingOutAll = false; }
    });
  }
}
