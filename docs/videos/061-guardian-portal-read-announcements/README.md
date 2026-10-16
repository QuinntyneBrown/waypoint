# 061 · Reading announcements

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [061-guardian-portal-read-announcements.mp4](061-guardian-portal-read-announcements.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Families need program and cohort news in one place, and need to tell what they have not read yet. It follows [the design](../../detailed-designs/guardian-portal/read-announcements/README.md) and the acceptance criteria of L2-GUARDIAN-002 in [the L2 specification](../../specs/guardian-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain which announcements a guardian sees.
- Describe the unread badge and Mark all read.
- Recognise the empty state.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which announcements does a guardian see? | Only those targeted to guardians or to their child's cohort; youth-only messages are not shown. |
| What does Mark all read do? | Clears the unread state for that guardian only. |
| What if there is no news? | The page says no announcements yet and that news will appear there. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/unread-1280.png` | Unread messages and the Mark all read button |
| `images/read-1280.png` | Badges and button are gone |
| `images/empty-1280.png` | An empty page explains itself |

Captures are made by `frontend/video-capture/061-guardian-portal-read-announcements.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why the news page exists |
| 00:20-01:05 | The news page | Two guardian messages, one youth-only message hidden |
| 01:05-01:40 | Unread and mark all read | Badges and the button |
| 01:40-02:05 | When there is nothing yet | Empty state |
| 02:05-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/061-guardian-portal-read-announcements.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/061-guardian-portal-read-announcements --engine kokoro
node tools/video-build/build-video.mjs docs/videos/061-guardian-portal-read-announcements
```

## Pitfalls

- Announcement dates are the date of the capture run.
