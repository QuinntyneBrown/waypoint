import { CanDeactivateFn } from '@angular/router';
import { Observable } from 'rxjs';

// Traces to: L2-NFR-007
export interface UnsavedChangesAware {
  canDeactivate(): boolean | Promise<boolean> | Observable<boolean>;
}

export const unsavedChangesGuard: CanDeactivateFn<UnsavedChangesAware> = (component) =>
  component.canDeactivate();
