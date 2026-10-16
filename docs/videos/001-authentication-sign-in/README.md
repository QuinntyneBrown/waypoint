# 001 · Signing in to Waypoint

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [001-authentication-sign-in.mp4](001-authentication-sign-in.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Everything in Waypoint starts with signing in. This video shows what a person sees, where they land, and how Waypoint keeps the door safe without giving anything away. It follows [the sign-in design](../../detailed-designs/authentication/sign-in/README.md) and the acceptance criteria of L2-AUTH-001 and L2-AUTH-006 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe what happens when someone signs in with the right credentials.
- Explain why a wrong email and a wrong password look the same.
- State when an account locks, for how long, and the two ways back in.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Where does someone land after signing in? | The home made for their role; a youth lands on the youth home |
| What does a wrong guess reveal? | Nothing: the same "invalid email or password" message for an unknown email and a wrong password |
| When does an account lock? | After five failed attempts, for fifteen minutes, with reset password and contact your mentor still available |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/form-1280.png` | The empty sign-in page, including keep me signed in |
| `images/error-1280.png` | The generic failure message |
| `images/youth-home-1280.png` | Amara's home after signing in |
| `images/locked-1280.png` | The account-locked page |

Captures are made by `frontend/video-capture/001-authentication-sign-in.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why signing in matters |
| 00:15-01:00 | Signing in | Amara signs in and lands on the youth home |
| 01:00-01:25 | Keep me signed in | The box and the shared-device warning |
| 01:25-02:15 | When something goes wrong | Generic error, lockout, recovery |
| 02:15-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/001-authentication-sign-in.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/001-authentication-sign-in --engine kokoro
node tools/video-build/build-video.mjs docs/videos/001-authentication-sign-in
```

## Pitfalls

- The lock threshold and duration are configured values; fifteen minutes is the default in the specification.
