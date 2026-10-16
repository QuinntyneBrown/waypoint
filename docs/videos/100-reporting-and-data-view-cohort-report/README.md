# 100 · The cohort report

> **Runtime:** about 2 min 15 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [100-reporting-and-data-view-cohort-report.mp4](100-reporting-and-data-view-cohort-report.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Staff need to see how a cohort is doing and compare it. This video shows how to generate the cohort report and read each part of it. It follows [the view-cohort-report design](../../detailed-designs/reporting-and-data/view-cohort-report/README.md) and the acceptance criteria of L2-REPORT-001 in [the reporting and data L2 specification](../../specs/reporting-and-data/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how to choose a cohort and a comparison.
- Name the parts of the cohort report.
- Explain how the youth summary shows who is at risk.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What can a cohort be compared with? | The program average and previous term by default, or another cohort |
| What does the youth summary show? | Each young person with a status and completion |
| Who can see the report? | It is scoped to the requester; a mentor sees only their own youth |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/selector-1280.png` | The cohort and compare menus |
| `images/summary-1280.png` | Headline and three figures |
| `images/detail-1280.png` | Module completion, youth summary, comparisons |

Captures are made by `frontend/video-capture/100-reporting-and-data-view-cohort-report.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Why the cohort report |
| 00:18-00:47 | Generating the report | Cohort and comparison |
| 00:47-01:37 | Reading the report | Headline, cards, comparisons |
| 01:37-01:53 | Who can see it | Scope and export |
| 01:53-02:13 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/100-reporting-and-data-view-cohort-report.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/100-reporting-and-data-view-cohort-report --engine kokoro
node tools/video-build/build-video.mjs docs/videos/100-reporting-and-data-view-cohort-report
```

## Pitfalls

- The seeded cohort has one youth, so the comparison cards show one cohort against the averages.
- The cohort name is generated per run and is not spoken.
