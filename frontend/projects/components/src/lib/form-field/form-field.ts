import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
  booleanAttribute,
  input,
} from '@angular/core';

/** Groups a control with its label, help text and error message. */
@Component({
  selector: 'div[w-form-field], fieldset[w-form-field]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-field', '[class.is-error]': 'invalid()' },
  template: '<ng-content />',
  styles: `
    .w-field {
      display: block;
      margin-bottom: var(--w-s5);
    }
    .w-field.is-error .w-input,
    .w-field.is-error .w-select,
    .w-field.is-error .w-textarea {
      border-color: var(--w-error);
      box-shadow: 0 0 0 3px var(--w-error-bg);
    }
  `,
})
export class FormField {
  readonly invalid = input(false, { transform: booleanAttribute });
}

/** Names a control. Bind it with `for`/`id`. */
@Component({
  selector: 'label[w-label], span[w-label], legend[w-label]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-label' },
  template: '<ng-content />',
  styles: `
    .w-label {
      display: block;
      font-weight: 600;
      font-size: 0.85rem;
      margin-bottom: 6px;
      color: var(--w-ink);
    }
  `,
})
export class Label {}

/** Red "required" marker inside a label. Decorative: the control has `required`. */
@Component({
  selector: 'span[w-label-required]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-label__req' },
  template: '<ng-content />',
  styles: `
    .w-label__req {
      color: var(--w-error);
    }
  `,
})
export class LabelRequired {}

/** Muted "(optional)" marker inside a label. */
@Component({
  selector: 'span[w-label-optional]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-label__opt' },
  template: '<ng-content />',
  styles: `
    .w-label__opt {
      color: var(--w-muted);
      font-weight: 400;
    }
  `,
})
export class LabelOptional {}

/** Muted caption under a control. Link it with `aria-describedby`. */
@Component({
  selector: 'p[w-help], div[w-help], span[w-help]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-help' },
  template: '<ng-content />',
  styles: `
    .w-help {
      font-size: 0.8rem;
      color: var(--w-muted);
      margin-top: 5px;
    }
  `,
})
export class Help {}

/** Red message under a control. Render it only while the control is invalid. */
@Component({
  selector: 'p[w-error-text], div[w-error-text]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  host: { class: 'w-error-text' },
  template: '<ng-content />',
  styles: `
    .w-error-text {
      font-size: 0.8rem;
      color: var(--w-error);
      margin-top: 5px;
      font-weight: 500;
    }
  `,
})
export class ErrorText {}
