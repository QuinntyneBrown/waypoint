# 082 · Finding answers in the help center

> **Runtime:** about 2 min 51 s · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [082-platform-services-browse-help.mp4](082-platform-services-browse-help.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

The help center lets signed-in people find answers themselves. This video shows topics, expanding an answer, role-appropriate content, search, the no-results state and the contact support card. It follows [the design](../../detailed-designs/platform-services/browse-help/README.md) and the acceptance criteria of L2-PLATFORM-005 (criterion 1) in [the platform services L2 specification](../../specs/platform-services/L2.md).

## Learning objectives

By the end, the viewer can:

- Open the help center and read an answer.
- Explain why a mentor and a youth see different questions.
- Use search, and know what happens when nothing matches.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| Are the answers the same for every role? | No. Each question is written for one or more roles, and a person sees only theirs |
| What if search finds nothing? | The page says "No help articles found" and offers the support team |
| Where does someone go if the answer is not there? | The Contact support button at the bottom of the page |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/help-1280.png` | The help center as a youth sees it |
| `images/answer-1280.png` | An expanded answer |
| `images/mentor-1280.png` | The same page as a mentor sees it |
| `images/search-1280.png` | Search narrowed to one question, with the contact support card |
| `images/none-1280.png` | The no-results state |

Captures are made by `frontend/video-capture/082-platform-services-browse-help.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:18 | Welcome | Welcome |
| 00:18-01:01 | Opening the help center | Opening the help center |
| 01:01-01:31 | Written for your role | Written for your role |
| 01:31-02:03 | Searching | Searching |
| 02:03-02:27 | Still need a hand | Still need a hand |
| 02:27-02:51 | Things to remember | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/082-platform-services-browse-help.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/082-platform-services-browse-help --engine kokoro
node tools/video-build/build-video.mjs docs/videos/082-platform-services-browse-help
```

## Pitfalls

- The help content is a fixed list in the front end, so the questions shown are those at the time of capture. Search also matches keywords, so a word can match a question that does not contain it.
