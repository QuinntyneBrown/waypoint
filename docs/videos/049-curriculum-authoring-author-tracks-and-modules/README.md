# 049 · Authoring tracks and modules

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [049-curriculum-authoring-author-tracks-and-modules.mp4](049-curriculum-authoring-author-tracks-and-modules.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Tracks and modules give the curriculum its shape. This video shows creating a track, adding modules with prerequisites, ordering and publishing. It follows [the author tracks and modules design](../../detailed-designs/curriculum-authoring/author-tracks-and-modules/README.md) and the acceptance criteria of L2-CURRIC-001 in [the curriculum authoring L2 specification](../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a track and a module hold.
- Explain the prerequisite rule when reordering.
- Describe the draft and published states.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What identifies a track? | A name and a unique key; tracks are data-driven |
| What stops a reorder? | A prerequisite may not come after the module that needs it |
| When do youth see a module? | Once it is published, in the cohorts it is assigned to |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/track-dialog-1280.png` | The create track form |
| `images/track-picker-1280.png` | The new draft track |
| `images/module-form-1280.png` | The add module form with a prerequisite |
| `images/module-draft-1280.png` | A draft module |
| `images/prerequisite-blocked-1280.png` | The refused move |
| `images/module-published-1280.png` | A published module |
| `images/track-published-1280.png` | The published track |

Captures are made by `frontend/video-capture/049-curriculum-authoring-author-tracks-and-modules.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Tracks and modules |
| 00:20-01:10 | Creating a track | Name, key, description |
| 01:10-01:50 | Adding modules | Prerequisite |
| 01:50-02:20 | Putting modules in order | Reorder rules |
| 02:20-02:55 | Publishing | Module and track |
| 02:55-03:20 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/049-curriculum-authoring-author-tracks-and-modules.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/049-curriculum-authoring-author-tracks-and-modules --engine kokoro
node tools/video-build/build-video.mjs docs/videos/049-curriculum-authoring-author-tracks-and-modules
```

## Pitfalls

- The In review step in L2-CURRIC-001 criterion 4 is not shown; the portal offers draft and published only. The cohort visibility statement comes from the same criterion.
