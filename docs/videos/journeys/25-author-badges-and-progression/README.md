# 25 · Author badges and progression

> **Runtime:** about 3 min 46 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [25-author-badges-and-progression.mp4](25-author-badges-and-progression.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Growth in Waypoint is shown by stages, competencies and badges. This video follows Pat Morgan building a two-stage framework, defining an automatic badge and a manual badge, and awarding the manual badge to Amara Bell. It follows the designs for [the progression framework](../../../detailed-designs/curriculum-authoring/author-progression-framework/README.md), [authoring badges](../../../detailed-designs/curriculum-authoring/author-badges/README.md), [awarding badges](../../../detailed-designs/curriculum-authoring/award-badges/README.md), and the acceptance criteria in [the curriculum authoring L2 specification](../../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe stages and competencies and how order is saved.
- Name the five automatic badge rules.
- Explain the difference between automatic and manual badges.
- Explain what archiving a badge does.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is a competency? | One skill or habit that belongs to a stage |
| What are the five automatic rules? | Module completed, lesson completed, quiz passed, assignment approved, five session attendance streak |
| What is recorded when a manual badge is awarded? | Who awarded it and when, in the audit log |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/stage-1280.png` | A stage with competencies |
| `images/order-saved-1280.png` | The order saved message |
| `images/auto-form-1280.png` | The automatic badge form |
| `images/auto-card-1280.png` | The automatic badge card |
| `images/award-dialog-1280.png` | The award dialog |
| `images/manual-card-1280.png` | The manual badge card |

Captures are made by `frontend/video-capture/journeys/25-author-badges-and-progression.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:21 | Welcome | Waypoint measures growth, not only finished lessons. |
| 00:23-01:01 | Stages and competencies | Pat opens the progression framework page, which keeps growth stages in order, and presses add stage. |
| 01:03-01:29 | Changing the order | Stages can move. |
| 01:31-02:27 | An automatic badge | Now for badges. |
| 02:29-03:14 | A manual badge | Not every kind of recognition can be measured. |
| 03:16-03:46 | Things to remember | A stage has a name, a description, and a place in the order, and competencies belong to a stage. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/25
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/25-author-badges-and-progression --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/25-author-badges-and-progression
```

## Pitfalls

- Stage numbers are hidden in the stage image because they depend on other stages in the shared test data.
- Archiving is narrated but not captured.
