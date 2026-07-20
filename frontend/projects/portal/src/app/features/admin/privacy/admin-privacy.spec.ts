import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import type { ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PrivacyApi } from '@wu/api';
import type { AdminDataSubjectRequest } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AdminPrivacyPage } from './admin-privacy';

const CONTACT_ID = '11111111-1111-1111-1111-111111111111';
const DONATION_ID = '22222222-2222-2222-2222-222222222222';
const REQUEST: AdminDataSubjectRequest = {
  id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  reference: 'DSR-0001',
  subjectUserId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  subjectName: 'Privacy Subject',
  subjectEmail: 'subject@example.test',
  actionType: 'access',
  status: 'Pending',
  requestedAt: '2026-07-20T00:00:00Z',
  completedByUserId: null,
  details: 'Please include my legacy public submissions.',
};

describe('AdminPrivacyPage verified ownership controls', () => {
  let fixture: ComponentFixture<AdminPrivacyPage>;
  let fulfill: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    fulfill = vi.fn(() =>
      of({
        ...REQUEST,
        status: 'Completed',
        completedAt: '2026-07-20T01:00:00Z',
      }),
    );

    await TestBed.configureTestingModule({
      imports: [AdminPrivacyPage],
      providers: [
        provideRouter([]),
        {
          provide: AuthStore,
          useValue: {
            user: signal({ displayName: 'Privacy Administrator' }),
            signOut: vi.fn(),
          },
        },
        {
          provide: PrivacyApi,
          useValue: {
            adminRequests: vi.fn(() => of({ items: [REQUEST], page: 1, pageSize: 25, total: 1 })),
            fulfill,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminPrivacyPage);
    fixture.detectChanges();
    reviewRequest();
  });

  it('exposes categorized inputs and blocks malformed or unattested references', () => {
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelectorAll('fieldset textarea')).toHaveLength(5);
    expect(root.textContent).toContain('email-address match alone never establishes ownership');

    enter('verified-contact-requests', 'not-a-guid');
    submit();
    expect(fulfill).not.toHaveBeenCalled();
    expect(input('verified-contact-requests').getAttribute('aria-invalid')).toBe('true');
    expect(root.textContent).toContain('Enter full, non-zero GUIDs only');

    enter('verified-contact-requests', CONTACT_ID);
    submit();
    expect(fulfill).not.toHaveBeenCalled();
    expect(root.textContent).toContain('Explicitly attest verified ownership');
  });

  it('submits normalized references and omits empty categories', () => {
    enter('verified-contact-requests', `${CONTACT_ID},${CONTACT_ID.toUpperCase()}`);
    enter('verified-donation-pledges', DONATION_ID);
    input('verified-ownership-attestation').click();
    fixture.detectChanges();

    submit();

    expect(fulfill).toHaveBeenCalledWith(REQUEST.id, {
      displayName: null,
      email: null,
      pronouns: null,
      clearPronouns: false,
      resolutionNote: null,
      verifiedOwnershipAttested: true,
      verifiedContactRequestIds: [CONTACT_ID],
      verifiedDonationPledgeIds: [DONATION_ID],
    });
  });

  function reviewRequest(): void {
    const button = [...(fixture.nativeElement as HTMLElement).querySelectorAll('button')].find(
      (candidate) => candidate.textContent?.includes('Review request'),
    );
    expect(button).toBeDefined();
    button!.click();
    fixture.detectChanges();
  }

  function input(id: string): HTMLInputElement | HTMLTextAreaElement {
    const element = (fixture.nativeElement as HTMLElement).querySelector(`#${id}`);
    expect(element).not.toBeNull();
    return element as HTMLInputElement | HTMLTextAreaElement;
  }

  function enter(id: string, value: string): void {
    const element = input(id) as HTMLTextAreaElement;
    element.value = value;
    element.dispatchEvent(new Event('input', { bubbles: true }));
    fixture.detectChanges();
  }

  function submit(): void {
    const form = (fixture.nativeElement as HTMLElement).querySelector('form');
    expect(form).not.toBeNull();
    form!.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }
});
