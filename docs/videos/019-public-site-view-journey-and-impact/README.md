# 019 · The journey and impact on the home page

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [019-public-site-view-journey-and-impact.mp4](019-public-site-view-journey-and-impact.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Visitors want to see the path through the program and its size. This video shows the four step journey and impact numbers on the home page, and why they never count individual youth. It follows [the design](../../detailed-designs/public-site/view-journey-and-impact/README.md) and the acceptance criteria of L2-PUB-002 (criterion 3) and L2-AUTH-010 in [the public site L2 specification](../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the four steps of the journey.
- Name the four impact numbers.
- Explain why the numbers reveal nothing about individual youth.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What are the four steps? | Apply, Join a cohort, Learn the modules, Lead and graduate |
| What do the impact numbers count? | Learning tracks, cohorts, published modules and neighbourhoods |
| Do they count youth? | No; they are counts of the program structure |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/hero-1280.png` | The home page headline and buttons |
| `images/journey-1280.png` | The four step journey |
| `images/impact-1280.png` | The four impact numbers |

Captures are made by `frontend/video-capture/019-public-site-view-journey-and-impact.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | What visitors want to know |
| 00:20-00:42 | The home page | Headline and buttons |
| 00:42-01:22 | Your journey in four steps | The four cards |
| 01:22-02:08 | Growing leaders across the city | The impact numbers |
| 02:08-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/019-public-site-view-journey-and-impact.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/019-public-site-view-journey-and-impact --engine kokoro
node tools/video-build/build-video.mjs docs/videos/019-public-site-view-journey-and-impact
```

## Pitfalls

- The impact numbers in the captures come from demo content substituted in the browser for the video; the page is the real one.
