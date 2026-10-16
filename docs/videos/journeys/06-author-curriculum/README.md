# 06 · Author curriculum

> **Runtime:** about 3 min 37 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [06-author-curriculum.mp4](06-author-curriculum.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Nothing reaches a young person until an administrator has written it. This video follows Pat Morgan through the whole authoring path: a track, ordered modules with a prerequisite, a lesson with leader guide notes, and publishing from the lesson up to the track. It follows the designs for [program tracks and modules](../../../detailed-designs/curriculum-authoring/author-tracks-and-modules/README.md), [lessons](../../../detailed-designs/curriculum-authoring/author-lessons/README.md), and the acceptance criteria in [the curriculum authoring L2 specification](../../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how a track, its modules and its lessons fit together.
- Explain why a prerequisite module can never move below the module that needs it.
- State which lesson fields youth never see.
- Explain that nothing is visible to youth until published.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How is a curriculum built? | Track first, then modules in order, then lessons inside modules |
| What stops a bad module order? | A prerequisite must remain before its dependent module |
| What do youth never see? | Leader guide notes |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/track-dialog-1280.png` | The create track dialog |
| `images/module-form-1280.png` | The module form with a prerequisite |
| `images/prerequisite-blocked-1280.png` | The refused move |
| `images/lesson-dialog-1280.png` | The add lesson dialog |
| `images/lesson-published-1280.png` | The published lesson card |
| `images/track-published-1280.png` | The published track |

Captures are made by `frontend/video-capture/journeys/06-author-curriculum.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:21 | Welcome | Before a young person can learn anything in Waypoint, someone has to write what they will learn. |
| 00:23-01:02 | Creating a track | Pat opens the curriculum builder and presses new track. |
| 01:04-01:46 | Adding modules in order | Modules come next. |
| 01:48-02:35 | Writing a lesson | Now for the lesson itself. |
| 02:37-03:09 | Publishing | Everything Pat has written is hidden until it is published. |
| 03:11-03:37 | Things to remember | A curriculum is built from the top down. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/06
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/06-author-curriculum --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/06-author-curriculum
```

## Pitfalls

- Captured names carry a short random tag so the capture can be re-run against the shared test data.
- Archiving a lesson is not shown here; it is covered in the lessons video.
