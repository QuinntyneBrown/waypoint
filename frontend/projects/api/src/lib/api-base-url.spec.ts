import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from './api-base-url';

describe('API_BASE_URL', () => {
  it('defaults to an empty string (same-origin, relative requests)', () => {
    expect(TestBed.inject(API_BASE_URL)).toBe('');
  });

  it('is overridable through DI', () => {
    TestBed.configureTestingModule({
      providers: [{ provide: API_BASE_URL, useValue: 'http://127.0.0.1:5211' }],
    });
    expect(TestBed.inject(API_BASE_URL)).toBe('http://127.0.0.1:5211');
  });
});
