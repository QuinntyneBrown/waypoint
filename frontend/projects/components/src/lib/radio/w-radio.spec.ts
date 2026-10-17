import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WRadio, WRadioBox } from './w-radio';

@Component({
  imports: [WRadio, WRadioBox],
  template: `
    <label w-radio>
      <input type="radio" name="rsvp" value="going" />
      <span w-radio-box></span>
      <span>Going</span>
    </label>
  `,
})
class Host {}

describe('WRadio', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('wraps a native radio and keeps its label text', async () => {
    const label = (await render()).querySelector('label')!;
    expect(label.classList.contains('w-radio')).toBe(true);
    expect(label.querySelector('input[type="radio"]')).not.toBeNull();
    expect(label.textContent).toContain('Going');
  });

  it('renders the box next to the input', async () => {
    const box = (await render()).querySelector('input + span')!;
    expect(box.classList.contains('w-radio__box')).toBe(true);
  });
});
