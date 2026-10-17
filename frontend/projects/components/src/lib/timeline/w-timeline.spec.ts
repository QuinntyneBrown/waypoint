import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WTimeline, WTimelineItem } from './w-timeline';

@Component({
  imports: [WTimeline, WTimelineItem],
  template: `
    <div w-timeline>
      <article w-timeline-item done>First</article>
      <article w-timeline-item [current]="current()" data-testid="current-module">Second</article>
      <article w-timeline-item>Third</article>
    </div>
  `,
})
class Host {
  readonly current = signal(true);
}

describe('WTimeline', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const items = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('article'),
    ) as HTMLElement[];
    return { fixture, root: fixture.nativeElement as HTMLElement, items };
  }

  it('lists steps along a vertical line and keeps their content', async () => {
    const { root, items } = await render();
    expect(root.querySelector('div')?.classList.contains('w-timeline')).toBe(true);
    expect(items.map((i) => i.textContent)).toEqual(['First', 'Second', 'Third']);
    expect(items[0].classList.contains('w-timeline__item')).toBe(true);
    expect(items[1].getAttribute('data-testid')).toBe('current-module');
  });

  it('marks done and current steps', async () => {
    const { fixture, items } = await render();
    expect(items.map((i) => i.classList.contains('is-done'))).toEqual([true, false, false]);
    expect(items.map((i) => i.classList.contains('is-current'))).toEqual([false, true, false]);
    fixture.componentInstance.current.set(false);
    await fixture.whenStable();
    expect(items[1].classList.contains('is-current')).toBe(false);
  });
});
