# 086 · Editing your profile

> **Runtime:** about 2 min 22 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [086-platform-services-edit-profile.mp4](086-platform-services-edit-profile.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Everyone can edit their profile details and photo. This video shows the form, the email check, the photo check and the saved confirmation. It follows [the design](../../detailed-designs/platform-services/edit-profile/README.md) and the acceptance criteria of L2-PLATFORM-002 (criteria 1 and 3) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the profile fields and which are required.
- Explain the email and photo checks.
- Recognise the saved confirmation.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which fields are required? | Display name and email |
| What files are accepted as a photo? | JPG or PNG up to 5 MB, checked by content, stored privately |
| How does the person know the save worked? | A short "Profile updated." message |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The profile form |
| `images/invalid-1280.png` | The invalid email message |
| `images/photo-error-1280.png` | The refused photo message |
| `images/saved-1280.png` | The profile updated message |

Captures are made by `frontend/video-capture/086-platform-services-edit-profile.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | Welcome |
| 00:16-00:42 | The profile form | The profile form |
| 00:42-01:04 | Checks before saving | Checks before saving |
| 01:04-01:48 | The photo | The photo |
| 01:48-01:56 | Saving | Saving |
| 01:56-02:22 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/086-platform-services-edit-profile.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/086-platform-services-edit-profile --engine kokoro
node tools/video-build/build-video.mjs docs/videos/086-platform-services-edit-profile
```

## Pitfalls

- The video does not show read-only managed fields (L2-PLATFORM-002 criterion 2). A successful photo upload and removal are described from the page, not shown.
