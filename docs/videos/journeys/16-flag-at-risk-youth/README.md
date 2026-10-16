# 16 · Flag at-risk youth

> **Runtime:** about 3 min 12 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [16-flag-at-risk-youth.mp4](16-flag-at-risk-youth.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Some young people drift away quietly. This video follows one mentor through the at-risk list: reading why a youth is on it, responding, resolving the flag with a reason, and raising a flag by hand. It draws on [manage at-risk flags](../../../detailed-designs/mentoring/manage-at-risk-flags/README.md) and L2-MENTOR-007 in [the mentoring L2 specification](../../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Say how a youth lands on the at-risk list.
- List the four ways a mentor can respond from a card.
- Name the resolution reasons and who can see flags.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How does a youth get flagged? | Automatically when a signal is met, or by the mentor by hand, with a reason |
| What can a mentor do from a flag card? | View the youth, log a check-in, send a message, or unflag |
| What does unflagging need? | A confirmation and a resolution reason; the flag is visible to the director and audited |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/list-1280.png` | Active flags with reasons, and the manual flag form |
| `images/message-1280.png` | The message box on the card |
| `images/unflag-1280.png` | The confirmation with a resolution reason |
| `images/resolved-1280.png` | No active flags |
| `images/raised-1280.png` | A flag raised by hand |

Captures are made by `frontend/video-capture/journeys/16-flag-at-risk-youth.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:21 | Welcome | Some young people quietly drift away. |
| 00:23-00:57 | The at-risk list | Grace opens At-risk. |
| 00:59-01:30 | Responding | Each card gives Grace four ways to respond. |
| 01:32-02:02 | Resolving a flag | Later, David is back on track. |
| 02:04-02:43 | Raising a flag by hand | Sometimes a concern is not caught by any automatic signal. |
| 02:45-03:12 | Things to remember | The list shows each youth's reasons, and who flagged them and when. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/16
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/16-flag-at-risk-youth --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/16-flag-at-risk-youth
```

## Pitfalls

- Threshold values for the automatic signals are configurable and marked "confirm with product owner" in the L2; the video does not state them.
- The seeded flag is shown as a mentor-raised concern on its card.
