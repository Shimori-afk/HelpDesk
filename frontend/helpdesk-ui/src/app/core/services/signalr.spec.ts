import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Signalr } from './signalr';

describe('Signalr', () => {
  let service: Signalr;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(Signalr);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
