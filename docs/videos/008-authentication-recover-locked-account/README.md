# 008 · Recovering a locked account

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [008-authentication-recover-locked-account.mp4](008-authentication-recover-locked-account.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

It follows [the recover locked account design](../../detailed-designs/authentication/recover-locked-account/README.md) and the acceptance criteria of L2-AUTH-006 and L2-AUTH-001 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what happens after five wrong passwords.
- Name the two recovery paths that stay open during a lock.
- Explain how an account becomes usable again.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| When does an account lock? | After five failed sign-ins; the screen states about fifteen minutes |
| What can a locked person do? | Reset the password or contact the mentor |
| How does the lock end? | It lifts on its own when the time has passed, with no manual unlock |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/fourth-miss-1280.png` | The sign-in page after a fourth miss |
| `images/locked-1280.png` | The account-locked screen |
| `images/reset-1280.png` | The reset your password page reached from the lock screen |
| `images/contact-1280.png` | The public contact page |

Captures are made by `frontend/video-capture/008-authentication-recover-locked-account.capture.ts`.

## Run sheet

| Segment | Content |
| --- | --- |
| Welcome | Why a lock exists |
| Five misses | Imani misses four times, then the lock screen |
| Two ways back | Reset password and contact your mentor |
| Trying again | The lock lifts by itself |
| Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/008-authentication-recover-locked-account.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/008-authentication-recover-locked-account --engine kokoro
node tools/video-build/build-video.mjs docs/videos/008-authentication-recover-locked-account
```

## Pitfalls

- The lock threshold and duration are configured values; five attempts and fifteen minutes are the defaults. The contact your mentor link opens the public contact page, not a direct message to a named mentor.
