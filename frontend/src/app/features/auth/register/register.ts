import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="auth-wrapper py-5 d-flex align-items-center justify-content-center min-vh-100">
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-md-7 col-lg-6">
            <div class="text-center mb-4">
              <a routerLink="/" class="d-inline-flex align-items-center fw-bold text-primary text-decoration-none fs-3">
                <span class="material-icons me-2">biotech</span>
                Neo Leboretory
              </a>
            </div>
            
            <div class="glass-card shadow-lg p-4">
              <h3 class="fw-bold mb-2">Patient Registration</h3>
              <p class="text-secondary small mb-4">Create your account to book tests and access records.</p>
              
              <form (submit)="onSubmit($event)">
                <div class="row g-3">
                  <!-- First Name -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">First Name</label>
                    <input type="text" [(ngModel)]="formData.firstName" name="firstName" class="form-control bg-transparent" placeholder="John" required />
                  </div>
                  <!-- Last Name -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Last Name</label>
                    <input type="text" [(ngModel)]="formData.lastName" name="lastName" class="form-control bg-transparent" placeholder="Doe" required />
                  </div>
                  <!-- Email -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Email Address</label>
                    <input type="email" [(ngModel)]="formData.email" name="email" class="form-control bg-transparent" placeholder="john.doe&#64;example.com" required />
                  </div>
                  <!-- Phone Number -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Phone Number</label>
                    <input type="tel" [(ngModel)]="formData.phoneNumber" name="phoneNumber" class="form-control bg-transparent" placeholder="9876543210" required />
                  </div>
                  
                  <!-- DOB -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Date of Birth</label>
                    <input type="date" [(ngModel)]="formData.dateOfBirth" name="dateOfBirth" class="form-control bg-transparent" required />
                  </div>
                  <!-- Gender -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Gender</label>
                    <select [(ngModel)]="formData.gender" name="gender" class="form-select bg-transparent" required>
                      <option value="Male">Male</option>
                      <option value="Female">Female</option>
                      <option value="Other">Other</option>
                    </select>
                  </div>

                  <!-- Blood Group -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Blood Group (Optional)</label>
                    <select [(ngModel)]="formData.bloodGroup" name="bloodGroup" class="form-select bg-transparent">
                      <option value="">Select Blood Group</option>
                      <option value="A+">A+</option>
                      <option value="A-">A-</option>
                      <option value="B+">B+</option>
                      <option value="B-">B-</option>
                      <option value="AB+">AB+</option>
                      <option value="AB-">AB-</option>
                      <option value="O+">O+</option>
                      <option value="O-">O-</option>
                    </select>
                  </div>
                  <!-- Address -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Address</label>
                    <input type="text" [(ngModel)]="formData.address" name="address" class="form-control bg-transparent" placeholder="12 Street Name, City" required />
                  </div>

                  <!-- Password -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Password</label>
                    <input type="password" [(ngModel)]="formData.password" name="password" class="form-control bg-transparent" placeholder="••••••••" required />
                  </div>
                  <!-- Confirm Password -->
                  <div class="col-md-6">
                    <label class="form-label small fw-medium">Confirm Password</label>
                    <input type="password" [(ngModel)]="confirmPassword" name="confirmPassword" class="form-control bg-transparent" placeholder="••••••••" required />
                  </div>
                  
                  <div class="col-12 mt-4">
                    <button type="submit" class="btn btn-primary-custom w-100 py-2 d-flex align-items-center justify-content-center gap-2" [disabled]="isLoading()">
                      <span *ngIf="isLoading()" class="spinner-border spinner-border-sm"></span>
                      Register
                    </button>
                  </div>
                </div>
              </form>

              <div class="text-center mt-4">
                <span class="text-secondary small">Already have an account? </span>
                <a routerLink="/auth/login" class="small text-decoration-none fw-medium">Login Here</a>
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
export class RegisterComponent {
  auth = inject(AuthService);
  toast = inject(NotificationService);
  router = inject(Router);

  isLoading = signal(false);
  confirmPassword = '';
  
  formData = {
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    dateOfBirth: '',
    gender: 'Male',
    bloodGroup: '',
    address: ''
  };

  onSubmit(event: Event): void {
    event.preventDefault();

    if (this.formData.password !== this.confirmPassword) {
      this.toast.showError('Passwords do not match.');
      return;
    }

    this.isLoading.set(true);
    this.auth.register(this.formData).subscribe({
      next: (response: any) => {
        this.isLoading.set(false);
        this.toast.showSuccess(response.Message || 'Registration successful! Verification email sent.');
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
