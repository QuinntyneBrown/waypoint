# 032 · Browsing the curriculum

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [032-learning-experience-browse-curriculum.mp4](032-learning-experience-browse-curriculum.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The curriculum page is where a youth finds what to do next. This video shows the modules, the lock rules and the empty state. It follows [the browse curriculum design](../../detailed-designs/learning-experience/browse-curriculum/README.md) and the acceptance criteria of L2-LEARN-001 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a module card shows.
- Explain what keeps a quiz or a later module locked, and where the reason is shown.
- Say what a youth sees for a track with nothing published.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What modules does a youth see? | Only published modules, with status and completion percentage |
| Why is something locked? | An earlier lesson or module is not complete; the unlock condition is shown and the server enforces it |
| What if a track has no modules? | A "not set up yet" message |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/map-1280.png` | Tracks, the first module, lesson status |
| `images/locked-1280.png` | Locked module quiz and locked second module |
| `images/empty-1280.png` | Track that is not set up yet |

Captures are made by `frontend/video-capture/032-learning-experience-browse-curriculum.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why the curriculum page matters |
| 00:15-00:55 | The curriculum page | Tracks, modules, percentages, lesson status |
| 00:55-01:35 | Gating | Locked quiz and module, server enforcement |
| 01:35-02:10 | More than one track | Switching tracks and the empty state |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/032-learning-experience-browse-curriculum.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/032-learning-experience-browse-curriculum --engine kokoro
node tools/video-build/build-video.mjs docs/videos/032-learning-experience-browse-curriculum
```

## Pitfalls

- Module and lesson names on screen include a random suffix because the capture uses seeded test data.
