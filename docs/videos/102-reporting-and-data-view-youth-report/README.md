# 102 · The youth report

> **Runtime:** about 2 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [102-reporting-and-data-view-youth-report.mp4](102-reporting-and-data-view-youth-report.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Before a check-in or a guardian conversation, staff need the whole picture of one young person. This video shows the youth report and each card in it. It follows [the view-youth-report design](../../detailed-designs/reporting-and-data/view-youth-report/README.md) and the acceptance criteria of L2-REPORT-001 in [the reporting and data L2 specification](../../specs/reporting-and-data/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how to generate the youth report.
- Name the three rings and the cards below them.
- State that private note details are not in the report.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What do the three rings show? | Curriculum progress, average quiz score and attendance rate |
| What does the mentor notes summary include? | The number of check-ins and the latest date and engagement label, not the private notes |
| Who can see it? | It is scoped to the requester; a mentor sees only their own youth |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/selector-1280.png` | The youth menu |
| `images/rings-1280.png` | Headline and rings |
| `images/modules-1280.png` | Modules, quizzes, attendance |
| `images/notes-1280.png` | Competencies, notes summary, badges |

Captures are made by `frontend/video-capture/102-reporting-and-data-view-youth-report.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | Why the youth report |
| 00:16-00:32 | Generating the report | One menu, one button |
| 00:32-00:56 | The headline | Three rings |
| 00:56-01:46 | The detail | Cards |
| 01:46-02:07 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/102-reporting-and-data-view-youth-report.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/102-reporting-and-data-view-youth-report --engine kokoro
node tools/video-build/build-video.mjs docs/videos/102-reporting-and-data-view-youth-report
```

## Pitfalls

- The seeded youth stands in for a young person who is falling behind; progress is fifty percent and attendance is fifty percent in the seed.
