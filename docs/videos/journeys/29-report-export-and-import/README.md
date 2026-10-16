# 29 · Report, export and import

> **Runtime:** about 3 min 52 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [29-report-export-and-import.mp4](29-report-export-and-import.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Staff report on the program and move data in and out. This video follows Pat Morgan filtering the reports hub, exporting exactly what is on screen, and importing a roster in three reviewed steps. It follows the designs for [exploring reports](../../../detailed-designs/reporting-and-data/explore-reports/README.md), [exporting filtered data](../../../detailed-designs/reporting-and-data/export-filtered-data/README.md), [importing a roster](../../../detailed-designs/reporting-and-data/import-roster/README.md), and the acceptance criteria in [the reporting and data L2 specification](../../../specs/reporting-and-data/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain that every figure follows the report filters.
- Explain that an export matches the screen and the role.
- Describe the three import steps.
- Name the row statuses in the import preview.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What do the filters scope? | Every overview and staff figure, and the export |
| What does an export cover? | Exactly the current report and filters, and only what the role can see |
| Which rows become accounts? | Only rows marked ready |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/filtered-1280.png` | The filtered reports hub |
| `images/csv-1280.png` | The CSV export message |
| `images/mapping-1280.png` | Upload and column mapping |
| `images/preview-1280.png` | The row preview |
| `images/result-1280.png` | The import result |

Captures are made by `frontend/video-capture/journeys/29-report-export-and-import.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Every program needs to answer a simple question, how are we doing? |
| 00:22-01:05 | Running a report | Pat opens the reports hub. |
| 01:07-01:48 | Exporting what you see | At the top right are two buttons, export CSV and export PDF. |
| 01:50-02:36 | Importing a roster | Now data comes the other way. |
| 02:38-03:28 | Preview and confirm | Pat presses preview roster, and Waypoint checks every row. |
| 03:30-03:52 | Things to remember | Every report figure follows the filters. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/29
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/29-report-export-and-import --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/29-report-export-and-import
```

## Pitfalls

- The PDF export and the audit log of exports are covered in the exporting video.
