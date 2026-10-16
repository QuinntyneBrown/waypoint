# 055 · Keeping watch on at-risk youth

> **Runtime:** about 2 min 45 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [055-mentoring-manage-at-risk-flags.mp4](055-mentoring-manage-at-risk-flags.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

At-risk flags are how a quiet drift becomes an action. This video shows the list, the reasons, the actions on each card and the resolution reasons. It follows [the manage at-risk flags design](../../detailed-designs/mentoring/manage-at-risk-flags/README.md) and the acceptance criteria of L2-MENTOR-007 in [the mentoring L2 specification](../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Say how a youth ends up on the at-risk list.
- Name the actions a mentor can take on a flag.
- List the resolution reasons and say who can see flag changes.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How does a flag appear? | From an automatic signal, or raised by the mentor |
| What can a mentor do from a card? | View youth, log a check-in, message, unflag |
| What does unflagging need? | A confirmation and a resolution reason: Reconnected, Handed to director, Left program, Flagged in error |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/list-1280.png` | The list and the manual flag form |
| `images/message-1280.png` | The message box on a card |
| `images/unflag-1280.png` | The unflag confirmation |
| `images/resolved-1280.png` | The empty list after resolving |

Captures are made by `frontend/video-capture/055-mentoring-manage-at-risk-flags.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why early signals matter |
| 00:20-01:00 | The at-risk list | Reasons, manual flags |
| 01:00-01:30 | Taking action | The four actions, message |
| 01:30-02:25 | Resolving a flag | Reasons, empty state, audit |
| 02:25-02:45 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/055-mentoring-manage-at-risk-flags.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/055-mentoring-manage-at-risk-flags --engine kokoro
node tools/video-build/build-video.mjs docs/videos/055-mentoring-manage-at-risk-flags
```

## Pitfalls

- The signal thresholds are defaults whose configurability is marked confirm with product owner in the specification, so the narration names the kinds of signal and no numbers.
- The seeded flag text and date come from the seed data.
