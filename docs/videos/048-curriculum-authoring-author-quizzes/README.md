# 048 · Authoring quizzes

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [048-curriculum-authoring-author-quizzes.mp4](048-curriculum-authoring-author-quizzes.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Quizzes decide whether a lesson has landed. This video shows the quiz rules, the four question types and how correct answers are set. It follows [the author quizzes design](../../detailed-designs/curriculum-authoring/author-quizzes/README.md) and the acceptance criteria of L2-CURRIC-003 in [the curriculum authoring L2 specification](../../specs/curriculum-authoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the quiz rules.
- Name the four question types.
- Explain how scoring uses the saved answers.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What rules does a quiz have? | Pass mark, attempts allowed, feedback mode, shuffle, required for module completion |
| How is a correct choice marked? | By its option number, explicitly |
| How is a quiz scored? | Using exactly the saved correct answers and pass mark |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/settings-1280.png` | The add quiz form with rules |
| `images/choice-question-1280.png` | A multiple choice question |
| `images/short-answer-1280.png` | A short answer question |
| `images/rules-1280.png` | The quiz settings in the editor |

Captures are made by `frontend/video-capture/048-curriculum-authoring-author-quizzes.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why quizzes |
| 00:15-01:10 | Setting the rules | Pass mark, attempts, feedback |
| 01:10-01:50 | A multiple choice question | Options and correct number |
| 01:50-02:10 | A short answer question | Accepted answers |
| 02:10-02:40 | Order and scoring | Reorder and promise |
| 02:40-03:05 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/048-curriculum-authoring-author-quizzes.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/048-curriculum-authoring-author-quizzes --engine kokoro
node tools/video-build/build-video.mjs docs/videos/048-curriculum-authoring-author-quizzes
```

## Pitfalls

- The reorder buttons are described but not shown, because the editor prints a raw type value beside each question in the list.
