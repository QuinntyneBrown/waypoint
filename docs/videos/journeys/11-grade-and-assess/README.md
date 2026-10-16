# 11 · Grade and assess

> **Runtime:** about 4 min 12 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [11-grade-and-assess.mp4](11-grade-and-assess.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Nothing a youth hands in counts until a mentor has looked at it. This video follows one mentor through grading an assignment, approving a quiz score and assessing a competency. It draws on [grade submissions](../../../detailed-designs/mentoring/grade-submissions/README.md), [approve quiz assessments](../../../detailed-designs/mentoring/approve-quiz-assessments/README.md) and [assess competencies](../../../detailed-designs/mentoring/assess-competencies/README.md), and on L2-MENTOR-003, L2-MENTOR-004 and L2-MENTOR-005 in [the mentoring L2 specification](../../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the three outcomes of grading a submission and when feedback is required.
- Explain why a quiz score needs approval and what the approval record keeps.
- Name the four competency levels and what a saved assessment records.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| When is feedback required? | When a mentor requests a revision; a draft keeps work without changing what the youth sees |
| Why does a quiz score need a mentor? | The auto score does not count until approved or overridden (whole number, zero to one hundred); both numbers are kept |
| What does a saved competency assessment keep? | The level, the notes, the mentor and the time; the summary and reports use the latest |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/queue-1280.png` | Reviews queue with an overdue assignment and a quiz |
| `images/submission-1280.png` | Submitted work beside the rubric |
| `images/ready-1280.png` | Ten out of ten with a comment |
| `images/quiz-1280.png` | Auto score, pass mark and override box |
| `images/quiz-record-1280.png` | Approval record |
| `images/competency-1280.png` | Competency card saved as Proficient |

Captures are made by `frontend/video-capture/journeys/11-grade-and-assess.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:22 | Welcome | A young person's work is not finished when they hand it in. |
| 00:24-00:51 | The review queue | Grace opens Reviews. |
| 00:53-02:07 | Grading a submission | Grace presses Review on the service reflection. |
| 02:09-03:07 | Approving a quiz score | Back in the queue, Grace opens the quiz. |
| 03:09-03:43 | Assessing a competency | Finally, Grace opens Amara's competencies page. |
| 03:45-04:12 | Things to remember | Score each rubric criterion within its range, and the total is worked out for you. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/11
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/11-grade-and-assess --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/11-grade-and-assess
```

## Pitfalls

- The seeded titles carry a short random suffix; it is not part of the product.
- The rubric points and badge are applied on approval; the capture shows the screens, not the youth side.
