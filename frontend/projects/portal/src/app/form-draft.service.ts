import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStore } from '@w/auth';

interface DraftField {
  key: string;
  value: string;
  checked?: boolean;
}

interface TransientFormDraft {
  ownerUserId: string;
  url: string;
  fields: DraftField[];
}

const STORAGE_KEY = 'w.transient-form-draft';

/**
 * Captures ordinary form controls before error or expiry navigation and restores them after recovery.
 * Passwords and files are deliberately excluded. Feature-specific durable drafts remain separate.
 */
@Injectable({ providedIn: 'root' })
export class FormDraftService {
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthStore);

  captureCurrentForm(): void {
    const window = this.document.defaultView;
    const ownerUserId = this.auth.user()?.userId;
    if (!window || !ownerUserId) return;

    const controls = Array.from(
      this.document.querySelectorAll<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>(
        'form input, form select, form textarea',
      ),
    );
    const fields = controls.flatMap<DraftField>((control) => {
      if (
        control instanceof window.HTMLInputElement &&
        ['file', 'password'].includes(control.type)
      ) {
        return [];
      }
      if (
        control instanceof window.HTMLInputElement &&
        control.type === 'radio' &&
        !control.checked
      ) {
        return [];
      }
      const key = control.name || control.id || control.getAttribute('formcontrolname');
      if (!key) return [];
      return [
        {
          key,
          value: control.value,
          ...(control instanceof window.HTMLInputElement &&
          ['checkbox', 'radio'].includes(control.type)
            ? { checked: control.checked }
            : {}),
        },
      ];
    });

    if (fields.length > 0) {
      const draft: TransientFormDraft = { ownerUserId, url: this.router.url, fields };
      window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify(draft));
    }
  }

  draftUrl(): string | null {
    return this.read()?.url ?? null;
  }

  restoreAndClear(): boolean {
    const window = this.document.defaultView;
    const draft = this.read();
    if (
      !window ||
      !draft ||
      draft.url !== this.router.url ||
      draft.ownerUserId !== this.auth.user()?.userId
    ) {
      return false;
    }

    let restored = 0;
    for (const field of draft.fields) {
      const escaped = window.CSS?.escape ? window.CSS.escape(field.key) : field.key;
      const controls = Array.from(
        this.document.querySelectorAll<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>(
          `[name="${escaped}"], #${escaped}, [formcontrolname="${escaped}"]`,
        ),
      );
      const control = controls.find((candidate) =>
        candidate instanceof window.HTMLInputElement && candidate.type === 'radio'
          ? candidate.value === field.value
          : true,
      );
      if (!control) continue;
      control.value = field.value;
      if (control instanceof window.HTMLInputElement && field.checked !== undefined) {
        control.checked = field.checked;
      }
      control.dispatchEvent(new window.Event('input', { bubbles: true }));
      control.dispatchEvent(new window.Event('change', { bubbles: true }));
      restored++;
    }

    if (restored > 0) {
      window.sessionStorage.removeItem(STORAGE_KEY);
      return true;
    }
    return false;
  }

  private read(): TransientFormDraft | null {
    const raw = this.document.defaultView?.sessionStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as TransientFormDraft;
    } catch {
      this.document.defaultView?.sessionStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }
}
