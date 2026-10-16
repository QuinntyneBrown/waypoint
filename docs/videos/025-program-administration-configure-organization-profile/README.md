# 025 · The organization profile

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [025-program-administration-configure-organization-profile.mp4](025-program-administration-configure-organization-profile.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/configure-organization-profile/README.md) and the acceptance criteria of L2-ADMIN-006 (acceptance criteria 1 and 5) in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- List the identity fields an administrator can set.
- Explain that the colour palette is fixed for every organization.
- Say where guardians see the display name and contact email.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Which fields make up the identity? | Organization name, display name, logo URL, city, contact email |
| Can an organization change its colours? | No, the palette is fixed platform-wide for accessibility |
| Where does the contact email appear? | On guardian consent forms and receipts |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/identity-1280.png` | The identity panel |
| `images/saved-1280.png` | Saved message |
| `images/guardian-form-1280.png` | Guardian form with display name and contact email |

Captures are made by `frontend/video-capture/025-program-administration-configure-organization-profile.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why identity matters |
| 00:15-01:05 | Organization identity | Five fields and the fixed palette |
| 01:05-01:35 | Saving | Audit log, administrators only |
| 01:35-02:20 | Where it shows up | Guardian consent form |
| 02:20-02:40 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/025-program-administration-configure-organization-profile.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/025-program-administration-configure-organization-profile --engine kokoro
node tools/video-build/build-video.mjs docs/videos/025-program-administration-configure-organization-profile
```

## Pitfalls

- The logo address field is left empty in the capture; no logo is shown.
- Organization settings are shared by the whole demo database.
