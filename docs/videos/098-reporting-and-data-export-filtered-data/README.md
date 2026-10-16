# 098 · Exporting filtered data

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [098-reporting-and-data-export-filtered-data.mp4](098-reporting-and-data-export-filtered-data.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Exports must contain exactly what the current view and filters show, only what the requester may see, and must be audit logged. This video shows CSV and PDF export from the reports hub, the audit trail, and the denial for other roles. It follows [the export-filtered-data design](../../detailed-designs/reporting-and-data/export-filtered-data/README.md) and the acceptance criteria of L2-REPORT-003 in [the reporting and data L2 specification](../../specs/reporting-and-data/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain that an export matches the current view and filters.
- Distinguish the CSV and PDF outputs.
- State that exports are audit logged and that audit log export is limited to administrators.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does an export contain? | Exactly the current report within the current filters, limited to what the requester may see |
| How do CSV and PDF differ? | CSV is raw rows, PDF a formatted summary |
| Where is an export recorded? | In the audit log, as data exported |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/filtered-1280.png` | The filtered hub |
| `images/csv-1280.png` | CSV export message |
| `images/pdf-1280.png` | PDF export message on the youth report |
| `images/audit-1280.png` | Audit log filtered to data exported |
| `images/denied-1280.png` | A mentor opening the audit log |

Captures are made by `frontend/video-capture/098-reporting-and-data-export-filtered-data.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:19 | Welcome | Why exports |
| 00:19-01:10 | Exporting what you see | CSV from a filtered hub |
| 01:10-01:47 | One report, two formats | PDF from the youth report |
| 01:47-02:25 | Safe by design | Scope, audit log, denial |
| 02:25-02:47 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/098-reporting-and-data-export-filtered-data.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/098-reporting-and-data-export-filtered-data --engine kokoro
node tools/video-build/build-video.mjs docs/videos/098-reporting-and-data-export-filtered-data
```

## Pitfalls

- The audit entry shows the filter values as raw identifiers; the narration does not read them out.
- Whether director-level roles are separate from program administrators in the portal is not shown; the video repeats the specification wording only.
