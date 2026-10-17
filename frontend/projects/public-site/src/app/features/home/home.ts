import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, PublicContentView, PublicSiteService } from '@waypoint/api';
import { WButton, WAlert, WCard, WCardBody, WCardTitle } from '@waypoint/components';

// Traces to: L2-PUB-002
@Component({
  selector: 'w-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, WButton, WAlert, WCard, WCardBody, WCardTitle],
  templateUrl: './home.html',
})
export class Home {
  private readonly api = inject(PublicSiteService);

  protected readonly content = signal<PublicContentView | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getContent().subscribe({
      next: (content) => {
        this.content.set(content);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(ApiError.from(error).title);
        this.loading.set(false);
      },
    });
  }
}
