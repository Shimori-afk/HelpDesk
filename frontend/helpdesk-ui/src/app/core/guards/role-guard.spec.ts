import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, CanActivateFn, provideRouter, RouterStateSnapshot, UrlTree } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { roleGuard } from './role-guard';
import { Auth } from '../services/auth';

describe('roleGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => roleGuard(...guardParameters));

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    });
  });

  afterEach(() => localStorage.clear());

  function loginAs(role: string): void {
    localStorage.setItem(
      'helpdesk.user',
      JSON.stringify({ id: 1, name: 'Test User', email: 't@example.com', role })
    );
    TestBed.inject(Auth);
  }

  function routeWithRoles(roles?: string[]): ActivatedRouteSnapshot {
    return { data: roles ? { roles } : {} } as unknown as ActivatedRouteSnapshot;
  }

  it('allows navigation when the route declares no role restriction', () => {
    const result = executeGuard(routeWithRoles(), {} as RouterStateSnapshot);
    expect(result).toBe(true);
  });

  it('allows navigation when the user has one of the allowed roles', () => {
    loginAs('Admin');
    const result = executeGuard(routeWithRoles(['Admin']), {} as RouterStateSnapshot);
    expect(result).toBe(true);
  });

  it('redirects to /forbidden when the user lacks the required role', () => {
    loginAs('Employee');
    const result = executeGuard(routeWithRoles(['Admin']), {} as RouterStateSnapshot);
    expect(result).toBeInstanceOf(UrlTree);
  });

  it('redirects to /forbidden when nobody is logged in', () => {
    const result = executeGuard(routeWithRoles(['Admin']), {} as RouterStateSnapshot);
    expect(result).toBeInstanceOf(UrlTree);
  });
});
