/** Public marketing-site form models (anonymous). */

// Traces to: L2-PUB-003 (AC1 contact captured + acknowledged)
export interface SubmitContactRequest {
  name: string;
  email: string;
  topic: string;
  message: string;
}

export interface SubmitContactResult {
  contactRequestId: string;
  acknowledgement: string;
}
