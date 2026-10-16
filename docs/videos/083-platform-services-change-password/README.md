# 083 · Changing your password

> **Runtime:** about 2 min 29 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [083-platform-services-change-password.mp4](083-platform-services-change-password.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

A signed-in person can change their own password from profile settings. This video shows the form, the on-screen rules, and the confirmation. It follows [the design](../../detailed-designs/platform-services/change-password/README.md) and the acceptance criteria of L2-PLATFORM-002 (criterion 4) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Name the three fields of the change password form.
- State the password rule and when the button is available.
- Say what happens to the person’s other sessions after a change.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| When is the Change password button available? | Only when the new password meets the rule and both copies match |
| What is the password rule shown on screen? | At least 8 characters, including a letter and a number |
| What happens to other sessions? | They are invalidated, as the L2 criterion states |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/ready-1280.png` | The form filled in correctly |
| `images/policy-1280.png` | The rule hint with a short password |
| `images/mismatch-1280.png` | The passwords do not match message |
| `images/changed-1280.png` | The password changed confirmation |

Captures are made by `frontend/video-capture/083-platform-services-change-password.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:16 | Welcome | Welcome |
| 00:16-00:49 | Where to find it | Where to find it |
| 00:49-01:39 | Rules on the screen | Rules on the screen |
| 01:39-02:05 | Saving the change | Saving the change |
| 02:05-02:29 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/083-platform-services-change-password.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/083-platform-services-change-password --engine kokoro
node tools/video-build/build-video.mjs docs/videos/083-platform-services-change-password
```

## Pitfalls

- The script does not show a wrong current password. In the running portal that case returns a forbidden response and the front end shows the 403 page instead of an inline message, so the video does not claim a behaviour for it. The other-sessions statement comes from the L2 criterion and is not shown on screen.
