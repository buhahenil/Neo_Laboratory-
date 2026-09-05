import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="auth-wrapper d-flex align-items-center justify-content-center min-vh-100">
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-md-5">
            <div class="text-center mb-4">
              <a routerLink="/" class="d-inline-flex align-items-center fw-bold text-primary text-decoration-none fs-3">
                <span class="material-icons me-2">biotech</span>
                Neo Leboretory
              </a>
            </div>
            
            <div class="glass-card shadow-lg p-4">
              <h3 class="fw-bold mb-2">Sign In</h3>
              <p class="text-secondary small mb-4">Access patient portal or management panels.</p>
              
              <form (submit)="onSubmit($event)">
                <div class="mb-3">
                  <label class="form-label small fw-medium">Email Address</label>
                  <div class="input-group border rounded-3 p-1">
                    <span class="input-group-text border-0 bg-transparent text-muted"><span class="material-icons">email</span></span>
                    <input type="email" [(ngModel)]="email" name="email" class="form-control border-0 shadow-none bg-transparent" placeholder="name@domain.com" required />
                  </div>
                </div>

                <div class="mb-4">
                  <div class="d-flex justify-content-between mb-1">
                    <label class="form-label small fw-medium mb-0">Password</label>
                    <a routerLink="/auth/forgot-password" class="small text-decoration-none">Forgot?</a>
                  </div>
                  <div class="input-group border rounded-3 p-1">
                    <span class="input-group-text border-0 bg-transparent text-muted"><span class="material-icons">lock</span></span>
                    <input [type]="showLoginPassword ? 'text' : 'password'" [(ngModel)]="password" name="password" class="form-control border-0 shadow-none bg-transparent" placeholder="••••••••" required />
                    <button type="button" class="btn border-0 bg-transparent text-muted px-2 py-0" (click)="showLoginPassword = !showLoginPassword">
                      <span class="material-icons fs-5">{{ showLoginPassword ? 'visibility_off' : 'visibility' }}</span>
                    </button>
                  </div>
                </div>

                <div class="mb-4">
                  <label class="form-label small fw-medium mb-1">Active Branch (Staff Only)</label>
                  <div class="input-group border rounded-3 p-1">
                    <span class="input-group-text border-0 bg-transparent text-muted"><span class="material-icons">corporate_fare</span></span>
                    <select [(ngModel)]="selectedBranchId" name="loginBranch" class="form-select border-0 shadow-none bg-transparent">
                      <option [value]="0">Select Branch (For Staff)</option>
                      <option *ngFor="let br of branches" [value]="br.branchId">{{ br.name }}</option>
                    </select>
                  </div>
                </div>

                <button type="submit" class="btn btn-primary-custom w-100 py-2 d-flex align-items-center justify-content-center gap-2" [disabled]="isLoading()">
                  <span *ngIf="isLoading()" class="spinner-border spinner-border-sm"></span>
                  Sign In
                </button>
              </form>

              <!-- Helper Testing credentials -->
              <div class="mt-4 p-3 bg-light rounded border small text-dark">
                <div class="fw-semibold text-secondary mb-1">Testing Credentials:</div>
                <div><strong>Admin:</strong> admin&#64;lab.com / Admin&#64;123</div>
                <div><strong>Staff:</strong> staff&#64;lab.com / Staff&#64;123</div>
                <div><strong>Patient:</strong> patient&#64;lab.com / Patient&#64;123</div>
              </div>

              <div class="text-center mt-4">
                <span class="text-secondary small">New patient? </span>
                <a routerLink="/auth/register" class="small text-decoration-none fw-medium">Create Account</a>
              </div>

            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-wrapper {
      background: radial-gradient(circle at 10% 20%, rgba(59, 130, 246, 0.05) 0%, rgba(139, 92, 246, 0.05) 100%);
    }
  `]
})
export class LoginComponent implements OnInit {
  auth = inject(AuthService);
  private api = inject(ApiService);
  toast = inject(NotificationService);
  router = inject(Router);

  email = '';
  password = '';
  showLoginPassword = false;
  selectedBranchId = 0;
  branches: any[] = [];
  isLoading = signal(false);

  ngOnInit(): void {
    this.api.get<any[]>('branch').subscribe({
      next: (data) => this.branches = data.filter(b => b.isActive)
    });
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    if (!this.email || !this.password) return;

    this.isLoading.set(true);
    this.auth.login({ email: this.email, password: this.password, branchId: this.selectedBranchId }).subscribe({
      next: (user) => {
        this.isLoading.set(false);
        this.toast.showSuccess(`Welcome back, ${user.firstName}!`);
        
        // Redirect based on role
        if (user.role === 'Admin') {
          this.router.navigate(['/admin/dashboard']);
        } else if (user.role === 'Staff') {
          this.router.navigate(['/staff/dashboard']);
        } else {
          this.router.navigate(['/patient/dashboard']);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
