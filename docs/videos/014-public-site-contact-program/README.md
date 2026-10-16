# 014 · Contacting the program

> **Runtime:** about 2 min 30 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [014-public-site-contact-program.mp4](014-public-site-contact-program.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

Not everyone is ready to apply. This video shows the public contact form, its field-level validation and the acknowledgement. It follows [the design](../../detailed-designs/public-site/contact-program/README.md) and the acceptance criteria of L2-PUB-003 (criterion 1) and L2-NFR-001 in [the public site L2 specification](../../specs/public-site/L2.md).

## Learning objectives

By the end, the viewer can:

- List the four required fields of the contact form.
- Explain what happens when a field is missing or the email is malformed.
- Describe the acknowledgement a visitor sees after sending.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What does the form require? | Name, email, topic and message |
| What happens with bad input? | Nothing is sent; each field shows its own message |
| What confirms success? | The message sent page |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/page-1280.png` | Top of the contact page |
| `images/form-1280.png` | The empty form |
| `images/errors-1280.png` | Field-level errors |
| `images/sent-1280.png` | The message sent acknowledgement |

Captures are made by `frontend/video-capture/014-public-site-contact-program.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:20 | Welcome | Why a contact form |
| 00:20-01:05 | The contact page | Contact details and the form |
| 01:05-01:30 | When something is missing | Field-level errors |
| 01:30-02:10 | Message sent | Acknowledgement and server-side checks |
| 02:10-02:30 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/014-public-site-contact-program.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/014-public-site-contact-program --engine kokoro
node tools/video-build/build-video.mjs docs/videos/014-public-site-contact-program
```

## Pitfalls

- The server-side validation and sanitising is stated from L2-PUB-003 criterion 4; it has no visible screen.
