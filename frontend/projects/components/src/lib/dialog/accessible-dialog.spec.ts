import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AccessibleDialog } from './accessible-dialog';

@Component({
  imports: [AccessibleDialog],
  template: `
    <button id="opener" type="button" (click)="open.set(true)">Open dialog</button>
    @if (open()) {
      <section wuAccessibleDialog aria-labelledby="dialog-title" (wuDialogEscape)="open.set(false)">
        <h2 id="dialog-title">Keyboard dialog</h2>
        <button id="first" type="button" wuDialogInitialFocus>First</button>
        <button id="disabled" type="button" disabled>Disabled</button>
        <a id="last" href="/last">Last</a>
      </section>
    }
  `,
})
class Host {
  readonly open = signal(false);
}

async function render(): Promise<ComponentFixture<Host>> {
  await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
  const fixture = TestBed.createComponent(Host);
  fixture.detectChanges();
  return fixture;
}

async function openDialog(fixture: ComponentFixture<Host>): Promise<HTMLElement> {
  const opener = fixture.nativeElement.querySelector('#opener') as HTMLButtonElement;
  opener.focus();
  opener.click();
  fixture.detectChanges();
  await fixture.whenStable();
  await Promise.resolve();
  return fixture.nativeElement.querySelector('[wuAccessibleDialog]') as HTMLElement;
}

function press(target: HTMLElement, key: string, shiftKey = false): KeyboardEvent {
  const event = new KeyboardEvent('keydown', { key, shiftKey, bubbles: true, cancelable: true });
  target.dispatchEvent(event);
  return event;
}

// Traces to: L2-NFR-010 AC4
describe('AccessibleDialog', () => {
  it('provides modal semantics and moves initial focus to the requested control', async () => {
    const fixture = await render();
    const dialog = await openDialog(fixture);

    expect(dialog.getAttribute('role')).toBe('dialog');
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(document.activeElement?.id).toBe('first');
  });

  it('wraps Tab and Shift+Tab inside the dialog', async () => {
    const fixture = await render();
    const dialog = await openDialog(fixture);
    const first = dialog.querySelector('#first') as HTMLButtonElement;
    const last = dialog.querySelector('#last') as HTMLAnchorElement;

    last.focus();
    expect(press(last, 'Tab').defaultPrevented).toBe(true);
    expect(document.activeElement).toBe(first);

    expect(press(first, 'Tab', true).defaultPrevented).toBe(true);
    expect(document.activeElement).toBe(last);
  });

  it('closes on Escape and restores focus to the opener', async () => {
    const fixture = await render();
    const dialog = await openDialog(fixture);

    expect(press(dialog.querySelector('#first') as HTMLElement, 'Escape').defaultPrevented).toBe(
      true,
    );
    fixture.detectChanges();
    await fixture.whenStable();
    await Promise.resolve();

    expect(fixture.nativeElement.querySelector('[wuAccessibleDialog]')).toBeNull();
    expect(document.activeElement?.id).toBe('opener');
  });
});
