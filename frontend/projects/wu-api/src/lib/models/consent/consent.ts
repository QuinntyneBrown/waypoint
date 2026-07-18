/** Youth participation gate. A youth with no guardian on record is treated as an adult (allowed). */
export interface ConsentStatus {
  participationAllowed: boolean;
}

export interface ConsentDelivery {
  channel: string;
  destination: string;
}

export interface RequestedConsent {
  youthName: string;
  guardianDestination: string;
  expiresAt: string;
}

export interface RequestedConsentCapture {
  guardianName: string;
  relationship: string;
  participationConsent: boolean;
  mediaConsent: boolean;
  mediaAllPhotosVideo: boolean;
  mediaGroupOnly: boolean;
  mediaFirstNameCaptions: boolean;
  signature: string;
}

export interface ConsentReceipt {
  consentId: string;
  referenceId: string;
}
