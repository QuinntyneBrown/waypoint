import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Checkbox } from './checkbox';
import { CheckboxBox } from './checkbox-box';

@Component({
  imports: [Checkbox, CheckboxBox],
  template: `
    <label w-checkbox>
      <input type="checkbox" />
      <span w-checkbox-box></span>
      <span>Keep me signed in</span>
    </label>
  `,
})
class Host {}

describe('Checkbox', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('wraps a native checkbox and keeps its label text', async () => {
    const label = (await render()).querySelector('label')!;
    expect(label.classList.contains('w-check')).toBe(true);
    expect(label.querySelector('input[type="checkbox"]')).not.toBeNull();
    expect(label.textContent).toContain('Keep me signed in');
  });

  it('renders the box next to the input', async () => {
    const box = (await render()).querySelector('input + span')!;
    expect(box.classList.contains('w-check__box')).toBe(true);
  });
});
