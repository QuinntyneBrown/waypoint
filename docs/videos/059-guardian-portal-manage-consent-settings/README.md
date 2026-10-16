# 059 · Managing consent settings

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [059-guardian-portal-manage-consent-settings.mp4](059-guardian-portal-manage-consent-settings.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A guardian's consent decides whether a minor can take part, so guardians need one clear place to see it and change it. It follows [the design](../../detailed-designs/guardian-portal/manage-consent-settings/README.md) and the acceptance criteria of L2-GUARDIAN-003 in [the L2 specification](../../specs/guardian-portal/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what the participation consent card shows and why it is read only.
- Explain the optional media choices and that each is saved on its own.
- State what withdrawing consent does.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Can a guardian edit consent already given? | No. It is shown read only with signer, date, method, validity and reference. |
| What are the media choices? | Allow media use, with all photos and video, group photos only, and first-name captions saved independently. |
| What happens on withdrawal? | A confirmation is required, the badge shows Withdrawn and permissions that depend on it are revoked going forward. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/record-1280.png` | Participation consent, read only |
| `images/media-options-1280.png` | Each option is separate |
| `images/media-saved-1280.png` | Media consent saved |
| `images/withdrawn-1280.png` | The record stays; the Withdraw button is gone |

Captures are made by `frontend/video-capture/059-guardian-portal-manage-consent-settings.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why guardians manage consent |
| 00:20-01:10 | Participation consent | The read-only record card |
| 01:10-01:55 | Media choices | Options and saving |
| 01:55-02:40 | Withdrawing consent | Confirmation and the Withdrawn badge |
| 02:40-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/059-guardian-portal-manage-consent-settings.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/059-guardian-portal-manage-consent-settings --engine kokoro
node tools/video-build/build-video.mjs docs/videos/059-guardian-portal-manage-consent-settings
```

## Pitfalls

- The captured child, module and session names carry random suffixes from the Testing seed; the narration does not read them out.
