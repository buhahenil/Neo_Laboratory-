import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { FormsModule } from '@angular/forms';

interface TestCatalogItem {
  testId: number;
  name: string;
  code: string;
  price: number;
  sampleType: string;
  categoryName: string;
  deliveryTimeHours: number;
}

interface PackageCatalogItem {
  packageId: number;
  name: string;
  code: string;
  price: number;
  description: string;
}

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="landing-page-container">
      <!-- Top Promo Bar -->
      <div class="bg-primary text-white text-center py-2 px-3 small fw-semibold d-flex align-items-center justify-content-center gap-2">
        <span class="material-icons fs-6">verified</span>
        <span>ISO 15189:2022 NABL Accredited Diagnostic Center — Fast 12-24h Digital Report Delivery</span>
      </div>

      <!-- Navbar -->
      <nav class="navbar navbar-expand-lg glass-header sticky-top py-3">
        <div class="container">
          <a class="navbar-brand d-flex align-items-center fw-bold text-primary fs-3" href="#">
            <span class="material-icons me-2 fs-2">biotech</span>
            Neo Laboratory
          </a>
          <button class="navbar-toggler border-0 p-1" type="button" (click)="toggleMobileMenu()">
            <span class="material-icons">menu</span>
          </button>
          
          <div class="collapse navbar-collapse" [class.show]="isMobileMenuOpen">
            <ul class="navbar-nav mx-auto mb-2 mb-lg-0 gap-3 fw-semibold">
              <li class="nav-item"><a class="nav-link" href="#home" (click)="closeMenu()">Home</a></li>
              <li class="nav-item"><a class="nav-link" href="#features" (click)="closeMenu()">Why Us</a></li>
              <li class="nav-item"><a class="nav-link" href="#tests" (click)="closeMenu()">Tests Directory</a></li>
              <li class="nav-item"><a class="nav-link" href="#about" (click)="closeMenu()">Accreditation</a></li>
              <li class="nav-item"><a class="nav-link" href="#contact" (click)="closeMenu()">Contact</a></li>
            </ul>
            
            <div class="d-flex align-items-center gap-3">
              <button class="btn btn-sm btn-outline-secondary d-flex align-items-center p-2 rounded-circle" (click)="toggleTheme()">
                <span class="material-icons">{{ isDarkMode() ? 'light_mode' : 'dark_mode' }}</span>
              </button>
              
              <ng-container *ngIf="auth.isAuthenticated(); else authButtons">
                <a [routerLink]="dashboardLink()" class="btn btn-primary-custom d-flex align-items-center gap-1 shadow-sm px-4">
                  <span class="material-icons fs-6">dashboard</span>
                  Dashboard
                </a>
                <button (click)="auth.logout()" class="btn btn-outline-danger d-flex align-items-center gap-1 px-3">
                  <span class="material-icons fs-6">logout</span>
                  Logout
                </button>
              </ng-container>
              
              <ng-template #authButtons>
                <a routerLink="/auth/login" class="btn btn-outline-primary fw-semibold px-4 rounded-pill">Login</a>
                <a routerLink="/auth/register" class="btn btn-primary-custom rounded-pill px-4">Register</a>
              </ng-template>
            </div>
          </div>
        </div>
      </nav>

      <!-- Hero Section -->
      <section id="home" class="py-5 bg-gradient-hero">
        <div class="hero-glow-orb"></div>
        <div class="container py-5 position-relative z-1">
          <div class="row align-items-center g-5">
            <div class="col-lg-6 text-start">
              <span class="badge bg-primary-subtle text-primary px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm">DIGITAL CLINICAL CENTRE</span>
              <h1 class="hero-title fw-extrabold mb-3">Accurate Diagnostics, <br><span class="text-primary-gradient">Healthy Life Choices</span></h1>
              <p class="lead text-secondary mb-4">Book blood test packages, schedule professional home sample collection, and download digitally-verified pathology reports directly from your workspace dashboard.</p>
              
              <!-- Search Box -->
              <div class="glass-search-container d-flex gap-2 align-items-center mb-4">
                <span class="material-icons text-muted ms-2">search</span>
                <input type="text" [(ngModel)]="searchQuery" (input)="filterTests()" class="form-control border-0 shadow-none bg-transparent py-2" placeholder="Search blood test, lipid profile, CBC..." />
                <button class="btn btn-primary-custom rounded-pill px-4" (click)="scrollToTests()">Search</button>
              </div>

              <!-- Quick Badges -->
              <div class="d-flex flex-wrap gap-2 text-secondary small align-items-center">
                <span class="fw-bold">Popular:</span>
                <span class="badge bg-body-secondary text-secondary cursor-pointer" (click)="setSearch('CBC')">CBC</span>
                <span class="badge bg-body-secondary text-secondary cursor-pointer" (click)="setSearch('Lipid')">Lipid Profile</span>
                <span class="badge bg-body-secondary text-secondary cursor-pointer" (click)="setSearch('Sugar')">Blood Sugar</span>
              </div>
            </div>

            <!-- Hero graphic/card -->
            <div class="col-lg-6">
              <div class="glass-card p-4 border border-light shadow-2xl position-relative overflow-hidden">
                <div class="d-flex align-items-center gap-3 mb-4">
                  <div class="p-3 bg-primary-subtle text-primary rounded-circle"><span class="material-icons fs-1">science</span></div>
                  <div>
                    <h5 class="fw-bold mb-0">Diagnostic Excellence</h5>
                    <p class="text-secondary small mb-0">Empowering health with automation</p>
                  </div>
                </div>

                <div class="row g-3 text-start">
                  <div class="col-6">
                    <div class="benefit-box">
                      <span class="material-icons text-primary mb-1">alarm</span>
                      <h6 class="fw-bold mb-1">12-24 Hours</h6>
                      <span class="text-secondary small">Average Report Time</span>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="benefit-box">
                      <span class="material-icons text-success mb-1">verified_user</span>
                      <h6 class="fw-bold mb-1">NABL Accredited</h6>
                      <span class="text-secondary small">ISO 15189:2022</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Key Benefits / Why Us -->
      <section id="features" class="py-5 bg-body-tertiary border-top border-bottom">
        <div class="container py-4">
          <div class="text-center mb-5 max-width-600 mx-auto">
            <span class="text-primary fw-bold text-uppercase small tracking-wider">Our Advantages</span>
            <h2 class="fw-bold mt-2">Why Doctors & Patients Trust Neo</h2>
            <p class="text-secondary">Highly-automated diagnostics with patient convenience at the core.</p>
          </div>

          <div class="row g-4">
            <div class="col-md-4">
              <div class="glass-card hoverable h-100 p-4 text-start">
                <div class="feature-icon-wrapper">
                  <span class="material-icons fs-2">house_siding</span>
                </div>
                <h5 class="fw-bold mb-2">Home Sample Collection</h5>
                <p class="text-secondary small mb-0">Our professional clinical technologists collect blood and urine samples from the comfort of your home at your scheduled time slots.</p>
              </div>
            </div>

            <div class="col-md-4">
              <div class="glass-card hoverable h-100 p-4 text-start">
                <div class="feature-icon-wrapper text-success bg-success-subtle">
                  <span class="material-icons fs-2 text-success">qr_code_2</span>
                </div>
                <h5 class="fw-bold mb-2">Digitally Verified Reports</h5>
                <p class="text-secondary small mb-0">Download secure PDF reports equipped with QR verification links that can be instantly scanned to prove authenticity anywhere.</p>
              </div>
            </div>

            <div class="col-md-4">
              <div class="glass-card hoverable h-100 p-4 text-start">
                <div class="feature-icon-wrapper text-warning bg-warning-subtle">
                  <span class="material-icons fs-2 text-warning">location_city</span>
                </div>
                <h5 class="fw-bold mb-2">Multi-Branch Locations</h5>
                <p class="text-secondary small mb-0">We have fully functioning physical branch locations across the city, allowing you to walk in for direct counseling and testing.</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Diagnostic Catalog Section -->
      <section id="tests" class="py-5">
        <div class="container py-4">
          <div class="text-center mb-5 max-width-600 mx-auto">
            <span class="text-primary fw-bold text-uppercase small">Tests Catalog</span>
            <h2 class="fw-bold mt-2">Available Diagnostic Tests</h2>
            <p class="text-secondary">Search and select tests from our database or book one of our custom packages.</p>
          </div>

          <div class="row">
            <!-- Filtered Tests Catalog -->
            <div class="col-12 mb-4">
              <h4 class="fw-bold text-primary mb-3">Lab Tests Directory</h4>
              <div class="row g-4">
                <div class="col-md-6 col-lg-4" *ngFor="let test of filteredTests">
                  <div class="catalog-card h-100 d-flex flex-column justify-content-between">
                    <div>
                      <div class="d-flex justify-content-between align-items-start mb-3">
                        <span class="badge bg-primary-subtle text-primary">{{ test.categoryName }}</span>
                        <span class="text-muted small">Code: {{ test.code }}</span>
                      </div>
                      <h5 class="fw-bold mb-2">{{ test.name }}</h5>
                      <div class="small text-secondary mb-3 d-flex align-items-center gap-1">
                        <span class="material-icons fs-6">science</span>
                        <span>Sample: {{ test.sampleType }}</span>
                      </div>
                    </div>
                    <div class="d-flex justify-content-between align-items-center pt-2 border-top">
                      <div>
                        <span class="text-muted small d-block">Price</span>
                        <span class="fw-bold fs-5 text-primary">INR {{ test.price | number:'1.2-2' }}</span>
                      </div>
                      <a routerLink="/patient/book" class="btn btn-sm btn-outline-primary rounded-pill px-4">Book Now</a>
                    </div>
                  </div>
                </div>
                
                <div class="col-12 text-center py-5 border rounded-3 bg-light" *ngIf="filteredTests.length === 0">
                  <span class="material-icons text-muted fs-1 mb-2">sentiment_dissatisfied</span>
                  <p class="text-secondary">No tests found matching "{{ searchQuery }}". Try searching for 'CBC' or 'Glucose'.</p>
                </div>
              </div>
            </div>

            <!-- Health Packages -->
            <div class="col-12 mt-5">
              <div class="p-4 bg-primary-gradient-card rounded-4 text-white text-start mb-5 shadow-lg position-relative overflow-hidden">
                <div class="row align-items-center">
                  <div class="col-lg-8 z-1">
                    <h3 class="fw-bold mb-2">Looking for a comprehensive health check?</h3>
                    <p class="mb-0 opacity-90">Our health packages cover a complete profile including kidney function, liver metrics, CBC, and cardiac flags.</p>
                  </div>
                  <div class="col-lg-4 text-lg-end mt-4 mt-lg-0 z-1">
                    <a routerLink="/patient/book" class="btn btn-light text-primary fw-bold px-4 py-2 rounded-pill">Book Full Profile</a>
                  </div>
                </div>
              </div>

              <h4 class="fw-bold text-primary mb-4 text-center">Featured Health Packages</h4>
              <div class="row g-4 justify-content-center">
                <div class="col-md-6 col-lg-5" *ngFor="let pkg of packages">
                  <div class="package-card h-100 d-flex flex-column justify-content-between">
                    <div>
                      <div class="d-flex justify-content-between align-items-center mb-3">
                        <span class="badge bg-success text-white rounded-pill px-3">BEST SELLER</span>
                        <span class="text-muted small">Code: {{ pkg.code }}</span>
                      </div>
                      <h4 class="fw-bold mb-2 text-primary">{{ pkg.name }}</h4>
                      <p class="text-secondary mb-4">{{ pkg.description }}</p>
                    </div>
                    <div class="d-flex justify-content-between align-items-center pt-3 border-top">
                      <div>
                        <span class="text-muted small d-block">Package Price</span>
                        <span class="fw-bold fs-4 text-primary">INR {{ pkg.price | number:'1.2-2' }}</span>
                      </div>
                      <a routerLink="/patient/book" class="btn btn-primary-custom rounded-pill px-4">Book Package</a>
                    </div>
                  </div>
                </div>
              </div>
            </div>

          </div>
        </div>
      </section>

      <!-- About Section -->
      <section id="about" class="py-5 bg-body-tertiary">
        <div class="container py-4">
          <div class="row align-items-center g-5">
            <div class="col-lg-6">
              <span class="text-primary fw-bold">ACCREDITATION</span>
              <h2 class="fw-bold mt-2 mb-4">Dedicated to Precision & Patient Care</h2>
              <p class="text-secondary mb-3">Neo Laboratory Pathology Network is a NABL-accredited diagnostic provider equipped with automated biochemistry, hematology, and immunology analyzers.</p>
              <p class="text-secondary mb-4">Our processes are highly automated, running on a custom framework that guarantees complete patient audit logs, QR-coded PDF reports, and zero errors.</p>
              
              <div class="row g-3">
                <div class="col-6 d-flex align-items-center gap-2">
                  <span class="material-icons text-success">check_circle</span>
                  <span class="fw-medium text-secondary">NABL Accredited</span>
                </div>
                <div class="col-6 d-flex align-items-center gap-2">
                  <span class="material-icons text-success">check_circle</span>
                  <span class="fw-medium text-secondary">Home Sample Collection</span>
                </div>
                <div class="col-6 d-flex align-items-center gap-2">
                  <span class="material-icons text-success">check_circle</span>
                  <span class="fw-medium text-secondary">24x7 Digital Support</span>
                </div>
                <div class="col-6 d-flex align-items-center gap-2">
                  <span class="material-icons text-success">check_circle</span>
                  <span class="fw-medium text-secondary">Secured PDF Reports</span>
                </div>
              </div>
            </div>
            
            <div class="col-lg-6">
              <div class="glass-card bg-primary-dark text-white p-4 shadow-xl border-0 rounded-4">
                <span class="material-icons fs-1 mb-3 text-warning">health_and_safety</span>
                <h3 class="fw-bold mb-3">NABL Accredited Standard Lab</h3>
                <p class="mb-4">Our lab adheres to rigorous Quality Control standard protocols (Internal & External Quality Assessments) so you always get 100% reliable clinical test metrics.</p>
                <div class="border-top border-white-50 pt-3 d-flex align-items-center justify-content-between">
                  <span class="fw-medium text-white-50">Certificate: NABL-QC-901</span>
                  <span class="badge bg-white text-primary">ISO 15189:2022</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Contact Section -->
      <section id="contact" class="py-5">
        <div class="container py-4">
          <div class="row g-5">
            <div class="col-lg-5 text-start">
              <h2 class="fw-bold">Get in Touch</h2>
              <p class="text-secondary mb-4">Have queries regarding a diagnostic report or need custom booking assistance? Drop us a message.</p>
              
              <div class="d-flex gap-3 align-items-center mb-3">
                <span class="material-icons text-primary fs-3">phone</span>
                <div>
                  <span class="text-muted small d-block">Helpline</span>
                  <span class="fw-bold">1800-200-NEOLEB</span>
                </div>
              </div>

              <div class="d-flex gap-3 align-items-center mb-3">
                <span class="material-icons text-primary fs-3">email</span>
                <div>
                  <span class="text-muted small d-block">Email Support</span>
                  <span class="fw-bold">help&#64;neolaboratory.lab</span>
                </div>
              </div>

              <div class="d-flex gap-3 align-items-center">
                <span class="material-icons text-primary fs-3">place</span>
                <div>
                  <span class="text-muted small d-block">Headquarters</span>
                  <span class="fw-bold">101 Healthcare Avenue, New York</span>
                </div>
              </div>
            </div>
            
            <div class="col-lg-7">
              <div class="glass-card shadow-lg p-4">
                <h4 class="fw-bold mb-3">Send Message</h4>
                <form (submit)="sendMessage($event)">
                  <div class="row g-3 text-start">
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">Your Name</label>
                      <input type="text" [(ngModel)]="contactForm.name" name="name" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label small fw-medium">Email Address</label>
                      <input type="email" [(ngModel)]="contactForm.email" name="email" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-12">
                      <label class="form-label small fw-medium">Subject</label>
                      <input type="text" [(ngModel)]="contactForm.subject" name="subject" class="form-control bg-transparent" required />
                    </div>
                    <div class="col-12">
                      <label class="form-label small fw-medium">Message Body</label>
                      <textarea [(ngModel)]="contactForm.message" name="message" rows="4" class="form-control bg-transparent" required></textarea>
                    </div>
                    <div class="col-12 text-end">
                      <button type="submit" class="btn btn-primary-custom rounded-pill px-5">Send Message</button>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Footer -->
      <footer class="bg-dark text-white-50 py-5">
        <div class="container">
          <div class="row g-4 mb-4 text-start">
            <div class="col-md-4">
              <a class="d-flex align-items-center fw-bold text-white fs-3 mb-3 text-decoration-none" href="#">
                <span class="material-icons me-2 text-primary fs-2">biotech</span>
                Neo Laboratory
              </a>
              <p>Neo Laboratory is dedicated to providing high-quality, precise pathology reports using advanced laboratory automation and secure online scheduling.</p>
            </div>
            <div class="col-md-4">
              <h5 class="text-white fw-bold mb-3">Quick Links</h5>
              <ul class="list-unstyled gap-2 d-grid">
                <li><a href="#home" class="text-white-50 text-decoration-none">Home</a></li>
                <li><a href="#tests" class="text-white-50 text-decoration-none">Tests & Packages</a></li>
                <li><a href="#about" class="text-white-50 text-decoration-none">About Us</a></li>
                <li><a href="#contact" class="text-white-50 text-decoration-none">Contact Us</a></li>
              </ul>
            </div>
            <div class="col-md-4">
              <h5 class="text-white fw-bold mb-3">Accreditation</h5>
              <p>NABL (ISO 15189:2022) Certified. Registered Pathology Center with Central Health Department.</p>
              <div class="d-flex gap-2">
                <span class="badge bg-secondary">NABL Certified</span>
                <span class="badge bg-secondary">ISO 15189</span>
              </div>
            </div>
          </div>
          <hr class="border-secondary">
          <div class="d-flex flex-column flex-md-row justify-content-between align-items-center small">
            <span>&copy; 2026 Neo Laboratory Network. All rights reserved.</span>
            <div class="d-flex gap-3 mt-3 mt-md-0">
              <a href="#" class="text-white-50 text-decoration-none">Privacy Policy</a>
              <a href="#" class="text-white-50 text-decoration-none">Terms of Service</a>
            </div>
          </div>
        </div>
      </footer>

    </div>
  `,
  styles: [`
    .bg-gradient-hero {
      background: radial-gradient(circle at 80% 20%, rgba(59, 130, 246, 0.1) 0%, transparent 50%),
                  radial-gradient(circle at 20% 80%, rgba(139, 92, 246, 0.08) 0%, transparent 50%),
                  linear-gradient(180deg, var(--bg-color) 0%, rgba(59, 130, 246, 0.02) 100%);
      position: relative;
      overflow: hidden;
    }
    .hero-glow-orb {
      position: absolute;
      width: 450px;
      height: 450px;
      background: radial-gradient(circle, rgba(37, 99, 235, 0.08) 0%, transparent 70%);
      top: -150px;
      right: -100px;
      z-index: 0;
      pointer-events: none;
    }
    .hero-title {
      font-size: 3.5rem;
      letter-spacing: -0.03em;
      line-height: 1.15;
    }
    .text-primary-gradient {
      background: linear-gradient(90deg, #2563eb, #8b5cf6);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    @media (max-width: 768px) {
      .hero-title {
        font-size: 2.25rem;
      }
    }
    .glass-search-container {
      background: rgba(255, 255, 255, 0.75) !important;
      backdrop-filter: blur(20px) !important;
      -webkit-backdrop-filter: blur(20px) !important;
      border: 1px solid rgba(255, 255, 255, 0.4) !important;
      border-radius: 50px !important;
      padding: 6px 12px !important;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.04) !important;
      body.dark-theme & {
        background: rgba(30, 41, 59, 0.65) !important;
        border: 1px solid rgba(255, 255, 255, 0.08) !important;
        box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2) !important;
      }
    }
    .feature-icon-wrapper {
      width: 56px;
      height: 56px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 16px;
      background: rgba(37, 99, 235, 0.08);
      color: var(--primary-color);
      margin-bottom: 1.25rem;
    }
    .catalog-card {
      background: var(--card-bg) !important;
      border: 1px solid var(--card-border) !important;
      border-radius: 20px !important;
      padding: 1.5rem !important;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
      box-shadow: 0 4px 20px rgba(0,0,0,0.02) !important;
      body.dark-theme & {
        box-shadow: 0 4px 20px rgba(0,0,0,0.2) !important;
      }
      &:hover {
        transform: translateY(-5px);
        box-shadow: 0 12px 30px rgba(37, 99, 235, 0.08) !important;
        border-color: rgba(37, 99, 235, 0.3) !important;
      }
    }
    .package-card {
      background: linear-gradient(145deg, var(--card-bg) 0%, rgba(37, 99, 235, 0.02) 100%) !important;
      border: 1px solid var(--card-border) !important;
      border-radius: 24px !important;
      padding: 2rem !important;
      position: relative;
      overflow: hidden;
      transition: all 0.3s ease !important;
      &:hover {
        transform: translateY(-6px);
        border-color: var(--primary-color) !important;
        box-shadow: 0 15px 35px rgba(37, 99, 235, 0.12) !important;
      }
    }
    .bg-primary-gradient-card {
      background: linear-gradient(135deg, #2563eb 0%, #7c3aed 100%);
    }
    .bg-primary-dark {
      background-color: #0f172a !important;
      border: 1px solid rgba(255, 255, 255, 0.08) !important;
    }
    .max-width-600 {
      max-width: 600px;
    }
    .cursor-pointer {
      cursor: pointer;
    }
    .benefit-box {
      background: var(--card-bg) !important;
      border: 1px solid var(--card-border) !important;
      border-radius: 12px;
      padding: 1rem;
      transition: all 0.2s ease;
      body.dark-theme & {
        background: rgba(255, 255, 255, 0.03) !important;
      }
    }
  `]
})
export class LandingPageComponent implements OnInit {
  auth = inject(AuthService);
  private api = inject(ApiService);

  isMobileMenuOpen = false;
  isDarkMode = signal(false);
  searchQuery = '';
  
  tests: TestCatalogItem[] = [];
  filteredTests: TestCatalogItem[] = [];
  packages: PackageCatalogItem[] = [];

  contactForm = {
    name: '',
    email: '',
    subject: '',
    message: ''
  };

  ngOnInit(): void {
    this.loadCatalog();
    this.initTheme();
  }

  loadCatalog(): void {
    this.api.get<TestCatalogItem[]>('test').subscribe({
      next: (data) => {
        this.tests = data.filter(t => t.price > 0);
        this.filteredTests = [...this.tests];
      }
    });

    this.api.get<PackageCatalogItem[]>('test/packages').subscribe({
      next: (data) => {
        this.packages = data;
      }
    });
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

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMenu(): void {
    this.isMobileMenuOpen = false;
  }

  setSearch(val: string): void {
    this.searchQuery = val;
    this.filterTests();
  }

  filterTests(): void {
    if (!this.searchQuery) {
      this.filteredTests = [...this.tests];
      return;
    }
    const query = this.searchQuery.toLowerCase();
    this.filteredTests = this.tests.filter(t => 
      t.name.toLowerCase().includes(query) || 
      t.code.toLowerCase().includes(query) ||
      t.categoryName.toLowerCase().includes(query)
    );
  }

  scrollToTests(): void {
    const el = document.getElementById('tests');
    if (el) el.scrollIntoView({ behavior: 'smooth' });
  }

  dashboardLink(): string {
    const role = this.auth.userRole();
    if (role === 'Admin') return '/admin/dashboard';
    if (role === 'Staff') return '/staff/dashboard';
    return '/patient/dashboard';
  }

  sendMessage(event: Event): void {
    event.preventDefault();
    alert(`Thank you, ${this.contactForm.name}! Your message has been sent successfully.`);
    this.contactForm = { name: '', email: '', subject: '', message: '' };
  }
}
