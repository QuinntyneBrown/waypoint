import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, PublicContentView, PublicSiteApi } from '@wu/api';

// Traces to: L2-PUB-002
@Component({
  selector: 'wu-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './home.html',
})
export class Home {
  private readonly api = inject(PublicSiteApi);

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
