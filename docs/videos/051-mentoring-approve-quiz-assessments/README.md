# 051 · Approving quiz scores

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [051-mentoring-approve-quiz-assessments.mp4](051-mentoring-approve-quiz-assessments.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Quiz scores are produced by the machine, but a mentor decides what counts. This video shows the review, the override, the audit record and approve all. It follows [the approve quiz assessments design](../../detailed-designs/mentoring/approve-quiz-assessments/README.md) and the acceptance criteria of L2-MENTOR-004 in [the mentoring L2 specification](../../specs/mentoring/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain why an auto-scored quiz does not count until a mentor approves it.
- Describe the override rules and what is kept for audit.
- Use approve all for pending quiz assessments.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the youth see? | Only the approved score |
| What range can an override take? | A whole number from 0 to 100 |
| What is kept when a score is overridden? | The auto score and the approved score, with the mentor and time |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/queue-1280.png` | The reviews queue with the quiz |
| `images/review-1280.png` | Answers, auto score and override box |
| `images/range-1280.png` | The out-of-range message |
| `images/record-1280.png` | The approval record |
| `images/approved-all-1280.png` | The approve all result |

Captures are made by `frontend/video-capture/051-mentoring-approve-quiz-assessments.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why the auto score is not final |
| 00:20-00:55 | The review queue | Queue and review page |
| 00:55-01:45 | Approve or override | Range check, approval record |
| 01:45-02:05 | Approve all | One action for every pending quiz |
| 02:05-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/051-mentoring-approve-quiz-assessments.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/051-mentoring-approve-quiz-assessments --engine kokoro
node tools/video-build/build-video.mjs docs/videos/051-mentoring-approve-quiz-assessments
```

## Pitfalls

- Names on screen are renamed to the cast; the quiz and assignment titles carry a random suffix from the seed data.
