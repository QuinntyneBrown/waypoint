# 036 · Reviewing a quiz result

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [036-learning-experience-review-quiz-results.mp4](036-learning-experience-review-quiz-results.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The quiz result is where a youth learns how they did. This video shows the wait for approval, the approved result with a badge, the question review and retake. It follows [the review quiz results design](../../detailed-designs/learning-experience/review-quiz-results/README.md) and the acceptance criteria of L2-LEARN-005 in [the learning experience L2 specification](../../specs/learning-experience/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain why a score may not show right after submitting.
- Describe what the approved result shows.
- State when a retake is offered.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| When is a score hidden? | When mentor approval is required and not yet given; an unapproved score is never shown |
| What does the result show? | Score, pass or fail against the pass mark, badge earned once, per-question review per feedback mode |
| When can a youth retake? | When retake is allowed and attempts remain |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/waiting-1280.png` | Waiting for mentor approval |
| `images/result-1280.png` | Approved score, passed, badge |
| `images/retake-1280.png` | Question review and retake button |

Captures are made by `frontend/video-capture/036-learning-experience-review-quiz-results.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | What the result page answers |
| 00:15-00:50 | Waiting for approval | No score yet |
| 00:50-01:25 | The approved result | Score, pass, badge |
| 01:25-02:00 | Question review | Correct or not per question |
| 02:00-02:25 | Retaking | When it is offered |
| 02:25-02:50 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/036-learning-experience-review-quiz-results.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/036-learning-experience-review-quiz-results --engine kokoro
node tools/video-build/build-video.mjs docs/videos/036-learning-experience-review-quiz-results
```

## Pitfalls

- The capture shows an attempt with all answers correct, so a wrong selection is described in the narration but not shown on screen.
