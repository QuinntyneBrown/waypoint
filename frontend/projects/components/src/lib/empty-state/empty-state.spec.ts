import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Icon } from '../icon/icon';
import { EmptyState } from './empty-state';
import { EmptyStateIcon } from './empty-state-icon';
import { EmptyStateText } from './empty-state-text';
import { EmptyStateTitle } from './empty-state-title';

@Component({
  imports: [EmptyState, EmptyStateIcon, EmptyStateTitle, EmptyStateText, Icon],
  template: `
    <div w-empty-state role="status">
      <div w-empty-state-icon><w-icon name="users" /></div>
      <h3 w-empty-state-title>No youth assigned yet</h3>
      <p w-empty-state-text>Assigned youth will appear here.</p>
    </div>
  `,
})
class Host {}

describe('EmptyState', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return (fixture.nativeElement as HTMLElement).firstElementChild as HTMLElement;
  }

  it('is a centred empty state and keeps its role', async () => {
    const empty = await render();
    expect(empty.className).toBe('w-empty');
    expect(empty.getAttribute('role')).toBe('status');
  });

  it('renders the icon, title and text parts', async () => {
    const empty = await render();
    expect(empty.querySelector('.w-empty__icon svg')).not.toBeNull();
    expect(empty.querySelector('h3.w-empty__title')?.textContent).toBe('No youth assigned yet');
    expect(empty.querySelector('p.w-empty__text')?.textContent).toBe(
      'Assigned youth will appear here.',
    );
  });
});
