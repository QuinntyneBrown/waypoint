import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import { MentorAssignmentOverview } from '../models/mentor-assignments/mentor-assignments';

@Injectable({ providedIn: 'root' })
export class MentorAssignmentsApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  overview(): Observable<MentorAssignmentOverview> {
    return this.http.get<MentorAssignmentOverview>(`${this.base}/api/mentor-assignments`);
  }

  assignYouth(youthId: string, mentorId: string): Observable<void> {
    return this.http.put<void>(`${this.base}/api/mentor-assignments/youth/${youthId}`, {
      mentorId,
    });
  }

  setCohortMentors(
    cohortId: string,
    leadMentorId: string,
    supportingMentorIds: string[],
  ): Observable<void> {
    return this.http.put<void>(`${this.base}/api/mentor-assignments/cohorts/${cohortId}`, {
      leadMentorId,
      supportingMentorIds,
    });
  }
}
