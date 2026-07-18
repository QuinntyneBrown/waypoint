import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WuAuth } from './wu-auth';

describe('WuAuth', () => {
  let component: WuAuth;
  let fixture: ComponentFixture<WuAuth>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WuAuth],
    }).compileComponents();

    fixture = TestBed.createComponent(WuAuth);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
