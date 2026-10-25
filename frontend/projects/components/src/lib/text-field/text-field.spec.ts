import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { TextField } from './text-field';

@Component({
  imports: [TextField],
  template: `<input ph-text-field type="email" placeholder="name@school.org" />`,
})
class Host {}

describe('TextField', () => {
  it('styles the native input with the ph-input class and keeps its attributes', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = (fixture.nativeElement as HTMLElement).querySelector('input')!;
    expect(el.classList.contains('ph-input')).toBe(true);
    expect(el.hasAttribute('ph-text-field')).toBe(true);
  });
});
