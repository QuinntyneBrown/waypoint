import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Button, ButtonVariant } from './button';

@Component({
  imports: [Button],
  template: `
    <button
      w-button
      [variant]="variant()"
      [size]="size()"
      [block]="block()"
      [busy]="busy()"
      (click)="clicks.set(clicks() + 1)"
    >
      Save changes
    </button>
    <a w-button variant="ghost" size="sm" href="/mentor/youth">Back to my youth</a>
    <button w-button variant="ghost" iconOnly aria-label="Exit quiz" type="button">x</button>
  `,
})
class Host {
  variant = signal<ButtonVariant>('primary');
  size = signal<'sm' | 'md' | 'lg'>('md');
  block = signal(false);
  busy = signal(false);
  clicks = signal(0);
}

async function setup() {
  await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
  const fixture = TestBed.createComponent(Host);
  await fixture.whenStable();
  const root = fixture.nativeElement as HTMLElement;
  return {
    fixture,
    host: fixture.componentInstance,
    button: root.querySelector('button')!,
    link: root.querySelector('a')!,
    icon: root.querySelectorAll('button')[1],
  };
}

describe('Button', () => {
  it('renders the base class and variant class and keeps its content', async () => {
    const { button } = await setup();
    expect(button.classList.contains('w-btn')).toBe(true);
    expect(button.classList.contains('w-btn--primary')).toBe(true);
    expect(button.textContent?.trim()).toBe('Save changes');
  });

  it('applies exactly one variant class, none for neutral', async () => {
    const { fixture, host, button } = await setup();
    for (const variant of ['secondary', 'ghost', 'subtle', 'danger', 'link'] as const) {
      host.variant.set(variant);
      await fixture.whenStable();
      expect(button.className).toContain(`w-btn--${variant}`);
      expect(
        button.className.match(/w-btn--(primary|secondary|ghost|subtle|danger|link)/g),
      ).toEqual([`w-btn--${variant}`]);
    }
    host.variant.set('neutral');
    await fixture.whenStable();
    expect(button.className).toBe('w-btn');
  });

  it('applies size and block classes', async () => {
    const { fixture, host, button } = await setup();
    host.size.set('sm');
    host.block.set(true);
    await fixture.whenStable();
    expect(button.classList.contains('w-btn--sm')).toBe(true);
    expect(button.classList.contains('w-btn--block')).toBe(true);
    host.size.set('lg');
    await fixture.whenStable();
    expect(button.classList.contains('w-btn--lg')).toBe(true);
    expect(button.classList.contains('w-btn--sm')).toBe(false);
  });

  it('styles an anchor like a button', async () => {
    const { link } = await setup();
    expect(link.classList.contains('w-btn')).toBe(true);
    expect(link.classList.contains('w-btn--ghost')).toBe(true);
    expect(link.classList.contains('w-btn--sm')).toBe(true);
  });

  it('marks an icon-only button', async () => {
    const { icon } = await setup();
    expect(icon.classList.contains('w-btn--icon')).toBe(true);
    expect(icon.getAttribute('aria-label')).toBe('Exit quiz');
  });

  it('is busy without being disabled, and ignores clicks while busy', async () => {
    const { fixture, host, button } = await setup();
    button.click();
    expect(host.clicks()).toBe(1);

    host.busy.set(true);
    await fixture.whenStable();
    expect(button.classList.contains('is-loading')).toBe(true);
    expect(button.getAttribute('aria-busy')).toBe('true');
    expect(button.disabled).toBe(false);
    button.click();
    expect(host.clicks()).toBe(1);

    host.busy.set(false);
    await fixture.whenStable();
    expect(button.getAttribute('aria-busy')).toBeNull();
    button.click();
    expect(host.clicks()).toBe(2);
  });
});
