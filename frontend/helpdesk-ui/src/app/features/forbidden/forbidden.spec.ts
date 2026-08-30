import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Forbidden } from './forbidden';

describe('Forbidden', () => {
  let component: Forbidden;
  let fixture: ComponentFixture<Forbidden>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Forbidden],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(Forbidden);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
