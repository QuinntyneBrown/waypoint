# 047 · Authoring the progression framework

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [047-curriculum-authoring-author-progression-framework.mp4](047-curriculum-authoring-author-progression-framework.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The framework defines what growth means in Waypoint. This video shows how stages and competencies are added, ordered and removed. It follows [the author progression framework design](../../detailed-designs/curriculum-authoring/author-progression-framework/README.md) and the acceptance criteria of L2-CURRIC-006 in [the curriculum authoring L2 specification](../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe a stage and a competency.
- Show how stages are reordered.
- Explain what the framework feeds.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a stage hold? | A name, a description and a place in the order |
| How is order changed? | Move stage up and down; the order is saved |
| What uses the framework? | Each youth current stage, mentor competency assessment and reports |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/stage-dialog-1280.png` | The add stage form |
| `images/competency-dialog-1280.png` | The add competency form |
| `images/stage-one-1280.png` | A stage with two competencies |
| `images/order-saved-1280.png` | The order saved message |
| `images/removed-1280.png` | The competency removed message |

Captures are made by `frontend/video-capture/047-curriculum-authoring-author-progression-framework.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | What the framework is |
| 00:20-00:55 | Adding a stage | Name and description |
| 00:55-01:25 | Adding competencies | Per stage |
| 01:25-01:50 | Changing the order | Move up and down |
| 01:50-02:10 | Removing a competency | Remove |
| 02:10-02:35 | Why it matters | What it drives |
| 02:35-03:00 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/047-curriculum-authoring-author-progression-framework.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/047-curriculum-authoring-author-progression-framework --engine kokoro
node tools/video-build/build-video.mjs docs/videos/047-curriculum-authoring-author-progression-framework
```

## Pitfalls

- The stage number label is hidden in one capture because it depends on stages from other demo runs.
