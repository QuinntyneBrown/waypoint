import { inject, Injectable } from '@angular/core';
import { OnboardingApi } from '@w/api';
import { roleHome } from '@w/auth';
import { catchError, map, Observable, of } from 'rxjs';

/**
 * Decides where a freshly-signed-in user lands. For a Youth this runs the entry
 * chain once at sign-in: incomplete onboarding → the resumable wizard; otherwise
 * their role home. (Consent gating is layered on in M5.) Non-youth roles go
 * straight to their role home.
 */
@Injectable({ providedIn: 'root' })
export class EntryResolver {
  private readonly onboarding = inject(OnboardingApi);

  resolve(role: string): Observable<string> {
    if (role !== 'Youth') return of(roleHome(role));
    return this.onboarding.status().pipe(
      map((status) => (status.onboardingCompleted ? '/youth/home' : '/onboarding')),
      // Fail open: a transient status error shouldn't strand the user on sign-in.
      catchError(() => of('/youth/home')),
    );
  }
}
