import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { WSwitch, WSwitchTrack } from './w-switch';

@Component({
  imports: [WSwitch, WSwitchTrack],
  template: `
    <label w-switch>
      <input type="checkbox" aria-label="Reduce motion" />
      <span w-switch-track></span>
    </label>
  `,
})
class Host {}

describe('WSwitch', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('wraps a native checkbox and keeps its accessible name', async () => {
    const label = (await render()).querySelector('label')!;
    expect(label.classList.contains('w-switch')).toBe(true);
    expect(label.querySelector('input[type="checkbox"]')?.getAttribute('aria-label')).toBe(
      'Reduce motion',
    );
  });

  it('renders the track next to the input', async () => {
    const track = (await render()).querySelector('input + span')!;
    expect(track.classList.contains('w-switch__track')).toBe(true);
  });
});
