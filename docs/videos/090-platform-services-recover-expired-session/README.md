# 090 · Coming back after a session expires

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [090-platform-services-recover-expired-session.mp4](090-platform-services-recover-expired-session.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Sessions end after inactivity. This video shows the session-expired page and how a draft is restored after signing in again. It follows [the design](../../detailed-designs/platform-services/recover-expired-session/README.md) and the acceptance criteria of L2-PLATFORM-010 (criterion 1) and L2-AUTH-007 in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain what a person sees when a session expires.
- State what happens to work in progress.
- Describe where the person lands after signing in again.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the person see? | A page headed "You’ve been signed out" with a sign in again button |
| What happens to the draft? | It is kept, and restored after sign-in with the message "Draft restored after session expiry" |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/draft-1280.png` | The reflection before expiry |
| `images/expired-1280.png` | The session-expired page |
| `images/restored-1280.png` | The restored reflection |

Captures are made by `frontend/video-capture/090-platform-services-recover-expired-session.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why sessions end |
| 00:20-00:45 | Work in progress | Amara is writing a reflection |
| 00:45-01:20 | You have been signed out | The session-expired page |
| 01:20-02:00 | Back where she left off | Sign in and the draft returns |
| 02:00-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/090-platform-services-recover-expired-session.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/090-platform-services-recover-expired-session --engine kokoro
node tools/video-build/build-video.mjs docs/videos/090-platform-services-recover-expired-session
```

## Pitfalls

- The capture moves a fake browser clock forward to reach the inactivity limit, as the end-to-end tests do. The narration gives no timings.
- The reflection title in the capture comes from seeded demo data.
