import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WSkeleton } from './w-skeleton';

@Component({
  imports: [WSkeleton],
  template: `
    <span w-skeleton></span>
    <span w-skeleton variant="title"></span>
    <span w-skeleton variant="block"></span>
  `,
})
class Host {}

describe('WSkeleton', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return Array.from((fixture.nativeElement as HTMLElement).children) as HTMLElement[];
  }

  it('is a plain placeholder by default', async () => {
    const [plain] = await render();
    expect(plain.className).toBe('w-skeleton');
  });

  it('applies the shape variants', async () => {
    const [, title, block] = await render();
    expect(title.classList.contains('w-skeleton--title')).toBe(true);
    expect(block.classList.contains('w-skeleton--block')).toBe(true);
  });
});
