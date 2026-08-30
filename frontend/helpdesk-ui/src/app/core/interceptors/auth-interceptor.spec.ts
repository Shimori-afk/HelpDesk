import { TestBed } from '@angular/core/testing';
import {
  HttpErrorResponse,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
  HttpResponse,
  provideHttpClient
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { authInterceptor } from './auth-interceptor';
import { environment } from '../../../environments/environment';

const mockUser = {
  id: 1,
  name: 'Ada Employee',
  email: 'ada@example.com',
  role: 'Employee',
  status: 'Active',
  departmentId: null,
  departmentName: null,
  createdAt: new Date().toISOString()
};

describe('authInterceptor', () => {
  const interceptor: HttpInterceptorFn = (req, next) =>
    TestBed.runInInjectionContext(() => authInterceptor(req, next));

  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('attaches the bearer token to an authenticated request', () => {
    localStorage.setItem('helpdesk.accessToken', 'token-123');
    const req = new HttpRequest('GET', `${environment.apiUrl}/tickets`);
    const next: HttpHandlerFn = (r) => {
      expect(r.headers.get('Authorization')).toBe('Bearer token-123');
      return of(new HttpResponse({ status: 200 }));
    };

    interceptor(req, next).subscribe();
  });

  it('does not attach a token to the login request', () => {
    localStorage.setItem('helpdesk.accessToken', 'token-123');
    const req = new HttpRequest('POST', `${environment.apiUrl}/auth/login`, {});
    const next: HttpHandlerFn = (r) => {
      expect(r.headers.has('Authorization')).toBe(false);
      return of(new HttpResponse({ status: 200 }));
    };

    interceptor(req, next).subscribe();
  });

  it('refreshes the token and retries the original request after a 401', () => {
    localStorage.setItem('helpdesk.accessToken', 'expired-token');
    localStorage.setItem('helpdesk.refreshToken', 'refresh-token');

    const req = new HttpRequest('GET', `${environment.apiUrl}/tickets`);
    let callCount = 0;
    const next: HttpHandlerFn = () => {
      callCount++;
      return callCount === 1
        ? throwError(() => new HttpErrorResponse({ status: 401 }))
        : of(new HttpResponse({ status: 200 }));
    };

    let completed = false;
    interceptor(req, next).subscribe(() => {
      completed = true;
    });

    httpMock.expectOne(`${environment.apiUrl}/auth/refresh`).flush({
      accessToken: 'new-access-token',
      accessTokenExpiresAt: new Date().toISOString(),
      refreshToken: 'new-refresh-token',
      user: mockUser
    });

    expect(completed).toBe(true);
    expect(callCount).toBe(2);
    expect(localStorage.getItem('helpdesk.accessToken')).toBe('new-access-token');
  });

  it('logs out and redirects to /login when the refresh itself fails', () => {
    localStorage.setItem('helpdesk.accessToken', 'expired-token');
    localStorage.setItem('helpdesk.refreshToken', 'refresh-token');

    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate');

    const req = new HttpRequest('GET', `${environment.apiUrl}/tickets`);
    const next: HttpHandlerFn = () => throwError(() => new HttpErrorResponse({ status: 401 }));

    let erroredOut = false;
    interceptor(req, next).subscribe({
      error: () => {
        erroredOut = true;
      }
    });

    httpMock
      .expectOne(`${environment.apiUrl}/auth/refresh`)
      .flush({ title: 'Invalid refresh token' }, { status: 401, statusText: 'Unauthorized' });
    httpMock.expectOne(`${environment.apiUrl}/auth/logout`).flush({});

    expect(erroredOut).toBe(true);
    expect(localStorage.getItem('helpdesk.accessToken')).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });
});
