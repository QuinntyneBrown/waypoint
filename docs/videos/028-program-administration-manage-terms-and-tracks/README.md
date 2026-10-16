# 028 · Terms and tracks

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [028-program-administration-manage-terms-and-tracks.mp4](028-program-administration-manage-terms-and-tracks.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/manage-terms-and-tracks/README.md) and the acceptance criteria of L2-ADMIN-006 (acceptance criteria 2, 3 and 5) in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the fields of a term and its three statuses.
- Explain what pausing a track does and does not do.
- State who can change terms and tracks.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What statuses can a term have? | Upcoming, Active, Completed |
| What happens to progress when a track is paused? | It is retained; only new enrolment and work assignment are blocked |
| Who can change terms and tracks? | Only a program administrator; changes are audit logged |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/term-form-1280.png` | Term form |
| `images/term-saved-1280.png` | Term list |
| `images/track-enabled-1280.png` | Enabled track |
| `images/track-paused-1280.png` | Paused track |

Captures are made by `frontend/video-capture/028-program-administration-manage-terms-and-tracks.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Terms and tracks |
| 00:20-01:15 | Creating a term | Form and list |
| 01:15-01:45 | Pausing a track | Track availability |
| 01:45-02:15 | What pausing means | Progress retained |
| 02:15-02:35 | Who can change it | Administrators, audit log |
| 02:35-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/028-program-administration-manage-terms-and-tracks.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/028-program-administration-manage-terms-and-tracks --engine kokoro
node tools/video-build/build-video.mjs docs/videos/028-program-administration-manage-terms-and-tracks
```

## Pitfalls

- The track panel capture shows only one track; other tracks in the shared demo database were hidden for the shot.
- The shared database also lists terms created by earlier capture runs.
