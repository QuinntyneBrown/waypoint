# 18 · Take attendance

> **Runtime:** about 3 min 29 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [18-take-attendance.mp4](18-take-attendance.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Attendance feeds rates and badge streaks, and sessions do not always have a signal. This video follows one mentor marking a roster, saving it, and then saving again with no connection. It draws on [record attendance](../../../detailed-designs/engagement/record-attendance/README.md) and [sync offline attendance](../../../detailed-designs/engagement/sync-offline-attendance/README.md), and on L2-ENGAGE-002 and L2-ENGAGE-003 in [the engagement L2 specification](../../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the four marks, the tally and the quick actions.
- Explain what saving makes the record count toward.
- Describe what happens offline and when two edits conflict.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What marks can a mentor give? | Present, late, excused or absent, with an optional note and a running tally |
| What happens when there is no connection? | A banner shows, the marks are saved on the device with a pending sync badge, and they upload by themselves |
| What if someone else edited the session meanwhile? | The conflict is shown and the mentor chooses; nothing is overwritten silently |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/roster-1280.png` | The roster with nobody marked |
| `images/marked-1280.png` | Late with a note and the tally |
| `images/saved-1280.png` | Attendance saved |
| `images/queued-1280.png` | Offline banner, pending sync badge, saved offline |
| `images/synced-1280.png` | All attendance changes synced |

Captures are made by `frontend/video-capture/journeys/18-take-attendance.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:17 | Welcome | Attendance is one of the most important records a youth program keeps. |
| 00:19-00:50 | The roster | From the calendar, Grace presses Take attendance on the session. |
| 00:52-01:25 | Marking | Each row has four buttons. |
| 01:27-01:53 | Saving | Grace presses Save attendance, and the page says, attendance saved. |
| 01:55-03:01 | When the connection drops | Sessions do not always happen where the signal is good. |
| 03:03-03:29 | Things to remember | Each youth is marked present, late, excused or absent, with a running tally and an optional note. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/18
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/18-take-attendance --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/18-take-attendance
```

## Pitfalls

- The conflict screen is described in the narration but not captured; it is shown in the offline sync design video.
- The seeded session carries a short random suffix in its title.
