import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

interface BranchItem {
  branchId: number;
  name: string;
  address: string;
  city: string;
}

interface TestItem {
  testId: number;
  name: string;
  code: string;
  price: number;
  categoryName: string;
  selected?: boolean;
}

interface PackageItem {
  packageId: number;
  name: string;
  code: string;
  price: number;
  description: string;
  selected?: boolean;
}

interface TimeSlotItem {
  slotId: number;
  startTime: string;
  endTime: string;
}

@Component({
  selector: 'app-booking-wizard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container py-5">
      <!-- Wizard Progress Header -->
      <div class="row justify-content-center mb-5">
        <div class="col-lg-8">
          <div class="glass-card p-4">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <h4 class="fw-bold mb-0 text-primary">Appointment Booking Wizard</h4>
              <a routerLink="/patient/dashboard" class="btn btn-sm btn-outline-secondary d-flex align-items-center gap-1">
                <span class="material-icons fs-6">arrow_back</span>
                Cancel
              </a>
            </div>
            
            <!-- Progress Bar -->
            <div class="progress" style="height: 6px;">
              <div class="progress-bar bg-primary" [style.width.%]="progressPercentage()"></div>
            </div>
            
            <div class="row text-center mt-3 small text-secondary fw-semibold">
              <div class="col-3" [class.text-primary]="currentStep() >= 1">1. Branch</div>
              <div class="col-3" [class.text-primary]="currentStep() >= 2">2. Diagnostics</div>
              <div class="col-3" [class.text-primary]="currentStep() >= 3">3. Slot</div>
              <div class="col-3" [class.text-primary]="currentStep() >= 4">4. Payment</div>
            </div>
          </div>
        </div>
      </div>

      <!-- STEP CONTENTS -->
      <div class="row justify-content-center">
        <div class="col-lg-8">
          
          <!-- STEP 1: BRANCH SELECTION -->
          <div *ngIf="currentStep() === 1" class="glass-card">
            <h5 class="fw-bold mb-3">Step 1: Select Laboratory Branch</h5>
            <p class="text-secondary small mb-4">Choose the closest laboratory location where sample processing will take place.</p>
            
            <div class="d-grid gap-3">
              <div *ngFor="let branch of branches" 
                   (click)="selectBranchItem(branch)" 
                   class="p-3 rounded border hoverable cursor-pointer d-flex justify-content-between align-items-center"
                   [class.border-primary]="selectedBranch()?.branchId === branch.branchId"
                   [class.bg-primary-subtle]="selectedBranch()?.branchId === branch.branchId">
                <div>
                  <h6 class="fw-bold mb-1">{{ branch.name }}</h6>
                  <span class="text-secondary small">{{ branch.address }}, {{ branch.city }}</span>
                </div>
                <span class="material-icons text-primary" *ngIf="selectedBranch()?.branchId === branch.branchId">check_circle</span>
              </div>
            </div>

            <div class="text-end mt-4 pt-3 border-top">
              <button class="btn btn-primary-custom px-4" [disabled]="!selectedBranch()" (click)="nextStep()">
                Next Step
              </button>
            </div>
          </div>

          <!-- STEP 2: TESTS AND PACKAGES -->
          <div *ngIf="currentStep() === 2" class="glass-card">
            <h5 class="fw-bold mb-3">Step 2: Select Tests & Packages</h5>
            <p class="text-secondary small mb-4">Choose multiple individual lab tests or complete check-up packages.</p>

            <!-- Packages Catalog -->
            <div class="mb-4">
              <h6 class="fw-bold text-primary mb-3">Healthcare Packages</h6>
              <div class="row g-3">
                <div class="col-md-6" *ngFor="let pkg of packages()">
                  <div class="p-3 rounded border hoverable cursor-pointer h-100 d-flex flex-column justify-content-between"
                       (click)="togglePackage(pkg)"
                       [class.border-primary]="pkg.selected"
                       [class.bg-primary-subtle]="pkg.selected">
                    <div>
                      <div class="d-flex justify-content-between mb-2">
                        <span class="badge bg-primary text-white">PKG</span>
                        <span class="text-muted small">{{ pkg.code }}</span>
                      </div>
                      <h6 class="fw-bold mb-1">{{ pkg.name }}</h6>
                      <p class="text-secondary small mb-3">{{ pkg.description }}</p>
                    </div>
                    <div class="d-flex justify-content-between align-items-center pt-2 border-top">
                      <span class="fw-bold text-primary">INR {{ pkg.price }}</span>
                      <span class="material-icons text-primary" *ngIf="pkg.selected">check_circle</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Tests Catalog -->
            <div class="mb-4">
              <h6 class="fw-bold text-primary mb-3">Individual Tests</h6>
              <div class="list-group">
                <div *ngFor="let test of tests()" 
                     (click)="toggleTest(test)" 
                     class="list-group-item list-group-item-action d-flex justify-content-between align-items-center cursor-pointer py-3"
                     [class.list-group-item-primary]="test.selected">
                  <div>
                    <h6 class="fw-bold mb-1">{{ test.name }}</h6>
                    <span class="badge bg-secondary-subtle text-secondary me-2">{{ test.categoryName }}</span>
                    <span class="text-muted small">Code: {{ test.code }}</span>
                  </div>
                  <div class="d-flex align-items-center gap-3">
                    <span class="fw-bold text-primary">INR {{ test.price }}</span>
                    <span class="material-icons text-primary" *ngIf="test.selected">check_circle</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Floating Subtotal Details -->
            <div class="p-3 bg-body-tertiary rounded border mb-4 d-flex justify-content-between align-items-center">
              <div>
                <span class="text-muted small d-block">Subtotal Selection</span>
                <span class="fw-bold fs-5 text-dark">INR {{ selectionSubtotal() }}</span>
              </div>
              <span class="text-secondary small">{{ selectedTestsCount() }} Tests, {{ selectedPackagesCount() }} Packages</span>
            </div>

            <div class="d-flex justify-content-between mt-4 pt-3 border-top">
              <button class="btn btn-outline-secondary px-4" (click)="prevStep()">Back</button>
              <button class="btn btn-primary-custom px-4" [disabled]="selectionSubtotal() === 0" (click)="nextStep()">
                Next Step
              </button>
            </div>
          </div>

          <!-- STEP 3: DATE & TIME SLOT -->
          <div *ngIf="currentStep() === 3" class="glass-card">
            <h5 class="fw-bold mb-3">Step 3: Select Date & Time Slot</h5>
            <p class="text-secondary small mb-4">Choose a date and a time slot for blood sample collection.</p>

            <div class="row g-4">
              <!-- Date Picker -->
              <div class="col-md-6">
                <label class="form-label small fw-medium">Appointment Date</label>
                <input type="date" [(ngModel)]="bookingDate" (change)="onDateChange()" [min]="minDate" class="form-control bg-transparent" required />
              </div>

              <!-- Time Slots Grid -->
              <div class="col-md-6">
                <label class="form-label small fw-medium d-block mb-3">Available Slots</label>
                <div class="d-grid gap-2" style="grid-template-columns: repeat(2, 1fr);" *ngIf="timeSlots.length > 0; else noSlots">
                  <button type="button" *ngFor="let slot of timeSlots" 
                          (click)="selectedSlot.set(slot)" 
                          class="btn btn-outline-primary py-2 d-flex flex-column align-items-center"
                          [class.active]="selectedSlot()?.slotId === slot.slotId">
                    <span class="fw-bold">{{ slot.startTime.substring(0,5) }}</span>
                    <span class="small" style="font-size: 0.7rem;">to {{ slot.endTime.substring(0,5) }}</span>
                  </button>
                </div>
                
                <ng-template #noSlots>
                  <div class="p-3 bg-light rounded text-center text-muted small">
                    Select a valid date to query available slots.
                  </div>
                </ng-template>
              </div>
            </div>

            <div class="d-flex justify-content-between mt-5 pt-3 border-top">
              <button class="btn btn-outline-secondary px-4" (click)="prevStep()">Back</button>
              <button class="btn btn-primary-custom px-4" [disabled]="!bookingDate || !selectedSlot()" (click)="nextStep()">
                Next Step
              </button>
            </div>
          </div>

          <!-- STEP 4: REVIEW & PAYMENT SIMULATOR -->
          <div *ngIf="currentStep() === 4" class="glass-card">
            <h5 class="fw-bold mb-3">Step 4: Review & Payment Simulator</h5>
            <p class="text-secondary small mb-4">Confirm booking parameters and process your payment.</p>

            <!-- Order Review Panel -->
            <div class="p-3 bg-body-tertiary rounded border mb-4">
              <h6 class="fw-bold text-primary mb-3">Order Breakdown</h6>
              
              <!-- Items list -->
              <div class="small text-secondary mb-3 gap-2 d-grid">
                <div class="d-flex justify-content-between" *ngFor="let item of getSelectedItemsList()">
                  <span>{{ item.name }} ({{ item.type }})</span>
                  <span class="fw-semibold">INR {{ item.price }}</span>
                </div>
              </div>

              <!-- Price Totals -->
              <div class="border-top pt-3 small text-secondary gap-2 d-grid">
                <div class="d-flex justify-content-between">
                  <span>Subtotal Cost:</span>
                  <span>INR {{ selectionSubtotal() }}</span>
                </div>
                <div class="d-flex justify-content-between text-success">
                  <span>Discount Applied (10%):</span>
                  <span>INR -{{ discountAmount() | number:'1.2-2' }}</span>
                </div>
                <div class="d-flex justify-content-between">
                  <span>GST Tax (18%):</span>
                  <span>INR {{ taxAmount() | number:'1.2-2' }}</span>
                </div>
                <div class="d-flex justify-content-between text-dark fw-bold fs-5 pt-2 border-top">
                  <span>Total Payable:</span>
                  <span>INR {{ finalPayable() | number:'1.2-2' }}</span>
                </div>
              </div>
            </div>

            <!-- Booking Details Review -->
            <div class="p-3 bg-light rounded text-dark small mb-4">
              <div class="mb-1"><strong>Branch Location:</strong> {{ selectedBranch()?.name }}</div>
              <div class="mb-1"><strong>Scheduled Date:</strong> {{ bookingDate }}</div>
              <div><strong>Selected Slot:</strong> {{ selectedSlot()?.startTime?.substring(0,5) }} - {{ selectedSlot()?.endTime?.substring(0,5) }}</div>
            </div>

            <!-- Payment gateway selector -->
            <div class="mb-4">
              <label class="form-label small fw-medium">Choose Payment Method</label>
              <div class="d-flex gap-3">
                <div class="form-check p-3 rounded border hoverable flex-grow-1 cursor-pointer"
                     [class.border-primary]="paymentMethod() === 'Online'"
                     (click)="paymentMethod.set('Online')">
                  <input class="form-check-input ms-0 me-2" type="radio" name="payMethod" id="payOnline" [checked]="paymentMethod() === 'Online'" />
                  <label class="form-check-label fw-bold" for="payOnline">Online Pay (Stripe/Razorpay)</label>
                </div>
                
                <div class="form-check p-3 rounded border hoverable flex-grow-1 cursor-pointer"
                     [class.border-primary]="paymentMethod() === 'Cash'"
                     (click)="paymentMethod.set('Cash')">
                  <input class="form-check-input ms-0 me-2" type="radio" name="payMethod" id="payCash" [checked]="paymentMethod() === 'Cash'" />
                  <label class="form-check-label fw-bold" for="payCash">Cash On Collection</label>
                </div>
              </div>
            </div>

            <!-- Online Card Form simulator -->
            <div class="glass-card mb-4 border-primary border-top border-3" *ngIf="paymentMethod() === 'Online'">
              <h6 class="fw-bold mb-3 d-flex align-items-center gap-2 text-primary">
                <span class="material-icons">credit_card</span> Secure Card Checkout (Simulator)
              </h6>
              
              <div class="row g-3">
                <div class="col-12">
                  <label class="form-label small fw-medium">Card Number</label>
                  <input type="text" [(ngModel)]="cardNumber" class="form-control bg-transparent" placeholder="4111 2222 3333 4444" required />
                </div>
                <div class="col-md-6">
                  <label class="form-label small fw-medium">Expiry Date</label>
                  <input type="text" class="form-control bg-transparent" placeholder="MM/YY" required />
                </div>
                <div class="col-md-6">
                  <label class="form-label small fw-medium">CVV Code</label>
                  <input type="password" class="form-control bg-transparent" placeholder="123" required />
                </div>
              </div>
            </div>

            <!-- Action buttons -->
            <div class="d-flex justify-content-between mt-4 pt-3 border-top">
              <button class="btn btn-outline-secondary px-4" (click)="prevStep()">Back</button>
              <button class="btn btn-success py-2 px-5 fw-bold d-flex align-items-center gap-2" [disabled]="isSubmitting()" (click)="checkoutAppointment()">
                <span *ngIf="isSubmitting()" class="spinner-border spinner-border-sm"></span>
                Place Booking
              </button>
            </div>
          </div>

        </div>
      </div>
    </div>
  `,
  styles: [`
    .cursor-pointer {
      cursor: pointer;
    }
  `]
})
export class BookingWizardComponent implements OnInit {
  auth = inject(AuthService);
  private api = inject(ApiService);
  private toast = inject(NotificationService);
  private router = inject(Router);

  currentStep = signal(1);
  branches: BranchItem[] = [];
  tests = signal<TestItem[]>([]);
  packages = signal<PackageItem[]>([]);
  timeSlots: TimeSlotItem[] = [];

  // Selections
  selectedBranch = signal<BranchItem | null>(null);
  bookingDate = '';
  selectedSlot = signal<TimeSlotItem | null>(null);
  paymentMethod = signal('Online');
  
  // Checkout simulator
  cardNumber = '4111 2222 3333 4444';
  isSubmitting = signal(false);
  
  minDate = '';

  ngOnInit(): void {
    this.initMinDate();
    this.loadInitialData();
  }

  initMinDate(): void {
    const today = new Date();
    // Min booking is tomorrow
    today.setDate(today.getDate() + 1);
    this.minDate = today.toISOString().split('T')[0];
  }

  loadInitialData(): void {
    // 1. Fetch Branches
    this.api.get<BranchItem[]>('branch').subscribe({
      next: (data) => this.branches = data.filter((b: any) => b.isActive)
    });

    // 2. Fetch Tests Catalog
    this.api.get<TestItem[]>('test').subscribe({
      next: (data) => this.tests.set(data.filter((t: any) => t.isActive))
    });

    // 3. Fetch Packages Catalog
    this.api.get<PackageItem[]>('test/packages').subscribe({
      next: (data) => this.packages.set(data.filter((p: any) => p.isActive))
    });
  }

  selectBranchItem(branch: BranchItem): void {
    this.selectedBranch.set(branch);
    this.selectedSlot.set(null);
    this.timeSlots = [];
    if (this.bookingDate) {
      this.fetchTimeSlots();
    }
  }

  fetchTimeSlots(): void {
    const branch = this.selectedBranch();
    if (!this.bookingDate || !branch) return;

    this.api.get<TimeSlotItem[]>(`branch/${branch.branchId}/slots`).subscribe({
      next: (slots) => this.timeSlots = slots
    });
  }

  onDateChange(): void {
    this.selectedSlot.set(null);
    this.fetchTimeSlots();
  }

  toggleTest(test: TestItem): void {
    this.tests.update(arr => arr.map(t => t.testId === test.testId ? { ...t, selected: !t.selected } : t));
  }

  togglePackage(pkg: PackageItem): void {
    this.packages.update(arr => arr.map(p => p.packageId === pkg.packageId ? { ...p, selected: !p.selected } : p));
  }

  selectionSubtotal = computed(() => {
    const testsCost = this.tests().filter(t => t.selected).reduce((acc, curr) => acc + curr.price, 0);
    const packagesCost = this.packages().filter(p => p.selected).reduce((acc, curr) => acc + curr.price, 0);
    return testsCost + packagesCost;
  });

  selectedTestsCount = computed(() => this.tests().filter(t => t.selected).length);
  selectedPackagesCount = computed(() => this.packages().filter(p => p.selected).length);

  discountAmount = computed(() => this.selectionSubtotal() * 0.10); // 10% Discount
  taxAmount = computed(() => (this.selectionSubtotal() - this.discountAmount()) * 0.18); // 18% Tax
  finalPayable = computed(() => (this.selectionSubtotal() - this.discountAmount()) + this.taxAmount());

  progressPercentage = computed(() => {
    return (this.currentStep() / 4) * 100;
  });

  nextStep(): void {
    if (this.currentStep() < 4) {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  getSelectedItemsList() {
    const list: any[] = [];
    this.tests().filter(t => t.selected).forEach(t => list.push({ name: t.name, price: t.price, type: 'Test' }));
    this.packages().filter(p => p.selected).forEach(p => list.push({ name: p.name, price: p.price, type: 'Package' }));
    return list;
  }

  checkoutAppointment(): void {
    const user = this.auth.currentUser();
    if (!user || !user.patientId) return;

    this.isSubmitting.set(true);

    const testIds = this.tests().filter(t => t.selected).map(t => t.testId);
    const packageIds = this.packages().filter(p => p.selected).map(p => p.packageId);

    const payload = {
      patientId: user.patientId,
      branchId: this.selectedBranch()?.branchId,
      appointmentDate: this.bookingDate,
      slotId: this.selectedSlot()?.slotId,
      testIds: testIds,
      packageIds: packageIds,
      totalAmount: this.selectionSubtotal(),
      discountAmount: this.discountAmount(),
      paidAmount: this.paymentMethod() === 'Online' ? this.finalPayable() : 0,
      paymentMethod: this.paymentMethod(),
      paymentStatus: this.paymentMethod() === 'Online' ? 'Paid' : 'Unpaid',
      notes: 'Booked online via patient dashboard.'
    };

    // 1. Book appointment and generate Invoice
    this.api.post<any>('appointment/book', payload).subscribe({
      next: (bookingRes) => {
        
        // 2. If Payment is Online, simulate checkout transaction log
        if (this.paymentMethod() === 'Online') {
          const transactionPayload = {
            amount: this.finalPayable(),
            paymentMethod: 'Online',
            transactionId: `TXN-${GuidSimulator()}`
          };

          this.api.post<any>(`appointment/${bookingRes.AppointmentId}/payment`, transactionPayload).subscribe({
            next: () => {
              this.isSubmitting.set(false);
              this.toast.showSuccess('Appointment booked and paid successfully!');
              this.router.navigate(['/patient/dashboard']);
            },
            error: () => this.isSubmitting.set(false)
          });
        } else {
          this.isSubmitting.set(false);
          this.toast.showSuccess('Appointment booked successfully. Pay Cash on collection.');
          this.router.navigate(['/patient/dashboard']);
        }

      },
      error: () => this.isSubmitting.set(false)
    });
  }
}

function GuidSimulator(): string {
  return Math.random().toString(36).substring(2, 8).toUpperCase();
}
