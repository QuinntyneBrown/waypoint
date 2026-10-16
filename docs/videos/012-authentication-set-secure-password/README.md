# 012 · Setting a secure password

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [012-authentication-set-secure-password.mp4](012-authentication-set-secure-password.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

It follows [the set secure password design](../../detailed-designs/authentication/set-secure-password/README.md) and the acceptance criteria of L2-AUTH-008 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- State the password rule.
- Describe what the person sees when the rule is not met.
- Explain how passwords are stored.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What is the rule? | At least eight characters, with a letter and a number |
| Where does it apply? | Everywhere a password is set: invitation, reset and change |
| How is a password stored? | Only as a scrambled version, never as plain text |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/too-short-1280.png` | A password that is too short |
| `images/mismatch-1280.png` | The two boxes differ |
| `images/shown-1280.png` | The show password toggle |
| `images/invite-1280.png` | The same rule on the invitation page |

Captures are made by `frontend/video-capture/012-authentication-set-secure-password.capture.ts`.

## Run sheet

| Segment | Content |
| --- | --- |
| Welcome | One rule |
| The rule | Too short is refused |
| Matching and showing | Mismatch message, eye button |
| The same rule everywhere | The invitation page |
| Kept safe | Scrambled storage |
| Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/012-authentication-set-secure-password.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/012-authentication-set-secure-password --engine kokoro
node tools/video-build/build-video.mjs docs/videos/012-authentication-set-secure-password
```

## Pitfalls

- The password rule is marked "confirm with product owner" in the specification, so the video states it as the current rule. Storage details come from the specification, not from a screen. The Kept safe part is text only.
