# 20 · Assign work

> **Runtime:** about 2 min 59 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [20-assign-work.mp4](20-assign-work.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Assigning work is how a mentor steers a youth. This video follows the whole path: the mentor builds and confirms an assignment, and the youth finds it on the home page. It draws on [assign work](../../../detailed-designs/mentoring/assign-work/README.md) and L2-MENTOR-008 in [the mentoring L2 specification](../../../specs/mentoring/L2.md). The youth home side is shown as it is in the product.

## Learning objectives

By the end, the viewer can:

- List what a mentor chooses when assigning work.
- Explain why nothing is created until the mentor confirms.
- Say where the youth finds the assigned work.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who can receive an assignment? | Selected youth, or a whole cohort |
| What does the mentor set besides the item? | A due date, a reminder and an optional message |
| Where does the youth see it? | Under assigned work on the youth home, with a button to open it |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | Assign work form |
| `images/filled-1280.png` | Recipient, due date, reminder, message |
| `images/confirm-1280.png` | Confirmation window |
| `images/assigned-1280.png` | Assigned to one youth message |
| `images/youth-home-1280.png` | Youth home, assigned work |

Captures are made by `frontend/video-capture/journeys/20-assign-work.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:17 | Welcome | The journey and the cast |
| 00:17-00:47 | Choosing what to assign | Type, item and summary panel |
| 00:47-01:27 | Choosing who and when | Recipients, due date, reminder, message |
| 01:27-01:53 | Review and confirm | The confirmation window |
| 01:53-02:10 | Done | One record per recipient, count reported |
| 02:10-02:33 | What Amara sees | Assigned work on the youth home |
| 02:33-02:59 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/20
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/20-assign-work --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/20-assign-work
```

## Pitfalls

- The youth home also lists a seeded project from the test data; the narration refers only to the lesson Grace assigned.
- The seeded titles carry a short random suffix; it is not part of the product.
