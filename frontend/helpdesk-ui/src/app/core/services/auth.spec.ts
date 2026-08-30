import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Auth } from './auth';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/auth.model';

const mockUser = {
  id: 1,
  name: 'Ada Employee',
  email: 'ada@example.com',
  role: 'Employee' as const,
  status: 'Active' as const,
  departmentId: null,
  departmentName: null,
  createdAt: new Date().toISOString()
};

const mockResponse: AuthResponse = {
  accessToken: 'access-123',
  accessTokenExpiresAt: new Date(Date.now() + 3_600_000).toISOString(),
  refreshToken: 'refresh-456',
  user: mockUser
};

describe('Auth', () => {
  let service: Auth;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(Auth);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('starts unauthenticated when localStorage has no stored session', () => {
    expect(service.isAuthenticated()).toBe(false);
    expect(service.currentUser()).toBeNull();
  });

  it('stores the session and updates signals on successful login', () => {
    service.login({ email: mockUser.email, password: 'Password123!' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentUser()?.name).toBe('Ada Employee');
    expect(service.getAccessToken()).toBe('access-123');
    expect(service.getRefreshToken()).toBe('refresh-456');
  });

  it('clears the session on logout', () => {
    service.login({ email: mockUser.email, password: 'Password123!' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(mockResponse);

    service.logout();
    httpMock.expectOne(`${environment.apiUrl}/auth/logout`).flush({});

    expect(service.isAuthenticated()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(service.getAccessToken()).toBeNull();
    expect(localStorage.getItem('helpdesk.user')).toBeNull();
  });

  it('hasRole is false when nobody is logged in', () => {
    expect(service.hasRole('Admin', 'Agent')).toBe(false);
  });

  it('hasRole reflects the logged-in users role', () => {
    service.login({ email: mockUser.email, password: 'Password123!' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(mockResponse);

    expect(service.hasRole('Employee')).toBe(true);
    expect(service.hasRole('Agent', 'Admin')).toBe(false);
  });
});
