# 31 · Configure organization

> **Runtime:** about 3 min 48 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [31-configure-organization.mp4](31-configure-organization.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Organization-wide settings shape what everyone sees. This video follows Pat Morgan setting the identity, the guardian consent policy and a term, then shows where the policy appears on a guardian consent form. It follows the designs for [the organization profile](../../../detailed-designs/program-administration/configure-organization-profile/README.md), [the consent policy](../../../detailed-designs/program-administration/configure-consent-policy/README.md), [terms and tracks](../../../detailed-designs/program-administration/manage-terms-and-tracks/README.md), and the acceptance criteria in [the program administration L2 specification](../../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the organization identity fields.
- Name the three consent policy controls.
- Explain what pausing a track keeps and blocks.
- Explain where guardians see the statement and contact email.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What can the organization not change? | The colour palette, which stays fixed for accessibility |
| Who can save settings? | Only a program administrator, and it is recorded in the audit log |
| What does a guardian see? | The participation statement and the contact email |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/identity-1280.png` | The organization identity panel |
| `images/policy-1280.png` | The consent policy panel |
| `images/saved-1280.png` | The saved message |
| `images/term-saved-1280.png` | The terms panel |
| `images/guardian-form-1280.png` | The guardian consent form |

Captures are made by `frontend/video-capture/journeys/31-configure-organization.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Every program has a name, a place, a contact, and rules about guardian permission. |
| 00:20-01:01 | Organization identity | Pat opens Organization settings. |
| 01:03-01:40 | The consent policy | Beside it is the guardian consent policy. |
| 01:42-02:03 | Saving | Pat presses save organization settings. |
| 02:05-02:39 | Terms and tracks | Lower on the same page is the panel called terms and cohort cycles. |
| 02:41-03:21 | What the guardian sees | Now meet Ruth Clarke, a guardian. |
| 03:23-03:48 | Things to remember | Identity, consent policy, terms, and track availability all live on one page. |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/31
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/31-configure-organization --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/31-configure-organization
```

## Pitfalls

- Pausing a track is narrated but not captured, because the shared test data holds many tracks.
- The guardian form heading uses the shared test organization name, and the child name is a seeded one.
