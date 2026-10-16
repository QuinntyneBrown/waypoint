# 002 · Accepting an invitation

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [002-authentication-accept-invitation.mp4](002-authentication-accept-invitation.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Nobody can sign themselves up for Waypoint; people join by invitation. This video shows what the invitee sees, what they must provide, and what happens to links that cannot be used. It follows [the accept-invitation design](../../detailed-designs/authentication/accept-invitation/README.md) and the acceptance criteria of L2-AUTH-004 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- List what an invitee provides to accept an invitation.
- Explain that an invitation link works once.
- Describe what the invitee sees when a link is used, revoked or expired.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does an invitee provide? | A display name, a password that meets the policy, and agreement to the community guidelines |
| Where do they go next? | The first-time setup (onboarding), which can be skipped |
| What if the link is reused, revoked or expired? | It does not work; the page says why and, for an inactive link, offers a request for a new invite |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/invite-1280.png` | The join page with the masked email address |
| `images/filled-1280.png` | The form completed |
| `images/onboarding-1280.png` | The first onboarding step after joining |
| `images/used-1280.png` | The already-used state |
| `images/revoked-1280.png` | The no-longer-active state with the request button |

Captures are made by `frontend/video-capture/002-authentication-accept-invitation.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Invitation-only joining |
| 00:15-00:40 | Opening the link | The personal link and masked address |
| 00:40-01:25 | Setting up the account | Name, password, guidelines, then onboarding |
| 01:25-02:10 | Links that no longer work | Single use, revoked, expired, resend and revoke |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 002
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/002-authentication-accept-invitation --engine kokoro
node tools/video-build/build-video.mjs docs/videos/002-authentication-accept-invitation
```

## Pitfalls

- The fourteen-day expiry is marked "confirm with product owner" in the specification; the video only repeats what the page itself says.
- The capture shows the revoked state; the expired state uses the same page with a different heading.
