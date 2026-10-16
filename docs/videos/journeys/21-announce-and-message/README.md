# 21 · Announce and message

> **Runtime:** about 3 min 38 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [21-announce-and-message.mp4](21-announce-and-message.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

People in the program need both a way to reach a whole group and a safe place to talk one to one. This video follows an administrator publishing an announcement, and a mentor and youth in a private thread and a broadcast. It draws on [publish announcements](../../../detailed-designs/engagement/publish-announcements/README.md) and [exchange messages](../../../detailed-designs/engagement/exchange-messages/README.md), and on L2-ENGAGE-006 and L2-ENGAGE-007 in [the engagement L2 specification](../../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how an announcement is targeted and delivered.
- Explain who can read a message thread.
- Describe what a broadcast sends and what the review step shows.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who receives an announcement? | The audience and recipients chosen by staff; each person sees only what is meant for them |
| Who can read a message? | Only the two people in the thread |
| What does a broadcast do? | Sends one personal message per youth after a review step that shows the count |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/compose-1280.png` | The compose form |
| `images/sent-unread-1280.png` | The sent announcement with zero of one read |
| `images/youth-new-1280.png` | The youth announcements page |
| `images/mentor-thread-1280.png` | The mentor inbox and thread |
| `images/confirm-1280.png` | The broadcast review dialog |
| `images/youth-thread-1280.png` | The youth thread with the reply and the reminder |

Captures are made by `frontend/video-capture/journeys/21-announce-and-message.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:24 | Welcome | Some news is for a whole group. |
| 00:26-01:02 | Composing an announcement | Pat opens the announcement page, and fills in the compose form. |
| 01:04-01:37 | What recipients see | The announcement appears in Pat's list with a sent label, the cohort and the audience beside it. |
| 01:39-02:29 | A private thread | Announcements are for groups. |
| 02:31-03:10 | Broadcast to my group | Sometimes Grace has the same news for everyone. |
| 03:12-03:38 | Things to remember | Announcements go to a chosen audience and role, sent now or scheduled, and staff can follow how many have read them. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/21
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/21-announce-and-message --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/21-announce-and-message
```

## Pitfalls

- The read count rising is described in the narration; the capture shows the zero of one state.
- Scheduling for later is described but not captured.
