import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ErrorText } from './error-text';
import { FormField } from './form-field';
import { Help } from './help';
import { Label } from './label';
import { LabelOptional } from './label-optional';
import { LabelRequired } from './label-required';

@Component({
  imports: [FormField, Label, LabelRequired, LabelOptional, Help, ErrorText],
  template: `
    <div w-form-field [invalid]="invalid()">
      <label w-label for="email">Email <span w-label-required>*</span></label>
      <input id="email" />
      <p w-help>Use your school address.</p>
      <p w-error-text>Enter a valid email address.</p>
    </div>
    <label w-label for="church">Church <span w-label-optional>(optional)</span></label>
  `,
})
class Host {
  invalid = signal(false);
}

describe('form field parts', () => {
  async function setup() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return { fixture, root: fixture.nativeElement as HTMLElement };
  }

  it('applies the design-system classes to each part', async () => {
    const { root } = await setup();
    expect(root.querySelector('div')!.classList.contains('w-field')).toBe(true);
    expect(root.querySelector('label')!.classList.contains('w-label')).toBe(true);
    expect(root.querySelector('span')!.classList.contains('w-label__req')).toBe(true);
    expect(root.querySelectorAll('span')[1].classList.contains('w-label__opt')).toBe(true);
    expect(root.querySelector('p')!.classList.contains('w-help')).toBe(true);
    expect(root.querySelectorAll('p')[1].classList.contains('w-error-text')).toBe(true);
  });

  it('marks the field in error when invalid', async () => {
    const { fixture, root } = await setup();
    const field = root.querySelector('div')!;
    expect(field.classList.contains('is-error')).toBe(false);
    fixture.componentInstance.invalid.set(true);
    await fixture.whenStable();
    expect(field.classList.contains('is-error')).toBe(true);
  });
});
