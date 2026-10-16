# 13 · Author quizzes and assignments

> **Runtime:** about 3 min 55 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [13-author-quizzes-and-assignments.mp4](13-author-quizzes-and-assignments.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Quizzes and assignments are how learning is checked. This video follows Pat Morgan creating a quiz with its attempt rules and a question, then an assignment with a rubric and due rules, and publishing it. It follows the designs for [authoring quizzes](../../../detailed-designs/curriculum-authoring/author-quizzes/README.md), [authoring assignments](../../../detailed-designs/curriculum-authoring/author-assignments/README.md), and the acceptance criteria in [the curriculum authoring L2 specification](../../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the quiz rules: pass mark, attempts, feedback mode, shuffle and required.
- Explain that the author sets every correct answer.
- Describe the parts of an assignment: submission types, rubric and due rules.
- Explain that a draft is hidden until published.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What governs a quiz? | Pass mark, attempts allowed, feedback mode, shuffle and whether it is required |
| Who decides the correct answer? | The author, and the score uses exactly what was saved |
| What does an assignment need? | A title, a task description and at least one submission type |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/quiz-rules-1280.png` | The add quiz dialog |
| `images/question-1280.png` | A multiple choice question |
| `images/assignment-dialog-1280.png` | The add assignment dialog |
| `images/assignment-draft-1280.png` | The draft assignment card |
| `images/published-1280.png` | The publish message |

Captures are made by `frontend/video-capture/journeys/13-author-quizzes-and-assignments.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Lessons teach. |
| 00:22-01:00 | Setting the quiz rules | Pat opens the curriculum builder, finds a module, and presses add quiz. |
| 01:02-01:44 | Adding questions | A quiz editor opens, with a form for questions. |
| 01:46-02:25 | Building the assignment | Now the assignment. |
| 02:27-03:01 | Rubric and due rules | Below that is the rubric, a list of criteria that each carry a point value. |
| 03:03-03:27 | Publishing | When Pat presses create assignment, it appears in the module as a draft, showing its total of eight points. |
| 03:29-03:55 | Things to remember | A quiz has rules for pass mark, attempts, feedback, shuffling, and whether it is required. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/13
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/13-author-quizzes-and-assignments --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/13-author-quizzes-and-assignments
```

## Pitfalls

- The short answer question, reordering and the quiz settings editor are covered in the quizzes video.
