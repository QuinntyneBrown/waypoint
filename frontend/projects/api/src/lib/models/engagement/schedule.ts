// Traces to: L2-ENGAGE-005, L2-ENGAGE-008
export type RsvpResponse = 'going' | 'maybe' | "can't make it";

export interface YouthSession {
  sessionId: string;
  title: string;
  startsAt: string;
  durationMinutes: number | null;
  location: string | null;
  response: 'Going' | 'Maybe' | 'CantAttend' | null;
  note: string | null;
  reminderSet: boolean;
  reminderAt: string | null;
}

export interface SubmitRsvpInput {
  response: RsvpResponse;
  note: string | null;
  reminderSet: boolean;
}

export interface RsvpResult {
  id: string;
}
