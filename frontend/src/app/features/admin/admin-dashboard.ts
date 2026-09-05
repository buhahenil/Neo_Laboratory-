import { Component, OnInit, AfterViewInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { Chart } from 'chart.js/auto';

@Component({
  selector: 'app-admin-dashboard',
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
            Neo Leboratory
          </a>
          <button class="btn text-white d-md-none p-0" (click)="toggleSidebar()">
            <span class="material-icons">close</span>
          </button>
        </div>
        
        <div class="p-3 border-bottom border-secondary small d-flex align-items-center gap-2 text-white-50">
          <span class="material-icons text-primary fs-3">admin_panel_settings</span>
          <div>
            <span class="d-block fw-semibold text-white">System Admin</span>
            <span class="small">Control Center</span>
          </div>
        </div>

        <ul class="nav flex-column p-2 gap-1 flex-grow-1">
          <li>
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'dashboard'" (click)="setTab('dashboard')">
              <span class="material-icons">dashboard</span> Analytics
            </button>
          </li>
          <li>
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'users'" (click)="setTab('users')">
              <span class="material-icons">people</span> Doctor & Staff CRUD
            </button>
          </li>
          <li>
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'catalog'" (click)="setTab('catalog')">
              <span class="material-icons">medical_services</span> Test Catalogs
            </button>
          </li>
          <li>
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'branches'" (click)="setTab('branches')">
              <span class="material-icons">corporate_fare</span> Branches & Slots
            </button>
          </li>
          <li>
            <button class="nav-link w-100 text-start d-flex align-items-center gap-2" [class.active]="activeTab === 'audit'" (click)="setTab('audit')">
              <span class="material-icons">policy</span> Audit Logs
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
            <h4 class="fw-bold mb-0 fs-5 text-truncate">Admin Management Panel</h4>
          </div>
          
          <div class="d-flex align-items-center gap-2 gap-md-3">
            <!-- Theme Toggle -->
            <button class="btn btn-sm btn-outline-secondary d-flex align-items-center p-2 rounded-circle" (click)="toggleTheme()">
              <span class="material-icons">{{ isDarkMode() ? 'light_mode' : 'dark_mode' }}</span>
            </button>
            
            <button class="btn btn-outline-primary d-flex align-items-center gap-1" (click)="backupDatabase()">
              <span class="material-icons">backup</span>
              <span class="d-none d-sm-inline">Backup DB</span>
            </button>
          </div>
        </header>

        <!-- Body -->
        <main class="dashboard-main-content">
          
          <!-- 1. DASHBOARD ANALYTICS TAB -->
          <div *ngIf="activeTab === 'dashboard'">
            <!-- Filters Bar -->
            <div class="glass-card mb-4">
              <div class="row g-3 align-items-end">
                <div class="col-sm-6 col-md-3">
                  <label class="form-label small fw-bold text-muted mb-1">Branch</label>
                  <select [(ngModel)]="filterBranchId" (change)="loadAdminData()" class="form-select form-select-sm">
                    <option value="">All Branches</option>
                    <option *ngFor="let br of branches" [value]="br.branchId">{{ br.name }}</option>
                  </select>
                </div>
                <div class="col-sm-6 col-md-2">
                  <label class="form-label small fw-bold text-muted mb-1">Year</label>
                  <select [(ngModel)]="filterYear" (change)="loadAdminData()" class="form-select form-select-sm">
                    <option value="">All Years</option>
                    <option *ngFor="let y of years" [value]="y">{{ y }}</option>
                  </select>
                </div>
                <div class="col-sm-6 col-md-2">
                  <label class="form-label small fw-bold text-muted mb-1">Month</label>
                  <select [(ngModel)]="filterMonth" (change)="loadAdminData()" class="form-select form-select-sm">
                    <option value="">All Months</option>
                    <option *ngFor="let m of months" [value]="m.value">{{ m.name }}</option>
                  </select>
                </div>
                <div class="col-sm-6 col-md-2">
                  <label class="form-label small fw-bold text-muted mb-1">Start Date</label>
                  <input type="date" [(ngModel)]="filterStartDate" (change)="loadAdminData()" class="form-control form-control-sm" />
                </div>
                <div class="col-sm-6 col-md-2">
                  <label class="form-label small fw-bold text-muted mb-1">End Date</label>
                  <input type="date" [(ngModel)]="filterEndDate" (change)="loadAdminData()" class="form-control form-control-sm" />
                </div>
                <div class="col-sm-12 col-md-1">
                  <button (click)="clearFilters()" class="btn btn-sm btn-outline-danger w-100 d-flex align-items-center justify-content-center gap-1 py-2">
                    <span class="material-icons fs-6">clear</span> Clear
                  </button>
                </div>
              </div>
            </div>

            <!-- Metrics Cards -->
            <div class="row g-4 mb-4">
              <!-- Total Patients -->
              <div class="col-md-4 col-lg-2 col-6">
                <div class="glass-card hoverable d-flex align-items-center gap-2 p-3">
                  <div class="p-2 bg-primary-subtle text-primary rounded-circle"><span class="material-icons fs-3">people</span></div>
                  <div>
                    <h5 class="fw-bold mb-0">{{ statsData.totalPatients }}</h5>
                    <p class="text-secondary xxs-text mb-0 text-truncate">Total Patients</p>
                  </div>
                </div>
              </div>

              <!-- Total Collection -->
              <div class="col-md-4 col-lg-2 col-6">
                <div class="glass-card hoverable d-flex align-items-center gap-2 p-3">
                  <div class="p-2 bg-success-subtle text-success rounded-circle"><span class="material-icons fs-3">payments</span></div>
                  <div>
                    <h5 class="fw-bold mb-0">INR {{ statsData.totalRevenue | number:'1.0-0' }}</h5>
                    <p class="text-secondary xxs-text mb-0 text-truncate">Total Collection</p>
                  </div>
                </div>
              </div>

              <!-- Total Appointments -->
              <div class="col-md-4 col-lg-2 col-6">
                <div class="glass-card hoverable d-flex align-items-center gap-2 p-3">
                  <div class="p-2 bg-warning-subtle text-warning rounded-circle"><span class="material-icons fs-3">event_note</span></div>
                  <div>
                    <h5 class="fw-bold mb-0">{{ statsData.totalAppointments }}</h5>
                    <p class="text-secondary xxs-text mb-0 text-truncate">Appointments</p>
                  </div>
                </div>
              </div>

              <!-- New Patients -->
              <div class="col-md-4 col-lg-2 col-6">
                <div class="glass-card hoverable d-flex align-items-center gap-2 p-3">
                  <div class="p-2 bg-info-subtle text-info rounded-circle"><span class="material-icons fs-3">person_add</span></div>
                  <div>
                    <h5 class="fw-bold mb-0">{{ statsData.newPatients }}</h5>
                    <p class="text-secondary xxs-text mb-0 text-truncate">New Patients</p>
                  </div>
                </div>
              </div>

              <!-- Returning Patients -->
              <div class="col-md-4 col-lg-2 col-6">
                <div class="glass-card hoverable d-flex align-items-center gap-2 p-3">
                  <div class="p-2 bg-danger-subtle text-danger rounded-circle"><span class="material-icons fs-3">autorenew</span></div>
                  <div>
                    <h5 class="fw-bold mb-0">{{ statsData.returningPatients }}</h5>
                    <p class="text-secondary xxs-text mb-0 text-truncate">Returning</p>
                  </div>
                </div>
              </div>

              <!-- Avg Revenue per Patient -->
              <div class="col-md-4 col-lg-2 col-12">
                <div class="glass-card hoverable d-flex align-items-center gap-2 p-3">
                  <div class="p-2 bg-secondary-subtle text-secondary rounded-circle"><span class="material-icons fs-3">trending_up</span></div>
                  <div>
                    <h5 class="fw-bold mb-0">INR {{ statsData.averageRevenuePerPatient | number:'1.0-0' }}</h5>
                    <p class="text-secondary xxs-text mb-0 text-truncate">Avg Rev/Patient</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Charts Row 1: Collection Trend & Patient Growth -->
            <div class="row g-4 mb-4">
              <div class="col-lg-6">
                <div class="glass-card">
                  <h6 class="fw-bold text-primary mb-3">Collection Trend</h6>
                  <div style="position: relative; height:240px;">
                    <canvas id="revenueChart"></canvas>
                  </div>
                </div>
              </div>

              <div class="col-lg-6">
                <div class="glass-card">
                  <h6 class="fw-bold text-primary mb-3">Monthly Patient Growth</h6>
                  <div style="position: relative; height:240px;">
                    <canvas id="patientGrowthChart"></canvas>
                  </div>
                </div>
              </div>
            </div>

            <!-- Charts Row 2: Branch Comparisons -->
            <div class="row g-4 mb-4">
              <div class="col-lg-6">
                <div class="glass-card">
                  <h6 class="fw-bold text-primary mb-3">Branch-wise Collection Comparison</h6>
                  <div style="position: relative; height:240px;">
                    <canvas id="branchCollectionChart"></canvas>
                  </div>
                </div>
              </div>

              <div class="col-lg-6">
                <div class="glass-card">
                  <h6 class="fw-bold text-primary mb-3">Branch-wise Patient Distribution</h6>
                  <div style="position: relative; height:240px;">
                    <canvas id="branchPatientChart"></canvas>
                  </div>
                </div>
              </div>
            </div>

            <!-- Charts Row 3: Popular Tests & Recent Activities -->
            <div class="row g-4 mb-4">
              <div class="col-lg-4">
                <div class="glass-card">
                  <h6 class="fw-bold text-primary mb-3">Popular Diagnostic Tests</h6>
                  <div style="position: relative; height:320px;">
                    <canvas id="popularChart"></canvas>
                  </div>
                </div>
              </div>

              <div class="col-lg-8">
                <!-- Recent Activity List -->
                <div class="glass-card h-100">
                  <h6 class="fw-bold text-primary mb-3">Recent Booking Activities</h6>
                  <div class="table-responsive" style="max-height: 310px; overflow-y: auto;">
                    <table class="table align-middle small text-secondary">
                      <thead>
                        <tr class="bg-light">
                          <th>BOOKING ID</th>
                          <th>PATIENT NAME</th>
                          <th>APPOINTMENT DATE</th>
                          <th>STATUS</th>
                          <th>TOTAL CHARGE</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr *ngFor="let act of statsData.recentActivity">
                          <td><strong>APP-{{ act.appointmentId }}</strong></td>
                          <td>{{ act.patientName }}</td>
                          <td>{{ act.appointmentDate | date:'mediumDate' }}</td>
                          <td>
                            <span class="badge bg-primary-subtle text-primary">{{ act.status }}</span>
                          </td>
                          <td>INR {{ act.totalAmount }}</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 2. USERS CRUD TAB -->
          <div *ngIf="activeTab === 'users'" class="glass-card">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="fw-bold text-primary mb-0">Doctor & Laboratory Staff Registers</h5>
              <div class="d-flex gap-2">
                <button (click)="openDoctorModal()" class="btn btn-primary-custom d-flex align-items-center gap-1">
                  <span class="material-icons fs-5">person_add</span> Doctor
                </button>
                <button (click)="openStaffModal()" class="btn btn-secondary-custom d-flex align-items-center gap-1">
                  <span class="material-icons fs-5">person_add</span> Staff
                </button>
              </div>
            </div>

            <!-- Tabs inside Users -->
            <ul class="nav nav-pills mb-3 gap-2">
              <li class="nav-item">
                <button class="btn btn-sm btn-outline-secondary" [class.active]="userSubTab === 'doctors'" (click)="userSubTab = 'doctors'">Doctors Directory</button>
              </li>
              <li class="nav-item">
                <button class="btn btn-sm btn-outline-secondary" [class.active]="userSubTab === 'staff'" (click)="userSubTab = 'staff'">Laboratory Staff</button>
              </li>
            </ul>

            <!-- Doctors table -->
            <div *ngIf="userSubTab === 'doctors'">
              <!-- DT Controls -->
              <div class="datatable-header">
                <div class="datatable-length">
                  <span>Show</span>
                  <select [(ngModel)]="pageSizeDoctors" (change)="pageIndexDoctors = 0" class="form-select form-select-sm">
                    <option [value]="5">5</option>
                    <option [value]="10">10</option>
                    <option [value]="25">25</option>
                    <option [value]="50">50</option>
                  </select>
                  <span>entries</span>
                </div>
                <div class="datatable-search">
                  <div class="input-group">
                    <span class="text-muted d-flex align-items-center"><span class="material-icons fs-5">search</span></span>
                    <input type="text" [(ngModel)]="searchQueryDoctors" (ngModelChange)="pageIndexDoctors = 0" class="form-control" placeholder="Search doctors..." />
                  </div>
                </div>
              </div>

              <div class="table-responsive">
                <table class="table align-middle small text-secondary">
                  <thead>
                    <tr class="bg-light">
                      <th (click)="setSortDoctors('doctorId')" class="cursor-pointer select-none">DR_ID <i class="bi" [ngClass]="getSortIconDoctors('doctorId')"></i></th>
                      <th (click)="setSortDoctors('name')" class="cursor-pointer select-none">NAME <i class="bi" [ngClass]="getSortIconDoctors('name')"></i></th>
                      <th (click)="setSortDoctors('email')" class="cursor-pointer select-none">EMAIL <i class="bi" [ngClass]="getSortIconDoctors('email')"></i></th>
                      <th (click)="setSortDoctors('phoneNumber')" class="cursor-pointer select-none">PHONE <i class="bi" [ngClass]="getSortIconDoctors('phoneNumber')"></i></th>
                      <th (click)="setSortDoctors('specialization')" class="cursor-pointer select-none">SPECIALIZATION <i class="bi" [ngClass]="getSortIconDoctors('specialization')"></i></th>
                      <th (click)="setSortDoctors('designation')" class="cursor-pointer select-none">DESIGNATION <i class="bi" [ngClass]="getSortIconDoctors('designation')"></i></th>
                      <th (click)="setSortDoctors('commissionRate')" class="cursor-pointer select-none">COMM. RATE (%) <i class="bi" [ngClass]="getSortIconDoctors('commissionRate')"></i></th>
                      <th>ACTIONS</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let doc of paginatedDoctors">
                      <td><strong>DOC-{{ doc.doctorId }}</strong></td>
                      <td>Dr. {{ doc.firstName }} {{ doc.lastName }}</td>
                      <td>{{ doc.email }}</td>
                      <td>{{ doc.phoneNumber }}</td>
                      <td>{{ doc.specialization }}</td>
                      <td>{{ doc.designation }}</td>
                      <td>{{ doc.commissionRate }}%</td>
                      <td>
                        <div class="d-flex gap-2 align-items-center">
                          <button (click)="openDoctorModal(doc)" class="btn btn-sm btn-outline-primary d-flex p-1 rounded-circle" title="Edit Doctor">
                            <span class="material-icons fs-6">edit</span>
                          </button>
                          <button (click)="deleteDoctor(doc.doctorId)" class="btn btn-sm btn-outline-danger d-flex p-1 rounded-circle" title="Delete Doctor">
                            <span class="material-icons fs-6">delete</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                    <tr *ngIf="filteredDoctors.length === 0">
                      <td colspan="8" class="text-center py-4 text-muted">No matching records found</td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- DT Info and Paginate -->
              <div class="datatable-footer" *ngIf="filteredDoctors.length > 0">
                <span>
                  Showing {{ pageIndexDoctors * pageSizeDoctors + 1 }} to {{ getMin((pageIndexDoctors + 1) * pageSizeDoctors, filteredDoctors.length) }} of {{ filteredDoctors.length }} entries
                </span>
                <nav>
                  <ul class="pagination mb-0">
                    <li class="page-item" [class.disabled]="pageIndexDoctors === 0">
                      <button class="page-link" (click)="pageIndexDoctors = pageIndexDoctors - 1">Previous</button>
                    </li>
                    <li class="page-item" *ngFor="let p of getArrayFromNumber(totalPagesDoctors); let idx = index" [class.active]="pageIndexDoctors === idx">
                      <button class="page-link" (click)="pageIndexDoctors = idx">{{ idx + 1 }}</button>
                    </li>
                    <li class="page-item" [class.disabled]="pageIndexDoctors >= totalPagesDoctors - 1">
                      <button class="page-link" (click)="pageIndexDoctors = pageIndexDoctors + 1">Next</button>
                    </li>
                  </ul>
                </nav>
              </div>
            </div>

            <!-- Staff table -->
            <div *ngIf="userSubTab === 'staff'">
              <!-- DT Controls -->
              <div class="datatable-header">
                <div class="datatable-length">
                  <span>Show</span>
                  <select [(ngModel)]="pageSizeStaff" (change)="pageIndexStaff = 0" class="form-select form-select-sm">
                    <option [value]="5">5</option>
                    <option [value]="10">10</option>
                    <option [value]="25">25</option>
                    <option [value]="50">50</option>
                  </select>
                  <span>entries</span>
                </div>
                <div class="datatable-search">
                  <div class="input-group">
                    <span class="text-muted d-flex align-items-center"><span class="material-icons fs-5">search</span></span>
                    <input type="text" [(ngModel)]="searchQueryStaff" (ngModelChange)="pageIndexStaff = 0" class="form-control" placeholder="Search staff..." />
                  </div>
                </div>
              </div>

              <div class="table-responsive">
                <table class="table align-middle small text-secondary">
                  <thead>
                    <tr class="bg-light">
                      <th (click)="setSortStaff('staffId')" class="cursor-pointer select-none">STAFF_ID <i class="bi" [ngClass]="getSortIconStaff('staffId')"></i></th>
                      <th (click)="setSortStaff('name')" class="cursor-pointer select-none">NAME <i class="bi" [ngClass]="getSortIconStaff('name')"></i></th>
                      <th (click)="setSortStaff('email')" class="cursor-pointer select-none">EMAIL <i class="bi" [ngClass]="getSortIconStaff('email')"></i></th>
                      <th (click)="setSortStaff('phoneNumber')" class="cursor-pointer select-none">PHONE <i class="bi" [ngClass]="getSortIconStaff('phoneNumber')"></i></th>
                      <th (click)="setSortStaff('branchName')" class="cursor-pointer select-none">BRANCH <i class="bi" [ngClass]="getSortIconStaff('branchName')"></i></th>
                      <th (click)="setSortStaff('designation')" class="cursor-pointer select-none">DESIGNATION <i class="bi" [ngClass]="getSortIconStaff('designation')"></i></th>
                      <th>ACTIONS</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let stf of paginatedStaff">
                      <td><strong>STF-{{ stf.staffId }}</strong></td>
                      <td>{{ stf.firstName }} {{ stf.lastName }}</td>
                      <td>{{ stf.email }}</td>
                      <td>{{ stf.phoneNumber }}</td>
                      <td>{{ stf.branchName }}</td>
                      <td>{{ stf.designation }}</td>
                      <td>
                        <div class="d-flex gap-2 align-items-center">
                          <button (click)="openStaffModal(stf)" class="btn btn-sm btn-outline-primary d-flex p-1 rounded-circle" title="Edit Staff">
                            <span class="material-icons fs-6">edit</span>
                          </button>
                          <button (click)="deleteStaff(stf.staffId)" class="btn btn-sm btn-outline-danger d-flex p-1 rounded-circle" title="Delete Staff">
                            <span class="material-icons fs-6">delete</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                    <tr *ngIf="filteredStaff.length === 0">
                      <td colspan="7" class="text-center py-4 text-muted">No matching records found</td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- DT Info and Paginate -->
              <div class="datatable-footer" *ngIf="filteredStaff.length > 0">
                <span>
                  Showing {{ pageIndexStaff * pageSizeStaff + 1 }} to {{ getMin((pageIndexStaff + 1) * pageSizeStaff, filteredStaff.length) }} of {{ filteredStaff.length }} entries
                </span>
                <nav>
                  <ul class="pagination mb-0">
                    <li class="page-item" [class.disabled]="pageIndexStaff === 0">
                      <button class="page-link" (click)="pageIndexStaff = pageIndexStaff - 1">Previous</button>
                    </li>
                    <li class="page-item" *ngFor="let p of getArrayFromNumber(totalPagesStaff); let idx = index" [class.active]="pageIndexStaff === idx">
                      <button class="page-link" (click)="pageIndexStaff = idx">{{ idx + 1 }}</button>
                    </li>
                    <li class="page-item" [class.disabled]="pageIndexStaff >= totalPagesStaff - 1">
                      <button class="page-link" (click)="pageIndexStaff = pageIndexStaff + 1">Next</button>
                    </li>
                  </ul>
                </nav>
              </div>
            </div>
          </div>

          <!-- 3. TEST CATALOGS TAB -->
          <div *ngIf="activeTab === 'catalog'" class="glass-card">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="fw-bold text-primary mb-0">Diagnostic Catalog Management</h5>
              <button (click)="openTestModal()" class="btn btn-primary-custom d-flex align-items-center gap-1">
                <span class="material-icons">add</span> Add New Test
              </button>
            </div>

            <!-- Tests Table -->
            <!-- DT Controls -->
            <div class="datatable-header">
              <div class="datatable-length">
                <span>Show</span>
                <select [(ngModel)]="pageSizeTests" (change)="pageIndexTests = 0" class="form-select form-select-sm">
                  <option [value]="5">5</option>
                  <option [value]="10">10</option>
                  <option [value]="25">25</option>
                  <option [value]="50">50</option>
                </select>
                <span>entries</span>
              </div>
              <div class="datatable-search">
                <div class="input-group">
                  <span class="text-muted d-flex align-items-center"><span class="material-icons fs-5">search</span></span>
                  <input type="text" [(ngModel)]="searchQueryTests" (ngModelChange)="pageIndexTests = 0" class="form-control" placeholder="Search tests..." />
                </div>
              </div>
            </div>

            <div class="table-responsive">
              <table class="table align-middle small text-secondary">
                <thead>
                  <tr class="bg-light">
                    <th (click)="setSortTests('code')" class="cursor-pointer select-none">CODE <i class="bi" [ngClass]="getSortIconTests('code')"></i></th>
                    <th (click)="setSortTests('name')" class="cursor-pointer select-none">TEST NAME <i class="bi" [ngClass]="getSortIconTests('name')"></i></th>
                    <th (click)="setSortTests('categoryName')" class="cursor-pointer select-none">CATEGORY <i class="bi" [ngClass]="getSortIconTests('categoryName')"></i></th>
                    <th (click)="setSortTests('sampleType')" class="cursor-pointer select-none">SAMPLE <i class="bi" [ngClass]="getSortIconTests('sampleType')"></i></th>
                    <th (click)="setSortTests('price')" class="cursor-pointer select-none">PRICE (INR) <i class="bi" [ngClass]="getSortIconTests('price')"></i></th>
                    <th (click)="setSortTests('normalRange')" class="cursor-pointer select-none">NORMAL RANGE <i class="bi" [ngClass]="getSortIconTests('normalRange')"></i></th>
                    <th (click)="setSortTests('deliveryTimeHours')" class="cursor-pointer select-none">TAT <i class="bi" [ngClass]="getSortIconTests('deliveryTimeHours')"></i></th>
                    <th (click)="setSortTests('isActive')" class="cursor-pointer select-none">STATUS <i class="bi" [ngClass]="getSortIconTests('isActive')"></i></th>
                    <th>ACTIONS</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let test of paginatedTests">
                    <td><strong>{{ test.code }}</strong></td>
                    <td>{{ test.name }}</td>
                    <td>{{ test.categoryName }}</td>
                    <td>{{ test.sampleType }}</td>
                    <td>INR {{ test.price }}</td>
                    <td>{{ test.normalRange || 'N/A' }}</td>
                    <td>{{ test.deliveryTimeHours }} Hrs</td>
                    <td>
                      <span class="badge" [ngClass]="test.isActive ? 'bg-success-subtle text-success' : 'bg-danger-subtle text-danger'">{{ test.isActive ? 'Active' : 'Disabled' }}</span>
                    </td>
                    <td>
                      <div class="d-flex gap-2">
                        <button (click)="openTestModal(test)" class="btn btn-sm btn-outline-primary d-flex p-1 rounded-circle" title="Edit Test">
                          <span class="material-icons fs-6">edit</span>
                        </button>
                        <button (click)="deleteTest(test.testId)" class="btn btn-sm btn-outline-danger d-flex p-1 rounded-circle" title="Delete Test">
                          <span class="material-icons fs-6">delete</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                  <tr *ngIf="filteredTests.length === 0">
                    <td colspan="9" class="text-center py-4 text-muted">No matching records found</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- DT Info and Paginate -->
            <div class="datatable-footer" *ngIf="filteredTests.length > 0">
              <span>
                Showing {{ pageIndexTests * pageSizeTests + 1 }} to {{ getMin((pageIndexTests + 1) * pageSizeTests, filteredTests.length) }} of {{ filteredTests.length }} entries
              </span>
              <nav>
                <ul class="pagination mb-0">
                  <li class="page-item" [class.disabled]="pageIndexTests === 0">
                    <button class="page-link" (click)="pageIndexTests = pageIndexTests - 1">Previous</button>
                  </li>
                  <li class="page-item" *ngFor="let p of getArrayFromNumber(totalPagesTests); let idx = index" [class.active]="pageIndexTests === idx">
                    <button class="page-link" (click)="pageIndexTests = idx">{{ idx + 1 }}</button>
                  </li>
                  <li class="page-item" [class.disabled]="pageIndexTests >= totalPagesTests - 1">
                    <button class="page-link" (click)="pageIndexTests = pageIndexTests + 1">Next</button>
                  </li>
                </ul>
              </nav>
            </div>
          </div>

          <!-- 4. BRANCHES & SLOTS TAB -->
          <div *ngIf="activeTab === 'branches'" class="glass-card">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="fw-bold text-primary mb-0">Clinic Locations & Time Slot Settings</h5>
              <button (click)="openBranchModal()" class="btn btn-primary-custom d-flex align-items-center gap-1">
                <span class="material-icons fs-5">add_location</span> Add Branch
              </button>
            </div>
            
            <div class="row g-4">
              <!-- Branches list -->
              <div class="col-md-6">
                <div class="p-3 bg-light rounded border mb-3 cursor-pointer" *ngFor="let br of branches" 
                     [class.border-primary]="selectedBranchId === br.branchId" 
                     (click)="selectBranch(br.branchId)">
                  <div class="d-flex justify-content-between align-items-center">
                    <div>
                      <h6 class="fw-bold mb-1">{{ br.name }}</h6>
                      <span class="text-secondary small">{{ br.address }}, {{ br.city }}</span>
                    </div>
                    <div class="d-flex align-items-center gap-2" (click)="$event.stopPropagation()">
                      <button (click)="openEditBranchModal(br)" class="btn btn-sm btn-outline-primary p-1 rounded-circle d-flex" title="Edit Branch">
                        <span class="material-icons fs-6">edit</span>
                      </button>
                      <button (click)="deleteBranch(br.branchId)" class="btn btn-sm btn-outline-danger p-1 rounded-circle d-flex" title="Delete Branch">
                        <span class="material-icons fs-6">delete</span>
                      </button>
                      <span class="material-icons text-primary ms-2" *ngIf="selectedBranchId === br.branchId">arrow_forward</span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Slots details -->
              <div class="col-md-6" *ngIf="selectedBranchId > 0">
                <div class="glass-card">
                  <h6 class="fw-bold text-primary mb-3">Time Slots Config</h6>
                  <div class="gap-2 d-grid mb-3">
                    <div class="p-2 border rounded d-flex justify-content-between align-items-center bg-white" *ngFor="let slot of slots">
                      <div>
                        <strong class="d-block text-dark">{{ slot.startTime?.substring(0,5) }} - {{ slot.endTime?.substring(0,5) }}</strong>
                        <span class="text-muted small">Max Bookings: {{ slot.maxBookings }}</span>
                      </div>
                      <div class="d-flex gap-2">
                        <button (click)="editTimeSlot(slot)" class="btn btn-sm btn-outline-primary d-flex p-1 rounded-circle" title="Edit Slot"><span class="material-icons fs-6">edit</span></button>
                        <button (click)="deleteTimeSlot(slot.slotId)" class="btn btn-sm btn-outline-danger d-flex p-1 rounded-circle" title="Delete Slot"><span class="material-icons fs-6">delete</span></button>
                      </div>
                    </div>
                  </div>
                  
                  <div class="bg-white p-3 rounded border mt-4">
                    <h6 class="fw-bold mb-3 border-bottom pb-2">{{ newSlotData.slotId ? 'Edit Time Slot' : 'Add New Time Slot' }}</h6>
                    <form>
                      <div class="row g-3">
                        <div class="col-sm-4">
                          <label class="small text-muted mb-1 fw-bold">Start Time</label>
                          <input type="time" [(ngModel)]="newSlotData.start" name="start" class="form-control" />
                        </div>
                        <div class="col-sm-4">
                          <label class="small text-muted mb-1 fw-bold">End Time</label>
                          <input type="time" [(ngModel)]="newSlotData.end" name="end" class="form-control" />
                        </div>
                        <div class="col-sm-4">
                          <label class="small text-muted mb-1 fw-bold">Capacity</label>
                          <input type="number" [(ngModel)]="newSlotData.maxBookings" name="maxBookings" class="form-control" />
                        </div>
                        <div class="col-12 d-flex gap-2 justify-content-end mt-2">
                          <button type="button" *ngIf="newSlotData.slotId" (click)="cancelSlotEdit()" class="btn btn-outline-secondary px-4">Cancel</button>
                          <button type="button" (click)="addTimeSlot()" class="btn btn-primary-custom px-4">
                            {{ newSlotData.slotId ? 'Update Slot' : 'Add Slot' }}
                          </button>
                        </div>
                      </div>
                    </form>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 5. AUDIT LOGS TAB -->
          <div *ngIf="activeTab === 'audit'" class="glass-card">
            <h5 class="fw-bold text-primary mb-4">Security Audit Trial Records</h5>
            <!-- DT Controls -->
            <div class="datatable-header">
              <div class="datatable-length">
                <span>Show</span>
                <select [(ngModel)]="pageSizeAudit" (change)="pageIndexAudit = 0" class="form-select form-select-sm">
                  <option [value]="5">5</option>
                  <option [value]="10">10</option>
                  <option [value]="25">25</option>
                  <option [value]="50">50</option>
                </select>
                <span>entries</span>
              </div>
              <div class="datatable-search">
                <div class="input-group">
                  <span class="text-muted d-flex align-items-center"><span class="material-icons fs-5">search</span></span>
                  <input type="text" [(ngModel)]="searchQueryAudit" (ngModelChange)="pageIndexAudit = 0" class="form-control" placeholder="Search logs..." />
                </div>
              </div>
            </div>

            <div class="table-responsive">
              <table class="table align-middle small text-secondary">
                <thead>
                  <tr class="bg-light">
                    <th>LOG ID</th>
                    <th>USER EMAIL</th>
                    <th>ACTION</th>
                    <th>TABLE</th>
                    <th>RECORD ID</th>
                    <th>CHANGES</th>
                    <th>TIMESTAMP</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let log of paginatedAuditLogs">
                    <td>{{ log.logId }}</td>
                    <td><strong>{{ log.userEmail || 'System/Guest' }}</strong></td>
                    <td><span class="badge bg-secondary-subtle">{{ log.action }}</span></td>
                    <td>{{ log.tableName }}</td>
                    <td>{{ log.recordId || '-' }}</td>
                    <td class="text-truncate" style="max-width: 250px;">{{ log.newValues }}</td>
                    <td>{{ log.timestamp | date:'medium' }}</td>
                  </tr>
                  <tr *ngIf="filteredAuditLogs.length === 0">
                    <td colspan="7" class="text-center py-4 text-muted">No matching records found</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- DT Info and Paginate -->
            <div class="datatable-footer" *ngIf="filteredAuditLogs.length > 0">
              <span>
                Showing {{ pageIndexAudit * pageSizeAudit + 1 }} to {{ getMin((pageIndexAudit + 1) * pageSizeAudit, filteredAuditLogs.length) }} of {{ filteredAuditLogs.length }} entries
              </span>
              <nav>
                <ul class="pagination mb-0">
                  <li class="page-item" [class.disabled]="pageIndexAudit === 0">
                    <button class="page-link" (click)="pageIndexAudit = pageIndexAudit - 1">Previous</button>
                  </li>
                  <li class="page-item" *ngFor="let p of getArrayFromNumber(totalPagesAudit); let idx = index" [class.active]="pageIndexAudit === idx">
                    <button class="page-link" (click)="pageIndexAudit = idx">{{ idx + 1 }}</button>
                  </li>
                  <li class="page-item" [class.disabled]="pageIndexAudit >= totalPagesAudit - 1">
                    <button class="page-link" (click)="pageIndexAudit = pageIndexAudit + 1">Next</button>
                  </li>
                </ul>
              </nav>
            </div>
          </div>

        </main>
      </div>
    </div>

    <!-- Modals Section -->
    <!-- 1. Doctor creation -->
    <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);" *ngIf="showDoctorModal">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content glass-card border-0 p-4 text-dark">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="fw-bold text-primary mb-0">{{ newDoc.doctorId ? 'Edit Doctor Profile' : 'Create Doctor Profile' }}</h5>
            <button type="button" class="btn-close shadow-none" (click)="showDoctorModal = false"></button>
          </div>
          <form (submit)="saveDoctor($event)">
            <div class="row g-3">
              <div class="col-md-6">
                <input type="text" [(ngModel)]="newDoc.firstName" name="fn" class="form-control" placeholder="First Name" required />
              </div>
              <div class="col-md-6">
                <input type="text" [(ngModel)]="newDoc.lastName" name="ln" class="form-control" placeholder="Last Name" required />
              </div>
              <div class="col-12">
                <input type="email" [(ngModel)]="newDoc.email" name="em" class="form-control" placeholder="Email Address" required [disabled]="newDoc.doctorId > 0" />
              </div>
              <div class="col-md-6">
                <input type="text" [(ngModel)]="newDoc.specialization" name="sp" class="form-control" placeholder="Specialization (e.g. Pathology)" required />
              </div>
              <div class="col-md-6">
                <input type="text" [(ngModel)]="newDoc.designation" name="dg" class="form-control" placeholder="Designation" required />
              </div>
              <div class="col-md-6">
                <input type="number" [(ngModel)]="newDoc.commissionRate" name="cr" class="form-control" placeholder="Commission Rate %" required />
              </div>
              <div class="col-md-6">
                <input type="tel" [(ngModel)]="newDoc.phoneNumber" name="ph" class="form-control" placeholder="Phone Number" />
              </div>
              <div class="col-12">
                <div class="input-group border rounded p-1 bg-light">
                  <span class="input-group-text border-0 bg-transparent text-muted"><span class="material-icons fs-5">lock</span></span>
                  <input [type]="showDocPassword ? 'text' : 'password'" [(ngModel)]="newDoc.password" name="pw" class="form-control border-0 shadow-none bg-transparent" placeholder="Password (leave empty to keep default)" />
                  <button type="button" class="btn border-0 bg-transparent text-muted px-2 py-0" (click)="showDocPassword = !showDocPassword">
                    <span class="material-icons fs-5">{{ showDocPassword ? 'visibility_off' : 'visibility' }}</span>
                  </button>
                </div>
              </div>
              <button type="submit" class="btn btn-primary-custom w-100 mt-4">{{ newDoc.doctorId ? 'Update Doctor Profile' : 'Save Doctor Profile' }}</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- 2. Staff creation -->
    <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);" *ngIf="showStaffModal">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content glass-card border-0 p-4 text-dark">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="fw-bold text-primary mb-0">{{ newStf.staffId ? 'Edit Staff Profile' : 'Create Staff Profile' }}</h5>
            <button type="button" class="btn-close shadow-none" (click)="showStaffModal = false"></button>
          </div>
          <form (submit)="saveStaff($event)">
            <div class="row g-3">
              <div class="col-md-6">
                <input type="text" [(ngModel)]="newStf.firstName" name="fn" class="form-control" placeholder="First Name" required />
              </div>
              <div class="col-md-6">
                <input type="text" [(ngModel)]="newStf.lastName" name="ln" class="form-control" placeholder="Last Name" required />
              </div>
              <div class="col-12">
                <input type="email" [(ngModel)]="newStf.email" name="em" class="form-control" placeholder="Email Address" required [disabled]="newStf.staffId > 0" />
              </div>
              <div class="col-12 mt-1">
                <label class="form-label small fw-bold text-muted mb-2">Assigned Branches (Access Control)</label>
                <div class="d-flex flex-wrap gap-3 border p-2 rounded bg-white">
                  <div class="form-check" *ngFor="let br of branches">
                    <input class="form-check-input" type="checkbox" [id]="'brCheck_' + br.branchId" 
                           [checked]="isStaffBranchSelected(br.branchId)" 
                           (change)="toggleStaffBranchSelection(br.branchId)">
                    <label class="form-check-label small" [for]="'brCheck_' + br.branchId">
                      {{ br.name }}
                    </label>
                  </div>
                </div>
              </div>
              <div class="col-md-6">
                <input type="text" [(ngModel)]="newStf.designation" name="dg" class="form-control" placeholder="Designation (e.g. Technician)" required />
              </div>
              <div class="col-12">
                <input type="tel" [(ngModel)]="newStf.phoneNumber" name="ph" class="form-control" placeholder="Phone Number" />
              </div>
              <div class="col-12">
                <div class="input-group border rounded p-1 bg-light">
                  <span class="input-group-text border-0 bg-transparent text-muted"><span class="material-icons fs-5">lock</span></span>
                  <input [type]="showStfPassword ? 'text' : 'password'" [(ngModel)]="newStf.password" name="pw" class="form-control border-0 shadow-none bg-transparent" placeholder="Password (leave empty to keep default)" />
                  <button type="button" class="btn border-0 bg-transparent text-muted px-2 py-0" (click)="showStfPassword = !showStfPassword">
                    <span class="material-icons fs-5">{{ showStfPassword ? 'visibility_off' : 'visibility' }}</span>
                  </button>
                </div>
              </div>
              <button type="submit" class="btn btn-primary-custom w-100 mt-4">{{ newStf.staffId ? 'Update Staff Profile' : 'Save Staff Profile' }}</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- 3. Branch creation/edit -->
    <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);" *ngIf="showBranchModal">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content glass-card border-0 p-4 text-dark">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="fw-bold text-primary mb-0">{{ branchFormData.branchId ? 'Edit Branch' : 'Add New Branch' }}</h5>
            <button type="button" class="btn-close shadow-none" (click)="showBranchModal = false"></button>
          </div>
          <form>
            <div class="row g-3">
              <div class="col-12">
                <input type="text" [(ngModel)]="branchFormData.name" name="bName" class="form-control" placeholder="Branch Name (e.g. Main Center)" />
              </div>
              <div class="col-12">
                <input type="text" [(ngModel)]="branchFormData.address" name="bAddress" class="form-control" placeholder="Complete Address" />
              </div>
              <div class="col-md-6">
                <input type="text" [(ngModel)]="branchFormData.city" name="bCity" class="form-control" placeholder="City" />
              </div>
              <div class="col-md-6">
                <input type="tel" [(ngModel)]="branchFormData.contactNumber" name="bContact" class="form-control" placeholder="Contact Number" />
              </div>
              <div class="col-12">
                <input type="email" [(ngModel)]="branchFormData.email" name="bEmail" class="form-control" placeholder="Branch Email" />
              </div>
              <div class="col-12" *ngIf="branchFormData.branchId">
                <div class="form-check form-switch">
                  <input class="form-check-input" type="checkbox" role="switch" id="activeSwitch" [(ngModel)]="branchFormData.isActive" name="bActive">
                  <label class="form-check-label" for="activeSwitch">Is Branch Active?</label>
                </div>
              </div>
              <button type="button" (click)="saveBranch()" class="btn btn-primary-custom w-100 mt-4">{{ branchFormData.branchId ? 'Update Branch' : 'Save Branch' }}</button>
            </div>
          </form>
        </div>
      </div>
    </div>
    <!-- 4. Test creation/edit -->
    <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);" *ngIf="showTestModal">
      <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content glass-card border-0 p-4 text-dark">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="fw-bold text-primary mb-0">{{ testFormData.testId ? 'Edit Test' : 'Add New Test' }}</h5>
            <button type="button" class="btn-close shadow-none" (click)="showTestModal = false"></button>
          </div>
          <form>
            <div class="row g-3">
              <div class="col-md-6">
                <label class="form-label small fw-bold text-muted mb-1">Test Name</label>
                <input type="text" [(ngModel)]="testFormData.name" name="tName" class="form-control" placeholder="e.g. Complete Blood Count" />
              </div>
              <div class="col-md-3">
                <label class="form-label small fw-bold text-muted mb-1">Code</label>
                <input type="text" [(ngModel)]="testFormData.code" name="tCode" class="form-control" placeholder="e.g. CBC-01" />
              </div>
              <div class="col-md-3">
                <label class="form-label small fw-bold text-muted mb-1">Price (INR)</label>
                <input type="number" [(ngModel)]="testFormData.price" name="tPrice" class="form-control" placeholder="Price" />
              </div>
              <div class="col-md-6">
                <label class="form-label small fw-bold text-muted mb-1">Category</label>
                <select [(ngModel)]="testFormData.categoryId" name="tCat" class="form-select">
                  <option [value]="0">Select Category</option>
                  <option *ngFor="let cat of categories" [value]="cat.categoryId">{{ cat.name }}</option>
                </select>
              </div>
              <div class="col-md-6">
                <label class="form-label small fw-bold text-muted mb-1">Sample Type</label>
                <input type="text" [(ngModel)]="testFormData.sampleType" name="tSample" class="form-control" placeholder="e.g. Blood, Urine" />
              </div>
              <div class="col-md-6">
                <label class="form-label small fw-bold text-muted mb-1">Normal Range</label>
                <input type="text" [(ngModel)]="testFormData.normalRange" name="tRange" class="form-control" placeholder="e.g. 50-120 mg/dL" />
              </div>
              <div class="col-md-6">
                <label class="form-label small fw-bold text-muted mb-1">Turnaround Time (Hrs)</label>
                <input type="number" [(ngModel)]="testFormData.deliveryTimeHours" name="tTAT" class="form-control" placeholder="24" />
              </div>
              <div class="col-12">
                <label class="form-label small fw-bold text-muted mb-1">Preparation Instructions</label>
                <input type="text" [(ngModel)]="testFormData.preparation" name="tPrep" class="form-control" placeholder="e.g. Fasting for 12 hours" />
              </div>
              <div class="col-12" *ngIf="testFormData.testId">
                <div class="form-check form-switch">
                  <input class="form-check-input" type="checkbox" role="switch" id="activeTestSwitch" [(ngModel)]="testFormData.isActive" name="tActive">
                  <label class="form-check-label" for="activeTestSwitch">Is Test Active?</label>
                </div>
              </div>
              <button type="button" (click)="saveTest()" class="btn btn-primary-custom w-100 mt-4">{{ testFormData.testId ? 'Update Test' : 'Save Test' }}</button>
            </div>
          </form>
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
    .cursor-pointer {
      cursor: pointer;
    }
  `]
})
export class AdminDashboardComponent implements OnInit, AfterViewInit {
  auth = inject(AuthService);
  private api = inject(ApiService);
  private toast = inject(NotificationService);

  isMobileMenuOpen = false;
  isDarkMode = signal(false);
  activeTab = 'dashboard';
  userSubTab = 'doctors';

  // Analytics DTO
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
    branchCollections: [],
    branchPatients: [],
    recentActivity: []
  };

  // Advanced Filters
  filterBranchId = '';
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

  // Directories list
  doctors: any[] = [];
  staff: any[] = [];
  tests: any[] = [];
  categories: any[] = [];
  branches: any[] = [];
  slots: any[] = [];
  auditLogs: any[] = [];
  selectedBranchId = 0;

  // Time slots adding
  newSlotData: any = { slotId: 0, start: '09:00', end: '10:00', maxBookings: 5 };

  // Modals state
  showDoctorModal = false;
  showStaffModal = false;
  showBranchModal = false;
  showTestModal = false;

  newDoc: any = { doctorId: 0, userId: 0, firstName: '', lastName: '', email: '', specialization: 'Pathology', designation: 'Consultant', commissionRate: 15.00, phoneNumber: '', password: '' };
  newStf: any = { staffId: 0, userId: 0, firstName: '', lastName: '', email: '', branchId: 0, branchIds: [], designation: 'Lab Technician', phoneNumber: '', password: '' };
  showDocPassword = false;
  showStfPassword = false;
  branchFormData: any = { branchId: 0, name: '', address: '', city: '', contactNumber: '', email: '', isActive: true };
  testFormData: any = { testId: 0, categoryId: 0, name: '', code: '', price: 0, sampleType: '', preparation: '', normalRange: '', deliveryTimeHours: 24, isActive: true };

  revenueChartInstance: any = null;
  popularChartInstance: any = null;

  // DataTables state - Doctors
  searchQueryDoctors = '';
  sortFieldDoctors = 'doctorId';
  sortAscDoctors = true;
  pageIndexDoctors = 0;
  pageSizeDoctors = 5;

  // DataTables state - Staff
  searchQueryStaff = '';
  sortFieldStaff = 'staffId';
  sortAscStaff = true;
  pageIndexStaff = 0;
  pageSizeStaff = 5;

  // DataTables state - Tests
  searchQueryTests = '';
  sortFieldTests = 'code';
  sortAscTests = true;
  pageIndexTests = 0;
  pageSizeTests = 5;

  // DataTables state - Audit Logs
  searchQueryAudit = '';
  pageIndexAudit = 0;
  pageSizeAudit = 10;

  getArrayFromNumber(n: number): number[] {
    return Array(n).fill(0).map((_, i) => i);
  }

  getMin(a: number, b: number): number {
    return Math.min(a, b);
  }

  getSortIconDoctors(field: string): string {
    if (this.sortFieldDoctors !== field) return 'bi-arrow-down-up text-muted small ms-1';
    return this.sortAscDoctors ? 'bi-arrow-up-short text-primary ms-1' : 'bi-arrow-down-short text-primary ms-1';
  }

  getSortIconStaff(field: string): string {
    if (this.sortFieldStaff !== field) return 'bi-arrow-down-up text-muted small ms-1';
    return this.sortAscStaff ? 'bi-arrow-up-short text-primary ms-1' : 'bi-arrow-down-short text-primary ms-1';
  }

  setSortDoctors(field: string): void {
    if (this.sortFieldDoctors === field) {
      this.sortAscDoctors = !this.sortAscDoctors;
    } else {
      this.sortFieldDoctors = field;
      this.sortAscDoctors = true;
    }
    this.pageIndexDoctors = 0;
  }

  setSortStaff(field: string): void {
    if (this.sortFieldStaff === field) {
      this.sortAscStaff = !this.sortAscStaff;
    } else {
      this.sortFieldStaff = field;
      this.sortAscStaff = true;
    }
    this.pageIndexStaff = 0;
  }

  get filteredDoctors(): any[] {
    let list = [...this.doctors];
    if (this.searchQueryDoctors.trim()) {
      const q = this.searchQueryDoctors.toLowerCase();
      list = list.filter(d => 
        (d.firstName + ' ' + d.lastName).toLowerCase().includes(q) || 
        d.email.toLowerCase().includes(q) || 
        (d.phoneNumber && d.phoneNumber.includes(q)) || 
        d.specialization.toLowerCase().includes(q) || 
        d.designation.toLowerCase().includes(q)
      );
    }
    list.sort((a, b) => {
      let valA = a[this.sortFieldDoctors];
      let valB = b[this.sortFieldDoctors];
      if (this.sortFieldDoctors === 'name') {
        valA = `${a.firstName} ${a.lastName}`.toLowerCase();
        valB = `${b.firstName} ${b.lastName}`.toLowerCase();
      } else if (typeof valA === 'string') {
        valA = valA.toLowerCase();
        valB = valB.toLowerCase();
      }
      if (valA < valB) return this.sortAscDoctors ? -1 : 1;
      if (valA > valB) return this.sortAscDoctors ? 1 : -1;
      return 0;
    });
    return list;
  }

  get paginatedDoctors(): any[] {
    const list = this.filteredDoctors;
    const start = this.pageIndexDoctors * this.pageSizeDoctors;
    return list.slice(start, start + this.pageSizeDoctors);
  }

  get totalPagesDoctors(): number {
    return Math.ceil(this.filteredDoctors.length / this.pageSizeDoctors) || 1;
  }

  get filteredStaff(): any[] {
    let list = [...this.staff];
    if (this.searchQueryStaff.trim()) {
      const q = this.searchQueryStaff.toLowerCase();
      list = list.filter(s => 
        (s.firstName + ' ' + s.lastName).toLowerCase().includes(q) || 
        s.email.toLowerCase().includes(q) || 
        (s.phoneNumber && s.phoneNumber.includes(q)) || 
        s.branchName.toLowerCase().includes(q) || 
        s.designation.toLowerCase().includes(q)
      );
    }
    list.sort((a, b) => {
      let valA = a[this.sortFieldStaff];
      let valB = b[this.sortFieldStaff];
      if (this.sortFieldStaff === 'name') {
        valA = `${a.firstName} ${a.lastName}`.toLowerCase();
        valB = `${b.firstName} ${b.lastName}`.toLowerCase();
      } else if (typeof valA === 'string') {
        valA = valA.toLowerCase();
        valB = valB.toLowerCase();
      }
      if (valA < valB) return this.sortAscStaff ? -1 : 1;
      if (valA > valB) return this.sortAscStaff ? 1 : -1;
      return 0;
    });
    return list;
  }

  get paginatedStaff(): any[] {
    const list = this.filteredStaff;
    const start = this.pageIndexStaff * this.pageSizeStaff;
    return list.slice(start, start + this.pageSizeStaff);
  }

  get totalPagesStaff(): number {
    return Math.ceil(this.filteredStaff.length / this.pageSizeStaff) || 1;
  }

  getSortIconTests(field: string): string {
    if (this.sortFieldTests !== field) return 'bi-arrow-down-up text-muted small ms-1';
    return this.sortAscTests ? 'bi-arrow-up-short text-primary ms-1' : 'bi-arrow-down-short text-primary ms-1';
  }

  setSortTests(field: string): void {
    if (this.sortFieldTests === field) {
      this.sortAscTests = !this.sortAscTests;
    } else {
      this.sortFieldTests = field;
      this.sortAscTests = true;
    }
    this.pageIndexTests = 0;
  }

  get filteredTests(): any[] {
    let list = [...this.tests];
    if (this.searchQueryTests.trim()) {
      const q = this.searchQueryTests.toLowerCase();
      list = list.filter(t => 
        t.code.toLowerCase().includes(q) || 
        t.name.toLowerCase().includes(q) || 
        t.categoryName.toLowerCase().includes(q) || 
        t.sampleType.toLowerCase().includes(q) || 
        t.price.toString().includes(q) || 
        (t.normalRange && t.normalRange.toLowerCase().includes(q))
      );
    }
    list.sort((a, b) => {
      let valA = a[this.sortFieldTests];
      let valB = b[this.sortFieldTests];
      if (typeof valA === 'string') {
        valA = valA.toLowerCase();
        valB = valB.toLowerCase();
      }
      if (valA < valB) return this.sortAscTests ? -1 : 1;
      if (valA > valB) return this.sortAscTests ? 1 : -1;
      return 0;
    });
    return list;
  }

  get paginatedTests(): any[] {
    const list = this.filteredTests;
    const start = this.pageIndexTests * this.pageSizeTests;
    return list.slice(start, start + this.pageSizeTests);
  }

  get totalPagesTests(): number {
    return Math.ceil(this.filteredTests.length / this.pageSizeTests) || 1;
  }

  get filteredAuditLogs(): any[] {
    let list = [...this.auditLogs];
    if (this.searchQueryAudit.trim()) {
      const q = this.searchQueryAudit.toLowerCase();
      list = list.filter(l => 
        l.action.toLowerCase().includes(q) || 
        l.tableName.toLowerCase().includes(q) || 
        (l.newValues && l.newValues.toLowerCase().includes(q)) ||
        l.recordId.toString().includes(q)
      );
    }
    return list;
  }

  get paginatedAuditLogs(): any[] {
    const list = this.filteredAuditLogs;
    const start = this.pageIndexAudit * this.pageSizeAudit;
    return list.slice(start, start + this.pageSizeAudit);
  }

  get totalPagesAudit(): number {
    return Math.ceil(this.filteredAuditLogs.length / this.pageSizeAudit) || 1;
  }

  ngOnInit(): void {
    this.initTheme();
    this.loadAdminData();
  }

  ngAfterViewInit(): void {
    // We render charts after data loads in loadAdminData
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
    this.loadAdminData();
  }

  patientGrowthChartInstance: any = null;
  branchCollectionChartInstance: any = null;
  branchPatientChartInstance: any = null;

  loadAdminData(): void {
    if (this.activeTab === 'dashboard') {
      if (this.branches.length === 0) {
        this.api.get<any[]>('branch').subscribe({ next: (data) => this.branches = data });
      }

      let params = '';
      const parts: string[] = [];
      if (this.filterBranchId) parts.push(`branchId=${this.filterBranchId}`);
      if (this.filterYear) parts.push(`year=${this.filterYear}`);
      if (this.filterMonth) parts.push(`month=${this.filterMonth}`);
      if (this.filterStartDate) parts.push(`startDate=${this.filterStartDate}`);
      if (this.filterEndDate) parts.push(`endDate=${this.filterEndDate}`);
      if (parts.length > 0) params = '?' + parts.join('&');

      this.api.get<any>('dashboard' + params).subscribe({
        next: (data) => {
          this.statsData = data;
          setTimeout(() => this.renderCharts(), 100);
        }
      });
    } else if (this.activeTab === 'users') {
      this.api.get<any[]>('admin/doctors').subscribe({ next: (data) => this.doctors = data });
      this.api.get<any[]>('admin/staff').subscribe({ next: (data) => this.staff = data });
      this.api.get<any[]>('branch').subscribe({ next: (data) => this.branches = data });
    } else if (this.activeTab === 'catalog') {
      this.api.get<any[]>('test').subscribe({ next: (data) => this.tests = data });
      this.api.get<any[]>('test/categories').subscribe({ next: (data) => this.categories = data });
    } else if (this.activeTab === 'branches') {
      this.api.get<any[]>('branch').subscribe({
        next: (data) => {
          this.branches = data;
          if (this.branches.length > 0) this.selectBranch(this.branches[0].branchId);
        }
      });
    } else if (this.activeTab === 'audit') {
      this.api.get<any[]>('admin/audit-logs').subscribe({ next: (data) => this.auditLogs = data });
    }
  }

  clearFilters(): void {
    this.filterBranchId = '';
    this.filterYear = '';
    this.filterMonth = '';
    this.filterStartDate = '';
    this.filterEndDate = '';
    this.loadAdminData();
  }

  renderCharts(): void {
    if (this.revenueChartInstance) this.revenueChartInstance.destroy();
    if (this.popularChartInstance) this.popularChartInstance.destroy();
    if (this.patientGrowthChartInstance) this.patientGrowthChartInstance.destroy();
    if (this.branchCollectionChartInstance) this.branchCollectionChartInstance.destroy();
    if (this.branchPatientChartInstance) this.branchPatientChartInstance.destroy();

    // 1. Revenue Chart
    const revCtx = document.getElementById('revenueChart') as HTMLCanvasElement;
    if (revCtx) {
      const labels = this.statsData.monthlyRevenue?.map((r: any) => r.monthName) || [];
      const data = this.statsData.monthlyRevenue?.map((r: any) => r.revenue) || [];

      this.revenueChartInstance = new Chart(revCtx, {
        type: 'line',
        data: {
          labels: labels.length > 0 ? labels : ['Jan', 'Feb', 'Mar'],
          datasets: [{
            label: 'Monthly Revenue (INR)',
            data: data.length > 0 ? data : [5000, 12000, 8000],
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

    // 2. Popular Tests
    const popCtx = document.getElementById('popularChart') as HTMLCanvasElement;
    if (popCtx) {
      const labels = this.statsData.popularTests?.map((t: any) => t.testName) || [];
      const data = this.statsData.popularTests?.map((t: any) => t.bookingCount) || [];

      this.popularChartInstance = new Chart(popCtx, {
        type: 'bar',
        data: {
          labels: labels.length > 0 ? labels : ['CBC', 'Lipid', 'Sugar'],
          datasets: [{
            label: 'Booking Count',
            data: data.length > 0 ? data : [12, 8, 15],
            backgroundColor: '#10b981'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false
        }
      });
    }

    // 3. Patient Growth Chart
    const patCtx = document.getElementById('patientGrowthChart') as HTMLCanvasElement;
    if (patCtx) {
      const labels = this.statsData.monthlyPatients?.map((p: any) => p.monthName) || [];
      const data = this.statsData.monthlyPatients?.map((p: any) => p.patientsCount) || [];

      this.patientGrowthChartInstance = new Chart(patCtx, {
        type: 'line',
        data: {
          labels: labels.length > 0 ? labels : ['Jan', 'Feb', 'Mar'],
          datasets: [{
            label: 'Monthly Patient Growth',
            data: data.length > 0 ? data : [10, 22, 18],
            borderColor: '#8b5cf6',
            backgroundColor: 'rgba(139, 92, 246, 0.1)',
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

    // 4. Branch Collection Comparison
    const branchColCtx = document.getElementById('branchCollectionChart') as HTMLCanvasElement;
    if (branchColCtx) {
      const labels = this.statsData.branchCollections?.map((b: any) => b.branchName) || [];
      const data = this.statsData.branchCollections?.map((b: any) => b.collection) || [];

      this.branchCollectionChartInstance = new Chart(branchColCtx, {
        type: 'bar',
        data: {
          labels: labels.length > 0 ? labels : ['Branch A', 'Branch B'],
          datasets: [{
            label: 'Branch Revenue (INR)',
            data: data.length > 0 ? data : [12000, 18000],
            backgroundColor: '#3b82f6'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false
        }
      });
    }

    // 5. Branch Patient Comparison
    const branchPatCtx = document.getElementById('branchPatientChart') as HTMLCanvasElement;
    if (branchPatCtx) {
      const labels = this.statsData.branchPatients?.map((b: any) => b.branchName) || [];
      const data = this.statsData.branchPatients?.map((b: any) => b.patientsCount) || [];

      this.branchPatientChartInstance = new Chart(branchPatCtx, {
        type: 'doughnut',
        data: {
          labels: labels.length > 0 ? labels : ['Branch A', 'Branch B'],
          datasets: [{
            data: data.length > 0 ? data : [40, 60],
            backgroundColor: ['#10b981', '#f59e0b', '#3b82f6', '#ec4899', '#8b5cf6']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false
        }
      });
    }
  }

  selectBranch(id: number): void {
    this.selectedBranchId = id;
    this.api.get<any[]>(`branch/${id}/slots`).subscribe({
      next: (slots) => this.slots = slots
    });
  }

  // --- MOCK DATABASE BACKUP ---
  backupDatabase(): void {
    this.api.post('admin/backup').subscribe({
      next: (res: any) => {
        this.toast.showSuccess(res.Message || 'Backup successful!');
        this.loadAdminData();
      }
    });
  }

  // --- CREATES & DELETES ---
  openDoctorModal(doc?: any): void {
    this.showDocPassword = false;
    if (doc) {
      this.newDoc = { ...doc, password: '' };
    } else {
      this.newDoc = { doctorId: 0, userId: 0, firstName: '', lastName: '', email: '', specialization: 'Pathology', designation: 'Consultant', commissionRate: 15.00, phoneNumber: '', password: '' };
    }
    this.showDoctorModal = true;
  }

  saveDoctor(event: Event): void {
    event.preventDefault();
    if (this.newDoc.doctorId) {
      this.api.put('admin/doctors', this.newDoc).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Doctor profile updated.');
          this.showDoctorModal = false;
          this.loadAdminData();
        }
      });
    } else {
      this.api.post('admin/doctors', this.newDoc).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Doctor profile created.');
          this.showDoctorModal = false;
          this.loadAdminData();
        }
      });
    }
  }

  deleteDoctor(id: number): void {
    if (confirm('Are you sure you want to delete this doctor?')) {
      this.api.delete(`admin/doctors/${id}`).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Doctor deleted.');
          this.loadAdminData();
        }
      });
    }
  }

  openStaffModal(stf?: any): void {
    this.showStfPassword = false;
    if (stf) {
      this.newStf = { ...stf, branchIds: stf.branchIds || [], password: '' };
    } else {
      if (this.branches.length === 0) {
        this.toast.showError('Please create a branch first.');
        return;
      }
      this.newStf = { staffId: 0, userId: 0, firstName: '', lastName: '', email: '', branchId: this.branches[0].branchId, branchIds: [], designation: 'Lab Technician', phoneNumber: '', password: '' };
    }
    this.showStaffModal = true;
  }

  isStaffBranchSelected(branchId: number): boolean {
    return this.newStf.branchIds && this.newStf.branchIds.includes(branchId);
  }

  toggleStaffBranchSelection(branchId: number): void {
    if (!this.newStf.branchIds) {
      this.newStf.branchIds = [];
    }
    const idx = this.newStf.branchIds.indexOf(branchId);
    if (idx > -1) {
      this.newStf.branchIds.splice(idx, 1);
    } else {
      this.newStf.branchIds.push(branchId);
    }
  }

  saveStaff(event: Event): void {
    event.preventDefault();
    if (!this.newStf.branchIds || this.newStf.branchIds.length === 0) {
      this.toast.showError('Please select at least one branch.');
      return;
    }
    this.newStf.branchId = this.newStf.branchIds[0]; // Set primary branch ID

    if (this.newStf.staffId) {
      this.api.put('admin/staff', this.newStf).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Staff profile updated.');
          this.showStaffModal = false;
          this.loadAdminData();
        }
      });
    } else {
      this.api.post('admin/staff', this.newStf).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Staff profile created.');
          this.showStaffModal = false;
          this.loadAdminData();
        }
      });
    }
  }

  deleteStaff(id: number): void {
    if (confirm('Are you sure you want to delete this staff member?')) {
      this.api.delete(`admin/staff/${id}`).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Staff deleted.');
          this.loadAdminData();
        }
      });
    }
  }

  deleteTest(id: number): void {
    if (confirm('Are you sure you want to delete this test?')) {
      this.api.delete(`test/${id}`).subscribe({
        next: () => {
          this.toast.showSuccess('Test removed from catalog.');
          this.loadAdminData();
        }
      });
    }
  }

  openTestModal(test?: any): void {
    if (test) {
      this.testFormData = { ...test };
    } else {
      this.testFormData = { testId: 0, categoryId: 0, name: '', code: '', price: 0, sampleType: '', preparation: '', normalRange: '', deliveryTimeHours: 24, isActive: true };
    }
    this.showTestModal = true;
  }

  saveTest(): void {
    if (!this.testFormData.name || !this.testFormData.code || this.testFormData.categoryId === 0) {
      this.toast.showError('Please provide Test Name, Code, and select a Category.');
      return;
    }

    if (this.testFormData.testId) {
      this.api.put('test', this.testFormData).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Test updated successfully.');
          this.showTestModal = false;
          this.loadAdminData();
        }
      });
    } else {
      this.api.post('test', this.testFormData).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Test created successfully.');
          this.showTestModal = false;
          this.loadAdminData();
        }
      });
    }
  }

  editTimeSlot(slot: any): void {
    this.newSlotData = {
      slotId: slot.slotId,
      start: slot.startTime ? slot.startTime.substring(0,5) : '',
      end: slot.endTime ? slot.endTime.substring(0,5) : '',
      maxBookings: slot.maxBookings
    };
  }

  cancelSlotEdit(): void {
    this.newSlotData = { slotId: 0, start: '09:00', end: '10:00', maxBookings: 5 };
  }

  deleteTimeSlot(id: number): void {
    if (confirm('Are you sure you want to delete this time slot?')) {
      this.api.delete(`branch/slots/${id}`).subscribe({
        next: () => {
          this.toast.showSuccess('Time slot deleted successfully.');
          this.selectBranch(this.selectedBranchId);
        }
      });
    }
  }

  addTimeSlot(): void {
    if (!this.newSlotData.start || !this.newSlotData.end) {
      this.toast.showError('Please provide both start and end times.');
      return;
    }
    
    if (this.newSlotData.start >= this.newSlotData.end) {
      this.toast.showError('End time must be after start time.');
      return;
    }

    const exists = this.slots.some(s => s.slotId !== this.newSlotData.slotId && s.startTime && s.endTime && s.startTime.substring(0,5) === this.newSlotData.start && s.endTime.substring(0,5) === this.newSlotData.end);
    if (exists) {
      this.toast.showError('This time slot is already configured for this branch.');
      return;
    }
    
    const payload = {
      slotId: this.newSlotData.slotId || 0,
      branchId: this.selectedBranchId,
      startTime: this.newSlotData.start + (this.newSlotData.start.length === 5 ? ':00' : ''),
      endTime: this.newSlotData.end + (this.newSlotData.end.length === 5 ? ':00' : ''),
      maxBookings: this.newSlotData.maxBookings || 5,
      isActive: true
    };

    if (payload.slotId > 0) {
      this.api.put('branch/slots', payload).subscribe({
        next: () => {
          this.toast.showSuccess('Time slot updated successfully.');
          this.selectBranch(this.selectedBranchId);
          this.cancelSlotEdit();
        }
      });
    } else {
      this.api.post('branch/slots', payload).subscribe({
        next: () => {
          this.toast.showSuccess('New time slot configured.');
          this.selectBranch(this.selectedBranchId);
          this.cancelSlotEdit();
        }
      });
    }
  }

  openBranchModal(): void {
    this.branchFormData = { branchId: 0, name: '', address: '', city: '', contactNumber: '', email: '', isActive: true };
    this.showBranchModal = true;
  }

  openEditBranchModal(branch: any): void {
    this.branchFormData = { ...branch };
    this.showBranchModal = true;
  }

  saveBranch(): void {
    if (!this.branchFormData.name || !this.branchFormData.address || !this.branchFormData.city) {
      this.toast.showError('Please fill in all required branch details.');
      return;
    }
    
    if (this.branchFormData.branchId) {
      // Update
      this.api.put('branch', this.branchFormData).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Branch updated successfully.');
          this.showBranchModal = false;
          this.loadAdminData();
        }
      });
    } else {
      // Create
      this.api.post('branch', this.branchFormData).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Branch created successfully.');
          this.showBranchModal = false;
          this.loadAdminData();
        }
      });
    }
  }

  deleteBranch(id: number): void {
    if (confirm('Are you sure you want to delete this branch? All related data may be affected.')) {
      this.api.delete(`branch/${id}`).subscribe({
        next: (res: any) => {
          this.toast.showSuccess(res.Message || 'Branch deleted successfully.');
          if (this.selectedBranchId === id) this.selectedBranchId = 0;
          this.loadAdminData();
        }
      });
    }
  }
}
