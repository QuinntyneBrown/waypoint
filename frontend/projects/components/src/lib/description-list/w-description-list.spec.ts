import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WDescriptionList } from './w-description-list';

@Component({
  imports: [WDescriptionList],
  template: `
    <dl w-description-list>
      <dt>Role</dt>
      <dd>Mentor</dd>
    </dl>
    <dl w-description-list vertical>
      <dt>Email</dt>
      <dd>ana@example.org</dd>
    </dl>
  `,
})
class Host {}

describe('WDescriptionList', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('dl'));
  }

  it('pairs terms with details side by side by default', async () => {
    const [list] = await render();
    expect(list.className).toBe('w-dl');
    expect(list.querySelector('dt')?.textContent).toBe('Role');
    expect(list.querySelector('dd')?.textContent).toBe('Mentor');
  });

  it('can stack terms above details', async () => {
    const [, vertical] = await render();
    expect(vertical.classList.contains('w-dl--vertical')).toBe(true);
  });
});
