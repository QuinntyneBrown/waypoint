# 030 · A person's record

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [030-program-administration-view-person-record.mp4](030-program-administration-view-person-record.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/view-person-record/README.md) and the acceptance criteria of L2-ADMIN-002 (acceptance criterion 1) in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the six tabs on a youth record.
- Say what guardian and consent information an administrator can see.
- Explain what the Notes tab leaves out.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which tabs does a youth record have? | Profile, Guardian and consent, Progress, Attendance, Notes, Account |
| What does the consent block show? | Validity, participation and media status, signed date and valid-until date |
| What never appears in Notes? | Private prayer requests |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/profile-1280.png` | Profile tab |
| `images/guardian-1280.png` | Guardian and consent |
| `images/progress-1280.png` | Progress |
| `images/attendance-1280.png` | Attendance |
| `images/notes-1280.png` | Notes |

Captures are made by `frontend/video-capture/030-program-administration-view-person-record.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Behind every row |
| 00:15-00:50 | The tabs | Six tabs and the Profile |
| 00:50-01:25 | Guardian and consent | Contact and consent record |
| 01:25-02:00 | Progress and attendance | Lessons and sessions |
| 02:00-02:35 | Notes and privacy | Summaries only |
| 02:35-02:55 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/030-program-administration-view-person-record.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/030-program-administration-view-person-record --engine kokoro
node tools/video-build/build-video.mjs docs/videos/030-program-administration-view-person-record
```

## Pitfalls

- The record belongs to a seeded demo youth, so the name on screen is a generated demo name.
