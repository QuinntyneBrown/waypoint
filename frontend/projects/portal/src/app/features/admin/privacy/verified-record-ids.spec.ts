import { FormControl, FormGroup } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import {
  buildVerifiedOwnershipPayload,
  parseVerifiedRecordIds,
  verifiedRecordIdsValidator,
  verifiedRecordSelectionValidator,
  type VerifiedRecordInputValues,
} from './verified-record-ids';

const FIRST_ID = '11111111-1111-1111-1111-111111111111';
const SECOND_ID = '22222222-2222-2222-2222-222222222222';

describe('verified legacy-record references', () => {
  it('parses comma and newline separators and removes case-insensitive duplicates', () => {
    expect(parseVerifiedRecordIds(`${FIRST_ID.toUpperCase()}, ${SECOND_ID}\n${FIRST_ID}`)).toEqual({
      ids: [FIRST_ID, SECOND_ID],
      invalid: [],
    });
  });

  it('rejects malformed and empty GUID references locally', () => {
    const control = new FormControl(
      `not-a-guid\n00000000-0000-0000-0000-000000000000`,
      verifiedRecordIdsValidator,
    );

    expect(control.getError('invalidGuidReferences')).toEqual([
      'not-a-guid',
      '00000000-0000-0000-0000-000000000000',
    ]);
  });

  it('requires attestation for references and rejects applicant/guardian overlap', () => {
    const form = ownershipForm();
    form.controls.verifiedApplicantApplicationIds.setValue(FIRST_ID);
    expect(form.hasError('verifiedOwnershipRequired')).toBe(true);

    form.controls.verifiedOwnershipAttested.setValue(true);
    expect(form.hasError('verifiedOwnershipRequired')).toBe(false);

    form.controls.verifiedGuardianApplicationIds.setValue(FIRST_ID);
    expect(form.getError('applicationOwnershipConflict')).toEqual([FIRST_ID]);
  });

  it('enforces the aggregate 200-reference server limit', () => {
    const form = ownershipForm();
    const references = Array.from({ length: 201 }, (_, index) => guid(index + 1));
    form.controls.verifiedContactRequestIds.setValue(references.join(','));
    form.controls.verifiedOwnershipAttested.setValue(true);

    expect(form.getError('verifiedRecordLimit')).toEqual({ actual: 201, max: 200 });
  });

  it('omits empty categories and emits a clean deduplicated ownership payload', () => {
    const values: VerifiedRecordInputValues = {
      verifiedOwnershipAttested: true,
      verifiedContactRequestIds: `${FIRST_ID},${FIRST_ID.toUpperCase()}`,
      verifiedVolunteerSignupIds: '',
      verifiedDonationPledgeIds: `\n${SECOND_ID}\n`,
      verifiedApplicantApplicationIds: '',
      verifiedGuardianApplicationIds: '',
    };

    expect(buildVerifiedOwnershipPayload(values)).toEqual({
      verifiedOwnershipAttested: true,
      verifiedContactRequestIds: [FIRST_ID],
      verifiedDonationPledgeIds: [SECOND_ID],
    });
    expect(
      buildVerifiedOwnershipPayload({
        ...values,
        verifiedContactRequestIds: '',
        verifiedDonationPledgeIds: '',
      }),
    ).toEqual({ verifiedOwnershipAttested: false });
  });
});

function ownershipForm() {
  return new FormGroup(
    {
      verifiedOwnershipAttested: new FormControl(false, { nonNullable: true }),
      verifiedContactRequestIds: new FormControl('', { nonNullable: true }),
      verifiedVolunteerSignupIds: new FormControl('', { nonNullable: true }),
      verifiedDonationPledgeIds: new FormControl('', { nonNullable: true }),
      verifiedApplicantApplicationIds: new FormControl('', { nonNullable: true }),
      verifiedGuardianApplicationIds: new FormControl('', { nonNullable: true }),
    },
    { validators: verifiedRecordSelectionValidator },
  );
}

function guid(value: number): string {
  return `${value.toString(16).padStart(8, '0')}-1234-1234-1234-123456789abc`;
}
