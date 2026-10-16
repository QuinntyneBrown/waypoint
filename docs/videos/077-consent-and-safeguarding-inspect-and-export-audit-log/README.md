# 077 · Inspecting and exporting the audit log

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [077-consent-and-safeguarding-inspect-and-export-audit-log.mp4](077-consent-and-safeguarding-inspect-and-export-audit-log.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Program administrators need to see who did what and when, without the log exposing private safeguarding details. It follows [the design](../../detailed-designs/consent-and-safeguarding/inspect-and-export-audit-log/README.md) and the acceptance criteria of L2-CONSENT-003 and L2-REPORT-003 in [the L2 specification](../../specs/consent-and-safeguarding/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe who can open the audit log and what it records.
- Use the filters to narrow the log.
- Explain why private note content is redacted.
- Describe the CSV export and that it is itself logged.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who can open the audit log? | Only program administrators. |
| What can the log be filtered by? | User, action and an inclusive date range, with ten rows to a page. |
| What does a private check-in entry show? | That it happened and that it was private; the note content is redacted. |
| Is an export recorded? | Yes. The export is itself written to the audit log as a data exported event. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/overview-1280.png` | An append-only record with filters |
| `images/redacted-1280.png` | The note content is never shown |
| `images/filtered-1280.png` | Three events, page one of one |
| `images/exported-1280.png` | The export is logged too |

Captures are made by `frontend/video-capture/077-consent-and-safeguarding-inspect-and-export-audit-log.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Purpose |
| 00:15-00:50 | The audit log page | Append-only record and filters |
| 00:50-01:30 | Reading the entries | Columns, redacted private check-in |
| 01:30-02:05 | Filtering and paging | Data exported events, pages |
| 02:05-02:45 | Exporting | CSV, logged export, integrity warning |
| 02:45-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/077-consent-and-safeguarding-inspect-and-export-audit-log.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/077-consent-and-safeguarding-inspect-and-export-audit-log --engine kokoro
node tools/video-build/build-video.mjs docs/videos/077-consent-and-safeguarding-inspect-and-export-audit-log
```

## Pitfalls

- The source column shows the Testing host values (api, desktop, loopback) because the capture drives the API directly.
- The integrity warning is described in the narration but not captured; it needs a failing chain check.
