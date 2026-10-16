# 050 · Awarding badges

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [050-curriculum-authoring-award-badges.mp4](050-curriculum-authoring-award-badges.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Manual badges are recognition from a person. This video shows how one is awarded and what Waypoint records. It follows [the award badges design](../../detailed-designs/curriculum-authoring/award-badges/README.md) and the acceptance criteria of L2-CURRIC-005 in [the curriculum authoring L2 specification](../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain which badges can be awarded by hand.
- Describe the award steps.
- State what is recorded for an award.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which badges have an award button? | Manual badges only |
| What is recorded? | The awarder identity and the time, written to the audit log |
| How do automatic badges differ? | Given once by rule, with a notification |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/manual-card-1280.png` | A manual badge with zero awards |
| `images/dialog-1280.png` | The award window |
| `images/awarded-1280.png` | The awarded message |
| `images/count-1280.png` | The card with one award |

Captures are made by `frontend/video-capture/050-curriculum-authoring-award-badges.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why awards matter |
| 00:15-00:45 | Finding the badge | Manual badge card |
| 00:45-01:15 | Choosing the youth | Recipient list |
| 01:15-01:55 | Confirming the award | Message and audit |
| 01:55-02:30 | Automatic awards | Rule-based |
| 02:30-02:50 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/050-curriculum-authoring-award-badges.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/050-curriculum-authoring-award-badges --engine kokoro
node tools/video-build/build-video.mjs docs/videos/050-curriculum-authoring-award-badges
```

## Pitfalls

- The video shows the award from the administrator page; the spec also lets a mentor award manual badges, but the mentor screen is not shown. The audit and notification statements come from L2-CURRIC-005 criteria 2 and 3.
