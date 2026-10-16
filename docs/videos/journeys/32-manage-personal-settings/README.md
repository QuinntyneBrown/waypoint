# 32 · Manage personal settings

> **Runtime:** about 4 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [32-manage-personal-settings.mp4](32-manage-personal-settings.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Shows where a user keeps their account the way they want it: profile, notification choices and display. It follows the designs [edit profile](../../../detailed-designs/platform-services/edit-profile/README.md), [change password](../../../detailed-designs/platform-services/change-password/README.md), [configure notifications](../../../detailed-designs/platform-services/configure-notifications/README.md), [configure display](../../../detailed-designs/platform-services/configure-display/README.md) and [switch theme](../../../detailed-designs/platform-services/switch-theme/README.md), and L2-PLATFORM-002, 003, 004 and 013 in [the platform services L2 specification](../../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Name what the profile tab holds and which fields are required.
- Explain how notification choices are made and when they take effect.
- Describe the display options and how a change previews.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which profile fields are required? | Display name and email; pronouns and photo are optional |
| When do notification choices save? | The moment a box is ticked, with a confirmation message |
| What can a person change on the display tab? | Text size, theme, high contrast, reduce motion, underline links, reading font |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/profile-1280.png` | The profile form |
| `images/profile-saved-1280.png` | Profile updated confirmation |
| `images/notifications-1280.png` | Events with in-app and email boxes |
| `images/notifications-saved-1280.png` | Announcements ticked and saved |
| `images/display-1280.png` | Text size at 120 percent with the dark theme |

Captures are made by `frontend/video-capture/journeys/32-manage-personal-settings.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Amara and her settings |
| 00:20-01:20 | Profile | Fields, save, password |
| 01:20-02:30 | Notifications | Per-event choices, saved at once |
| 02:30-03:30 | Display | Text size, theme, saving |
| 03:30-04:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/32
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/32-manage-personal-settings --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/32-manage-personal-settings
```

## Pitfalls

- Persistence of the saved theme after a reload is deliberately not claimed: in the running build used for capture, the theme returned to the device default after a reload while text size persisted. The narration claims persistence for text size only.
