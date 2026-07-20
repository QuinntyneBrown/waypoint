import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { WuShell } from './wu-shell';

@Component({
  imports: [WuShell],
  template: `<wu-shell
    role="youth"
    active="home"
    userName="Jayden Alcantara"
    (signOut)="onSignOut()"
  >
    <p class="content">home</p>
  </wu-shell>`,
})
class Host {
  signedOut = signal(0);
  onSignOut() {
    this.signedOut.update((n) => n + 1);
  }
}

async function render() {
  await TestBed.configureTestingModule({
    imports: [Host],
    providers: [provideRouter([])],
  }).compileComponents();
  const fixture = TestBed.createComponent(Host);
  await fixture.whenStable();
  return fixture;
}

describe('WuShell', () => {
  it('renders the role sidenav with the active item marked', async () => {
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;
    const items = el.querySelectorAll('.wu-sidenav__item');
    // 8 youth nav items + the sign-out foot button
    expect(items.length).toBeGreaterThanOrEqual(8);
    const active = el.querySelector('.wu-sidenav__item.is-active');
    expect(active?.textContent).toContain('Home');
    expect(active?.getAttribute('aria-current')).toBe('page');
  });

  it('projects page content into wu-main', async () => {
    const fixture = await render();
    const main = fixture.nativeElement.querySelector('main.wu-main .content');
    expect(main?.textContent).toBe('home');
  });

  it('does not expose the product role as an invalid ARIA role', async () => {
    const fixture = await render();
    const shell = fixture.nativeElement.querySelector('wu-shell') as HTMLElement;
    expect(shell.getAttribute('role')).toBeNull();
  });

  it('shows the mobile bottom nav for youth', async () => {
    const fixture = await render();
    expect(fixture.nativeElement.querySelectorAll('.wu-botnav__item').length).toBe(5);
  });

  it('emits signOut from the sidenav foot', async () => {
    const fixture = await render();
    const footBtn = [...fixture.nativeElement.querySelectorAll('.wu-sidenav__foot button')][0];
    footBtn.click();
    await fixture.whenStable();
    expect(fixture.componentInstance.signedOut()).toBe(1);
  });

  it('opens the account menu and emits signOut from it', async () => {
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.wu-menu')).toBeNull();
    (el.querySelector('button[aria-label="Account"]') as HTMLButtonElement).click();
    await fixture.whenStable();
    const danger = el.querySelector('.wu-menu__item--danger') as HTMLButtonElement;
    expect(danger).toBeTruthy();
    danger.click();
    await fixture.whenStable();
    expect(fixture.componentInstance.signedOut()).toBe(1);
  });
});
