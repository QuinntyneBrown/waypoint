import { describe, expect, it } from 'vitest';
import { isPasswordCompliant, maskEmail } from './password-reset';

describe('isPasswordCompliant', () => {
  it('requires 8+ chars with a letter and a number', () => {
    expect(isPasswordCompliant('Password123')).toBe(true);
    expect(isPasswordCompliant('abc12')).toBe(false); // too short
    expect(isPasswordCompliant('password')).toBe(false); // no digit
    expect(isPasswordCompliant('12345678')).toBe(false); // no letter
  });
});

describe('maskEmail', () => {
  it('keeps the first char and domain, masking the rest', () => {
    expect(maskEmail('jayden@example.com')).toBe('j•••@example.com');
    expect(maskEmail('e2e-x@wordup.test')).toBe('e•••@wordup.test');
  });

  it('returns the input unchanged when there is no local part', () => {
    expect(maskEmail('@nope')).toBe('@nope');
    expect(maskEmail('nodomain')).toBe('nodomain');
  });
});
