# 080 · Unlocking a young person's participation

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [080-consent-and-safeguarding-unlock-minor-participation.mp4](080-consent-and-safeguarding-unlock-minor-participation.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A minor without participation consent must be held back from learning and routed to obtain consent, and unlocked as soon as it is given. It follows [the design](../../detailed-designs/consent-and-safeguarding/unlock-minor-participation/README.md) and the acceptance criteria of L2-CONSENT-002 in [the L2 specification](../../specs/consent-and-safeguarding/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a blocked young person sees and can do.
- Explain how the guardian is asked and how they sign.
- State what happens to the young person's access afterwards.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does a blocked minor see? | A screen headed a guardian needs to sign off, with Notify my guardian, My guardian can sign now and Sign out. |
| How does the guardian sign? | Through a secure one-time link, with a typed signature, and no account is needed. |
| Is manual unlocking needed? | No. Access opens as soon as consent is recorded. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/gate-1280.png` | Held here until consent is recorded |
| `images/notified-1280.png` | Delivery confirmed, destination masked |
| `images/form-1280.png` | Names the youth, shows the expiry |
| `images/recorded-1280.png` | Access updates automatically |
| `images/unlocked-1280.png` | Ready to begin |

Captures are made by `frontend/video-capture/080-consent-and-safeguarding-unlock-minor-participation.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | The journey |
| 00:20-01:00 | The blocked screen | What Imani sees |
| 01:00-01:30 | Asking the guardian | Notify and sign now |
| 01:30-02:20 | The guardian signs | Form and receipt |
| 02:20-02:40 | Imani gets in | Home page |
| 02:40-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/080-consent-and-safeguarding-unlock-minor-participation.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/080-consent-and-safeguarding-unlock-minor-participation --engine kokoro
node tools/video-build/build-video.mjs docs/videos/080-consent-and-safeguarding-unlock-minor-participation
```

## Pitfalls

- The destination in the sent message is masked by the product (for example r***@...).
- The home page shows seeded cohort and session names with random suffixes.
