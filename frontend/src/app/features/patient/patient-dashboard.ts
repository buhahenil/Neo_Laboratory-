import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

interface AppointmentRecord {
  appointmentId: number;
  patientId: number;
  branchName: string;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
  paymentStatus: string;
  doctorFirstName?: string;
  doctorLastName?: string;
}

interface NotificationRecord {
  notificationId: number;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="dashboard-layout">
      <!-- Sidebar -->
      <aside class="dashboard-sidebar" [class.mobile-active]="isMobileMenuOpen">
        <div class="p-4 border-bottom border-secondary d-flex align-items-center justify-content-between">
          <a class="d-flex align-items-center fw-bold text-white text-decoration-none fs-4" href="#">
            <span class="material-icons me-2 text-primary">biotech</span>
            Neo Leboretory
          </a>
          <button class="btn text-white d-md-none p-0" (click)="toggleSidebar()">
            <span class="material-icons">close</span>
          </button>
        </div>
        
        <div class="p-3 border-bottom border-secondary small d-flex align-items-center gap-2">
          <span class="material-icons text-primary fs-3">account_circle</span>
          <div>
            <span class="d-block fw-semibold text-white">{{ auth.currentUser()?.firstName }} {{ auth.currentUser()?.lastName }}</span>
            <span class="text-white-50 text-muted">Patient Portal</span>
          </div>
        </div>

        <ul class="nav flex-column p-2 gap-1 flex-grow-1">
          <li class="nav-item">
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'home'" (click)="setTab('home')">
              <span class="material-icons">dashboard</span> Dashboard
            </button>
          </li>
          <li class="nav-item">
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'history'" (click)="setTab('history')">
              <span class="material-icons">history</span> Appointments & Reports
            </button>
          </li>
          <li class="nav-item">
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'profile'" (click)="setTab('profile')">
              <span class="material-icons">manage_accounts</span> Account Settings
            </button>
          </li>
        </ul>

        <div class="p-3 border-top border-secondary">
          <button class="btn btn-outline-danger w-100 d-flex align-items-center justify-content-center gap-2" (click)="auth.logout()">
            <span class="material-icons">logout</span> Logout
          </button>
        </div>
      </aside>

      <!-- Main Content -->
      <div class="dashboard-content-wrapper">
        <!-- Header -->
        <header class="glass-header py-3 px-4 d-flex align-items-center justify-content-between">
          <div class="d-flex align-items-center gap-3">
            <button class="btn btn-outline-secondary d-md-none p-1 rounded" (click)="toggleSidebar()">
              <span class="material-icons">menu</span>
            </button>
            <h4 class="fw-bold mb-0">Patient Workspace</h4>
          </div>
          
          <div class="d-flex align-items-center gap-3">
            <!-- Theme Toggle -->
            <button class="btn btn-sm btn-outline-secondary d-flex align-items-center p-2 rounded-circle" (click)="toggleTheme()">
              <span class="material-icons">{{ isDarkMode() ? 'light_mode' : 'dark_mode' }}</span>
            </button>
            
            <a routerLink="/patient/book" class="btn btn-primary-custom d-flex align-items-center gap-1">
              <span class="material-icons">add</span> Book Test
            </a>
          </div>
        </header>

        <!-- Dynamic Body -->
        <main class="dashboard-main-content">
          <!-- 1. HOME TAB -->
          <div *ngIf="activeTab === 'home'">
            <div class="row g-4 mb-4">
              <!-- Welcome Alert -->
              <div class="col-12">
                <div class="glass-card bg-primary-subtle border-primary text-dark p-4">
                  <h3 class="fw-bold mb-1 text-primary">Welcome, {{ auth.currentUser()?.firstName }}!</h3>
                  <p class="mb-0 text-secondary">Manage your upcoming checkups, view test results, and download secure PDF reports.</p>
                </div>
              </div>

              <!-- Quick Metrics -->
              <div class="col-md-6 col-lg-4">
                <div class="glass-card hoverable h-100 d-flex align-items-center gap-3">
                  <div class="p-3 bg-primary-subtle text-primary rounded-circle"><span class="material-icons fs-1">event_note</span></div>
                  <div>
                    <h3 class="fw-bold mb-0">{{ appointments.length }}</h3>
                    <p class="text-secondary small mb-0">Total Appointments</p>
                  </div>
                </div>
              </div>

