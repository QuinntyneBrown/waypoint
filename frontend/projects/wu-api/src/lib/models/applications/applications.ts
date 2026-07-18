/** Admin application-review models (L2-INTAKE-002). */

// Traces to: L2-INTAKE-002 (AC1 admin review queue)
export interface ApplicationSummary {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  isMinor: boolean;
  neighbourhood: string;
  trackInterest: string | null;
  status: string;
  createdAt: string;
}
