import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Register } from './register';
import { environment } from '../../../../environments/environment';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [provideRouter([{ path: 'dashboard', component: Register }]), provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('is invalid when required fields are empty', () => {
    expect(component.form.valid).toBe(false);
  });

  it('rejects a malformed email address', () => {
    component.form.setValue({ name: 'Ada', email: 'not-an-email', password: 'Password123!' });
    expect(component.form.controls.email.valid).toBe(false);
  });

  it('rejects a password shorter than 8 characters', () => {
    component.form.setValue({ name: 'Ada', email: 'ada@example.com', password: 'short' });
    expect(component.form.controls.password.valid).toBe(false);
  });

  it('is valid once name, a well-formed email, and a long-enough password are provided', () => {
    component.form.setValue({ name: 'Ada', email: 'ada@example.com', password: 'Password123!' });
    expect(component.form.valid).toBe(true);
  });

  it('does not submit the form to the API when it is invalid', () => {
    component.submit();
    httpMock.expectNone(`${environment.apiUrl}/auth/register`);
  });

  it('submits the form to the API when it is valid', () => {
    component.form.setValue({ name: 'Ada', email: 'ada@example.com', password: 'Password123!' });
    component.submit();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/register`);
    expect(req.request.body).toEqual({ name: 'Ada', email: 'ada@example.com', password: 'Password123!' });

    req.flush({
      accessToken: 'a',
      accessTokenExpiresAt: new Date().toISOString(),
      refreshToken: 'r',
      user: {
        id: 1,
        name: 'Ada',
        email: 'ada@example.com',
        role: 'Employee',
        status: 'Active',
        departmentId: null,
        departmentName: null,
        createdAt: new Date().toISOString()
      }
    });
  });
});
