import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Divider } from './divider';

@Component({ imports: [Divider], template: `<div ph-divider></div>` })
class Host {}

describe('Divider', () => {
  it('is a rule between groups of content', async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const divider = (fixture.nativeElement as HTMLElement).querySelector('div')!;
    expect(divider.className).toBe('ph-divider');
  });
});
