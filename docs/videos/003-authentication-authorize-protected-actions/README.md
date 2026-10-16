# 003 · Who is allowed to do what

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [003-authentication-authorize-protected-actions.mp4](003-authentication-authorize-protected-actions.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Signing in is not permission. This video shows what happens when someone is not signed in, has the wrong role, or reaches for a youth who is not theirs, and why hiding a button is never the only protection. It follows [the authorize-protected-actions design](../../detailed-designs/authentication/authorize-protected-actions/README.md) and the acceptance criteria of L2-AUTH-009 and L2-AUTH-010 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain what happens to a request when nobody is signed in.
- Describe what a person with the wrong role sees.
- Explain why a known youth identifier does not give a mentor access.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Who checks permission? | The server, on every protected action; the screen alone is never enough |
| What does the wrong role see? | "You don't have access to that", with back to your home and switch account; the attempt is logged |
| Can a mentor open any youth? | No, only youth assigned to them, even if they know another identifier |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/signed-out-1280.png` | The sign-in page a visitor lands on |
| `images/forbidden-1280.png` | The no-access page for a youth on an administrator address |
| `images/mentor-denied-1280.png` | The same page for a mentor on an unassigned youth |

Captures are made by `frontend/video-capture/003-authentication-authorize-protected-actions.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Sign-in is not permission |
| 00:15-00:35 | No sign-in, no access | Sent to sign in |
| 00:35-01:15 | The wrong role | No access page, server-side check, logging |
| 01:15-02:00 | Only your own people | Assigned youth, role scopes |
| 02:00-02:20 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 003
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/003-authentication-authorize-protected-actions --engine kokoro
node tools/video-build/build-video.mjs docs/videos/003-authentication-authorize-protected-actions
```

## Pitfalls

- The screen shows a redirect to sign in; the 401 and 403 responses themselves are not visible and are described only in words.
- The audit-log entry for a denied attempt is stated from the specification and is not shown on screen.
