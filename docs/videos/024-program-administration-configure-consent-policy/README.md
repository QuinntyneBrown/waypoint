# 024 · Setting the consent policy

> **Runtime:** about 2 min 50 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [024-program-administration-configure-consent-policy.mp4](024-program-administration-configure-consent-policy.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/configure-consent-policy/README.md) and the acceptance criteria of L2-ADMIN-006 (acceptance criteria 4 and 5) in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three consent settings and what each does.
- Explain that saving is audit logged and limited to a program administrator.
- Describe where a guardian sees the policy.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What are the three consent controls? | Consent statement, require re-consent each term, require media consent for youth media |
| Who may change them, and is it recorded? | Only a program administrator; saved and recorded in the audit log |
| What happens to a minor with no participation consent? | Blocked from participation and routed to obtain consent (L2-CONSENT-002) |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/policy-1280.png` | The consent policy panel |
| `images/saved-1280.png` | Saved message |
| `images/guardian-form-1280.png` | The guardian consent form |

Captures are made by `frontend/video-capture/024-program-administration-configure-consent-policy.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why policy matters |
| 00:15-01:10 | The consent policy panel | Statement and two checkboxes |
| 01:10-01:35 | Saving the policy | Audit log, administrators only |
| 01:35-02:25 | What the guardian sees | Statement and media policy on the form |
| 02:25-02:50 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/024-program-administration-configure-consent-policy.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/024-program-administration-configure-consent-policy --engine kokoro
node tools/video-build/build-video.mjs docs/videos/024-program-administration-configure-consent-policy
```

## Pitfalls

- Organization settings are shared by the whole demo database, so the statement wording on screen is whatever was saved last; the capture saves it itself immediately before the shot.
