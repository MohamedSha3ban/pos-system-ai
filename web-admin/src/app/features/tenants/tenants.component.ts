import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TenantSummary } from '../../core/models/models';
import { TenantService } from '../../core/services/tenant.service';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tenants.component.html'
})
export class TenantsComponent implements OnInit {
  private tenantService = inject(TenantService);

  tenants: TenantSummary[] = [];
  errorMessage = '';

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.tenantService.getAll().subscribe({ next: t => this.tenants = t, error: () => this.errorMessage = 'Could not load tenants.' });
  }

  toggleActive(t: TenantSummary): void {
    const action = t.isActive ? this.tenantService.deactivate(t.id) : this.tenantService.activate(t.id);
    action.subscribe({ next: () => this.reload(), error: () => this.errorMessage = 'Could not update tenant status.' });
  }
}
