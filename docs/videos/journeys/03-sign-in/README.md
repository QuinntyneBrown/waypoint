# 03 · Sign in

> **Runtime:** about 4 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [03-sign-in.mp4](03-sign-in.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Journey 3 in [the journey list](../../../journeys.md). It follows Amara, Grace and Imani through the sign-in door: a wrong guess, a correct sign-in, a landing page per role, and the lockout. It follows [the sign-in design](../../../detailed-designs/authentication/sign-in/README.md) and [the account recovery design](../../../detailed-designs/authentication/recover-locked-account/README.md), with criteria in [the authentication L2 specification](../../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what a person sees when signing in, and where each role lands.
- Explain why a wrong email and a wrong password look the same.
- State what a lockout looks like and the two ways back in.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Where does someone land after signing in? | The home made for their role: youth home or mentor dashboard |
| What does a wrong guess reveal? | Nothing: the same invalid email or password message |
| What happens after five misses? | The too many sign-in attempts page, about fifteen minutes, reset password and contact your mentor |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The sign-in page |
| `images/error-1280.png` | The generic failure message |
| `images/youth-home-1280.png` | Amara's youth home |
| `images/mentor-home-1280.png` | Grace's mentor dashboard |
| `images/locked-1280.png` | The account locked page |

Captures are made by `frontend/video-capture/journeys/03-sign-in.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | One door, three people |
| 00:20-01:20 | Opening the door | Sign-in page, generic error, correct sign-in |
| 01:20-02:30 | Landing at home | Youth home and mentor dashboard |
| 02:30-03:30 | Too many guesses | Lockout and two ways back |
| 03:30-04:00 | Things to remember | Recap |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts journeys/03
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/journeys/03-sign-in --engine kokoro
node tools/video-build/build-video.mjs docs/videos/journeys/03-sign-in
```

## Pitfalls

- The lock threshold and duration are configured values; five misses and fifteen minutes are the defaults in the specification.
- Amara's home shows the not-in-a-cohort state because the captured account has not been placed.
