# 068 · Exchanging messages

> **Runtime:** about 2 min 55 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [068-engagement-exchange-messages.mp4](068-engagement-exchange-messages.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Messaging is the private line between a youth and their mentor. This video shows a thread, live delivery, and the mentor broadcast with its review step. It follows [the exchange messages design](../../detailed-designs/engagement/exchange-messages/README.md) and the acceptance criteria of L2-ENGAGE-007 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe a private thread between a youth and a mentor.
- Explain how live delivery looks to the user.
- Describe how a broadcast reaches each youth and what the review step shows.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who can read a thread? | Only the two people in it |
| What does a broadcast become? | One personal message for each youth in the cohort |
| What does the mentor see before sending? | A review dialog with the cohort and the number of youth |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/youth-thread-1280.png` | Amara sends a message |
| `images/mentor-thread-1280.png` | Grace reads and replies |
| `images/youth-reply-1280.png` | The reply arrives live |
| `images/broadcast-1280.png` | The broadcast form |
| `images/confirm-1280.png` | The review dialog |
| `images/youth-broadcast-1280.png` | The broadcast in the youth thread |

Captures are made by `frontend/video-capture/068-engagement-exchange-messages.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Welcome |
| 00:20-01:01 | A private thread | A private thread |
| 01:01-01:47 | The mentor inbox | The mentor inbox |
| 01:47-02:30 | Broadcast to my group | Broadcast to my group |
| 02:30-02:55 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 068
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/068-engagement-exchange-messages --engine kokoro
node tools/video-build/build-video.mjs docs/videos/068-engagement-exchange-messages
```

## Pitfalls

- Staff access to threads under the safeguarding policy appears in the specification, is not shown on screen, and is not stated in the narration.
