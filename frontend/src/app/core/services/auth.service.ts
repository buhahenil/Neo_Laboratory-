import { Injectable, inject, signal, computed } from '@angular/core';
import { ApiService } from './api.service';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

export interface AuthUser {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  userId: number;
  patientId?: number;
  staffId?: number;
  doctorId?: number;
  branchId?: number;
  branchName?: string;
  designation?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private api = inject(ApiService);
  private router = inject(Router);

  // Angular Signals for reactive state
  currentUser = signal<AuthUser | null>(null);
  
  isAuthenticated = computed(() => this.currentUser() !== null);
  
  userRole = computed(() => this.currentUser()?.role || '');

  constructor() {
    this.loadUserFromStorage();
  }

  login(credentials: { email: string; password: string; branchId?: number }): Observable<AuthUser> {
    return this.api.post<AuthUser>('auth/login', credentials).pipe(
      tap(user => {
        this.setCurrentUser(user);
      })
    );
  }

  register(userData: any): Observable<any> {
    return this.api.post('auth/register', userData);
  }

  forgotPassword(email: string): Observable<any> {
    return this.api.post('auth/forgot-password', { email });
  }

  resetPassword(payload: any): Observable<any> {
    return this.api.post('auth/reset-password', payload);
  }

  changePassword(payload: any): Observable<any> {
    return this.api.post('auth/change-password', payload);
  }

  logout(): void {
    localStorage.removeItem('lab_user');
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  private setCurrentUser(user: AuthUser): void {
    localStorage.setItem('lab_user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  private loadUserFromStorage(): void {
    const saved = localStorage.getItem('lab_user');
    if (saved) {
      try {
        const user = JSON.parse(saved) as AuthUser;
        this.currentUser.set(user);
      } catch {
        localStorage.removeItem('lab_user');
      }
    }
  }
}
