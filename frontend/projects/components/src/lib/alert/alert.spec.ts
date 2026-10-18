import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Alert } from './alert';
import { AlertTitle } from './alert-title';

@Component({
  imports: [Alert, AlertTitle],
  template: `
    <p w-alert role="status">Plain</p>
    <p w-alert variant="error" role="alert">Could not save.</p>
    <div w-alert variant="danger">Could not load.</div>
    <div w-alert variant="info" icon="info">With icon</div>
    <div w-alert variant="warning">
      <div>
        <div w-alert-title>Consent required</div>
        Ask a guardian.
      </div>
    </div>
  `,
})
class Host {}

describe('Alert', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return Array.from((fixture.nativeElement as HTMLElement).children) as HTMLElement[];
  }

  it('is a neutral alert by default and keeps its text and role', async () => {
    const [plain] = await render();
    expect(plain.classList.contains('w-alert')).toBe(true);
    expect(plain.className).toBe('w-alert');
    expect(plain.getAttribute('role')).toBe('status');
    expect(plain.textContent?.trim()).toBe('Plain');
  });

  it('applies the tone variants', async () => {
    const [, error, danger, info, warning] = await render();
    expect(error.classList.contains('w-alert--error')).toBe(true);
    expect(danger.classList.contains('w-alert--danger')).toBe(true);
    expect(info.classList.contains('w-alert--info')).toBe(true);
    expect(warning.classList.contains('w-alert--warning')).toBe(true);
  });

  it('renders the icon and wraps the text in a body when given an icon', async () => {
    const [, , , info] = await render();
    expect(info.querySelector('svg.w-icon.w-alert__icon')).not.toBeNull();
    expect(info.querySelector(':scope > div')?.textContent?.trim()).toBe('With icon');
  });

  it('adds no icon or wrapper without an icon', async () => {
    const [plain] = await render();
    expect(plain.querySelector('svg')).toBeNull();
    expect(plain.querySelector('div')).toBeNull();
  });

  it('styles the title part', async () => {
    const [, , , , warning] = await render();
    expect(warning.querySelector('.w-alert__title')?.textContent?.trim()).toBe('Consent required');
  });
});
