import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { throwError } from 'rxjs';
import { errorInterceptor } from './error-interceptor';

describe('errorInterceptor', () => {
  const interceptor: HttpInterceptorFn = (req, next) =>
    TestBed.runInInjectionContext(() => errorInterceptor(req, next));

  let openSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    openSpy = vi.fn();
    TestBed.configureTestingModule({
      providers: [{ provide: MatSnackBar, useValue: { open: openSpy } }]
    });
  });

  it('shows a snackbar with the API error title on a non-401 error', () => {
    const req = new HttpRequest('GET', '/api/tickets');
    const next: HttpHandlerFn = () =>
      throwError(() => new HttpErrorResponse({ status: 409, error: { title: 'Conflict occurred' } }));

    interceptor(req, next).subscribe({ error: () => undefined });

    expect(openSpy).toHaveBeenCalledWith('Conflict occurred', 'Dismiss', { duration: 5000 });
  });

  it('does not show a snackbar for a 401 (handled by the auth interceptor instead)', () => {
    const req = new HttpRequest('GET', '/api/tickets');
    const next: HttpHandlerFn = () => throwError(() => new HttpErrorResponse({ status: 401 }));

    interceptor(req, next).subscribe({ error: () => undefined });

    expect(openSpy).not.toHaveBeenCalled();
  });

  it('falls back to a generic message when the API error has no title', () => {
    const req = new HttpRequest('GET', '/api/tickets');
    const next: HttpHandlerFn = () => throwError(() => new HttpErrorResponse({ status: 500 }));

    interceptor(req, next).subscribe({ error: () => undefined });

    expect(openSpy).toHaveBeenCalledWith('Something went wrong. Please try again.', 'Dismiss', { duration: 5000 });
  });
});
