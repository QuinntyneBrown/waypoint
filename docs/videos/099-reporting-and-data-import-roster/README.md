# 099 · Importing a roster

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [099-reporting-and-data-import-roster.mp4](099-reporting-and-data-import-roster.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Administrators migrate an existing roster from a CSV. This video shows upload validation, column mapping, the row preview with statuses, the confirmation, and the result counts. It follows [the import-roster design](../../detailed-designs/reporting-and-data/import-roster/README.md) and the acceptance criteria of L2-REPORT-004 in [the reporting and data L2 specification](../../specs/reporting-and-data/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the upload, map and import steps.
- Name the four row statuses in the preview.
- State that only ready rows create accounts and that re-running the same file creates no duplicates.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which files are accepted? | CSV only, up to two megabytes |
| What happens to duplicates and missing consent rows? | They are not imported; duplicates are skipped and missing consent is flagged for follow up |
| What if the file is imported twice? | People are matched by email, so no duplicate accounts are created |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/upload-1280.png` | The upload step |
| `images/wrong-type-1280.png` | A text file is refused |
| `images/mapping-1280.png` | Column mapping |
| `images/preview-1280.png` | Preview with statuses |
| `images/confirm-1280.png` | Confirmation dialog |
| `images/result-1280.png` | Import result |

Captures are made by `frontend/video-capture/099-reporting-and-data-import-roster.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Why import |
| 00:18-01:00 | Upload | Note, CSV only, wrong file, row and column count |
| 01:00-01:38 | Map the columns | Fields, skip, email |
| 01:38-02:39 | Preview and import | Statuses, confirm, result |
| 02:39-02:57 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/099-reporting-and-data-import-roster.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/099-reporting-and-data-import-roster --engine kokoro
node tools/video-build/build-video.mjs docs/videos/099-reporting-and-data-import-roster
```

## Pitfalls

- The page also shows an invalid status; L2-REPORT-004 names Ready, Missing consent and Duplicate.
- The roster and emails are synthetic.
