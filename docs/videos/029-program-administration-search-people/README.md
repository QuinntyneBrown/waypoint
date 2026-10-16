# 029 · Finding people

> **Runtime:** about 3 min · **Audience:** program staff and stakeholders · **Prerequisites:** none

**Video:** [029-program-administration-search-people.mp4](029-program-administration-search-people.mp4) · [Slides](slides.html) · [Transcript](script.md)

## Why this video exists

This video shows what a program administrator sees and does for this part of program administration. It follows [the design](../../detailed-designs/program-administration/search-people/README.md) and the acceptance criteria of L2-ADMIN-001 in [the program administration L2 specification](../../specs/program-administration/L2.md).

## Learning objectives

By the end, the viewer can:

- Use the directory search and the role and cohort filters.
- Explain paging and why paging resets when a filter changes.
- Describe the empty state and its two actions.

## Key questions

| Question | What a strong answer includes |
| --- | --- |
| What can an administrator search and filter on? | Name or email text, role, and cohort |
| What does an empty result show? | No people found, with Clear filters and Invite people |
| What happens on a phone? | Rows reflow to labelled cards with no sideways page scroll |

## Code / assets on screen

| File | What to show |
| --- | --- |
| `images/directory-1280.png` | Directory controls |
| `images/search-1280.png` | Search results |
| `images/name-1280.png` | Surname search |
| `images/filters-1280.png` | Role filter |
| `images/empty-1280.png` | Empty state |

Captures are made by `frontend/video-capture/029-program-administration-search-people.capture.ts`.

## Run sheet

| Time | Segment | Content |
| --- | --- | --- |
| 00:00-00:15 | Welcome | Why search matters |
| 00:15-00:55 | The directory | Controls and table |
| 00:55-01:25 | Searching | Search box |
| 01:25-01:55 | Filtering | Role and cohort |
| 01:55-02:30 | When nobody matches | Empty state |
| 02:30-02:55 | Recap | Things to remember |

## Demo commands

```powershell
cd frontend
npx playwright test -c playwright.capture.config.ts video-capture/029-program-administration-search-people.capture.ts
cd ..
node tools/video-audio/generate-audio.mjs docs/videos/029-program-administration-search-people --engine kokoro
node tools/video-build/build-video.mjs docs/videos/029-program-administration-search-people
```

## Pitfalls

- The search term in the capture is a short random tag in the demo email addresses, so the screen shows only the cast.
- The cohort filter is described but not shown in use.
