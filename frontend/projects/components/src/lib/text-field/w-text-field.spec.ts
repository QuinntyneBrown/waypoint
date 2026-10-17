import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WTextField } from './w-text-field';

@Component({
  imports: [WTextField],
  template: `<input w-text-field type="email" placeholder="name@school.org" />`,
})
class Host {}

describe('WTextField', () => {
  it('styles the native input with the w-input class and keeps its attributes', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = (fixture.nativeElement as HTMLElement).querySelector('input')!;
    expect(el.classList.contains('w-input')).toBe(true);
    expect(el.hasAttribute('w-text-field')).toBe(true);
  });
});
