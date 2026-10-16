# 076 · Safeguards for young people

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [076-consent-and-safeguarding-enforce-minor-safeguards.mp4](076-consent-and-safeguarding-enforce-minor-safeguards.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Because Waypoint serves minors, safeguards are enforced beyond ordinary privacy rules. This video names them for staff. It follows [the design](../../detailed-designs/consent-and-safeguarding/enforce-minor-safeguards/README.md) and the acceptance criteria of L2-CONSENT-004 and L2-CONSENT-002 in [the L2 specification](../../specs/consent-and-safeguarding/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain that participation consent is checked on every protected action.
- Describe what a guardian can and cannot see of mentor check-ins.
- State the rule for using media of a young person.
- Explain that a role does not by itself open youth records.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| When is consent checked? | On each protected youth action, server-side, not only at sign-in. |
| Which mentor check-ins reach guardians? | Only those the mentor explicitly shared. |
| When may media of a young person be used? | Only where media consent, with the applicable option, is on record and not withdrawn. |
| Does a mentor or volunteer role open every youth record? | No. Role assignment does not grant access beyond the role's defined scope. |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/lesson-1280.png` | Participation consent is on record |
| `images/gate-1280.png` | Checked again on the next protected action |
| `images/shared-only-1280.png` | Private check-ins never appear |

Captures are made by `frontend/video-capture/076-consent-and-safeguarding-enforce-minor-safeguards.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | The four safeguards |
| 00:20-01:05 | Consent before participation | Lesson, withdrawal, blocked screen |
| 01:05-01:40 | Private stays private | Shared mentor updates only |
| 01:40-02:05 | Media only with permission | Text slide |
| 02:05-02:30 | Roles do not open every door | Text slide |
| 02:30-03:00 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/076-consent-and-safeguarding-enforce-minor-safeguards.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/076-consent-and-safeguarding-enforce-minor-safeguards --engine kokoro
node tools/video-build/build-video.mjs docs/videos/076-consent-and-safeguarding-enforce-minor-safeguards
```

## Pitfalls

- The media and role safeguards have no dedicated screen in the design, so they use text-only slides.
- The guardian page capture uses a separate seeded family; the check-in names carry random suffixes.
