import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  LessonReadingState,
  ReflectionEditor,
  SaveReflectionInput,
  SaveReflectionResult,
  YouthCurriculum,
  YouthLesson,
} from '../models/learning/curriculum';
import { QuizIntro, YouthQuizAttempt, YouthQuizResult } from '../models/learning/quiz';
import {
  AssignmentBrief,
  AssignmentUpload,
  SubmissionResult,
  SubmitAssignmentInput,
  YouthSubmissionStatus,
} from '../models/learning/assignment';
import {
  CodingLesson,
  CodingProgressResult,
  SaveCodingProgressInput,
  SubmitCodingProjectInput,
} from '../models/learning/coding';
import { YouthJourney } from '../models/learning/journey';
import { YouthProgress } from '../models/learning/progress';
import { YouthHomeView } from '../models/learning/home';

@Injectable({ providedIn: 'root' })
export class LearningApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  home(): Observable<YouthHomeView> {
    return this.http.get<YouthHomeView>(`${this.base}/api/me/home`);
  }

  curriculum(trackId?: string): Observable<YouthCurriculum> {
    const suffix = trackId ? `/${encodeURIComponent(trackId)}` : '';
    return this.http.get<YouthCurriculum>(`${this.base}/api/me/curriculum${suffix}`);
  }

  journey(): Observable<YouthJourney> {
    return this.http.get<YouthJourney>(`${this.base}/api/me/journey`);
  }

  progress(): Observable<YouthProgress> {
    return this.http.get<YouthProgress>(`${this.base}/api/me/progress`);
  }

  lesson(lessonId: string): Observable<YouthLesson> {
    return this.http.get<YouthLesson>(
      `${this.base}/api/me/lessons/${encodeURIComponent(lessonId)}`,
    );
  }

  saveReadingState(lessonId: string, state: LessonReadingState): Observable<void> {
    return this.http.put<void>(
      `${this.base}/api/me/lessons/${encodeURIComponent(lessonId)}/reading-state`,
      state,
    );
  }

  completeLesson(lessonId: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/me/lessons/${encodeURIComponent(lessonId)}/complete`,
      {},
    );
  }

  codingLesson(lessonId: string): Observable<CodingLesson> {
    return this.http.get<CodingLesson>(
      `${this.base}/api/me/coding-lessons/${encodeURIComponent(lessonId)}`,
    );
  }

  saveCodingProgress(
    lessonId: string,
    input: SaveCodingProgressInput,
  ): Observable<CodingProgressResult> {
    return this.http.put<CodingProgressResult>(
      `${this.base}/api/me/coding-lessons/${encodeURIComponent(lessonId)}/progress`,
      input,
    );
  }

  submitCodingProject(
    lessonId: string,
    input: SubmitCodingProjectInput,
  ): Observable<SubmissionResult> {
    return this.http.post<SubmissionResult>(
      `${this.base}/api/me/coding-lessons/${encodeURIComponent(lessonId)}/submissions`,
      input,
    );
  }

  reflectionEditor(lessonId: string): Observable<ReflectionEditor> {
    return this.http.get<ReflectionEditor>(
      `${this.base}/api/me/reflections/lesson/${encodeURIComponent(lessonId)}`,
    );
  }

  saveReflection(input: SaveReflectionInput): Observable<SaveReflectionResult> {
    return this.http.post<SaveReflectionResult>(`${this.base}/api/me/reflections`, input);
  }

  quizIntro(quizId: string): Observable<QuizIntro> {
    return this.http.get<QuizIntro>(`${this.base}/api/me/quizzes/${encodeURIComponent(quizId)}`);
  }

  startQuiz(quizId: string): Observable<YouthQuizAttempt> {
    return this.http.post<YouthQuizAttempt>(
      `${this.base}/api/me/quizzes/${encodeURIComponent(quizId)}/attempts`,
      {},
    );
  }

  saveQuizAnswers(
    attemptId: string,
    answersJson: string,
    currentQuestionIndex: number,
  ): Observable<void> {
    return this.http.put<void>(
      `${this.base}/api/me/attempts/${encodeURIComponent(attemptId)}/answers`,
      { answersJson, currentQuestionIndex },
    );
  }

  submitQuiz(attemptId: string, answersJson: string): Observable<YouthQuizResult> {
    return this.http.post<YouthQuizResult>(
      `${this.base}/api/me/attempts/${encodeURIComponent(attemptId)}/submit`,
      { answersJson },
    );
  }

  quizResult(attemptId: string): Observable<YouthQuizResult> {
    return this.http.get<YouthQuizResult>(
      `${this.base}/api/me/attempts/${encodeURIComponent(attemptId)}`,
    );
  }

  assignmentBrief(assignmentId: string): Observable<AssignmentBrief> {
    return this.http.get<AssignmentBrief>(
      `${this.base}/api/me/assignments/${encodeURIComponent(assignmentId)}`,
    );
  }

  uploadAssignmentPhoto(assignmentId: string, file: File): Observable<AssignmentUpload> {
    const data = new FormData();
    data.append('file', file, file.name);
    return this.http.post<AssignmentUpload>(
      `${this.base}/api/me/assignments/${encodeURIComponent(assignmentId)}/uploads`,
      data,
    );
  }

  submitAssignment(
    assignmentId: string,
    input: SubmitAssignmentInput,
  ): Observable<SubmissionResult> {
    return this.http.post<SubmissionResult>(
      `${this.base}/api/me/assignments/${encodeURIComponent(assignmentId)}/submissions`,
      input,
    );
  }

  submissionStatus(submissionId: string): Observable<YouthSubmissionStatus> {
    return this.http.get<YouthSubmissionStatus>(
      `${this.base}/api/me/submissions/${encodeURIComponent(submissionId)}`,
    );
  }

  submissionFile(path: string): Observable<Blob> {
    return this.http.get(`${this.base}${path}`, { responseType: 'blob' });
  }

  resubmitAssignment(
    submissionId: string,
    input: { bodyText: string | null; photoUrl: string | null; linkUrl: string | null },
  ): Observable<void> {
    return this.http.post<void>(
      `${this.base}/api/me/submissions/${encodeURIComponent(submissionId)}/resubmit`,
      input,
    );
  }
}