              <div class="col-md-6 col-lg-4">
                <div class="glass-card hoverable h-100 d-flex align-items-center gap-3">
                  <div class="p-3 bg-success-subtle text-success rounded-circle"><span class="material-icons fs-1">verified_user</span></div>
                  <div>
                    <h3 class="fw-bold mb-0">{{ completedCount() }}</h3>
                    <p class="text-secondary small mb-0">Completed Reports</p>
                  </div>
                </div>
              </div>

              <div class="col-md-6 col-lg-4">
                <div class="glass-card hoverable h-100 d-flex align-items-center gap-3">
                  <div class="p-3 bg-warning-subtle text-warning rounded-circle"><span class="material-icons fs-1">hourglass_empty</span></div>
                  <div>
                    <h3 class="fw-bold mb-0">{{ pendingCount() }}</h3>
                    <p class="text-secondary small mb-0">Pending Bookings</p>
                  </div>
                </div>
              </div>
            </div>

            <div class="row g-4">
              <!-- Upcoming Appointment -->
              <div class="col-lg-7">
                <div class="glass-card h-100">
                  <h5 class="fw-bold text-primary mb-3">Next Scheduled Appointment</h5>
                  
                  <div *ngIf="nextAppointment(); else noUpcoming" class="p-3 bg-body-tertiary rounded border border-start border-4 border-primary">
                    <div class="d-flex justify-content-between align-items-start mb-3">
                      <div>
                        <h6 class="fw-bold mb-1">{{ nextAppointment()?.branchName }}</h6>
                        <span class="text-muted small">Date: {{ nextAppointment()?.appointmentDate }}</span>
                      </div>
                      <span class="badge bg-primary">{{ nextAppointment()?.status }}</span>
                    </div>
                    
                    <div class="d-flex justify-content-between align-items-center small text-secondary pt-2 border-top">
                      <span>Time: <strong>{{ nextAppointment()?.startTime }} - {{ nextAppointment()?.endTime }}</strong></span>
                      <span>Doctor: <strong>{{ nextAppointment()?.doctorFirstName ? 'Dr. ' + nextAppointment()?.doctorFirstName : 'Self Referrer' }}</strong></span>
                    </div>
                  </div>
                  
                  <ng-template #noUpcoming>
                    <div class="text-center py-5 text-secondary">
                      <span class="material-icons fs-1 text-muted mb-2">event_busy</span>
                      <p>You have no upcoming visits. Stay on top of your health!</p>
                      <a routerLink="/patient/book" class="btn btn-sm btn-outline-primary rounded-pill">Book Test Now</a>
                    </div>
                  </ng-template>
                </div>
              </div>

              <!-- Notifications -->
              <div class="col-lg-5">
                <div class="glass-card h-100">
                  <h5 class="fw-bold text-primary mb-3">Recent Notifications</h5>
                  <div class="notification-list overflow-y-auto max-height-300 gap-2 d-grid">
                    <div class="p-2 border-bottom d-flex align-items-start gap-2 text-secondary" *ngFor="let notif of notifications">
                      <span class="material-icons text-primary small pt-1">notifications</span>
                      <div>
                        <p class="mb-0 text-dark small">{{ notif.message }}</p>
                        <span class="small text-muted" style="font-size: 0.75rem;">{{ notif.createdAt | date:'short' }}</span>
                      </div>
                    </div>
                    
                    <div class="text-center py-5" *ngIf="notifications.length === 0">
                      <p class="text-secondary small">No notifications at this time.</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 2. HISTORY & REPORTS TAB -->
          <div *ngIf="activeTab === 'history'" class="glass-card">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="fw-bold text-primary mb-0">Diagnostic History & Records</h5>
              <div class="input-group max-width-300">
                <span class="input-group-text bg-transparent border-end-0 text-muted"><span class="material-icons">search</span></span>
                <input type="text" [(ngModel)]="historySearch" (input)="filterHistory()" class="form-control border-start-0 bg-transparent shadow-none" placeholder="Search by branch, status..." />
              </div>
            </div>

