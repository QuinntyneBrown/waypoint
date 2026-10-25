# Videos

Narrated videos made from a script, a slide deck and captions, one per detailed design in `docs/detailed-designs/`. See the `creating-videos` skill in `.claude/skills/` for the workflow. Tools live in `tools/video-audio/` and `tools/video-build/`; screen captures are made by `frontend/video-capture/` against the running app with synthetic data. Only the `.mp4` files are committed (Git LFS, see `.gitattributes`); `.mp3` files and `.cache/` folders are build outputs and are ignored. Each MP4 carries a selectable caption track.

## Authentication

| Video | Topic | Runtime |
| --- | --- | --- |
| [001 · Signing in to Waypoint](001-authentication-sign-in/README.md) | sign in | 2 min 44 s |
| [002 · Accepting an invitation](002-authentication-accept-invitation/README.md) | accept invitation | 2 min 56 s |
| [003 · Who is allowed to do what](003-authentication-authorize-protected-actions/README.md) | authorize protected actions | 2 min 48 s |
| [004 · Signing out and session expiry](004-authentication-end-user-session/README.md) | end user session | 2 min 31 s |
| [005 · Sending invitations](005-authentication-issue-invitations/README.md) | issue invitations | 2 min 56 s |
| [006 · Managing pending invitations](006-authentication-manage-pending-invitations/README.md) | manage pending invitations | 2 min 31 s |
| [007 · Setting up a new youth](007-authentication-onboard-youth/README.md) | onboard youth | 2 min 52 s |
| [008 · Recovering a locked account](008-authentication-recover-locked-account/README.md) | recover locked account | 2 min 50 s |
| [009 · Recovering a password](009-authentication-recover-password/README.md) | recover password | 3 min 07 s |
| [010 · Keeping a session safe](010-authentication-rotate-and-revoke-tokens/README.md) | rotate and revoke tokens | 3 min 09 s |
| [011 · Seeing only what your role allows](011-authentication-scope-data-by-role/README.md) | scope data by role | 2 min 51 s |
| [012 · Setting a secure password](012-authentication-set-secure-password/README.md) | set secure password | 2 min 53 s |

## Public site

| Video | Topic | Runtime |
| --- | --- | --- |
| [013 · Browsing current cohorts](013-public-site-browse-current-cohorts/README.md) | browse current cohorts | 2 min 44 s |
| [014 · Contacting the program](014-public-site-contact-program/README.md) | contact program | 2 min 46 s |
| [015 · Exploring the programs](015-public-site-explore-programs/README.md) | explore programs | 2 min 32 s |
| [016 · Finding your way around the public site](016-public-site-navigate-public-site/README.md) | navigate public site | 2 min 35 s |
| [017 · Offering to volunteer](017-public-site-register-volunteer-interest/README.md) | register volunteer interest | 2 min 42 s |
| [018 · Starting a donation](018-public-site-start-donation/README.md) | start donation | 2 min 42 s |
| [019 · The journey and impact on the home page](019-public-site-view-journey-and-impact/README.md) | view journey and impact | 2 min 35 s |

## Application intake

| Video | Topic | Runtime |
| --- | --- | --- |
| [020 · Deciding on an application and enrolling the applicant](020-application-intake-decide-and-enroll-applicant/README.md) | decide and enroll applicant | 3 min 53 s |
| [021 · Reviewing an application](021-application-intake-review-application/README.md) | review application | 3 min 38 s |
| [022 · Submitting an application](022-application-intake-submit-application/README.md) | submit application | 3 min 46 s |

## Program administration

