import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WuUi } from './wu-ui';

describe('WuUi', () => {
  let component: WuUi;
  let fixture: ComponentFixture<WuUi>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WuUi],
    }).compileComponents();

    fixture = TestBed.createComponent(WuUi);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
