# 097 · Exploring reports

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [097-reporting-and-data-explore-reports.mp4](097-reporting-and-data-explore-reports.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Staff need one place to see how the program is doing, scoped by date, cohort, track and mentor. This video shows the reports hub, its filters, the updated time and record count, and the empty and failure states. It follows [the explore-reports design](../../detailed-designs/reporting-and-data/explore-reports/README.md) and the acceptance criteria of L2-REPORT-001 in [the reporting and data L2 specification](../../specs/reporting-and-data/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the five filters on the reports hub and what each figure follows.
- Name the four views of the hub.
- Describe what staff see when a scope is empty or a report fails to load.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What do the figures follow? | The five filters; the updated time and the youth record count are shown with them |
| What happens with an empty scope? | The message no report data for these filters, with advice to change the filters |
| Who sees which data? | A mentor sees only their own youth (L2-REPORT-001 criterion 6) |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/filtered-1280.png` | The hub after the filters are applied |
| `images/empty-1280.png` | A cohort with no youth |
| `images/error-1280.png` | The failure page with try again |

Captures are made by `frontend/video-capture/097-reporting-and-data-explore-reports.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Why reports |
| 00:18-01:21 | Filtering the hub | Five filters, six figures |
| 01:21-02:00 | Four views | Overview, youth, cohort, staff |
| 02:00-02:35 | When there is nothing to show | Empty scope, load failure |
| 02:35-02:58 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/097-reporting-and-data-explore-reports.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/097-reporting-and-data-explore-reports --engine kokoro
node tools/video-build/build-video.mjs docs/videos/097-reporting-and-data-explore-reports
```

## Pitfalls

- The failure screenshot is the full-page server error, produced by a faked failed response for the hub request; it is the real error page, not a mock.
- Names of the seeded cohort and track are generated per run and are not spoken.
