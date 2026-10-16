# 111 · Securing the browser boundary

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [111-non-functional-secure-browser-boundary.mp4](111-non-functional-secure-browser-boundary.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Waypoint's protections between the browser and the server are mostly invisible. This video explains them in plain words with text-only slides: how requests are authorised without cookies, which web addresses the server trusts, the safety instructions sent with every response, and the rate limits on sensitive endpoints. It follows [the secure browser boundary design](../../detailed-designs/non-functional/secure-browser-boundary/README.md) and the acceptance criteria of L2-NFR-002 in [the non-functional L2 specification](../../specs/non-functional/L2.md), with L2-AUTH-001 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Explain why another website cannot act as a signed-in person.
- State what the safety instructions on a response tell the browser to do.
- Name the endpoints that are rate limited and what the server says when the limit is passed.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Why can a forged request from another site not act as the user? | The page attaches the token itself; no cookie is sent automatically |
| Which browsers may call the server? | Only pages from the configured, approved web addresses |
| Which endpoints are rate limited? | Sign-in, password reset and invitations, by default twenty requests a minute per address |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `slides.html` | Four text-only slides; there are no screen captures |

This video uses text-only slides; there are no screen captures.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why the protections are quiet |
| 00:20-01:05 | Who may talk to Waypoint | Token, no cookie, trusted addresses |
| 01:05-01:50 | Safety labels on every answer | Frame, file type, referrer, secure connection, content policy |
| 01:50-02:40 | Slowing down guessers | Rate limits and the account lock |
| 02:40-03:00 | Recap | Things to remember |

## Demo commands

```powershell
node tools/video-audio/generate-audio.mjs docs/videos/111-non-functional-secure-browser-boundary --engine kokoro
node tools/video-build/build-video.mjs docs/videos/111-non-functional-secure-browser-boundary
```

## Pitfalls

- The rate-limit numbers are defaults in configuration, not fixed values; say so if asked.
- Rate limiting is switched off in the test environment so the acceptance suite stays deterministic.
- The protections are enforced by the API; the narration does not claim anything about the public site's own headers.
