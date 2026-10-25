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
    <div ph-form-field [invalid]="invalid()">
      <label ph-label for="email">Email <span ph-label-required>*</span></label>
      <input id="email" />
      <p ph-help>Use your school address.</p>
      <p ph-error-text>Enter a valid email address.</p>
    </div>
    <label ph-label for="church">Church <span ph-label-optional>(optional)</span></label>
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
    expect(root.querySelector('div')!.classList.contains('ph-field')).toBe(true);
    expect(root.querySelector('label')!.classList.contains('ph-label')).toBe(true);
    expect(root.querySelector('span')!.classList.contains('ph-label__req')).toBe(true);
    expect(root.querySelectorAll('span')[1].classList.contains('ph-label__opt')).toBe(true);
    expect(root.querySelector('p')!.classList.contains('ph-help')).toBe(true);
    expect(root.querySelectorAll('p')[1].classList.contains('ph-error-text')).toBe(true);
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
