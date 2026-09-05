import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing-page/landing-page').then(c => c.LandingPageComponent)
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login').then(c => c.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register/register').then(c => c.RegisterComponent)
  },
  {
    path: 'auth/forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password').then(c => c.ForgotPasswordComponent)
  },
  {
    path: 'auth/reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password').then(c => c.ResetPasswordComponent)
  },
  {
    path: 'patient/dashboard',
    loadComponent: () => import('./features/patient/patient-dashboard').then(c => c.PatientDashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Patient'] }
  },
  {
    path: 'patient/book',
    loadComponent: () => import('./features/patient/booking-wizard').then(c => c.BookingWizardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Patient'] }
  },
  {
    path: 'admin/dashboard',
    loadComponent: () => import('./features/admin/admin-dashboard').then(c => c.AdminDashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'staff/dashboard',
    loadComponent: () => import('./features/staff/staff-dashboard').then(c => c.StaffDashboardComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Staff'] }
  },
  {
    path: '**',
    redirectTo: ''
  }
];
