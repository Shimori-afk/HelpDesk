import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Ticket } from './ticket';

describe('Ticket', () => {
  let service: Ticket;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(Ticket);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
