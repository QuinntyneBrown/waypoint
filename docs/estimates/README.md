# Estimates

- Overall effort: 437 days
- Complete: 100% of the designed scope, with the known gaps below

| Subsystem | Effort | Complete |
|---|---|---|
| [authentication](authentication.md) | 45 days | 100% |
| [consent-and-safeguarding](consent-and-safeguarding.md) | 18 days | 100% |
| [program-administration](program-administration.md) | 41 days | 100% |
| [public-site](public-site.md) | 22 days | 100% |
| [application-intake](application-intake.md) | 12 days | 100% |
| [curriculum-authoring](curriculum-authoring.md) | 24 days | 100% |
| [learning-experience](learning-experience.md) | 48 days | 100% |
| [mentoring](mentoring.md) | 34 days | 100% |
| [engagement](engagement.md) | 40 days | 100% |
| [guardian-portal](guardian-portal.md) | 17 days | 100% |
| [volunteer-portal](volunteer-portal.md) | 12 days | 100% |
| [reporting-and-data](reporting-and-data.md) | 26 days | 100% |
| [platform-services](platform-services.md) | 57 days | 100% |
| [non-functional](non-functional.md) | 41 days | 100% |

## Known gaps

Percentages measure the designs as written. These gaps sit outside them:

- **Volunteer assignment** — no product path assigns a volunteer to a session, so the volunteer portal is empty outside seeded data (descoped, `out-of-scope.md` #14).
- **Donation provider** — `start-donation` hands off to a placeholder provider URL until the product owner supplies the real one (DECISIONS #13).
- **Keyboard accessibility** — about 48 click handlers on non-focusable elements are lint warnings, not yet fixed (issue #35).
- **Journey and feature videos** — 115 videos have screenshots captured before the CSS fixes and need re-recording (issues #51, #62); they also predate the PromiseHub look (DECISIONS #25).

## Roadmap

1. Every subsystem is implemented and tested, including the PromiseHub design system and the `ph` prefix (DECISIONS #25), so no work remains.
