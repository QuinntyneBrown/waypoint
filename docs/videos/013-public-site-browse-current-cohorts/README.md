# 013 · Browsing current cohorts

> **Runtime:** about 2 min 40 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [013-public-site-browse-current-cohorts.mp4](013-public-site-browse-current-cohorts.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Visitors want to see who is running a cohort, where and when before they apply. This video shows the public About page and what each cohort card does and does not reveal. It follows [the design](../../detailed-designs/public-site/browse-current-cohorts/README.md) and the acceptance criteria of L2-PUB-002 in [the public site L2 specification](../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a cohort card shows: status, neighbourhood, dates and lead mentor.
- Explain why no youth, guardian or member information appears.
- Say what a visitor sees when no cohort is listed.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does each cohort card show? | Name, status badge, neighbourhood, dates and lead mentor |
| What is never shown? | Names of youth, guardians or member lists |
| What if no cohort is listed? | A message that visitors can still apply and the team will share upcoming options |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/about-1280.png` | The About page with the cohort list |
| `images/cohorts-1280.png` | Two cohort cards |
| `images/empty-1280.png` | The no cohorts are currently listed message |

Captures are made by `frontend/video-capture/013-public-site-browse-current-cohorts.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Why browsing cohorts matters |
| 00:18-00:55 | The About page | Ruth opens About |
| 00:55-01:50 | What each cohort shows | Card contents, privacy, single status |
| 01:50-02:15 | When nothing is listed | Empty state |
| 02:15-02:40 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/013-public-site-browse-current-cohorts.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/013-public-site-browse-current-cohorts --engine kokoro
node tools/video-build/build-video.mjs docs/videos/013-public-site-browse-current-cohorts
```

## Pitfalls

- The cohort names, neighbourhoods and mentor in the captures are demo content substituted in the browser for the video; the page is the real one. Statuses are worked out from the term dates.
- The empty state is shown by returning no cohorts to the real page.
