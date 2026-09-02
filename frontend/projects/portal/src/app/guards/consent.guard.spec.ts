import { TestBed } from '@angular/core/testing';
import { provideRouter, UrlTree } from '@angular/router';
import { ConsentApi } from '@w/api';
import { firstValueFrom, isObservable, Observable, of, throwError } from 'rxjs';
import { describe, expect, it } from 'vitest';
import { consentGuard } from './consent.guard';

function run() {
  return TestBed.runInInjectionContext(() =>
    consentGuard({} as never, { url: '/youth/home' } as never),
  );
}

async function resolve(result: unknown): Promise<boolean | UrlTree> {
  return isObservable(result)
    ? await firstValueFrom(result as Observable<boolean | UrlTree>)
    : (result as boolean | UrlTree);
}

function configure(api: Partial<ConsentApi>) {
  TestBed.configureTestingModule({
    providers: [provideRouter([]), { provide: ConsentApi, useValue: api }],
  });
}

describe('consentGuard', () => {
  it('permits youth navigation when participation is allowed', async () => {
    configure({ status: () => of({ participationAllowed: true }) });
    expect(await resolve(run())).toBe(true);
  });

  it('diverts to consent-required when participation is blocked', async () => {
    configure({ status: () => of({ participationAllowed: false }) });
    const result = (await resolve(run())) as UrlTree;
    expect(result).toBeInstanceOf(UrlTree);
    expect(result.toString()).toBe('/consent-required');
  });

  it('fails open on a transient status error (server remains the authority)', async () => {
    configure({ status: () => throwError(() => new Error('offline')) });
    expect(await resolve(run())).toBe(true);
  });
});
