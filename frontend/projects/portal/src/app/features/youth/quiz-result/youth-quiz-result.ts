import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, LearningApi, YouthQuizResult } from '@w/api';
import { AuthStore } from '@w/auth';
import { WShell } from '@w/ui';

// Traces to: L2-LEARN-005
@Component({
  selector: 'w-youth-quiz-result',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink],
  templateUrl: './youth-quiz-result.html',
})
export class YouthQuizResultPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly attemptId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly result = signal<YouthQuizResult | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.api.quizResult(this.attemptId).subscribe({
      next: (result) => this.result.set(result),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected selected(indexes: number[], index: number): boolean {
    return indexes.includes(index);
  }

  protected correct(indexes: number[], index: number): boolean {
    return indexes.includes(index);
  }

  protected retake(quizId: string): void {
    void this.router.navigate(['/youth/quizzes', quizId, 'attempt']);
  }
}
