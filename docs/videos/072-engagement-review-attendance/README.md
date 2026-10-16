# 072 · Reviewing attendance

> **Runtime:** about 2 min 48 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [072-engagement-review-attendance.mp4](072-engagement-review-attendance.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Staff review attendance to spot sessions and youth that need attention. This video shows the by-session and by-youth views, filters and exports. It follows [the review attendance design](../../detailed-designs/engagement/review-attendance/README.md) and the acceptance criteria of L2-ENGAGE-004 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Read the by-session view.
- Read the by-youth view and its flag.
- Say how filters and exports relate.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the by-session view show? | Present count, marked count, rate and a status such as saved or low turnout |
| What does the by-youth view show? | Attended, late, excused and absent counts, a rate, a trend and a below threshold flag |
| Do exports follow the filters? | Yes, CSV and PDF honour the filters |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/by-session-1280.png` | Attendance history by session |
| `images/by-youth-1280.png` | Attendance history by youth |

Captures are made by `frontend/video-capture/072-engagement-review-attendance.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Welcome |
| 00:18-01:04 | By session | By session |
| 01:04-01:41 | By youth | By youth |
| 01:41-02:20 | Filtering and exporting | Filtering and exporting |
| 02:20-02:48 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 072
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/072-engagement-review-attendance --engine kokoro
node tools/video-build/build-video.mjs docs/videos/072-engagement-review-attendance
```

## Pitfalls

- The seventy percent flag is the label shown by the screen; the narration says only what the label says.
