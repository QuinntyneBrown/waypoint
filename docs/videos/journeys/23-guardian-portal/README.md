# 23 · The guardian portal

> **Runtime:** about 3 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [23-guardian-portal.mp4](23-guardian-portal.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Guardians need to see how their young person is doing, and what they have consented to, without access to anything private. This journey walks the portal from home, to progress and schedule, to the consent record. It draws on [the child progress design](../../../detailed-designs/guardian-portal/view-child-progress/README.md), [the consent record design](../../../detailed-designs/guardian-portal/view-consent-record/README.md) and [the announcements design](../../../detailed-designs/guardian-portal/read-announcements/README.md), and the guardian criteria in [the guardian portal L2 specification](../../../specs/guardian-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- Name what a guardian can see: progress, attendance, badges, schedule and shared mentor updates.
- Name what is left out on purpose.
- Say where consent and contact details live.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Can a guardian change progress? | No, the view is read only |
| Which mentor notes appear? | Only check-ins the mentor chose to share |
| Which quizzes appear? | Only outcomes a mentor approved |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/home-1280.png` | Your youth at a glance |
| `images/modules-attendance-1280.png` | Modules and attendance history |
| `images/schedule-updates-1280.png` | Upcoming schedule and shared mentor updates |
| `images/consent-1280.png` | The participation consent card |

Captures are made by `frontend/video-capture/journeys/23-guardian-portal.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | What the portal is for |
| 00:20-00:55 | Home at a glance | Tiles and what is next |
| 00:55-01:55 | Progress and attendance | Modules, attendance, quizzes, badges |
| 01:55-02:35 | Schedule and shared updates | Schedule and shared check-ins |
| 02:35-03:10 | Consent | The record and the News page |
| 03:10-03:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/23
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/23-guardian-portal --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/23-guardian-portal
```

## Pitfalls

- The News page is mentioned but not captured; it is covered by the reading announcements video.
- Seeded session and module names carry a random suffix on screen.
