# 22 · Receive notifications

> **Runtime:** about 2 min 48 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [22-receive-notifications.mp4](22-receive-notifications.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Things happen in Waypoint all day, and people need to hear about them without hunting. This video follows one youth receiving a live notification, clearing it, following it to the right screen, and choosing how she wants to be told. It draws on [receive notifications](../../../detailed-designs/engagement/receive-notifications/README.md) and L2-ENGAGE-008 in [the engagement L2 specification](../../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how a live notification appears and how read state works.
- Explain what happens when someone is not connected.
- Say where notification preferences live.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the live updates badge mean? | Waypoint is connected, and new items appear without a refresh |
| What if someone is offline when it happens? | The notification waits and appears the next time they open the page |
| Where does a notification lead? | To the screen it is about, such as the messages page |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/live-1280.png` | A new message notification at the top of the list |
| `images/read-1280.png` | After mark all read |
| `images/deep-link-1280.png` | The messages page the notification opened |
| `images/preferences-1280.png` | Notification settings, in-app or email |

Captures are made by `frontend/video-capture/journeys/22-receive-notifications.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:22 | Welcome | Things happen in Waypoint all day. |
| 00:24-00:58 | A live notification | Amara opens her notifications page. |
| 01:00-01:26 | Read state | Unread items stand out from the ones Amara has already seen. |
| 01:28-02:03 | One step to the right place | Each notification carries a link to the place it is about. |
| 02:05-02:26 | Choosing how to hear | Not everyone wants every notice in every way. |
| 02:28-02:48 | Things to remember | Notifications arrive live while you are connected, and wait for you if you are not. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/22
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/22-receive-notifications --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/22-receive-notifications
```

## Pitfalls

- The preferences screenshot shows the first four kinds of notice only.
