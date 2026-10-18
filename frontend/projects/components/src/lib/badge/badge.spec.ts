import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Badge } from './badge';

@Component({
  imports: [Badge],
  template: `
    <span w-badge>Draft</span>
    <span w-badge [variant]="variant()">Status</span>
    <span w-badge variant="danger">Overloaded</span>
    <span w-badge variant="info" dot>New</span>
  `,
})
class Host {
  readonly variant = signal<'success' | ''>('success');
}

describe('Badge', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const badges = Array.from((fixture.nativeElement as HTMLElement).children) as HTMLElement[];
    return { fixture, badges };
  }

  it('is a neutral badge by default and keeps its text', async () => {
    const [plain] = (await render()).badges;
    expect(plain.className).toBe('w-badge');
    expect(plain.textContent).toBe('Draft');
  });

  it('applies and removes a tone variant', async () => {
    const { fixture, badges } = await render();
    expect(badges[1].classList.contains('w-badge--success')).toBe(true);
    fixture.componentInstance.variant.set('');
    await fixture.whenStable();
    expect(badges[1].classList.contains('w-badge--success')).toBe(false);
  });

  it('supports danger and the dot', async () => {
    const { badges } = await render();
    expect(badges[2].classList.contains('w-badge--danger')).toBe(true);
    expect(badges[3].classList.contains('w-badge--info')).toBe(true);
    expect(badges[3].classList.contains('w-badge--dot')).toBe(true);
  });
});
