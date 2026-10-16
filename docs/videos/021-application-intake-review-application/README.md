# 021 · Reviewing an application

> **Runtime:** about 3 min 10 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [021-application-intake-review-application.mp4](021-application-intake-review-application.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Before any decision, a program administrator has to see what was submitted and whether consent is in place. This video shows the application queue, the status filter and the review window. It follows [the review design](../../detailed-designs/application-intake/review-application/README.md) and the acceptance criteria of L2-INTAKE-002 in [the application intake L2 specification](../../specs/application-intake/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what the application queue shows for each application.
- Use the status filter to find applications that need consent.
- List what the review window shows and what the administrator can prepare there.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does each queue row show? | Applicant, age with a minor badge, neighbourhood, track, status, submitted date |
| How does an administrator find applications from minors without consent? | The application status menu, choosing needs consent |
| What is in the review window? | The consent notice, submitted details, guardian contact, and fields for interest track, cohort, mentor and decision note |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/queue-1280.png` | The applications queue |
| `images/filter-1280.png` | The queue filtered to needs consent |
| `images/detail-1280.png` | The review window for a minor |

Captures are made by `frontend/video-capture/021-application-intake-review-application.capture.ts`. The short code after the applicant's last name is added by the capture so the application can be found among others. The queue also shows other synthetic applications.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:25 | Welcome | Why there is one queue |
| 00:25-01:00 | The application queue | Columns, minor badge, statuses |
| 01:00-01:35 | Finding what needs attention | The status filter and the empty message |
| 01:35-02:45 | Opening the review | Consent notice, submitted details, decision fields |
| 02:45-03:10 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/021-application-intake-review-application.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/021-application-intake-review-application --engine kokoro
node tools/video-build/build-video.mjs docs/videos/021-application-intake-review-application
```

## Pitfalls

- The empty-queue message is described in the narration but not shown; there is no capture of it.
- Approving and declining are covered in the next video, not this one.
