import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Progress } from './progress';
import { ProgressRing } from './progress-ring';
import { ProgressRingLabel } from './progress-ring-label';

@Component({
  imports: [Progress, ProgressRing, ProgressRingLabel],
  template: `
    <div ph-progress label="Lesson progress" [value]="value()"></div>
    <div ph-progress aria-hidden="true" [value]="40"></div>
    <div ph-progress-ring size="lg" [value]="72"><span ph-progress-ring-label>72%</span></div>
  `,
})
class Host {
  readonly value = signal(25);
}

describe('Progress', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const els = Array.from((fixture.nativeElement as HTMLElement).children) as HTMLElement[];
    return { fixture, els };
  }

  it('is a labelled progress bar that reports its value', async () => {
    const { fixture, els } = await render();
    const bar = els[0];
    expect(bar.classList.contains('ph-progress')).toBe(true);
    expect(bar.getAttribute('role')).toBe('progressbar');
    expect(bar.getAttribute('aria-label')).toBe('Lesson progress');
    expect(bar.getAttribute('aria-valuemin')).toBe('0');
    expect(bar.getAttribute('aria-valuemax')).toBe('100');
    expect(bar.getAttribute('aria-valuenow')).toBe('25');
    expect(bar.style.getPropertyValue('--val')).toBe('25%');
    expect(bar.querySelector('.ph-progress__fill')).not.toBeNull();

    fixture.componentInstance.value.set(60);
    await fixture.whenStable();
    expect(bar.getAttribute('aria-valuenow')).toBe('60');
    expect(bar.style.getPropertyValue('--val')).toBe('60%');
  });

  it('stays decorative without a label', async () => {
    const { els } = await render();
    expect(els[1].getAttribute('role')).toBeNull();
    expect(els[1].getAttribute('aria-hidden')).toBe('true');
    expect(els[1].style.getPropertyValue('--val')).toBe('40%');
  });

  it('draws a ring with its label', async () => {
    const { els } = await render();
    const ring = els[2];
    expect(ring.classList.contains('ph-ring')).toBe(true);
    expect(ring.classList.contains('ph-ring--lg')).toBe(true);
    expect(ring.style.getPropertyValue('--val')).toBe('72');
    expect(ring.querySelector('.ph-ring__label')?.textContent).toBe('72%');
  });
});
