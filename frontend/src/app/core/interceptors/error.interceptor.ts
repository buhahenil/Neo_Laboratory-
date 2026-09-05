import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const toast = inject(NotificationService);

  return next(req).pipe(
    catchError(err => {
      // 401 Unauthorized or 403 Forbidden - Auto Logout (exclude login page)
      if ([401, 403].includes(err.status) && !req.url.includes('auth/login')) {
        authService.logout();
        toast.showError('Session expired or access denied. Please log in.');
      } else {
        let errorMessage = 'An unexpected error occurred.';
        if (err.error) {
          if (typeof err.error === 'string') {
            errorMessage = err.error;
          } else if (err.error.Message) {
            errorMessage = err.error.Message;
          } else if (err.error.message) {
            errorMessage = err.error.message;
          } else if (err.error.errors) {
            const errorList = [];
            for (const key in err.error.errors) {
              if (err.error.errors.hasOwnProperty(key)) {
                const messages = err.error.errors[key];
                if (Array.isArray(messages)) {
                  errorList.push(...messages);
                } else if (typeof messages === 'string') {
                  errorList.push(messages);
                }
              }
            }
            if (errorList.length > 0) {
              errorMessage = errorList.join(' ');
            }
          } else if (err.error.Detailed) {
            errorMessage = err.error.Detailed;
          }
        } else if (err.message) {
          errorMessage = err.message;
        }
        toast.showError(errorMessage);
      }
      return throwError(() => err);
    })
  );
};
