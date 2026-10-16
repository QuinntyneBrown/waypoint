# 101 · The staff activity report

> **Runtime:** about 2 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [101-reporting-and-data-view-staff-report.mp4](101-reporting-and-data-view-staff-report.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Administrators need to see whether the mentoring work is shared fairly. This video shows the staff activity view and its load balance label. It follows [the view-staff-report design](../../detailed-designs/reporting-and-data/view-staff-report/README.md) and the acceptance criteria of L2-REPORT-001 in [the reporting and data L2 specification](../../specs/reporting-and-data/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three counts on a mentor card.
- Explain the load balance label and the target range.
- Describe the empty state.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a mentor card show? | Name, email, caseload, check-ins, reviews, a load balance label and the target range |
| What does under target mean? | The caseload is below the target range |
| What is shown for an empty scope? | No staff activity for these filters, with advice to change the scope |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/staff-view-1280.png` | The staff activity tab |
| `images/mentor-card-1280.png` | One mentor card |
| `images/empty-1280.png` | An empty scope |

Captures are made by `frontend/video-capture/101-reporting-and-data-view-staff-report.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:17 | Welcome | Why the staff view |
| 00:17-00:39 | Opening the staff view | Follows the filters |
| 00:39-01:30 | One card per mentor | Counts, label, range |
| 01:30-01:46 | When there is nothing to show | Empty state |
| 01:46-02:06 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/101-reporting-and-data-view-staff-report.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/101-reporting-and-data-view-staff-report --engine kokoro
node tools/video-build/build-video.mjs docs/videos/101-reporting-and-data-view-staff-report
```

## Pitfalls

- The overloaded state (red warning) is described from the specification and the page code; the seeded data has no overloaded mentor, so it is not on screen.
