# 24 · Reset password

> **Runtime:** about 4 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [24-reset-password.mp4](24-reset-password.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Journey 24 in [the journey list](../../../journeys.md). It follows Amara from the forgot password link to signing in with a new password, and shows what an old link does. It follows [the recover password design](../../../detailed-designs/authentication/recover-password/README.md), with criteria in [the authentication L2 specification](../../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe each step from forgot password to a new password.
- Explain why an unknown email gets the same answer.
- State the link rules: one use, one hour.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the reset page reveal? | Nothing: same page and words whether or not the account exists; the address is shown only in part |
| What rules apply to the link? | Works once, expires after one hour |
| What happens after a reset? | Other sessions are signed out; the person signs in with the new password |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/sign-in-1280.png` | The forgot password link |
| `images/forgot-1280.png` | The reset your password page |
| `images/sent-1280.png` | The check your email page |
| `images/reset-1280.png` | The set a new password page |
| `images/success-1280.png` | Your password was updated |
| `images/youth-home-1280.png` | Home after signing in with the new password |
| `images/invalid-1280.png` | The invalid or expired link page |

Captures are made by `frontend/video-capture/journeys/24-reset-password.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why the path is safe |
| 00:20-01:30 | Asking for a link | Forgot password, check your email |
| 01:30-02:10 | Choosing a new password | The rule and the button |
| 02:10-02:50 | Back in | Password updated, sign in |
| 02:50-03:30 | When the link does not work | Invalid or expired |
| 03:30-04:00 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/24
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/24-reset-password --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/24-reset-password
```

## Pitfalls

- The one-hour expiry is the value the UI states; the other session sign-out is stated on the success page.
