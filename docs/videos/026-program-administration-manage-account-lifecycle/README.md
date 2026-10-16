# 026 · Managing an account

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [026-program-administration-manage-account-lifecycle.mp4](026-program-administration-manage-account-lifecycle.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/manage-account-lifecycle/README.md) and the acceptance criteria of L2-ADMIN-002 (acceptance criteria 2 to 5) in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Describe the account actions on a person record.
- Explain the warning shown before a staff role change.
- Contrast reversible deactivation with irreversible removal and what is retained.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What happens when an account is deactivated? | The person is signed out and cannot sign in; records are retained; it can be reactivated |
| What does removal change? | Access is revoked and the person leaves active use; progress and reporting records are retained; it cannot be undone and needs the name typed |
| What does the role-change warning say? | A staff role grants access to young people's records; effective immediately; sessions are revoked |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/account-1280.png` | Account tab |
| `images/reset-1280.png` | Reset sent |
| `images/role-dialog-1280.png` | Role change dialog |
| `images/deactivate-dialog-1280.png` | Deactivate dialog |
| `images/deactivated-1280.png` | Deactivated account |
| `images/remove-dialog-1280.png` | Remove dialog |

Captures are made by `frontend/video-capture/026-program-administration-manage-account-lifecycle.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | One record for every change |
| 00:20-01:00 | The Account tab | Details and password reset |
| 01:00-01:40 | Changing a role | Warning and confirm |
| 01:40-02:50 | Deactivating and removing | Reversible versus irreversible |
| 02:50-03:10 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/026-program-administration-manage-account-lifecycle.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/026-program-administration-manage-account-lifecycle --engine kokoro
node tools/video-build/build-video.mjs docs/videos/026-program-administration-manage-account-lifecycle
```

## Pitfalls

- The role list on the Account tab starts on Youth in the capture; the narration does not mention it.
- The specification marks the audit log for role change and password reset; the narration does not claim it for deactivation or removal.
