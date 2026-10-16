# 004 · Signing out and session expiry

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [004-authentication-end-user-session.mp4](004-authentication-end-user-session.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Shared computers are common in a youth program, so ending a session safely matters. This video shows signing out, automatic sign-out after inactivity, and how a draft survives it. It follows [the end-user-session design](../../detailed-designs/authentication/end-user-session/README.md) and the acceptance criteria of L2-AUTH-007 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Find the sign out button and say what it does.
- Describe what happens to an idle session.
- Explain what happens to work in progress when a session expires.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does sign out do? | Closes the session, removes the saved sign-in, and protected pages send the next person to sign in |
| What does an expired session show? | "You've been signed out", with a note that work in progress has been saved and a sign in again button |
| What happens to a draft? | It is kept for its owner and restored after signing in again, with a message |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/sign-out-button-1280.png` | The sign out button at the foot of the menu |
| `images/after-sign-out-1280.png` | The sign-in page after signing out |
| `images/draft-1280.png` | A reflection draft in progress |
| `images/expired-1280.png` | The session-expired page |
| `images/restored-1280.png` | The restored draft and its message |

Captures are made by `frontend/video-capture/004-authentication-end-user-session.capture.ts`. The idle timeout is simulated with a browser clock; the video does not state the timeout length.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why session ends matter |
| 00:15-00:50 | Signing out | The button and what it clears |
| 00:50-01:25 | When a session expires | Idle sign-out and the expired page |
| 01:25-02:00 | Picking up where you left off | Draft restored |
| 02:00-02:20 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 004
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/004-authentication-end-user-session --engine kokoro
node tools/video-build/build-video.mjs docs/videos/004-authentication-end-user-session
```

## Pitfalls

- The inactivity timeout length is not stated in the specification; do not quote a number.
