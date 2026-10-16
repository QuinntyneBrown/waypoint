# 14 · View journey and progress

> **Runtime:** about 3 min 14 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [14-view-journey-and-progress.mp4](14-view-journey-and-progress.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A youth should always know where they stand. This video follows one youth across the journey page, the badges page and My progress, using [view learning journey](../../../detailed-designs/learning-experience/view-learning-journey/README.md), [view badges](../../../detailed-designs/learning-experience/view-badges/README.md) and [review personal progress](../../../detailed-designs/learning-experience/review-personal-progress/README.md), and L2-LEARN-009 and L2-LEARN-010 in [the learning experience L2 specification](../../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Read the journey page: current module, roadmap, locked modules.
- Explain what an earned and an unearned badge show.
- Name the figures on My progress and where competency levels come from.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why can a youth not skip a locked module? | The next module opens only when the one before it is finished; the server decides |
| Where do competency levels come from? | From the mentor assessments, not from the youth |
| Do badge counts agree between pages? | Yes, they come from one source |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/journey-1280.png` | Journey banner and progress bar |
| `images/roadmap-1280.png` | Roadmap with a locked module |
| `images/collection-1280.png` | Badge collection counts |
| `images/badges-1280.png` | Badges page, earned badge |
| `images/progress-1280.png` | My progress overview |

Captures are made by `frontend/video-capture/journeys/14-view-journey-and-progress.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | The three pages |
| 00:18-00:41 | My Journey | The banner and progress bar |
| 00:41-01:19 | The roadmap | Module cards and the locked module |
| 01:19-01:39 | The badge collection | Counts on the journey page |
| 01:39-02:13 | The badges page | Earned badges and milestones |
| 02:13-02:48 | My progress | Overview figures and competencies |
| 02:48-03:14 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/14
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/14-view-journey-and-progress --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/14-view-journey-and-progress
```

## Pitfalls

- The journey list mentions streaks. No streak is shown anywhere in the current product, so the video does not mention them.
- The test database is shared, so the badge collection also lists badges seeded by other captures. The narration quotes only First Steps and Code Starter.
- The seeded names carry a short random suffix; it is not part of the product.
