# 027 · Managing cohorts

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [027-program-administration-manage-cohorts.mp4](027-program-administration-manage-cohorts.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/manage-cohorts/README.md) and the acceptance criteria of L2-ADMIN-003 in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the required and optional fields of a new cohort.
- Explain how existing youth are added and how the member count changes.
- Say what is recorded when a cohort is edited and what the list shows on error or when empty.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is required to create a cohort? | Name, curriculum track and lead mentor; dates and meeting location are also captured |
| How are youth added? | Choose an existing youth on the cohort page, or follow the invite path |
| What if the cohort list fails to load? | An error state with a Retry button; an empty list shows No cohorts yet |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/create-dialog-1280.png` | Create cohort |
| `images/card-1280.png` | Cohort card |
| `images/detail-1280.png` | Cohort detail |
| `images/members-1280.png` | Youth added |
| `images/edit-dialog-1280.png` | Edit cohort |

Captures are made by `frontend/video-capture/027-program-administration-manage-cohorts.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | What a cohort is |
| 00:15-01:05 | Creating a cohort | Form, validation, card |
| 01:05-01:55 | Adding youth | Existing youth or invite |
| 01:55-02:20 | Editing a cohort | Audit-logged edits |
| 02:20-02:45 | When the list will not load | Retry and empty state |
| 02:45-03:05 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/027-program-administration-manage-cohorts.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/027-program-administration-manage-cohorts --engine kokoro
node tools/video-build/build-video.mjs docs/videos/027-program-administration-manage-cohorts
```

## Pitfalls

- The error and empty states are described but not captured; they are covered by acceptance tests.
- The track and mentor names come from seeded demo data.
