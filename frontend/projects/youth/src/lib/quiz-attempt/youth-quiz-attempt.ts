import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiError, LearningService, YouthQuizAttempt, YouthQuizQuestion } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';

type QuizAnswers = Record<string, number[] | string>;

// Traces to: L2-LEARN-004
@Component({
  selector: 'w-youth-quiz-attempt',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell],
  templateUrl: './youth-quiz-attempt.html',
  styles: `
    .quiz-option {
      min-height: 2.75rem;
      display: flex;
      align-items: center;
      gap: var(--w-s3);
      padding: var(--w-s3);
      border: 1px solid var(--w-border);
      border-radius: var(--w-r-input);
      cursor: pointer;
    }
  `,
})
export class YouthQuizAttemptPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly doc = inject(DOCUMENT);
  private readonly quizId = this.route.snapshot.paramMap.get('id') ?? '';
  private saveInFlight = false;
  private queuedSave: { answersJson: string; position: number } | undefined;

  protected readonly attempt = signal<YouthQuizAttempt | undefined>(undefined);
  protected readonly currentIndex = signal(0);
  protected readonly answers = signal<QuizAnswers>({});
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly saveStatus = signal<string | null>(null);
  protected readonly submitting = signal(false);
  protected readonly currentQuestion = computed<YouthQuizQuestion | undefined>(
    () => this.attempt()?.questions[this.currentIndex()],
  );

  constructor() {
    this.api.startQuiz(this.quizId).subscribe({
      next: (attempt) => {
        this.attempt.set(attempt);
        this.currentIndex.set(attempt.currentQuestionIndex);
        this.answers.set(this.parseAnswers(attempt.answersJson));
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected isSelected(questionId: string, optionIndex: number): boolean {
    const answer = this.answers()[questionId];
    return Array.isArray(answer) && answer.includes(optionIndex);
  }

  protected choose(question: YouthQuizQuestion, optionIndex: number, checked: boolean): void {
    this.answers.update((current) => {
      const updated = { ...current };
      if (question.type === 'MultipleAnswer') {
        const existing = updated[question.id];
        const selected = new Set(Array.isArray(existing) ? existing : []);
        if (checked) selected.add(optionIndex);
        else selected.delete(optionIndex);
        updated[question.id] = [...selected].sort((a, b) => a - b);
      } else {
        updated[question.id] = [optionIndex];
      }
      return updated;
    });
    this.queueSave(this.currentIndex());
  }

  protected textAnswer(questionId: string): string {
    const answer = this.answers()[questionId];
    return typeof answer === 'string' ? answer : '';
  }

  protected writeText(questionId: string, value: string): void {
    this.answers.update((current) => ({ ...current, [questionId]: value }));
    this.queueSave(this.currentIndex());
  }

  protected move(delta: number): void {
    const view = this.attempt();
    if (!view) return;
    const next = Math.max(0, Math.min(view.questionCount - 1, this.currentIndex() + delta));
    this.currentIndex.set(next);
    this.queueSave(next);
  }

  protected answered(questionId: string): boolean {
    const answer = this.answers()[questionId];
    return typeof answer === 'string' ? answer.trim().length > 0 : (answer?.length ?? 0) > 0;
  }

  protected submit(): void {
    const view = this.attempt();
    if (
      !view ||
      this.submitting() ||
      this.saveInFlight ||
      !this.answered(this.currentQuestion()?.id ?? '') ||
      !this.doc.defaultView?.confirm(
        'Submit this quiz? Your answers cannot be changed for this attempt.',
      )
    ) {
      return;
    }

    this.submitting.set(true);
    this.api.submitQuiz(view.attemptId, JSON.stringify(this.answers())).subscribe({
      next: () => void this.router.navigate(['/youth/quiz-attempts', view.attemptId, 'result']),
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  private queueSave(position: number): void {
    this.queuedSave = { answersJson: JSON.stringify(this.answers()), position };
    this.flushSave();
  }

  private flushSave(): void {
    const view = this.attempt();
    if (!view || this.saveInFlight || !this.queuedSave) return;
    const pending = this.queuedSave;
    this.queuedSave = undefined;
    this.saveInFlight = true;
    this.saveStatus.set('Saving answer…');
    this.api.saveQuizAnswers(view.attemptId, pending.answersJson, pending.position).subscribe({
      next: () => {
        this.saveInFlight = false;
        if (this.queuedSave) this.flushSave();
        else this.saveStatus.set('Answer saved.');
      },
      error: (error: unknown) => {
        this.saveInFlight = false;
        this.saveStatus.set(null);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  private parseAnswers(json: string | null): QuizAnswers {
    if (!json) return {};
    try {
      return JSON.parse(json) as QuizAnswers;
    } catch {
      return {};
    }
  }
}
