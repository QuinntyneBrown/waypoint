import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WQuizOption } from './w-quiz-option';

@Component({
  imports: [WQuizOption],
  template: `
    <label w-quiz-option [selected]="picked()">
      <input type="radio" name="q1" />
      <span>Option A</span>
    </label>
    <ul>
      <li w-quiz-option selected correct>Option B</li>
    </ul>
  `,
})
class Host {
  readonly picked = signal(false);
}

describe('WQuizOption', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const root = fixture.nativeElement as HTMLElement;
    return { fixture, label: root.querySelector('label')!, item: root.querySelector('li')! };
  }

  it('is an answer choice on a label or a list item and keeps its content', async () => {
    const { label, item } = await render();
    expect(label.classList.contains('w-quiz__opt')).toBe(true);
    expect(label.querySelector('input[type="radio"]')).not.toBeNull();
    expect(item.classList.contains('w-quiz__opt')).toBe(true);
    expect(item.textContent).toBe('Option B');
  });

  it('marks selected and correct choices', async () => {
    const { fixture, label, item } = await render();
    expect(label.classList.contains('is-selected')).toBe(false);
    fixture.componentInstance.picked.set(true);
    await fixture.whenStable();
    expect(label.classList.contains('is-selected')).toBe(true);
    expect(item.classList.contains('is-selected')).toBe(true);
    expect(item.classList.contains('is-correct')).toBe(true);
    expect(label.classList.contains('is-correct')).toBe(false);
  });
});
