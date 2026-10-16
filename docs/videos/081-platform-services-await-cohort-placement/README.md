# 081 · Waiting to be placed in a cohort

> **Runtime:** about 2 min 33 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [081-platform-services-await-cohort-placement.mp4](081-platform-services-await-cohort-placement.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A youth can sign in before a mentor has placed them in a cohort. This video shows the no-cohort state on the youth home page, the message your mentor path, and why program content stays locked until placement. It follows [the design](../../detailed-designs/platform-services/await-cohort-placement/README.md) and the acceptance criteria of L2-PLATFORM-011 (criterion 3) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a youth sees on the home page before cohort placement.
- Explain why lessons and journey stay locked until placement.
- Say what the message your mentor button leads to while no mentor is assigned.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does an unplaced youth see? | The home page message "You’re not in a cohort yet", with a Message your mentor button |
| What happens to program content? | It stays locked until placement |
| What does the messages page say? | That the mentor has not been assigned yet and the conversation opens when placement is ready |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/no-cohort-1280.png` | The no-cohort message on the youth home page |
| `images/messages-1280.png` | The messages page before a mentor is assigned |

Captures are made by `frontend/video-capture/081-platform-services-await-cohort-placement.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Welcome |
| 00:15-01:14 | The waiting state | The waiting state |
| 01:14-02:06 | Reaching out | Reaching out |
| 02:06-02:33 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/081-platform-services-await-cohort-placement.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/081-platform-services-await-cohort-placement --engine kokoro
node tools/video-build/build-video.mjs docs/videos/081-platform-services-await-cohort-placement
```

## Pitfalls

- The captures use a youth registered through the public registration endpoint, who is never placed in a cohort. The slide on locked content is text only: the video states the L2 rule and does not show a locked screen.
