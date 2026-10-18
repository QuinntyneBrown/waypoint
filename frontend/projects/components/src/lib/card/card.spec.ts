import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Card, CardBody, CardFooter, CardHeader, CardTitle } from './card';

@Component({
  imports: [Card, CardHeader, CardTitle, CardBody, CardFooter],
  template: `
    <section w-card aria-labelledby="t">
      <div w-card-header><h2 w-card-title id="t">Title</h2></div>
      <div w-card-body>Body</div>
      <div w-card-footer>Footer</div>
    </section>
    <div w-card [variant]="variant()">Variant</div>
    <a w-card link variant="greige" href="/x">Link</a>
  `,
})
class Host {
  readonly variant = signal<'greige' | ''>('greige');
}

describe('Card', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const cards = Array.from((fixture.nativeElement as HTMLElement).children) as HTMLElement[];
    return { fixture, cards };
  }

  it('is a plain card with header, title, body and footer parts', async () => {
    const [card] = (await render()).cards;
    expect(card.className).toBe('w-card');
    expect(card.getAttribute('aria-labelledby')).toBe('t');
    expect(card.querySelector('.w-card__header .w-card__title')?.textContent).toBe('Title');
    expect(card.querySelector('.w-card__body')?.textContent).toBe('Body');
    expect(card.querySelector('.w-card__footer')?.textContent).toBe('Footer');
  });

  it('applies and removes a variant', async () => {
    const { fixture, cards } = await render();
    expect(cards[1].classList.contains('w-card--greige')).toBe(true);
    fixture.componentInstance.variant.set('');
    await fixture.whenStable();
    expect(cards[1].classList.contains('w-card--greige')).toBe(false);
  });

  it('makes a card a link on an anchor, alongside a variant', async () => {
    const [, , link] = (await render()).cards;
    expect(link.classList.contains('w-card--link')).toBe(true);
    expect(link.classList.contains('w-card--greige')).toBe(true);
    expect(link.getAttribute('href')).toBe('/x');
  });
});
