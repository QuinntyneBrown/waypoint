# 12 · Take quizzes

> **Runtime:** about 2 min 40 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [12-take-quizzes.mp4](12-take-quizzes.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A quiz is where a youth shows what a module taught. This video follows one attempt from the first screen to an approved result, using [take quizzes](../../../detailed-designs/learning-experience/take-quizzes/README.md) and [review quiz results](../../../detailed-designs/learning-experience/review-quiz-results/README.md), and L2-LEARN-004 and L2-LEARN-005 in [the learning experience L2 specification](../../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Say what the first quiz screen tells a youth.
- Explain how answers are saved and how a quiz is resumed.
- Describe what a youth sees while a mentor approval is pending, and after it.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| When is a score shown? | Only after the mentor approves, where approval is required |
| What if the page is reloaded mid quiz? | The youth returns to the same question with the earlier answer selected |
| What does submitting do? | It locks the answers for that attempt and the server scores them |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/intro-1280.png` | Quiz start page |
| `images/question-1280.png` | A question with answer saved |
| `images/resumed-1280.png` | Question two after a reload |
| `images/waiting-1280.png` | Waiting for mentor approval |
| `images/result-1280.png` | Approved score and badge |
| `images/review-1280.png` | Question review and retake |

Captures are made by `frontend/video-capture/journeys/12-take-quizzes.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | The journey and the cast |
| 00:18-00:48 | Before she starts | Quiz details and the badge |
| 00:48-01:06 | One question at a time | Autosaved answers |
| 01:06-01:21 | Picking up where she left off | Reload and resume |
| 01:21-01:49 | Submitting | Confirm, lock, wait for approval |
| 01:49-02:16 | The result | Approved score, review, retake |
| 02:16-02:40 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/12
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/12-take-quizzes --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/12-take-quizzes
```

## Pitfalls

- The seeded titles carry a short random suffix; it is not part of the product.
- The mentor approval in the capture is made through the mentor API; the mentor screen has its own video.
- Whether a quiz needs mentor approval is a setting of the quiz; this quiz does.
