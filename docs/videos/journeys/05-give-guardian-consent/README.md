# 05 · Give guardian consent

> **Runtime:** about 4 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [05-give-guardian-consent.mp4](05-give-guardian-consent.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Consent is the gate in front of every minor's participation. This journey follows one family end to end: the young person is blocked, the guardian signs through a secure link, and the guardian can later review or withdraw the consent. It draws on [the participation unlock design](../../../detailed-designs/consent-and-safeguarding/unlock-minor-participation/README.md), [guardian consent capture](../../../detailed-designs/consent-and-safeguarding/manage-guardian-consent/README.md) and [the guardian consent settings design](../../../detailed-designs/guardian-portal/manage-consent-settings/README.md), and the consent criteria in [the consent and safeguarding L2 specification](../../../specs/consent-and-safeguarding/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a young person without participation consent sees, and the two ways to ask a guardian.
- List what the guardian must provide, and what is optional.
- Explain what a guardian sees afterwards and what withdrawing does.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the guardian need to sign? | Full name, relationship, the participation permission and a typed signature; media choices are optional |
| Does the guardian need an account? | No, the secure one-time link opens the form |
| What does withdrawing do? | The badge turns to Withdrawn, the record stays, protected actions are blocked again until fresh consent |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/gate-1280.png` | The consent needed screen |
| `images/notified-1280.png` | The secure link confirmation |
| `images/form-1280.png` | The filled guardian form |
| `images/recorded-1280.png` | Consent recorded with reference |
| `images/record-1280.png` | The Active participation consent card |
| `images/withdrawn-1280.png` | The Withdrawn card |

Captures are made by `frontend/video-capture/journeys/05-give-guardian-consent.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:25 | Welcome | The journey in one paragraph |
| 00:25-00:55 | Imani is stopped | The consent needed screen |
| 00:55-01:25 | The secure link | Notify, or sign now |
| 01:25-02:30 | Ruth signs | The form and the recorded card |
| 02:30-03:30 | Looking back, or withdrawing | The Active record, withdrawal and effect |
| 03:30-04:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/05
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/05-give-guardian-consent --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/05-give-guardian-consent
```

## Pitfalls

- The validity period shown on the record comes from the consent policy; do not quote a fixed length.
- The capture shows the seeded sample record; its reference numbers differ on every run.
