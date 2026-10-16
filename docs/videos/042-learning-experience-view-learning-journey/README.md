# 042 · The learning journey

> **Runtime:** about 2 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [042-learning-experience-view-learning-journey.mp4](042-learning-experience-view-learning-journey.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The journey page is where a youth sees their roadmap. This video shows the current module, the locked modules and the badge collection. It follows [the view learning journey design](../../detailed-designs/learning-experience/view-learning-journey/README.md) and the acceptance criteria of L2-LEARN-009 and L2-LEARN-001 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how modules are shown: done, in progress or locked, with counts.
- Explain why a locked module cannot be opened early.
- Explain why badge counts match across screens.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the roadmap show? | Each module's state and a count of required items complete, with the current module highlighted |
| Why can a youth not skip a locked module? | It opens only after the earlier module is finished, decided by the server |
| Why do counts agree? | They come from a single source of truth |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/journey-1280.png` | The banner and progress bar |
| `images/roadmap-1280.png` | The current module card |
| `images/collection-1280.png` | The locked module and the badge collection |

Captures are made by `frontend/video-capture/042-learning-experience-view-learning-journey.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Three questions the page answers |
| 00:15-00:40 | My Journey | The banner |
| 00:40-01:00 | The roadmap | The current module |
| 01:00-01:25 | Locked modules | Why a module is locked |
| 01:25-01:50 | The badge collection | Counts and details link |
| 01:50-02:15 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 042
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/042-learning-experience-view-learning-journey --engine kokoro
node tools/video-build/build-video.mjs docs/videos/042-learning-experience-view-learning-journey
```

## Pitfalls

- The banner's heading is not readable in the capture (dark text on a dark banner); the narration describes only the visible line below it.
- Module and badge names on screen carry a random suffix because the capture uses seeded test data in a shared test database, which also adds extra badges.
