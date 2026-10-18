import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Stat } from './stat';
import { StatDelta } from './stat-delta';
import { StatLabel } from './stat-label';
import { StatValue } from './stat-value';

@Component({
  imports: [Stat, StatLabel, StatValue, StatDelta],
  template: `
    <div w-stat>
      <div w-stat-label>Pending reviews</div>
      <div w-stat-value>4</div>
      <div w-stat-delta [trend]="trend()">2 overdue</div>
    </div>
    <div class="w-kpi">
      <span w-stat-label>check-ins</span>
      <strong w-stat-value>9</strong>
    </div>
  `,
})
class Host {
  readonly trend = signal<'up' | 'down' | ''>('down');
}

describe('Stat', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return { fixture, root: fixture.nativeElement as HTMLElement };
  }

  it('shows a labelled value with an optional delta', async () => {
    const { root } = await render();
    const stat = root.querySelector('.w-statcard')!;
    expect(stat.querySelector('.w-statcard__label')?.textContent).toBe('Pending reviews');
    expect(stat.querySelector('.w-statcard__value')?.textContent).toBe('4');
    expect(stat.querySelector('.w-statcard__delta')?.textContent).toBe('2 overdue');
  });

  it('marks the delta direction', async () => {
    const { fixture, root } = await render();
    const delta = root.querySelector('.w-statcard__delta')!;
    expect(delta.classList.contains('is-down')).toBe(true);
    fixture.componentInstance.trend.set('up');
    await fixture.whenStable();
    expect(delta.classList.contains('is-up')).toBe(true);
    expect(delta.classList.contains('is-down')).toBe(false);
  });

  it('lets the label and value stand alone inside other layouts', async () => {
    const { root } = await render();
    const kpi = root.querySelector('.w-kpi')!;
    expect(kpi.querySelector('span.w-statcard__label')?.textContent).toBe('check-ins');
    expect(kpi.querySelector('strong.w-statcard__value')?.textContent).toBe('9');
  });
});
