import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Avatar, wuAvatarColor, wuInitials } from './avatar';

describe('avatar helpers', () => {
  it('derives up to two initials, uppercased', () => {
    expect(wuInitials('Jayden Alcantara')).toBe('JA');
    expect(wuInitials('madonna')).toBe('M');
    expect(wuInitials('')).toBe('?');
  });

  it('is deterministic for the same name', () => {
    expect(wuAvatarColor('Jayden Alcantara')).toBe(wuAvatarColor('Jayden Alcantara'));
  });

  it('paints from the PromiseHub pastel covers', () => {
    expect(wuAvatarColor('Jayden Alcantara')).toMatch(/^var\(--color-cover-[a-z]+\)$/);
  });
});

@Component({ imports: [Avatar], template: `<ph-avatar [name]="n" size="sm" />` })
class Host {
  n = 'Amara Okafor';
}

describe('Avatar', () => {
  it('renders initials, size class and a background colour', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = fixture.nativeElement.querySelector('ph-avatar') as HTMLElement;
    expect(el.textContent?.trim()).toBe('AO');
    expect(el.classList.contains('ph-avatar')).toBe(true);
    expect(el.classList.contains('ph-avatar--sm')).toBe(true);
    expect(el.style.background).toMatch(/--color-cover-/);
    expect(el.style.color).toBe('var(--color-fg-on-cover)');
  });
});
