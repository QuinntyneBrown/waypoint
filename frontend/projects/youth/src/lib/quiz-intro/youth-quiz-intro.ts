import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, LearningService, QuizIntro } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';

// Traces to: L2-LEARN-004
@Component({
  selector: 'w-youth-quiz-intro',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink],
  templateUrl: './youth-quiz-intro.html',
  styles: `
    .quiz-facts {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(9rem, 1fr));
      gap: var(--w-s3);
    }
  `,
})
export class YouthQuizIntroPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly quizId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly intro = signal<QuizIntro | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.api.quizIntro(this.quizId).subscribe({
      next: (intro) => this.intro.set(intro),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected begin(): void {
    if (this.intro()?.attemptsExhausted) return;
    void this.router.navigate(['/youth/quizzes', this.quizId, 'attempt']);
  }
}
