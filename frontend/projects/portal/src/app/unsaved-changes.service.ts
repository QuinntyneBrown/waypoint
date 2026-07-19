import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export type DraftSaveResult = boolean | Promise<boolean> | Observable<boolean>;

/** Shared navigation and browser-close boundary for editors with local changes. */
@Injectable({ providedIn: 'root' })
export class UnsavedChangesService {
  private readonly document = inject(DOCUMENT);

  confirmSaveOrStay(
    dirty: boolean,
    editorLabel: string,
    saveDraft: () => DraftSaveResult,
  ): DraftSaveResult {
    if (!dirty) return true;
    const shouldSave = this.document.defaultView?.confirm(
      `You have unsaved ${editorLabel} changes. Save your ${editorLabel} draft before leaving? ` +
        'Select OK to save the draft, or Cancel to stay.',
    );
    return shouldSave ? saveDraft() : false;
  }

  warnBeforeUnload(event: BeforeUnloadEvent, dirty: boolean, saveLocalDraft?: () => void): void {
    if (!dirty) return;
    saveLocalDraft?.();
    event.preventDefault();
    event.returnValue = '';
  }
}
