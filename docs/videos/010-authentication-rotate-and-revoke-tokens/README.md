# 010 · Keeping a session safe

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [010-authentication-rotate-and-revoke-tokens.mp4](010-authentication-rotate-and-revoke-tokens.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

It follows [the rotate and revoke tokens design](../../detailed-designs/authentication/rotate-and-revoke-tokens/README.md) and the acceptance criteria of L2-AUTH-012, L2-NFR-002 and L2-AUTH-007 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the access token and the refresh token in plain words.
- Explain what happens when an old refresh token is used again.
- State what signing out and changing a password do to tokens.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| How long does the access token last? | About an hour |
| What happens when a rotated token is used again? | The whole token family is cancelled and the person signs in again |
| What does signing out do? | It cancels the session refresh tokens and clears the session from the browser |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/signed-in-1280.png` | The home page after sign-in |
| `images/session-expired-1280.png` | The signed-out page after token reuse |
| `images/signed-out-1280.png` | The sign-in page after signing out |

Captures are made by `frontend/video-capture/010-authentication-rotate-and-revoke-tokens.capture.ts`.

## Run sheet

| Segment | Content |
| --- | --- |
| Welcome | What tokens are |
| Two keys | Access and refresh tokens, rotation |
| A copied key | Reuse detection and the signed-out page |
| Signing out | Shared-computer sign-out |
| In transit | Secure connections |
| Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/010-authentication-rotate-and-revoke-tokens.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/010-authentication-rotate-and-revoke-tokens --engine kokoro
node tools/video-build/build-video.mjs docs/videos/010-authentication-rotate-and-revoke-tokens
```

## Pitfalls

- The page shown after token reuse is the same session expired page used for idle timeouts; its wording mentions inactivity. The In transit part has no screen of its own, so it is text only.
