# 006 · Managing pending invitations

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [006-authentication-manage-pending-invitations.mp4](006-authentication-manage-pending-invitations.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Invitations are not always accepted on the first try. This video shows how staff track an invitation, resend it, or revoke it, and what the invitee sees afterwards. It follows [the manage-pending-invitations design](../../detailed-designs/authentication/manage-pending-invitations/README.md) and the acceptance criteria of L2-AUTH-004 in [the authentication L2 specification](../../specs/authentication/L2.md).

## Learning objectives

By the end, the viewer can:

- Find an invitation and read its status.
- Explain what resend and revoke each do.
- Describe what someone sees when they open a revoked link.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does resend do? | Issues a fresh link and expiry for a pending (or expired) invitation |
| What does revoke do? | After a confirmation, sets the status to revoked and the link stops working immediately |
| What does the invitee see? | "This invite is no longer active", with a request a new invite button |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/pending-1280.png` | A pending invitation with resend and revoke |
| `images/resent-1280.png` | The fresh-invitation message |
| `images/confirm-1280.png` | The revoke confirmation |
| `images/revoked-1280.png` | The revoked status and its message |
| `images/dead-link-1280.png` | The revoked link opened by the invitee |

Captures are made by `frontend/video-capture/006-authentication-manage-pending-invitations.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why invitations need managing |
| 00:15-00:50 | Tracking an invitation | Search, status, buttons |
| 00:50-01:10 | Sending a fresh link | Resend |
| 01:10-02:10 | Cancelling an invitation | Confirm, revoked, dead link |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts 006
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/006-authentication-manage-pending-invitations --engine kokoro
node tools/video-build/build-video.mjs docs/videos/006-authentication-manage-pending-invitations
```

## Pitfalls

- The specification says a fresh link and expiry are issued on resend; it does not say the earlier link stops working, so the video does not claim it.
