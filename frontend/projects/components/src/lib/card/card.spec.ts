import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Card } from './card';
import { CardBody } from './card-body';
import { CardFooter } from './card-footer';
import { CardHeader } from './card-header';
import { CardTitle } from './card-title';

@Component({
  imports: [Card, CardHeader, CardTitle, CardBody, CardFooter],
  template: `
    <section ph-card aria-labelledby="t">
      <div ph-card-header><h2 ph-card-title id="t">Title</h2></div>
      <div ph-card-body>Body</div>
      <div ph-card-footer>Footer</div>
    </section>
    <div ph-card [variant]="variant()">Variant</div>
    <a ph-card link variant="greige" href="/x">Link</a>
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
    expect(card.className).toBe('ph-card');
    expect(card.getAttribute('aria-labelledby')).toBe('t');
    expect(card.querySelector('.ph-card__header .ph-card__title')?.textContent).toBe('Title');
    expect(card.querySelector('.ph-card__body')?.textContent).toBe('Body');
    expect(card.querySelector('.ph-card__footer')?.textContent).toBe('Footer');
  });

  it('applies and removes a variant', async () => {
    const { fixture, cards } = await render();
    expect(cards[1].classList.contains('ph-card--greige')).toBe(true);
    fixture.componentInstance.variant.set('');
    await fixture.whenStable();
    expect(cards[1].classList.contains('ph-card--greige')).toBe(false);
  });

  it('makes a card a link on an anchor, alongside a variant', async () => {
    const [, , link] = (await render()).cards;
    expect(link.classList.contains('ph-card--link')).toBe(true);
    expect(link.classList.contains('ph-card--greige')).toBe(true);
    expect(link.getAttribute('href')).toBe('/x');
  });
});
