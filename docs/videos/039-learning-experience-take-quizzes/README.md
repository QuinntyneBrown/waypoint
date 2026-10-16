# 039 · Taking a quiz

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [039-learning-experience-take-quizzes.mp4](039-learning-experience-take-quizzes.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Quizzes produce scores and badges, so staff need to know how an attempt is protected. This video follows one attempt from the intro to the approved result. It follows [the take quizzes design](../../detailed-designs/learning-experience/take-quizzes/README.md) and the acceptance criteria of L2-LEARN-004 and L2-LEARN-005 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- List what the quiz intro tells a youth before an attempt.
- Explain that answers autosave and the quiz is resumable.
- Explain why a youth sees no score until a mentor approves, where approval is required.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the youth know before starting? | Number of questions, time, pass mark, attempts and badge reward |
| What happens on submit? | A confirmation that answers cannot change, then scoring on the server |
| What does the youth see before approval? | Only a waiting message, never an unapproved score |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/intro-1280.png` | The quiz intro |
| `images/question-1280.png` | A question with answer saved |
| `images/resumed-1280.png` | The quiz after a reload |
| `images/waiting-1280.png` | Waiting for mentor approval |
| `images/result-1280.png` | The approved result |

Captures are made by `frontend/video-capture/039-learning-experience-take-quizzes.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why quizzes matter |
| 00:15-00:50 | Before starting | The intro |
| 00:50-01:10 | One question at a time | Autosave |
| 01:10-01:30 | Picking up where she left off | Resume |
| 01:30-02:00 | Submitting | Lock and approval wait |
| 02:00-02:30 | The result | Score, badge, retake |
| 02:30-03:00 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 039
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/039-learning-experience-take-quizzes --engine kokoro
node tools/video-build/build-video.mjs docs/videos/039-learning-experience-take-quizzes
```

## Pitfalls

- Mentor approval applies only to quizzes that require it; the captured quiz does.
- The confirmation before submitting is a browser dialog, so it has no screen capture.
- The quiz and badge names on screen carry a random suffix because the capture uses seeded test data.
