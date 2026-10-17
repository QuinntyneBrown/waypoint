import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WSelect } from './w-select';

@Component({
  imports: [WSelect],
  template: `<select w-select>
    <option>All youth</option>
  </select>`,
})
class Host {}

describe('WSelect', () => {
  it('styles the native select with the w-select class and keeps its attributes', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = (fixture.nativeElement as HTMLElement).querySelector('select')!;
    expect(el.classList.contains('w-select')).toBe(true);
    expect(el.hasAttribute('w-select')).toBe(true);
  });
});
