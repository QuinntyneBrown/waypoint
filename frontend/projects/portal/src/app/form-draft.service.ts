import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';

interface DraftField {
  key: string;
  value: string;
  checked?: boolean;
}

interface TransientFormDraft {
  url: string;
  fields: DraftField[];
}

const STORAGE_KEY = 'wu.transient-form-draft';

/**
 * Captures ordinary form controls before an error-state navigation and restores them after retry.
 * Passwords and files are deliberately excluded. Feature-specific durable drafts remain separate.
 */
@Injectable({ providedIn: 'root' })
export class FormDraftService {
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);

  captureCurrentForm(): void {
    const window = this.document.defaultView;
    if (!window) return;

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
      const key = control.name || control.id;
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
      const draft: TransientFormDraft = { url: this.router.url, fields };
      window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify(draft));
    }
  }

  draftUrl(): string | null {
    return this.read()?.url ?? null;
  }

  restoreAndClear(): void {
    const window = this.document.defaultView;
    const draft = this.read();
    if (!window || !draft || draft.url !== this.router.url) return;

    for (const field of draft.fields) {
      const escaped = window.CSS?.escape ? window.CSS.escape(field.key) : field.key;
      const control = this.document.querySelector<
        HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
      >(`[name="${escaped}"], #${escaped}`);
      if (!control) continue;
      control.value = field.value;
      if (control instanceof window.HTMLInputElement && field.checked !== undefined) {
        control.checked = field.checked;
      }
      control.dispatchEvent(new window.Event('input', { bubbles: true }));
      control.dispatchEvent(new window.Event('change', { bubbles: true }));
    }

    window.sessionStorage.removeItem(STORAGE_KEY);
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
