import { TestBed } from '@angular/core/testing';
import { CanActivateFn, provideRouter, UrlTree } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { authGuard } from './auth-guard';
import { Auth } from '../services/auth';

describe('authGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => authGuard(...guardParameters));

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    });
  });

  it('allows navigation when authenticated', () => {
    localStorage.setItem(
      'helpdesk.user',
      JSON.stringify({ id: 1, name: 'Test', email: 't@example.com', role: 'Employee' })
    );
    TestBed.inject(Auth);

    const result = executeGuard({} as never, { url: '/dashboard' } as never);

    expect(result).toBe(true);
  });

  it('redirects to /login when not authenticated', () => {
    const result = executeGuard({} as never, { url: '/dashboard' } as never);

    expect(result).toBeInstanceOf(UrlTree);
  });
});
