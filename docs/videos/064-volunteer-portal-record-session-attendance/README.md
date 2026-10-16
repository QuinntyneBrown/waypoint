# 064 · Recording session attendance

> **Runtime:** about 3 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [064-volunteer-portal-record-session-attendance.mp4](064-volunteer-portal-record-session-attendance.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Taking attendance is the volunteer's main task, and it must work with a poor connection and without exposing a young person's record. This video shows the names-only roster, the bulk action and tallies, saving, and the offline queue. It follows [the attendance design](../../detailed-designs/volunteer-portal/record-session-attendance/README.md) and the acceptance criteria of L2-VOLUNTEER-001 in [the volunteer portal L2 specification](../../specs/volunteer-portal/L2.md), with L2-NFR-004 for the offline queue.

## Learning objectives

By the end, the viewer can:

- Say what a volunteer sees on the attendance page, and what is left out.
- Describe how all present, the tiles and the save button work together.
- Explain what happens to a save made while offline.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a volunteer see for each young person? | The name and a present and an absent control; no progress, notes or flags |
| When can attendance be saved? | Once every young person is marked |
| What happens offline? | The save is queued on the device, shown as waiting to sync, and sent once on reconnect; a clash with a newer save asks the volunteer to choose |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/roster-1280.png` | The empty roster |
| `images/marked-1280.png` | Everyone marked, tiles updated |
| `images/saved-1280.png` | The saved message |
| `images/offline-1280.png` | The offline bar and queued message |
| `images/synced-1280.png` | The synced message |

Captures are made by `frontend/video-capture/064-volunteer-portal-record-session-attendance.capture.ts`. The seed data adds a short code after each young person's first name.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Sam takes attendance |
| 00:20-01:05 | The roster | Names only |
| 01:05-02:00 | Marking everyone | All present, one absent, tiles, save rule |
| 02:00-02:20 | Saving | Saved message |
| 02:20-03:00 | When the connection drops | Offline bar, queue, sync, conflict |
| 03:00-03:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/064-volunteer-portal-record-session-attendance.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/064-volunteer-portal-record-session-attendance --engine kokoro
node tools/video-build/build-video.mjs docs/videos/064-volunteer-portal-record-session-attendance
```

## Pitfalls

- The conflict prompt (choose which attendance to keep) is described but not captured.
- The names on screen are synthetic seed names, not the cast's full records.
