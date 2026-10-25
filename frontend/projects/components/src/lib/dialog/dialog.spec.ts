import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Dialog } from './dialog';
import { DialogBody } from './dialog-body';
import { DialogFooter } from './dialog-footer';
import { DialogHeader } from './dialog-header';
import { DialogSubtitle } from './dialog-subtitle';
import { DialogTitle } from './dialog-title';

@Component({
  imports: [Dialog, DialogHeader, DialogTitle, DialogSubtitle, DialogBody, DialogFooter],
  template: `
    <button id="opener" type="button">Open</button>
    @if (open()) {
      <div ph-dialog size="lg" aria-labelledby="t" (dismiss)="dismissed.set(dismissed() + 1)">
        <div ph-dialog-header>
          <div>
            <h2 ph-dialog-title id="t">Create cohort</h2>
            <p ph-dialog-subtitle>Choose the track.</p>
          </div>
        </div>
        <div ph-dialog-body><input id="name" /></div>
        <div ph-dialog-footer><button id="save" type="button">Save</button></div>
      </div>
    }
  `,
})
class Host {
  readonly open = signal(true);
  readonly dismissed = signal(0);
}

describe('Dialog', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    const opener = (fixture.nativeElement as HTMLElement).querySelector<HTMLElement>('#opener')!;
    document.body.appendChild(fixture.nativeElement);
    opener.focus();
    await fixture.whenStable();
    await Promise.resolve();
    return { fixture, root: fixture.nativeElement as HTMLElement, opener };
  }

  it('is a modal dialog named by its title', async () => {
    const { root } = await render();
    const dialog = root.querySelector<HTMLElement>('[ph-dialog]')!;
    expect(dialog.classList.contains('ph-modal')).toBe(true);
    expect(dialog.classList.contains('ph-modal--lg')).toBe(true);
    expect(dialog.getAttribute('role')).toBe('dialog');
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(dialog.getAttribute('aria-labelledby')).toBe('t');
  });

  it('draws the scrim and panel around the parts', async () => {
    const { root } = await render();
    expect(root.querySelector('.ph-modal__scrim')).not.toBeNull();
    const panel = root.querySelector('.ph-modal__panel')!;
    expect(panel.querySelector('.ph-modal__header h2.ph-modal__title')?.textContent).toBe(
      'Create cohort',
    );
    expect(panel.querySelector('.ph-modal__header p.ph-modal__sub')?.textContent).toBe(
      'Choose the track.',
    );
    expect(panel.querySelector('.ph-modal__body input')).not.toBeNull();
    expect(panel.querySelector('.ph-modal__footer button')?.textContent).toBe('Save');
  });

  it('asks to be dismissed from the scrim and from Escape', async () => {
    const { fixture, root } = await render();
    root.querySelector<HTMLElement>('.ph-modal__scrim')!.click();
    expect(fixture.componentInstance.dismissed()).toBe(1);
    root
      .querySelector<HTMLElement>('[ph-dialog]')!
      .dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(fixture.componentInstance.dismissed()).toBe(2);
  });

  it('moves focus in and returns it to the opener when closed', async () => {
    const { fixture, root, opener } = await render();
    expect(root.querySelector('[ph-dialog]')!.contains(document.activeElement)).toBe(true);
    fixture.componentInstance.open.set(false);
    await fixture.whenStable();
    await Promise.resolve();
    expect(document.activeElement).toBe(opener);
  });
});
