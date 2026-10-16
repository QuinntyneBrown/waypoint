# 071 · Recording attendance

> **Runtime:** about 2 min 52 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [071-engagement-record-attendance.mp4](071-engagement-record-attendance.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Attendance is the program core record. This video shows the roster, the four statuses, the tally, notes, bulk actions and saving. It follows [the record attendance design](../../detailed-designs/engagement/record-attendance/README.md) and the acceptance criteria of L2-ENGAGE-002 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Mark a youth present, late, excused or absent.
- Read the running tally.
- Say what saving attendance feeds.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which statuses can a mentor use? | Present, late, excused and absent; volunteers use present and absent |
| What speeds up a big roster? | All present, then adjust single rows; clear resets |
| What does saved attendance drive? | Attendance rates and badge streak rules |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/roster-1280.png` | The roster before marking |
| `images/marked-1280.png` | Amara marked late with a note |
| `images/saved-1280.png` | Attendance saved |

Captures are made by `frontend/video-capture/071-engagement-record-attendance.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | Welcome |
| 00:16-00:54 | The roster | The roster |
| 00:54-01:42 | Marking each youth | Marking each youth |
| 01:42-02:25 | Saving | Saving |
| 02:25-02:52 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 071
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/071-engagement-record-attendance --engine kokoro
node tools/video-build/build-video.mjs docs/videos/071-engagement-record-attendance
```

## Pitfalls

- The volunteer attendance sheet is only mentioned, not shown; it has its own video.