            <div class="table-responsive">
              <table class="table align-middle">
                <thead>
                  <tr class="bg-light text-secondary small">
                    <th>BOOKING ID</th>
                    <th>BRANCH</th>
                    <th>APPOINTMENT DATE</th>
                    <th>TIME SLOT</th>
                    <th>STATUS</th>
                    <th>PAYMENT</th>
                    <th>BILLING</th>
                    <th>DIAGNOSIS REPORT</th>
                    <th>FEEDBACK</th>
                  </tr>
                </thead>
                <tbody class="small text-secondary">
                  <tr *ngFor="let app of filteredAppointments">
                    <td><strong>APP-{{ app.appointmentId }}</strong></td>
                    <td>{{ app.branchName }}</td>
                    <td>{{ app.appointmentDate | date:'mediumDate' }}</td>
                    <td>{{ app.startTime }}</td>
                    <td>
                      <span class="badge" [ngClass]="{
                        'bg-warning-subtle text-warning': app.status === 'Pending',
                        'bg-info-subtle text-info': app.status === 'Confirmed',
                        'bg-secondary-subtle text-dark': app.status === 'SampleCollected',
                        'bg-primary-subtle text-primary': app.status === 'ResultUploaded',
                        'bg-success-subtle text-success': app.status === 'Completed',
                        'bg-danger-subtle text-danger': app.status === 'Cancelled'
                      }">{{ app.status }}</span>
                    </td>
                    <td>
                      <span class="badge" [ngClass]="app.paymentStatus === 'Paid' ? 'bg-success-subtle text-success' : 'bg-danger-subtle text-danger'">
                        {{ app.paymentStatus }}
                      </span>
                    </td>
                    <td>
                      <!-- Receipt PDF Download -->
                      <button (click)="downloadInvoice(app.appointmentId)" class="btn btn-sm btn-outline-secondary p-1 rounded-circle" title="Download Invoice Receipt">
                        <span class="material-icons">download</span>
                      </button>
                    </td>
                    <td>
                      <!-- Report PDF Download -->
                      <button (click)="downloadReport(app.appointmentId)" 
                              [disabled]="app.status !== 'ResultUploaded' && app.status !== 'Completed'" 
                              class="btn btn-sm btn-primary-custom d-flex align-items-center gap-1 py-1 px-2 rounded-pill" 
                              title="Download Diagnostic Report PDF">
                        <span class="material-icons fs-6">picture_as_pdf</span>
                        Download
                      </button>
                    </td>
                    <td>
                      <!-- Feedback Modal opener -->
                      <button *ngIf="app.status === 'Completed' || app.status === 'ResultUploaded'" 
                              (click)="openFeedbackModal(app.appointmentId)" 
                              class="btn btn-sm btn-outline-primary rounded-pill px-2 py-1">
                        Review
                      </button>
                    </td>
                  </tr>
                  
                  <tr *ngIf="filteredAppointments.length === 0">
                    <td colspan="9" class="text-center py-4">No appointments found.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- 3. PROFILE & ACCOUNT TAB -->
          <div *ngIf="activeTab === 'profile'" class="row g-4">
            <!-- Profile Info -->
            <div class="col-lg-6">
              <div class="glass-card">
                <h5 class="fw-bold text-primary mb-3">Edit Patient Demographics</h5>
                <form (submit)="updateProfile($event)">
                  <div class="row g-3">
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">First Name</label>
                      <input type="text" [(ngModel)]="profileData.firstName" name="firstName" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">Last Name</label>
                      <input type="text" [(ngModel)]="profileData.lastName" name="lastName" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">Phone Number</label>
                      <input type="tel" [(ngModel)]="profileData.phoneNumber" name="phoneNumber" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">Date of Birth</label>
                      <input type="date" [(ngModel)]="profileData.dateOfBirth" name="dateOfBirth" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">Gender</label>
                      <select [(ngModel)]="profileData.gender" name="gender" class="form-select bg-transparent" required>
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                        <option value="Other">Other</option>
                      </select>
                    </div>
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">Blood Group</label>
                      <input type="text" [(ngModel)]="profileData.bloodGroup" name="bloodGroup" class="form-control bg-transparent" readonly />
                    </div>
                    <div class="col-12">
                      <label class="form-label small fw-medium">Home Address</label>
                      <input type="text" [(ngModel)]="profileData.address" name="address" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-12 text-end mt-4">
                      <button type="submit" class="btn btn-primary-custom" [disabled]="profileLoading()">Update Profile</button>
                    </div>
                  </div>
                </form>
              </div>
            </div>

            <!-- Password Change -->
            <div class="col-lg-6">
              <div class="glass-card">
                <h5 class="fw-bold text-primary mb-3">Change Security Password</h5>
                <form (submit)="changePassword($event)">
                  <div class="mb-3">
                    <label class="form-label small fw-medium">Old Password</label>
                    <input type="password" [(ngModel)]="oldPassword" name="oldPassword" class="form-control bg-transparent" required />
                  </div>
                  <div class="mb-3">
                    <label class="form-label small fw-medium">New Password</label>
                    <input type="password" [(ngModel)]="newPassword" name="newPassword" class="form-control bg-transparent" required />
                  </div>
                  <div class="mb-4">
                    <label class="form-label small fw-medium">Confirm New Password</label>
                    <input type="password" [(ngModel)]="confirmPassword" name="confirmPassword" class="form-control bg-transparent" required />
                  </div>
                  <div class="text-end">
                    <button type="submit" class="btn btn-outline-primary" [disabled]="passwordLoading()">Save Password</button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>

    <!-- Feedback Modal -->
    <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);" *ngIf="showFeedbackModal">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content glass-card border-0 p-0 text-dark">
          <div class="modal-header border-0 p-4 pb-0 d-flex justify-content-between">
            <h5 class="fw-bold text-primary mb-0">Share Your Experience</h5>
            <button type="button" class="btn-close shadow-none" (click)="closeFeedbackModal()"></button>
          </div>
          <div class="modal-body p-4">
            <form (submit)="submitFeedback($event)">
              <div class="mb-3 text-center">
                <label class="form-label small fw-medium d-block mb-3">Rate the quality of our diagnostics & report delivery</label>
                <div class="d-flex justify-content-center gap-2">
                  <button type="button" *ngFor="let star of [1,2,3,4,5]" 
                          (click)="feedbackRating = star" 
                          class="btn p-1 text-warning border-0 bg-transparent">
                    <span class="material-icons fs-1">{{ feedbackRating >= star ? 'star' : 'star_border' }}</span>
                  </button>
                </div>
              </div>
              
              <div class="mb-4">
                <label class="form-label small fw-medium">Comments / Suggestions</label>
                <textarea [(ngModel)]="feedbackComments" name="comments" rows="3" class="form-control bg-transparent" placeholder="Type comments here..."></textarea>
              </div>

              <button type="submit" class="btn btn-primary-custom w-100 py-2">Submit Feedback</button>
            </form>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .nav-link {
      color: var(--text-secondary);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      border: none;
      background: transparent;
      transition: all 0.2s ease;
      
      &:hover {
        background-color: rgba(59, 130, 246, 0.1);
        color: var(--primary-color);
      }
      
      &.active {
        background-color: var(--primary-color) !important;
        color: white !important;
      }
    }
    .max-height-300 {
      max-height: 300px;
    }
    .max-width-300 {
      max-width: 300px;
    }
  `]
})
export class PatientDashboardComponent implements OnInit {
  auth = inject(AuthService);
  private api = inject(ApiService);
  private toast = inject(NotificationService);

  isMobileMenuOpen = false;
  isDarkMode = signal(false);
  activeTab = 'home';
  historySearch = '';

  appointments: AppointmentRecord[] = [];
  filteredAppointments: AppointmentRecord[] = [];
  notifications: NotificationRecord[] = [];

  // Demographics Profile Edit
  profileData = {
    patientId: 0,
    firstName: '',
    lastName: '',
    phoneNumber: '',
    dateOfBirth: '',
    gender: 'Male',
    bloodGroup: '',
    address: ''
  };
  profileLoading = signal(false);

  // Password Change
  oldPassword = '';
  newPassword = '';
  confirmPassword = '';
  passwordLoading = signal(false);

  // Feedback Modal
  showFeedbackModal = false;
  feedbackAppId = 0;
  feedbackRating = 5;
  feedbackComments = '';

  ngOnInit(): void {
    this.initTheme();
    this.loadDashboardData();
  }

  initTheme(): void {
    const theme = localStorage.getItem('theme');
    if (theme === 'dark') {
      document.body.classList.add('dark-theme');
    } else if (theme === 'light') {
      document.body.classList.remove('dark-theme');
    }
    this.isDarkMode.set(document.body.classList.contains('dark-theme'));
  }

  toggleTheme(): void {
    const isDark = document.body.classList.toggle('dark-theme');
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
    this.isDarkMode.set(isDark);
  }

  toggleSidebar(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  setTab(tab: string): void {
    this.activeTab = tab;
    this.isMobileMenuOpen = false;
  }

  loadDashboardData(): void {
    const user = this.auth.currentUser();
    if (!user || !user.patientId) return;

    // Load patient appointments
    this.api.get<AppointmentRecord[]>(`appointment/patient/${user.patientId}`).subscribe({
      next: (data) => {
        this.appointments = data;
        this.filteredAppointments = [...this.appointments];
        this.populateProfileData();
      }
    });

    // Load patient notifications
    this.api.get<NotificationRecord[]>(`notification/user/${user.userId}`).subscribe({
      next: (data) => {
        this.notifications = data;
      }
    });
  }

  completedCount = computed(() => {
    return this.appointments.filter(a => a.status === 'Completed' || a.status === 'ResultUploaded').length;
  });

  pendingCount = computed(() => {
    return this.appointments.filter(a => a.status === 'Pending' || a.status === 'Confirmed').length;
  });

  nextAppointment() {
    const upcoming = this.appointments.filter(a => a.status === 'Pending' || a.status === 'Confirmed');
    return upcoming.length > 0 ? upcoming[0] : null;
  }

  filterHistory(): void {
    if (!this.historySearch) {
      this.filteredAppointments = [...this.appointments];
      return;
    }
    const query = this.historySearch.toLowerCase();
    this.filteredAppointments = this.appointments.filter(a => 
      a.branchName.toLowerCase().includes(query) || 
      a.status.toLowerCase().includes(query) || 
      a.appointmentId.toString().includes(query)
    );
  }

  populateProfileData(): void {
    const user = this.auth.currentUser();
    if (!user || !user.patientId) return;

    this.api.get<any>(`patient/${user.patientId}`).subscribe({
      next: (patient) => {
        this.profileData = {
          patientId: patient.patientId,
          firstName: patient.firstName,
          lastName: patient.lastName,
          phoneNumber: patient.phoneNumber || '',
          dateOfBirth: patient.dateOfBirth ? patient.dateOfBirth.split('T')[0] : '',
          gender: patient.gender,
          bloodGroup: patient.bloodGroup || 'A+',
          address: patient.address || ''
        };
      }
    });
  }

  // --- ACTIONS ---
  updateProfile(event: Event): void {
    event.preventDefault();
    this.profileLoading.set(true);
    this.api.put('patient', this.profileData).subscribe({
      next: (res: any) => {
        this.profileLoading.set(false);
        this.toast.showSuccess(res.Message || 'Profile updated successfully.');
        this.loadDashboardData();
      },
      error: () => this.profileLoading.set(false)
    });
  }

  changePassword(event: Event): void {
    event.preventDefault();
    if (this.newPassword !== this.confirmPassword) {
      this.toast.showError('New passwords do not match.');
      return;
    }

    this.passwordLoading.set(true);
    const payload = {
      userId: this.auth.currentUser()?.userId,
      oldPassword: this.oldPassword,
      newPassword: this.newPassword
    };

    this.auth.changePassword(payload).subscribe({
      next: (res: any) => {
        this.passwordLoading.set(false);
        this.toast.showSuccess('Password updated successfully.');
        this.oldPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
      },
      error: () => this.passwordLoading.set(false)
    });
  }

  // PDF Download helpers
  downloadInvoice(appId: number): void {
    this.api.getBlob(`appointment/${appId}/invoice-pdf`).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Invoice-${appId}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        this.toast.showSuccess('Invoice receipt downloaded successfully.');
      }
    });
  }

  downloadReport(appId: number): void {
    this.api.getBlob(`appointment/${appId}/report-pdf`).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `LabReport-${appId}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        this.toast.showSuccess('Diagnostic Report downloaded successfully.');
      }
    });
  }

  // Feedback modal
  openFeedbackModal(appId: number): void {
    this.feedbackAppId = appId;
    this.feedbackRating = 5;
    this.feedbackComments = '';
    this.showFeedbackModal = true;
  }

  closeFeedbackModal(): void {
    this.showFeedbackModal = false;
  }

  submitFeedback(event: Event): void {
    event.preventDefault();
    const payload = {
      patientId: this.auth.currentUser()?.patientId,
      rating: this.feedbackRating,
      comments: this.feedbackComments
    };

    this.api.post('appointment/feedback', payload).subscribe({
      next: (res: any) => {
        this.toast.showSuccess(res.Message || 'Thank you for your rating!');
        this.closeFeedbackModal();
      }
    });
  }
}
