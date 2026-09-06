import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

declare var Chart: any;

interface StaffAppointment {
  appointmentId: number;
  patientFirstName: string;
  patientLastName: string;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  status: string;
  paymentStatus: string;
  totalAmount: number;
  isPrinted?: boolean;
  printedCount?: number;
}

interface TestReportDetail {
  reportId: number;
  appointmentId: number;
  testId: number;
  testName: string;
  testCode: string;
  sampleType: string;
  normalRange: string;
  resultValue?: string;
  remarks?: string;
  status: string;
  isOutsourced?: boolean;
  externalLabName?: string;
  externalBarcode?: string;
}

@Component({
  selector: 'app-staff-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="dashboard-layout">
      <div class="sidebar-backdrop" *ngIf="isMobileMenuOpen" (click)="toggleSidebar()"></div>
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
        
        <div class="p-3 border-bottom border-secondary small d-flex align-items-center gap-2 text-white-50">
          <span class="material-icons text-primary fs-3">science</span>
          <div>
            <span class="d-block fw-semibold text-white">{{ auth.currentUser()?.firstName }} {{ auth.currentUser()?.lastName }}</span>
            <span class="small">Senior Technologist</span>
          </div>
        </div>

        <ul class="nav flex-column p-2 gap-1 flex-grow-1">
          <li>
            <button (click)="setTab('workspace')" class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeSubTab === 'workspace'">
              <span class="material-icons">biotech</span> Laboratory Board
            </button>
          </li>
          <li>
            <button (click)="setTab('analytics')" class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeSubTab === 'analytics'">
              <span class="material-icons">analytics</span> Branch Dashboard
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
          <div class="d-flex align-items-center gap-2 gap-md-3">
            <button class="btn btn-outline-secondary d-md-none p-1 rounded" (click)="toggleSidebar()">
              <span class="material-icons">menu</span>
            </button>
            <h4 class="fw-bold mb-0 fs-5 text-truncate">Lab Technician Workspace</h4>
          </div>
          
          <div class="d-flex align-items-center gap-2 gap-md-3">
            <button class="btn btn-sm btn-outline-secondary d-flex align-items-center p-2 rounded-circle" (click)="toggleTheme()">
              <span class="material-icons">{{ isDarkMode() ? 'light_mode' : 'dark_mode' }}</span>
            </button>
            
            <span class="badge bg-secondary-subtle text-secondary py-2 px-3 border rounded-pill">
              Branch: {{ staffBranchName() }}
            </span>
          </div>
        </header>

        <!-- Body -->
        <main class="dashboard-main-content">
          <!-- Laboratory Board Workspace -->
          <div *ngIf="activeSubTab === 'workspace'" class="row g-4">
            
            <!-- Left panel: Appointments List -->
            <div class="col-lg-6">
              <div class="glass-card h-100">
                <div class="d-flex justify-content-between align-items-center mb-4">
                  <h5 class="fw-bold text-primary mb-0">Branch Bookings</h5>
                  <span class="badge bg-primary">{{ appointments.length }} Registered</span>
                </div>

                <div class="list-group gap-2 overflow-y-auto max-height-600">
                  <div *ngFor="let app of appointments" 
                       (click)="selectAppointment(app)" 
                       class="list-group-item list-group-item-action p-3 rounded border cursor-pointer"
                       [class.border-primary]="selectedAppId === app.appointmentId"
                       [class.bg-primary-subtle]="selectedAppId === app.appointmentId">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                      <div>
                        <h6 class="fw-bold mb-0">Patient: {{ app.patientFirstName }} {{ app.patientLastName }}</h6>
                        <span class="text-muted small">Booking ID: APP-{{ app.appointmentId }}</span>
                      </div>
                      <span class="badge" [ngClass]="{
                        'bg-warning-subtle text-warning': app.status === 'Pending',
                        'bg-info-subtle text-info': app.status === 'Confirmed',
                        'bg-secondary-subtle text-dark': app.status === 'SampleCollected',
                        'bg-primary-subtle text-primary': app.status === 'ResultUploaded',
                        'bg-success-subtle text-success': app.status === 'Completed',
                        'bg-danger-subtle text-danger': app.status === 'Cancelled'
                      }">{{ app.status }}</span>
                    </div>

                    <div class="d-flex justify-content-between align-items-center small text-secondary pt-2 border-top">
                      <span>Schedule: <strong>{{ app.appointmentDate | date:'mediumDate' }} &#64; {{ app.startTime.substring(0,5) }}</strong></span>
                      <span>Payment: <strong [class.text-success]="app.paymentStatus === 'Paid'">{{ app.paymentStatus }}</strong></span>
                    </div>
                  </div>

                  <div class="text-center py-5 text-secondary" *ngIf="appointments.length === 0">
                    <span class="material-icons fs-1 text-muted mb-2">science</span>
                    <p>No bookings assigned to this branch location.</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Right panel: Details & Results Parameters Upload -->
            <div class="col-lg-6">
              <div class="glass-card h-100" *ngIf="selectedAppId > 0; else selectPrompt">
                <h5 class="fw-bold text-primary mb-3">Booking APP-{{ selectedAppId }} Overview</h5>
                
                <!-- Status Actions & Print Bar -->
                <div class="d-flex flex-wrap gap-2 mb-3 p-2 bg-light rounded border align-items-center justify-content-between">
                  <div class="d-flex align-items-center gap-2">
                    <span class="small fw-semibold text-secondary">Status:</span>
                    <button *ngIf="selectedApp.status === 'Pending'" (click)="updateStatus('Confirmed')" class="btn btn-sm btn-outline-info">Confirm Appointment</button>
                    <button *ngIf="selectedApp.status === 'Confirmed'" (click)="updateStatus('SampleCollected')" class="btn btn-sm btn-outline-secondary">Collect Sample</button>
                    <button *ngIf="selectedApp.status === 'ResultUploaded'" (click)="updateStatus('Completed')" class="btn btn-sm btn-success">Complete Booking</button>
                  </div>
                  
                  <div class="d-flex align-items-center gap-2">
                    <span class="badge bg-success-subtle text-success border border-success" *ngIf="selectedApp.isPrinted">
                      Printed (x{{ selectedApp.printedCount || 1 }})
                    </span>
                    <button (click)="printReportPdf(false)" class="btn btn-sm btn-outline-primary d-flex align-items-center gap-1">
                      <span class="material-icons fs-6">picture_as_pdf</span> Digital PDF
                    </button>
                    <button (click)="printReportPdf(true)" class="btn btn-sm btn-primary-custom d-flex align-items-center gap-1">
                      <span class="material-icons fs-6">print</span> Print on Letterhead
                    </button>
                  </div>
                </div>

                <!-- Tests Upload form -->
                <h6 class="fw-bold mb-3 text-secondary">Diagnostic Parameters & Outsourcing</h6>
                
                <div class="gap-3 d-grid">
                  <div class="p-3 border rounded" *ngFor="let rep of reports; let idx = index">
                    <div class="d-flex justify-content-between mb-2">
                      <span class="fw-bold text-dark">{{ rep.testName }} ({{ rep.testCode }})</span>
                      <div class="d-flex gap-2 align-items-center">
                        <span class="badge bg-warning-subtle text-warning border" *ngIf="rep.isOutsourced">Outsourced</span>
                        <span class="badge" [class.bg-success]="rep.status === 'Completed'" [class.bg-warning]="rep.status === 'Pending'">
                          {{ rep.status }}
                        </span>
                      </div>
                    </div>
                    
                    <div class="small text-secondary mb-2">
                      <span class="me-3">Sample: <strong>{{ rep.sampleType }}</strong></span>
                      <span>Ref Range: <strong>{{ rep.normalRange || 'N/A' }}</strong></span>
                    </div>

                    <!-- Result Values -->
                    <div class="row g-2 mb-2">
                      <div class="col-md-6">
                        <label class="form-label small fw-medium">Observed Value</label>
                        <input type="text" [(ngModel)]="reports[idx].resultValue" class="form-control form-control-sm" placeholder="e.g. 14.5 g/dL" [disabled]="rep.status === 'Completed'" />
                      </div>
                      <div class="col-md-6">
                        <label class="form-label small fw-medium">Remarks / Diagnostic Flags</label>
                        <input type="text" [(ngModel)]="reports[idx].remarks" class="form-control form-control-sm" placeholder="Normal/Borderline" [disabled]="rep.status === 'Completed'" />
                      </div>
                    </div>

                    <!-- Outsourcing Dispatch Box -->
                    <div class="p-2 bg-light rounded border mt-2">
                      <div class="form-check form-switch mb-1">
                        <input class="form-check-input" type="checkbox" role="switch" [id]="'outsourceSwitch_' + rep.reportId" [(ngModel)]="reports[idx].isOutsourced" (change)="saveOutsourceDispatch(rep)" />
                        <label class="form-check-label small fw-bold text-secondary" [for]="'outsourceSwitch_' + rep.reportId">Outsource to External Partner Lab</label>
                      </div>

                      <div class="row g-2 mt-1" *ngIf="rep.isOutsourced">
                        <div class="col-md-6">
                          <input type="text" [(ngModel)]="reports[idx].externalLabName" class="form-control form-control-sm" placeholder="External Lab Name (e.g. Thyrocare)" />
                        </div>
                        <div class="col-md-6">
                          <input type="text" [(ngModel)]="reports[idx].externalBarcode" class="form-control form-control-sm" placeholder="Sample Barcode No." />
                        </div>
                        <div class="col-12 text-end">
                          <button (click)="saveOutsourceDispatch(rep)" class="btn btn-sm btn-outline-secondary py-0 px-2 small">Save Dispatch Details</button>
                        </div>
                      </div>
                    </div>
                    
                    <div class="text-end mt-2" *ngIf="rep.status === 'Pending'">
                      <button (click)="uploadReportValue(rep)" class="btn btn-sm btn-primary-custom d-flex align-items-center gap-1 ms-auto py-1 px-3">
                        <span class="material-icons fs-6">publish</span> Save Value
                      </button>
                    </div>
                  </div>
                </div>

              </div>

              <ng-template #selectPrompt>
                <div class="glass-card h-100 d-flex align-items-center justify-content-center text-center py-5">
                  <div>
                    <span class="material-icons fs-1 text-muted mb-2">fact_check</span>
                    <p class="text-secondary">Select an appointment from the left roster to view parameters and upload diagnostic reports.</p>
                  </div>
                </div>
              </ng-template>
            </div>

          </div>

          <!-- Branch Dashboard Analytics Tab -->
          <div *ngIf="activeSubTab === 'analytics'">
            <!-- Filters Bar -->
            <div class="glass-card mb-4 p-4">
              <div class="d-flex flex-wrap gap-3 align-items-end">
                <div style="flex: 1; min-width: 120px;">
                  <label class="form-label small fw-bold text-secondary mb-1">Year</label>
                  <select [(ngModel)]="filterYear" (change)="loadBranchAnalytics()" class="form-select">
                    <option value="">All Years</option>
                    <option *ngFor="let y of years" [value]="y">{{ y }}</option>
                  </select>
                </div>
                <div style="flex: 1; min-width: 130px;">
                  <label class="form-label small fw-bold text-secondary mb-1">Month</label>
                  <select [(ngModel)]="filterMonth" (change)="loadBranchAnalytics()" class="form-select">
                    <option value="">All Months</option>
                    <option *ngFor="let m of months" [value]="m.value">{{ m.name }}</option>
                  </select>
                </div>
                <div style="flex: 1; min-width: 140px;">
                  <label class="form-label small fw-bold text-secondary mb-1">Start Date</label>
                  <input type="date" [(ngModel)]="filterStartDate" (change)="loadBranchAnalytics()" class="form-control" />
                </div>
                <div style="flex: 1; min-width: 140px;">
                  <label class="form-label small fw-bold text-secondary mb-1">End Date</label>
                  <input type="date" [(ngModel)]="filterEndDate" (change)="loadBranchAnalytics()" class="form-control" />
                </div>
                <div>
                  <button (click)="clearBranchFilters()" class="btn border-0 bg-danger-subtle text-danger d-inline-flex align-items-center justify-content-center gap-1 px-3" style="height: 38px; border-radius: 8px; font-weight: 600; font-size: 0.875rem;" title="Reset all filters">
                    <span class="material-icons" style="font-size: 16px;">close</span> Clear
                  </button>
                </div>
              </div>
            </div>

            <!-- KPI Cards -->
            <div class="row g-4 mb-4">
              <!-- Total Patients -->
              <div class="col-md-6 col-lg-3">
                <div class="glass-card hoverable d-flex align-items-center gap-3">
                  <div class="p-3 bg-primary-subtle text-primary rounded-circle"><span class="material-icons fs-2">people</span></div>
                  <div>
                    <h4 class="fw-bold mb-0">{{ statsData.totalPatients }}</h4>
                    <p class="text-secondary small mb-0">Total Patients</p>
                  </div>
                </div>
              </div>

              <!-- Today's Bookings -->
              <div class="col-md-6 col-lg-3">
                <div class="glass-card hoverable d-flex align-items-center gap-3">
                  <div class="p-3 bg-warning-subtle text-warning rounded-circle"><span class="material-icons fs-2">today</span></div>
                  <div>
                    <h4 class="fw-bold mb-0">{{ statsData.todayBookings }}</h4>
                    <p class="text-secondary small mb-0">Today's Bookings</p>
                  </div>
                </div>
              </div>

              <!-- New Patients -->
              <div class="col-md-6 col-lg-3">
                <div class="glass-card hoverable d-flex align-items-center gap-3">
                  <div class="p-3 bg-info-subtle text-info rounded-circle"><span class="material-icons fs-2">person_add</span></div>
                  <div>
                    <h4 class="fw-bold mb-0">{{ statsData.newPatients }}</h4>
                    <p class="text-secondary small mb-0">New Patients</p>
                  </div>
                </div>
              </div>

              <!-- Returning Patients -->
              <div class="col-md-6 col-lg-3">
                <div class="glass-card hoverable d-flex align-items-center gap-3">
                  <div class="p-3 bg-danger-subtle text-danger rounded-circle"><span class="material-icons fs-2">autorenew</span></div>
                  <div>
                    <h4 class="fw-bold mb-0">{{ statsData.returningPatients }}</h4>
                    <p class="text-secondary small mb-0">Returning Patients</p>
                  </div>
                </div>
              </div>

              <!-- Total Collection (Manager Only) -->
              <div class="col-md-6 col-lg-3" *ngIf="isBranchManager()">
                <div class="glass-card hoverable d-flex align-items-center gap-3">
                  <div class="p-3 bg-success-subtle text-success rounded-circle"><span class="material-icons fs-2">payments</span></div>
                  <div>
                    <h4 class="fw-bold mb-0">INR {{ statsData.totalRevenue | number:'1.2-2' }}</h4>
                    <p class="text-secondary small mb-0">Total Collection</p>
                  </div>
                </div>
              </div>

              <!-- Avg Revenue per Patient (Manager Only) -->
              <div class="col-md-6 col-lg-3" *ngIf="isBranchManager()">
                <div class="glass-card hoverable d-flex align-items-center gap-3">
                  <div class="p-3 bg-secondary-subtle text-secondary rounded-circle"><span class="material-icons fs-2">trending_up</span></div>
                  <div>
                    <h4 class="fw-bold mb-0">INR {{ statsData.averageRevenuePerPatient | number:'1.2-2' }}</h4>
                    <p class="text-secondary small mb-0">Avg Revenue/Patient</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Charts -->
            <div class="row g-4">
              <!-- Collection Chart (Manager Only) -->
              <div class="col-lg-6" *ngIf="isBranchManager()">
                <div class="glass-card">
                  <h5 class="fw-bold text-primary text-center mb-3 fs-5">Monthly Collection</h5>
                  <div style="position: relative; height:240px;">
                    <canvas id="branchRevenueChart"></canvas>
                  </div>
                </div>
              </div>

              <!-- Patients Growth Chart -->
              <div class="col-lg-6" [class.col-lg-12]="!isBranchManager()">
                <div class="glass-card">
                  <h5 class="fw-bold text-primary text-center mb-3 fs-5">Monthly Patients</h5>
                  <div style="position: relative; height:240px;">
                    <canvas id="branchPatientsChart"></canvas>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  `,
  styles: [`
    .cursor-pointer {
      cursor: pointer;
    }
    .max-height-600 {
      max-height: 600px;
    }
  `]
})
export class StaffDashboardComponent implements OnInit {
  auth = inject(AuthService);
  private api = inject(ApiService);
  private toast = inject(NotificationService);

  isMobileMenuOpen = false;
  isDarkMode = signal(false);
  activeSubTab = 'workspace';

  appointments: StaffAppointment[] = [];
  selectedAppId = 0;
  selectedApp: any = null;
  reports: TestReportDetail[] = [];
  staffBranchId = 0;
  staffBranchName = signal('Main Branch');

  // Branch stats
  statsData: any = {
    totalPatients: 0,
    totalAppointments: 0,
    todayBookings: 0,
    pendingReports: 0,
    totalRevenue: 0,
    newPatients: 0,
    returningPatients: 0,
    averageRevenuePerPatient: 0,
    popularTests: [],
    monthlyRevenue: [],
    monthlyPatients: [],
    recentActivity: []
  };

  // Branch filters
  filterYear = '';
  filterMonth = '';
  filterStartDate = '';
  filterEndDate = '';

  years: number[] = [2026, 2025, 2024];
  months = [
    { value: 1, name: 'January' },
    { value: 2, name: 'February' },
    { value: 3, name: 'March' },
    { value: 4, name: 'April' },
    { value: 5, name: 'May' },
    { value: 6, name: 'June' },
    { value: 7, name: 'July' },
    { value: 8, name: 'August' },
    { value: 9, name: 'September' },
    { value: 10, name: 'October' },
    { value: 11, name: 'November' },
    { value: 12, name: 'December' }
  ];

  branchRevenueChartInstance: any = null;
  branchPatientsChartInstance: any = null;

  isBranchManager(): boolean {
    const user = this.auth.currentUser();
    if (!user) return false;
    const designation = (user.designation || '').toLowerCase();
    const role = (user.role || '').toLowerCase();
    return designation.includes('manager') || designation.includes('admin') || designation.includes('director') || role === 'admin';
  }

  ngOnInit(): void {
    this.initTheme();
    this.loadStaffInfo();
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

  loadStaffInfo(): void {
    const user = this.auth.currentUser();
    if (!user) return;

    if (user.branchId) {
      this.staffBranchId = user.branchId;
      this.staffBranchName.set(user.branchName || 'Active Branch');
      this.loadBranchAppointments();
      if (this.activeSubTab === 'analytics') this.loadBranchAnalytics();
    } else {
      this.api.get<any>(`auth/staff-profile/${user.userId}`).subscribe({
        next: (staffProfile) => {
          if (staffProfile) {
            this.staffBranchId = staffProfile.branchId;
            this.staffBranchName.set(staffProfile.branchName);
            this.loadBranchAppointments();
            if (this.activeSubTab === 'analytics') this.loadBranchAnalytics();
          }
        }
      });
    }
  }

  loadBranchAppointments(): void {
    this.api.get<StaffAppointment[]>(`appointment/branch/${this.staffBranchId}`).subscribe({
      next: (data) => {
        this.appointments = data;
        // Keep selected appointment updated if loaded
        if (this.selectedAppId > 0) {
          const updated = this.appointments.find(a => a.appointmentId === this.selectedAppId);
          if (updated) this.selectedApp = updated;
        }
      }
    });
  }

  selectAppointment(app: StaffAppointment): void {
    this.selectedAppId = app.appointmentId;
    this.selectedApp = app;
    this.api.get<TestReportDetail[]>(`appointment/${app.appointmentId}/reports`).subscribe({
      next: (data) => this.reports = data
    });
  }

  updateStatus(status: string): void {
    const payload = {
      appointmentId: this.selectedAppId,
      status: status,
      staffId: this.auth.currentUser()?.staffId
    };

    this.api.put('appointment/status', payload).subscribe({
      next: (res: any) => {
        this.toast.showSuccess(res.Message || 'Status updated.');
        this.loadBranchAppointments();
        // Update selected app status locally
        this.selectedApp.status = status;
      }
    });
  }

  printReportPdf(preprinted: boolean): void {
    if (!this.selectedAppId) return;

    // Log print audit
    const staffId = this.auth.currentUser()?.staffId || 1;
    this.api.post(`appointment/${this.selectedAppId}/mark-printed`, { staffId }).subscribe({
      next: () => {
        if (this.selectedApp) {
          this.selectedApp.isPrinted = true;
          this.selectedApp.printedCount = (this.selectedApp.printedCount || 0) + 1;
        }
      }
    });

    const url = `${this.api.getBaseUrl()}/appointment/${this.selectedAppId}/report-pdf?preprinted=${preprinted}`;
    window.open(url, '_blank');
  }

  saveOutsourceDispatch(rep: TestReportDetail): void {
    const payload = {
      reportId: rep.reportId,
      isOutsourced: rep.isOutsourced || false,
      externalLabName: rep.externalLabName || '',
      externalBarcode: rep.externalBarcode || ''
    };

    this.api.put('appointment/reports/outsource-dispatch', payload).subscribe({
      next: (res: any) => {
        this.toast.showSuccess(res.Message || 'Outsourced dispatch updated.');
      }
    });
  }

  uploadReportValue(rep: TestReportDetail): void {
    if (!rep.resultValue) {
      this.toast.showError('Enter an observed parameter value first.');
      return;
    }

    const payload = {
      appointmentId: this.selectedAppId,
      testId: rep.testId,
      resultValue: rep.resultValue,
      remarks: rep.remarks || '',
      staffId: this.auth.currentUser()?.staffId
    };

    this.api.post('appointment/reports/upload', payload).subscribe({
      next: (res: any) => {
        this.toast.showSuccess(res.Message || 'Parameter value uploaded.');
        // Refresh details
        this.selectAppointment(this.selectedApp);
        this.loadBranchAppointments();
      }
    });
  }

  setTab(tab: string): void {
    this.activeSubTab = tab;
    this.isMobileMenuOpen = false;
    if (tab === 'analytics') {
      this.loadBranchAnalytics();
    } else {
      this.loadBranchAppointments();
    }
  }

  loadBranchAnalytics(): void {
    let params = `?branchId=${this.staffBranchId}`;
    if (this.filterYear) params += `&year=${this.filterYear}`;
    if (this.filterMonth) params += `&month=${this.filterMonth}`;
    if (this.filterStartDate) params += `&startDate=${this.filterStartDate}`;
    if (this.filterEndDate) params += `&endDate=${this.filterEndDate}`;

    this.api.get<any>('dashboard' + params).subscribe({
      next: (data) => {
        this.statsData = data;
        setTimeout(() => this.renderBranchCharts(), 100);
      }
    });
  }

  clearBranchFilters(): void {
    this.filterYear = '';
    this.filterMonth = '';
    this.filterStartDate = '';
    this.filterEndDate = '';
    this.loadBranchAnalytics();
  }

  renderBranchCharts(): void {
    if (this.branchRevenueChartInstance) this.branchRevenueChartInstance.destroy();
    if (this.branchPatientsChartInstance) this.branchPatientsChartInstance.destroy();

    // 1. Monthly Collection Chart
    if (this.isBranchManager()) {
      const revCtx = document.getElementById('branchRevenueChart') as HTMLCanvasElement;
      if (revCtx) {
        const labels = this.statsData.monthlyRevenue?.map((r: any) => r.monthName) || [];
        const data = this.statsData.monthlyRevenue?.map((r: any) => r.revenue) || [];

        this.branchRevenueChartInstance = new Chart(revCtx, {
          type: 'line',
          data: {
            labels: labels.length > 0 ? labels : ['Jan', 'Feb', 'Mar'],
            datasets: [{
              label: 'Monthly Collection (INR)',
              data: data.length > 0 ? data : [4000, 9000, 6000],
              borderColor: '#3b82f6',
              backgroundColor: 'rgba(59, 130, 246, 0.1)',
              tension: 0.3,
              fill: true
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false
          }
        });
      }
    }

    // 2. Monthly Patients Chart
    const patCtx = document.getElementById('branchPatientsChart') as HTMLCanvasElement;
    if (patCtx) {
      const labels = this.statsData.monthlyPatients?.map((p: any) => p.monthName) || [];
      const data = this.statsData.monthlyPatients?.map((p: any) => p.patientsCount) || [];

      this.branchPatientsChartInstance = new Chart(patCtx, {
        type: 'bar',
        data: {
          labels: labels.length > 0 ? labels : ['Jan', 'Feb', 'Mar'],
          datasets: [{
            label: 'Monthly Patients',
            data: data.length > 0 ? data : [8, 15, 12],
            backgroundColor: '#10b981'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false
        }
      });
    }
  }
}
