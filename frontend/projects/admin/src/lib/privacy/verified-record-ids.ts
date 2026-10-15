import type { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import type { FulfillDataSubjectRequest } from '@waypoint/api';

export const VERIFIED_RECORD_LIMIT = 200;
export const VERIFIED_RECORD_FIELDS = [
  'verifiedContactRequestIds',
  'verifiedVolunteerSignupIds',
  'verifiedDonationPledgeIds',
  'verifiedApplicantApplicationIds',
  'verifiedGuardianApplicationIds',
] as const;

export type VerifiedRecordField = (typeof VERIFIED_RECORD_FIELDS)[number];

export type VerifiedRecordInputValues = Record<VerifiedRecordField, string> & {
  verifiedOwnershipAttested: boolean;
};

export type VerifiedOwnershipPayload = Pick<
  FulfillDataSubjectRequest,
  'verifiedOwnershipAttested'
> &
  Partial<Pick<FulfillDataSubjectRequest, VerifiedRecordField>>;

export interface ParsedVerifiedRecordIds {
  ids: string[];
  invalid: string[];
}

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

export function parseVerifiedRecordIds(value: string): ParsedVerifiedRecordIds {
  const ids: string[] = [];
  const invalid: string[] = [];
  const seenIds = new Set<string>();
  const seenInvalid = new Set<string>();

  for (const token of value
    .split(/[,\r\n]+/)
    .map((entry) => entry.trim())
    .filter(Boolean)) {
    const normalized = token.toLowerCase();
    if (GUID_PATTERN.test(normalized) && normalized !== EMPTY_GUID) {
      if (!seenIds.has(normalized)) {
        seenIds.add(normalized);
        ids.push(normalized);
      }
    } else if (!seenInvalid.has(token)) {
      seenInvalid.add(token);
      invalid.push(token);
    }
  }

  return { ids, invalid };
}

export const verifiedRecordIdsValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = typeof control.value === 'string' ? control.value : '';
  const { invalid } = parseVerifiedRecordIds(value);
  return invalid.length > 0 ? { invalidGuidReferences: invalid } : null;
};

export const verifiedRecordSelectionValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const selections = recordSelections(control);
  const count = Object.values(selections).reduce((total, ids) => total + ids.length, 0);
  const errors: ValidationErrors = {};

  if (count > VERIFIED_RECORD_LIMIT) {
    errors['verifiedRecordLimit'] = { actual: count, max: VERIFIED_RECORD_LIMIT };
  }
  if (count > 0 && control.get('verifiedOwnershipAttested')?.value !== true) {
    errors['verifiedOwnershipRequired'] = true;
  }

  const applicantIds = new Set(selections.verifiedApplicantApplicationIds);
  const conflictingApplicationIds = selections.verifiedGuardianApplicationIds.filter((id) =>
    applicantIds.has(id),
  );
  if (conflictingApplicationIds.length > 0) {
    errors['applicationOwnershipConflict'] = conflictingApplicationIds;
  }

  return Object.keys(errors).length > 0 ? errors : null;
};

export function verifiedRecordCount(
  values: Pick<VerifiedRecordInputValues, VerifiedRecordField>,
): number {
  return VERIFIED_RECORD_FIELDS.reduce(
    (total, field) => total + parseVerifiedRecordIds(values[field]).ids.length,
    0,
  );
}

export function buildVerifiedOwnershipPayload(
  values: VerifiedRecordInputValues,
): VerifiedOwnershipPayload {
  const payload: VerifiedOwnershipPayload = { verifiedOwnershipAttested: false };
  let count = 0;

  for (const field of VERIFIED_RECORD_FIELDS) {
    const { ids } = parseVerifiedRecordIds(values[field]);
    count += ids.length;
    if (ids.length > 0) {
      payload[field] = ids;
    }
  }

  payload.verifiedOwnershipAttested = count > 0 && values.verifiedOwnershipAttested;
  return payload;
}

function recordSelections(control: AbstractControl): Record<VerifiedRecordField, string[]> {
  return Object.fromEntries(
    VERIFIED_RECORD_FIELDS.map((field) => {
      const value = control.get(field)?.value;
      return [field, parseVerifiedRecordIds(typeof value === 'string' ? value : '').ids];
    }),
  ) as Record<VerifiedRecordField, string[]>;
}
