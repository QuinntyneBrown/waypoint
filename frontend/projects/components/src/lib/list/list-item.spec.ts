import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ListItem } from './list-item';
import { ListItemBody } from './list-item-body';
import { ListItemMeta } from './list-item-meta';
import { ListItemTitle } from './list-item-title';

@Component({
  imports: [ListItem, ListItemBody, ListItemTitle, ListItemMeta],
  template: `
    <div ph-list-item data-testid="module">
      <div ph-list-item-body>
        <div ph-list-item-title>Module one</div>
        <div ph-list-item-meta>In progress · 40%</div>
      </div>
    </div>
  `,
})
class Host {}

describe('ListItem', () => {
  it('shows a row with a title and supporting detail', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const item = (fixture.nativeElement as HTMLElement).firstElementChild!;
    expect(item.className).toBe('ph-listitem');
    expect(item.getAttribute('data-testid')).toBe('module');
    expect(item.querySelector('.ph-listitem__body')).not.toBeNull();
    expect(item.querySelector('.ph-listitem__title')?.textContent).toBe('Module one');
    expect(item.querySelector('.ph-listitem__meta')?.textContent).toBe('In progress · 40%');
  });
});
