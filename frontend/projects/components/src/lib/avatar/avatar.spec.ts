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
});

@Component({ imports: [Avatar], template: `<w-avatar [name]="n" size="sm" />` })
class Host {
  n = 'Amara Okafor';
}

describe('Avatar', () => {
  it('renders initials, size class and a background colour', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const el = fixture.nativeElement.querySelector('w-avatar') as HTMLElement;
    expect(el.textContent?.trim()).toBe('AO');
    expect(el.classList.contains('w-avatar')).toBe(true);
    expect(el.classList.contains('w-avatar--sm')).toBe(true);
    expect(el.style.background).not.toBe('');
  });
});
