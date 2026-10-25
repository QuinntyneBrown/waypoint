import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Skeleton } from './skeleton';

@Component({
  imports: [Skeleton],
  template: `
    <span ph-skeleton></span>
    <span ph-skeleton variant="title"></span>
    <span ph-skeleton variant="block"></span>
  `,
})
class Host {}

describe('Skeleton', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return Array.from((fixture.nativeElement as HTMLElement).children) as HTMLElement[];
  }

  it('is a plain placeholder by default', async () => {
    const [plain] = await render();
    expect(plain.className).toBe('ph-skeleton');
  });

  it('applies the shape variants', async () => {
    const [, title, block] = await render();
    expect(title.classList.contains('ph-skeleton--title')).toBe(true);
    expect(block.classList.contains('ph-skeleton--block')).toBe(true);
  });
});
