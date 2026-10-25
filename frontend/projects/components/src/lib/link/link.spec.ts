import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Link } from './link';

@Component({
  imports: [Link],
  template: `
    <a ph-link href="/help">help centre</a>
    <a ph-link standalone href="/admin/cohorts">Back to cohorts</a>
  `,
})
class Host {}

describe('Link', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return (fixture.nativeElement as HTMLElement).querySelectorAll('a');
  }

  it('is an inline link by default and keeps its text', async () => {
    const [inline] = Array.from(await render());
    expect(inline.classList.contains('ph-link')).toBe(true);
    expect(inline.classList.contains('ph-link--standalone')).toBe(false);
    expect(inline.textContent?.trim()).toBe('help centre');
  });

  it('marks a standalone link', async () => {
    const links = Array.from(await render());
    expect(links[1].classList.contains('ph-link--standalone')).toBe(true);
  });
});
