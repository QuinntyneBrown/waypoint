import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WMessageBubble, WMessageBubbleTime, WMessageThread } from './w-message-thread';

@Component({
  imports: [WMessageThread, WMessageBubble, WMessageBubbleTime],
  template: `
    <div w-message-thread class="thread-log" role="log" aria-live="polite">
      <article w-message-bubble [direction]="direction()" data-message-id="m1">
        <span>Hello</span>
        <time w-message-bubble-time datetime="2026-10-17T10:00:00Z">10:00</time>
      </article>
      <article w-message-bubble direction="in">Hi back</article>
    </div>
  `,
})
class Host {
  readonly direction = signal<'in' | 'out'>('out');
}

describe('WMessageThread', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const root = fixture.nativeElement as HTMLElement;
    return { fixture, root, bubbles: Array.from(root.querySelectorAll('article')) };
  }

  it('is a log of bubbles and keeps its live-region attributes', async () => {
    const { root, bubbles } = await render();
    const thread = root.querySelector('div')!;
    expect(thread.classList.contains('w-thread')).toBe(true);
    expect(thread.classList.contains('thread-log')).toBe(true);
    expect(thread.getAttribute('role')).toBe('log');
    expect(bubbles[0].classList.contains('w-bubble')).toBe(true);
    expect(bubbles[0].getAttribute('data-message-id')).toBe('m1');
  });

  it('aligns sent and received bubbles', async () => {
    const { fixture, bubbles } = await render();
    expect(bubbles[0].classList.contains('w-bubble--out')).toBe(true);
    expect(bubbles[1].classList.contains('w-bubble--in')).toBe(true);
    fixture.componentInstance.direction.set('in');
    await fixture.whenStable();
    expect(bubbles[0].classList.contains('w-bubble--in')).toBe(true);
    expect(bubbles[0].classList.contains('w-bubble--out')).toBe(false);
  });

  it('shows the time under the message', async () => {
    const { bubbles } = await render();
    const time = bubbles[0].querySelector('time')!;
    expect(time.classList.contains('w-bubble__time')).toBe(true);
    expect(time.getAttribute('datetime')).toBe('2026-10-17T10:00:00Z');
  });
});
