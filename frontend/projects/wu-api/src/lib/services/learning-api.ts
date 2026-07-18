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

@Injectable({ providedIn: 'root' })
export class LearningApi {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  curriculum(trackId?: string): Observable<YouthCurriculum> {
    const suffix = trackId ? `/${encodeURIComponent(trackId)}` : '';
    return this.http.get<YouthCurriculum>(`${this.base}/api/me/curriculum${suffix}`);
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

  reflectionEditor(lessonId: string): Observable<ReflectionEditor> {
    return this.http.get<ReflectionEditor>(
      `${this.base}/api/me/reflections/lesson/${encodeURIComponent(lessonId)}`,
    );
  }

  saveReflection(input: SaveReflectionInput): Observable<SaveReflectionResult> {
    return this.http.post<SaveReflectionResult>(`${this.base}/api/me/reflections`, input);
  }

  quizIntro(quizId: string): Observable<QuizIntro> {
    return this.http.get<QuizIntro>(
      `${this.base}/api/me/quizzes/${encodeURIComponent(quizId)}`,
    );
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
}
