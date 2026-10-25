import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Radio } from './radio';
import { RadioBox } from './radio-box';

@Component({
  imports: [Radio, RadioBox],
  template: `
    <label ph-radio>
      <input type="radio" name="rsvp" value="going" />
      <span ph-radio-box></span>
      <span>Going</span>
    </label>
  `,
})
class Host {}

describe('Radio', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('wraps a native radio and keeps its label text', async () => {
    const label = (await render()).querySelector('label')!;
    expect(label.classList.contains('ph-radio')).toBe(true);
    expect(label.querySelector('input[type="radio"]')).not.toBeNull();
    expect(label.textContent).toContain('Going');
  });

  it('renders the box next to the input', async () => {
    const box = (await render()).querySelector('input + span')!;
    expect(box.classList.contains('ph-radio__box')).toBe(true);
  });
});
