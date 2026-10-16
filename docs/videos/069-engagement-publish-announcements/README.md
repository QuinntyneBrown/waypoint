# 069 · Publishing announcements

> **Runtime:** about 2 min 47 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [069-engagement-publish-announcements.mp4](069-engagement-publish-announcements.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Announcements reach a chosen audience and can be tracked. This video shows composing, targeting, scheduling, what a recipient sees, and the read rate. It follows [the publish announcements design](../../detailed-designs/engagement/publish-announcements/README.md) and the acceptance criteria of L2-ENGAGE-006 in [the engagement L2 specification](../../specs/engagement/L2.md).

## Learning objectives

By the end, the viewer can:

- Compose an announcement with an audience and a role.
- Choose between send now and schedule for later.
- Read the read count and read rate after sending.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who can an announcement target? | An audience of the organization or one cohort, and recipients of youth, youth and guardians, guardians, mentors or everyone |
| What does a recipient see? | Only announcements meant for them, with new markers and mark all read |
| How do staff know it was read? | The read count and aggregate read rate on the list |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/compose-top-1280.png` | Title and message |
| `images/compose-delivery-1280.png` | Audience, recipients and delivery |
| `images/scheduled-1280.png` | A scheduled announcement |
| `images/sent-unread-1280.png` | A sent announcement, zero of one read |
| `images/youth-new-1280.png` | The announcement marked new |
| `images/youth-read-1280.png` | After mark all read |
| `images/sent-read-1280.png` | One of one read |

Captures are made by `frontend/video-capture/069-engagement-publish-announcements.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Welcome |
| 00:15-00:47 | Composing an announcement | Composing an announcement |
| 00:47-01:27 | Send now or schedule | Send now or schedule |
| 01:27-01:50 | What recipients see | What recipients see |
| 01:50-02:22 | Tracking reads | Tracking reads |
| 02:22-02:47 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 069
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/069-engagement-publish-announcements --engine kokoro
node tools/video-build/build-video.mjs docs/videos/069-engagement-publish-announcements
```

## Pitfalls

- The announcement list is organization-wide, so captures crop to the announcement the narration is about.
