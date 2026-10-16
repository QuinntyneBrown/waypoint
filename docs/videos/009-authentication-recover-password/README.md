# 009 · Recovering a password

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [009-authentication-recover-password.mp4](009-authentication-recover-password.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

It follows [the recover password design](../../detailed-designs/authentication/recover-password/README.md) and the acceptance criteria of L2-AUTH-005, L2-AUTH-008 and L2-AUTH-012 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Walk through requesting a reset link and setting a new password.
- Explain why a wrong email gets the same page as a right one.
- Say what happens to an expired or used link.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Does the page reveal whether an email has an account? | No: the same confirmation appears either way, and the email is sent only if the account exists |
| How long does a link last? | One hour, and it works once |
| What happens to other sessions? | They are signed out when the password is updated |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/forgot-1280.png` | Requesting a reset link |
| `images/sent-1280.png` | The check your email confirmation |
| `images/reset-1280.png` | Choosing the new password |
| `images/success-1280.png` | The password updated page |
| `images/invalid-1280.png` | The invalid or expired link page |

Captures are made by `frontend/video-capture/009-authentication-recover-password.capture.ts`.

## Run sheet

| Segment | Content |
| --- | --- |
| Welcome | Why it is safe |
| Asking for a link | Forgot password, masked confirmation |
| Choosing a new password | The rule and the matching boxes |
| All set | Success and signed-out sessions |
| When the link does not work | Used or expired link |
| Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/009-authentication-recover-password.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/009-authentication-recover-password --engine kokoro
node tools/video-build/build-video.mjs docs/videos/009-authentication-recover-password
```

## Pitfalls

- The one-hour link life comes from the screen text; the specification only asks for an expired-link state. Keep the email address masked on screen.
