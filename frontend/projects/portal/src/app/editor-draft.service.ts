import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { AuthStore } from '@wu/auth';

interface OwnedEditorDraft<T> {
  ownerUserId: string;
  value: T;
}

const STORAGE_PREFIX = 'wu.editor-draft.';

/** Session-local, owner-bound storage for authoring forms that have no server draft aggregate. */
@Injectable({ providedIn: 'root' })
export class EditorDraftService {
  private readonly document = inject(DOCUMENT);
  private readonly auth = inject(AuthStore);

  save<T>(key: string, value: T): boolean {
    const ownerUserId = this.auth.user()?.userId;
    if (!ownerUserId) return false;
    try {
      const storage = this.document.defaultView?.sessionStorage;
      if (!storage) return false;
      storage.setItem(this.key(ownerUserId, key), JSON.stringify({ ownerUserId, value }));
      return true;
    } catch {
      return false;
    }
  }

  take<T>(key: string): T | null {
    const ownerUserId = this.auth.user()?.userId;
    if (!ownerUserId) return null;
    try {
      const storage = this.document.defaultView?.sessionStorage;
      if (!storage) return null;
      const storageKey = this.key(ownerUserId, key);
      const raw = storage.getItem(storageKey);
      if (!raw) return null;
      const draft = JSON.parse(raw) as OwnedEditorDraft<T>;
      if (draft.ownerUserId !== ownerUserId || draft.value === undefined) {
        storage.removeItem(storageKey);
        return null;
      }
      storage.removeItem(storageKey);
      return draft.value;
    } catch {
      this.clear(key);
      return null;
    }
  }

  clear(key: string): void {
    const ownerUserId = this.auth.user()?.userId;
    if (!ownerUserId) return;
    try {
      this.document.defaultView?.sessionStorage.removeItem(this.key(ownerUserId, key));
    } catch {
      // Storage can be unavailable under restrictive browser privacy settings.
    }
  }

  private key(ownerUserId: string, key: string): string {
    return `${STORAGE_PREFIX}${ownerUserId}.${key}`;
  }
}