| Video | Topic | Runtime |
| --- | --- | --- |
| [023 · Assigning mentors](023-program-administration-assign-mentors/README.md) | assign mentors | 3 min 09 s |
| [024 · Setting the consent policy](024-program-administration-configure-consent-policy/README.md) | configure consent policy | 3 min 03 s |
| [025 · The organization profile](025-program-administration-configure-organization-profile/README.md) | configure organization profile | 2 min 55 s |
| [026 · Managing an account](026-program-administration-manage-account-lifecycle/README.md) | manage account lifecycle | 3 min 03 s |
| [027 · Managing cohorts](027-program-administration-manage-cohorts/README.md) | manage cohorts | 2 min 52 s |
| [028 · Terms and tracks](028-program-administration-manage-terms-and-tracks/README.md) | manage terms and tracks | 3 min 00 s |
| [029 · Finding people](029-program-administration-search-people/README.md) | search people | 2 min 57 s |
| [030 · A person's record](030-program-administration-view-person-record/README.md) | view person record | 2 min 49 s |
| [031 · The progress board](031-program-administration-view-progress-board/README.md) | view progress board | 2 min 58 s |

## Learning experience

| Video | Topic | Runtime |
| --- | --- | --- |
| [032 · Browsing the curriculum](032-learning-experience-browse-curriculum/README.md) | browse curriculum | 2 min 32 s |
| [033 · Completing a coding lesson](033-learning-experience-complete-coding-lessons/README.md) | complete coding lessons | 2 min 33 s |
| [034 · Reading and completing a lesson](034-learning-experience-complete-lessons/README.md) | complete lessons | 2 min 24 s |
| [035 · Reviewing your own progress](035-learning-experience-review-personal-progress/README.md) | review personal progress | 2 min 22 s |
| [036 · Reviewing a quiz result](036-learning-experience-review-quiz-results/README.md) | review quiz results | 2 min 19 s |
| [037 · Submitting an assignment](037-learning-experience-submit-assignments/README.md) | submit assignments | 2 min 56 s |
| [038 · Submitting a coding project](038-learning-experience-submit-coding-projects/README.md) | submit coding projects | 2 min 48 s |
| [039 · Taking a quiz](039-learning-experience-take-quizzes/README.md) | take quizzes | 2 min 49 s |
| [040 · Tracking and revising a submission](040-learning-experience-track-and-revise-submissions/README.md) | track and revise submissions | 2 min 37 s |
| [041 · Viewing badges](041-learning-experience-view-badges/README.md) | view badges | 2 min 18 s |
| [042 · The learning journey](042-learning-experience-view-learning-journey/README.md) | view learning journey | 2 min 25 s |
| [043 · Writing a reflection](043-learning-experience-write-reflections/README.md) | write reflections | 2 min 45 s |
| [116 · Streaming a lesson video](116-learning-experience-stream-lesson-video/README.md) | stream lesson video | 5 min 33 s |

## Curriculum authoring

| Video | Topic | Runtime |
| --- | --- | --- |
| [044 · Authoring assignments](044-curriculum-authoring-author-assignments/README.md) | author assignments | 3 min 14 s |
| [045 · Authoring badges](045-curriculum-authoring-author-badges/README.md) | author badges | 3 min 09 s |
| [046 · Authoring lessons](046-curriculum-authoring-author-lessons/README.md) | author lessons | 3 min 18 s |
| [047 · Authoring the progression framework](047-curriculum-authoring-author-progression-framework/README.md) | author progression framework | 3 min 19 s |
| [048 · Authoring quizzes](048-curriculum-authoring-author-quizzes/README.md) | author quizzes | 3 min 17 s |
| [049 · Authoring tracks and modules](049-curriculum-authoring-author-tracks-and-modules/README.md) | author tracks and modules | 3 min 20 s |
| [050 · Awarding badges](050-curriculum-authoring-award-badges/README.md) | award badges | 2 min 37 s |
| [115 · Uploading a lesson video](115-curriculum-authoring-upload-lesson-video/README.md) | upload lesson video | 5 min 39 s |

## Mentoring

| Video | Topic | Runtime |
| --- | --- | --- |
| [051 · Approving quiz scores](051-mentoring-approve-quiz-assessments/README.md) | approve quiz assessments | 2 min 59 s |
| [052 · Assessing competencies](052-mentoring-assess-competencies/README.md) | assess competencies | 2 min 32 s |
| [053 · Assigning work](053-mentoring-assign-work/README.md) | assign work | 3 min 01 s |
| [054 · Grading a submission](054-mentoring-grade-submissions/README.md) | grade submissions | 3 min 09 s |
| [055 · Keeping watch on at-risk youth](055-mentoring-manage-at-risk-flags/README.md) | manage at risk flags | 2 min 56 s |
| [056 · The mentor dashboard and roster](056-mentoring-manage-mentor-roster/README.md) | manage mentor roster | 2 min 40 s |
| [057 · Recording a check-in](057-mentoring-record-check-ins/README.md) | record check ins | 3 min 11 s |
| [058 · The youth record](058-mentoring-view-youth-record/README.md) | view youth record | 2 min 58 s |

## Guardian portal

| Video | Topic | Runtime |
| --- | --- | --- |
| [059 · Managing consent settings](059-guardian-portal-manage-consent-settings/README.md) | manage consent settings | 3 min 10 s |
| [060 · Keeping contact details up to date](060-guardian-portal-manage-contact-details/README.md) | manage contact details | 2 min 19 s |
| [061 · Reading announcements](061-guardian-portal-read-announcements/README.md) | read announcements | 2 min 23 s |
| [062 · Following a young person's progress](062-guardian-portal-view-child-progress/README.md) | view child progress | 2 min 49 s |
| [063 · Viewing the consent record](063-guardian-portal-view-consent-record/README.md) | view consent record | 2 min 24 s |

## Volunteer portal

| Video | Topic | Runtime |
| --- | --- | --- |
| [064 · Recording session attendance](064-volunteer-portal-record-session-attendance/README.md) | record session attendance | 3 min 45 s |
| [065 · Viewing today's session](065-volunteer-portal-view-todays-session/README.md) | view todays session | 2 min 48 s |
| [066 · Viewing your upcoming schedule](066-volunteer-portal-view-upcoming-schedule/README.md) | view upcoming schedule | 2 min 54 s |

## Engagement

| Video | Topic | Runtime |
| --- | --- | --- |
| [067 · Browsing the program calendar](067-engagement-browse-calendar/README.md) | browse calendar | 2 min 45 s |
| [068 · Exchanging messages](068-engagement-exchange-messages/README.md) | exchange messages | 2 min 56 s |
| [069 · Publishing announcements](069-engagement-publish-announcements/README.md) | publish announcements | 2 min 49 s |
| [070 · Receiving notifications](070-engagement-receive-notifications/README.md) | receive notifications | 2 min 53 s |
| [071 · Recording attendance](071-engagement-record-attendance/README.md) | record attendance | 2 min 50 s |
| [072 · Reviewing attendance](072-engagement-review-attendance/README.md) | review attendance | 2 min 43 s |
| [073 · Replying to a session](073-engagement-rsvp-to-session/README.md) | rsvp to session | 2 min 34 s |
| [074 · Scheduling sessions](074-engagement-schedule-sessions/README.md) | schedule sessions | 2 min 34 s |
| [075 · Recording attendance without a connection](075-engagement-sync-offline-attendance/README.md) | sync offline attendance | 2 min 56 s |

## Consent and safeguarding

| Video | Topic | Runtime |
| --- | --- | --- |
| [076 · Safeguards for young people](076-consent-and-safeguarding-enforce-minor-safeguards/README.md) | enforce minor safeguards | 2 min 45 s |
| [077 · Inspecting and exporting the audit log](077-consent-and-safeguarding-inspect-and-export-audit-log/README.md) | inspect and export audit log | 3 min 15 s |
| [078 · Capturing and managing guardian consent](078-consent-and-safeguarding-manage-guardian-consent/README.md) | manage guardian consent | 3 min 00 s |
| [079 · Recording audit events](079-consent-and-safeguarding-record-audit-events/README.md) | record audit events | 2 min 33 s |
| [080 · Unlocking a young person's participation](080-consent-and-safeguarding-unlock-minor-participation/README.md) | unlock minor participation | 3 min 11 s |

## Platform services

| Video | Topic | Runtime |
| --- | --- | --- |
| [081 · Waiting to be placed in a cohort](081-platform-services-await-cohort-placement/README.md) | await cohort placement | 2 min 32 s |
| [082 · Finding answers in the help center](082-platform-services-browse-help/README.md) | browse help | 2 min 52 s |
| [083 · Changing your password](083-platform-services-change-password/README.md) | change password | 2 min 37 s |
| [084 · Making Waypoint easier to read](084-platform-services-configure-display/README.md) | configure display | 2 min 37 s |
| [085 · Choosing which notifications you get](085-platform-services-configure-notifications/README.md) | configure notifications | 2 min 36 s |
| [086 · Editing your profile](086-platform-services-edit-profile/README.md) | edit profile | 2 min 36 s |
| [087 · When an account is deactivated](087-platform-services-handle-deactivated-account/README.md) | handle deactivated account | 2 min 19 s |
| [088 · Planned maintenance](088-platform-services-handle-planned-maintenance/README.md) | handle planned maintenance | 2 min 27 s |
| [089 · When a guardian needs to sign off](089-platform-services-obtain-required-consent/README.md) | obtain required consent | 2 min 37 s |
| [090 · Coming back after a session expires](090-platform-services-recover-expired-session/README.md) | recover expired session | 2 min 15 s |
| [091 · When a page is not for your role](091-platform-services-recover-from-forbidden-access/README.md) | recover from forbidden access | 2 min 08 s |
| [092 · When a page cannot be found](092-platform-services-recover-from-missing-content/README.md) | recover from missing content | 2 min 16 s |
| [093 · When something goes wrong on our side](093-platform-services-recover-from-server-error/README.md) | recover from server error | 2 min 08 s |
| [094 · Replacing an expired invitation](094-platform-services-replace-expired-invitation/README.md) | replace expired invitation | 2 min 39 s |
| [095 · Working without a connection](095-platform-services-show-offline-sync-state/README.md) | show offline sync state | 2 min 25 s |
| [096 · Asking the program for help](096-platform-services-submit-support-request/README.md) | submit support request | 2 min 31 s |

## Reporting and data

| Video | Topic | Runtime |
| --- | --- | --- |
| [097 · Exploring reports](097-reporting-and-data-explore-reports/README.md) | explore reports | 3 min 12 s |
| [098 · Exporting filtered data](098-reporting-and-data-export-filtered-data/README.md) | export filtered data | 3 min 09 s |
| [099 · Importing a roster](099-reporting-and-data-import-roster/README.md) | import roster | 3 min 25 s |
| [100 · The cohort report](100-reporting-and-data-view-cohort-report/README.md) | view cohort report | 2 min 31 s |
| [101 · The staff activity report](101-reporting-and-data-view-staff-report/README.md) | view staff report | 2 min 15 s |
| [102 · The youth report](102-reporting-and-data-view-youth-report/README.md) | view youth report | 2 min 22 s |

## Non functional

| Video | Topic | Runtime |
| --- | --- | --- |
| [103 · Adapting to every screen size](103-non-functional-adapt-layouts/README.md) | adapt layouts | 2 min 55 s |
| [104 · Live notifications and messages](104-non-functional-deliver-realtime-events/README.md) | deliver realtime events | 2 min 42 s |
| [105 · Accessible to everyone](105-non-functional-meet-accessibility-standard/README.md) | meet accessibility standard | 2 min 34 s |
| [106 · Staying fast](106-non-functional-meet-performance-budgets/README.md) | meet performance budgets | 3 min 08 s |
| [107 · Running and watching Waypoint](107-non-functional-operate-and-observe-system/README.md) | operate and observe system | 3 min 20 s |
| [108 · Protecting against overwritten work](108-non-functional-protect-concurrent-updates/README.md) | protect concurrent updates | 2 min 44 s |
| [109 · Protecting personal information](109-non-functional-protect-pii-and-data-rights/README.md) | protect pii and data rights | 3 min 19 s |
| [110 · Protecting work you have not saved](110-non-functional-protect-unsaved-work/README.md) | protect unsaved work | 2 min 33 s |
| [111 · Securing the browser boundary](111-non-functional-secure-browser-boundary/README.md) | secure browser boundary | 3 min 11 s |
| [112 · Syncing attendance reliably](112-non-functional-sync-attendance-reliably/README.md) | sync attendance reliably | 3 min 20 s |
| [113 · Checking what goes in and what comes out](113-non-functional-validate-input-and-output/README.md) | validate input and output | 2 min 56 s |

## Components

| Video | Topic | Runtime |
| --- | --- | --- |
| [114 · The Waypoint video player](114-components-video-player/README.md) | video player | 5 min 27 s |
