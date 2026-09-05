import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-forgot-password',
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
              <h3 class="fw-bold mb-2">Forgot Password</h3>
              <p class="text-secondary small mb-4">Enter your email and we'll send you a password reset token.</p>
              
              <form (submit)="onSubmit($event)">
                <div class="mb-4">
                  <label class="form-label small fw-medium">Email Address</label>
                  <div class="input-group border rounded-3 p-1">
                    <span class="input-group-text border-0 bg-transparent text-muted"><span class="material-icons">email</span></span>
                    <input type="email" [(ngModel)]="email" name="email" class="form-control border-0 shadow-none bg-transparent" placeholder="name@domain.com" required />
                  </div>
                </div>

                <button type="submit" class="btn btn-primary-custom w-100 py-2 d-flex align-items-center justify-content-center gap-2" [disabled]="isLoading()">
                  <span *ngIf="isLoading()" class="spinner-border spinner-border-sm"></span>
                  Send Token
                </button>
              </form>

              <div class="text-center mt-4">
                <a routerLink="/auth/login" class="small text-decoration-none fw-medium">Back to Login</a>
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
export class ForgotPasswordComponent {
  auth = inject(AuthService);
  toast = inject(NotificationService);
  router = inject(Router);

  email = '';
  isLoading = signal(false);

  onSubmit(event: Event): void {
    event.preventDefault();
    if (!this.email) return;

    this.isLoading.set(true);
    this.auth.forgotPassword(this.email).subscribe({
      next: (res: any) => {
        this.isLoading.set(false);
        this.toast.showSuccess(res.Message || 'Reset token dispatched to your email.');
        // Redirect to Reset Password with token parameter
        this.router.navigate(['/auth/reset-password']);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
