# 070 · Receiving notifications

> **Runtime:** about 2 min 55 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [070-engagement-receive-notifications.mp4](070-engagement-receive-notifications.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Notifications tell people that something happened and where to go. This video shows a live notification, read state, the link and the notification settings. It follows [the receive notifications design](../../detailed-designs/engagement/receive-notifications/README.md) and the acceptance criteria of L2-ENGAGE-008 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe how a live notification appears.
- Explain read state and mark all read.
- Say what a notification link does and where preferences are set.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What happens if the user is not connected? | The notification appears on the next load |
| What does opening a notification do? | It goes to the screen the notification is about |
| Where do people choose how to be notified? | The notifications tab in settings, in-app and email |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/live-1280.png` | A new message notification |
| `images/read-1280.png` | After mark all read |
| `images/deep-link-1280.png` | The messages page opened from the notification |
| `images/preferences-1280.png` | The notification settings |

Captures are made by `frontend/video-capture/070-engagement-receive-notifications.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:19 | Welcome | Welcome |
| 00:19-01:01 | A live notification | A live notification |
| 01:01-01:29 | Read state | Read state |
| 01:29-02:07 | One tap to the right place | One tap to the right place |
| 02:07-02:29 | Choosing how to hear | Choosing how to hear |
| 02:29-02:55 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 070
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/070-engagement-receive-notifications --engine kokoro
node tools/video-build/build-video.mjs docs/videos/070-engagement-receive-notifications
```

## Pitfalls

- The narration names only the settings visible in the capture.
