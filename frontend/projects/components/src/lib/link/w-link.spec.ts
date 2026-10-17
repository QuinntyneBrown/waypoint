import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WLink } from './w-link';

@Component({
  imports: [WLink],
  template: `
    <a w-link href="/help">help centre</a>
    <a w-link standalone href="/admin/cohorts">Back to cohorts</a>
  `,
})
class Host {}

describe('WLink', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return (fixture.nativeElement as HTMLElement).querySelectorAll('a');
  }

  it('is an inline link by default and keeps its text', async () => {
    const [inline] = Array.from(await render());
    expect(inline.classList.contains('w-link')).toBe(true);
    expect(inline.classList.contains('w-link--standalone')).toBe(false);
    expect(inline.textContent?.trim()).toBe('help centre');
  });

  it('marks a standalone link', async () => {
    const links = Array.from(await render());
    expect(links[1].classList.contains('w-link--standalone')).toBe(true);
  });
});
