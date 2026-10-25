import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { DescriptionList } from './description-list';

@Component({
  imports: [DescriptionList],
  template: `
    <dl ph-description-list>
      <dt>Role</dt>
      <dd>Mentor</dd>
    </dl>
    <dl ph-description-list vertical>
      <dt>Email</dt>
      <dd>ana@example.org</dd>
    </dl>
  `,
})
class Host {}

describe('DescriptionList', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('dl'));
  }

  it('pairs terms with details side by side by default', async () => {
    const [list] = await render();
    expect(list.className).toBe('ph-dl');
    expect(list.querySelector('dt')?.textContent).toBe('Role');
    expect(list.querySelector('dd')?.textContent).toBe('Mentor');
  });

  it('can stack terms above details', async () => {
    const [, vertical] = await render();
    expect(vertical.classList.contains('ph-dl--vertical')).toBe(true);
  });
});
