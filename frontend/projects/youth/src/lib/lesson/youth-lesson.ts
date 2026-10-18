import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  inject,
  OnDestroy,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, LearningService, LessonReadingState, YouthLesson } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { Button, Shell, Alert, Card, CardBody, Scripture } from '@waypoint/components';

// Traces to: L2-LEARN-002
@Component({
  selector: 'w-youth-lesson',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Shell, RouterLink, Button, Alert, Card, CardBody, Scripture],
  templateUrl: './youth-lesson.html',
  styleUrl: './youth-lesson.css',
})
export class YouthLessonPage implements OnDestroy {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly doc = inject(DOCUMENT);
  private readonly lessonId = this.route.snapshot.paramMap.get('id') ?? '';
  private positionTimer: number | undefined;
  private readingStateInFlight = false;
  private queuedReadingState: LessonReadingState | undefined;

  protected readonly lesson = signal<YouthLesson | undefined>(undefined);
  protected readonly textSize = signal<LessonReadingState['textSize']>('medium');
  protected readonly bookmarked = signal(false);
  protected readonly mediaFailures = signal<Set<string>>(new Set());
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly readingStateStatus = signal<string | null>(null);

  constructor() {
    this.api.lesson(this.lessonId).subscribe({
      next: (lesson) => {
        this.lesson.set(lesson);
        this.textSize.set(lesson.readingState.textSize);
        this.bookmarked.set(lesson.readingState.bookmarked);
        if (lesson.readingState.position > 0) {
          this.doc.defaultView?.setTimeout(
            () => this.doc.defaultView?.scrollTo({ top: lesson.readingState.position }),
            0,
          );
        }
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected setTextSize(size: LessonReadingState['textSize']): void {
    this.textSize.set(size);
    this.saveReadingState();
  }

  protected toggleBookmark(): void {
    this.bookmarked.update((value) => !value);
    this.saveReadingState();
  }

  protected mediaFailed(mediaId: string): void {
    this.mediaFailures.update((current) => new Set(current).add(mediaId));
  }

  protected retryMedia(mediaId: string): void {
    this.mediaFailures.update((current) => {
      const updated = new Set(current);
      updated.delete(mediaId);
      return updated;
    });
  }

  protected markComplete(): void {
    if (
      this.saving() ||
      !this.doc.defaultView?.confirm(
        'Mark this lesson complete? Your next step will be the reflection.',
      )
    ) {
      return;
    }

    this.saving.set(true);
    this.api.completeLesson(this.lessonId).subscribe({
      next: () => void this.router.navigate(['/youth/lessons', this.lessonId, 'reflection']),
      error: (error: unknown) => {
        this.saving.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  @HostListener('window:scroll')
  protected rememberPosition(): void {
    const win = this.doc.defaultView;
    if (!win || !this.lesson()) return;
    if (this.positionTimer !== undefined) win.clearTimeout(this.positionTimer);
    this.positionTimer = win.setTimeout(() => this.saveReadingState(), 300);
  }

  ngOnDestroy(): void {
    const win = this.doc.defaultView;
    if (win && this.positionTimer !== undefined) win.clearTimeout(this.positionTimer);
  }

  private saveReadingState(): void {
    if (!this.lesson()) return;
    this.queuedReadingState = {
      textSize: this.textSize(),
      bookmarked: this.bookmarked(),
      position: Math.max(0, Math.round(this.doc.defaultView?.scrollY ?? 0)),
    };
    this.flushReadingState();
  }

  private flushReadingState(): void {
    if (this.readingStateInFlight || !this.queuedReadingState) return;

    const state = this.queuedReadingState;
    this.queuedReadingState = undefined;
    this.readingStateInFlight = true;
    this.readingStateStatus.set('Saving reading preferences…');
    this.api.saveReadingState(this.lessonId, state).subscribe({
      next: () => {
        this.readingStateInFlight = false;
        if (this.queuedReadingState) {
          this.flushReadingState();
        } else {
          this.readingStateStatus.set('Reading preferences saved.');
        }
      },
      error: (error: unknown) => {
        this.readingStateInFlight = false;
        this.readingStateStatus.set(null);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }
}
