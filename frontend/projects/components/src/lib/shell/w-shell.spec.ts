import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { WShell } from './w-shell';

@Component({
  imports: [WShell],
  template: `<w-shell
    role="youth"
    active="home"
    userName="Jayden Alcantara"
    (signOut)="onSignOut()"
  >
    <p class="content">home</p>
  </w-shell>`,
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

describe('WShell', () => {
  it('renders the role sidenav with the active item marked', async () => {
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;
    const items = el.querySelectorAll('.w-sidenav__item');
    // 8 youth nav items + the sign-out foot button
    expect(items.length).toBeGreaterThanOrEqual(8);
    const active = el.querySelector('.w-sidenav__item.is-active');
    expect(active?.textContent).toContain('Home');
    expect(active?.getAttribute('aria-current')).toBe('page');
  });

  it('projects page content into w-main', async () => {
    const fixture = await render();
    const main = fixture.nativeElement.querySelector('main.w-main .content');
    expect(main?.textContent).toBe('home');
  });

  it('does not expose the product role as an invalid ARIA role', async () => {
    const fixture = await render();
    const shell = fixture.nativeElement.querySelector('w-shell') as HTMLElement;
    expect(shell.getAttribute('role')).toBeNull();
  });

  it('shows the mobile bottom nav for youth', async () => {
    const fixture = await render();
    expect(fixture.nativeElement.querySelectorAll('.w-botnav__item').length).toBe(5);
  });

  it('emits signOut from the sidenav foot', async () => {
    const fixture = await render();
    const footBtn = [...fixture.nativeElement.querySelectorAll('.w-sidenav__foot button')][0];
    footBtn.click();
    await fixture.whenStable();
    expect(fixture.componentInstance.signedOut()).toBe(1);
  });

  it('opens the account menu and emits signOut from it', async () => {
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.w-menu')).toBeNull();
    [...el.querySelectorAll('button')].find((b) => b.textContent?.includes('Account'))!.click();
    await fixture.whenStable();
    const danger = el.querySelector('.w-menu__item--danger') as HTMLButtonElement;
    expect(danger).toBeTruthy();
    danger.click();
    await fixture.whenStable();
    expect(fixture.componentInstance.signedOut()).toBe(1);
  });
});
