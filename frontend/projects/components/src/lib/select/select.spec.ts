import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Select } from './select';

@Component({
  imports: [Select],
  template: `<select ph-select>
    <option>All youth</option>
  </select>`,
})
class Host {}

describe('Select', () => {
  it('styles the native select with the ph-select class and keeps its attributes', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = (fixture.nativeElement as HTMLElement).querySelector('select')!;
    expect(el.classList.contains('ph-select')).toBe(true);
    expect(el.hasAttribute('ph-select')).toBe(true);
  });
});
