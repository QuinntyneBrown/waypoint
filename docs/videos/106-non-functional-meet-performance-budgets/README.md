# 106 · Staying fast

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [106-non-functional-meet-performance-budgets.mp4](106-non-functional-meet-performance-budgets.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Staff and young people need Waypoint to stay quick, including on a shared Chromebook and as the program grows. This video explains the four performance targets in plain words, shows paged loading in the privacy request history, and describes the automated checks that enforce the budgets. It follows [the meet performance budgets design](../../detailed-designs/non-functional/meet-performance-budgets/README.md) and the acceptance criteria of L2-NFR-008 in [the non-functional L2 specification](../../specs/non-functional/L2.md).

## Learning objectives

By the end, the viewer can:

- State the response-time targets in plain words.
- Explain why long lists load in pages.
- Name the automated checks that enforce the budgets.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What are the server response targets? | Median 300 milliseconds, 95th percentile 800 milliseconds, network excluded |
| Why do long lists arrive in pages? | So payloads stay bounded however large the organisation grows |
| How are the budgets checked? | An API load check in continuous integration and a browser test of the main youth screen |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/first-page-1280.png` | History showing twenty-five of twenty-six requests |
| `images/load-more-1280.png` | The load more requests button |
| `images/all-loaded-1280.png` | History showing all twenty-six |

Captures are made by `frontend/video-capture/106-non-functional-meet-performance-budgets.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:25 | Welcome | Why budgets matter |
| 00:25-01:10 | The targets | Four budgets |
| 01:10-02:00 | Pages you can see | Showing 25 of 26, load more |
| 02:00-02:40 | How the targets are checked | Load check and browser test |
| 02:40-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/106-non-functional-meet-performance-budgets.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/106-non-functional-meet-performance-budgets --engine kokoro
node tools/video-build/build-video.mjs docs/videos/106-non-functional-meet-performance-budgets
```

## Pitfalls

- Timing numbers cannot be shown on screen; the paged history is the visible effect of the bounded-payload rule.
- The three second figure is the budget used by the browser test; the requirement gives it as an example for a mid-tier device.
