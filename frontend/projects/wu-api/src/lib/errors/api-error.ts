import { HttpErrorResponse } from '@angular/common/http';

/** A single field validation error from the FluentValidation pipeline. */
export interface FieldError {
  propertyName: string;
  errorMessage: string;
}

/** Uniform backend error envelope: `{ title, errors? }`. */
export interface ProblemResponse {
  title: string;
  errors?: FieldError[];
}

/** Normalised API error carrying the HTTP status and the parsed envelope. */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly title: string,
    readonly fieldErrors: FieldError[] = [],
  ) {
    super(title);
    this.name = 'ApiError';
  }

  /** True when the envelope carried a field error for the given property (case-insensitive). */
  fieldError(property: string): string | undefined {
    return this.fieldErrors.find((e) => e.propertyName.toLowerCase() === property.toLowerCase())
      ?.errorMessage;
  }

  static from(err: unknown): ApiError {
    if (err instanceof ApiError) return err;
    if (err instanceof HttpErrorResponse) {
      const body = err.error as ProblemResponse | string | null;
      if (body && typeof body === 'object') {
        return new ApiError(err.status, body.title ?? defaultTitle(err.status), body.errors ?? []);
      }
      return new ApiError(err.status, defaultTitle(err.status));
    }
    return new ApiError(0, 'Something went wrong. Please try again.');
  }
}

function defaultTitle(status: number): string {
  switch (status) {
    case 0:
      return 'Can’t reach the server. Check your connection and try again.';
    case 401:
      return 'Your session is no longer valid. Please sign in again.';
    case 403:
      return 'You don’t have access to that.';
    case 404:
      return 'We couldn’t find that.';
    case 500:
      return 'Something went wrong on our end. Please try again.';
    default:
      return 'Something went wrong. Please try again.';
  }
}
