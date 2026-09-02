import { WRole } from '@w/ui';

export interface HelpArticle {
  question: string;
  answer: string;
  roles?: readonly WRole[];
  keywords?: string;
}

export interface HelpTopic {
  title: string;
  icon: string;
  articles: readonly HelpArticle[];
}

const ALL_ROLES: readonly WRole[] = ['youth', 'mentor', 'admin', 'guardian', 'volunteer'];

export const HELP_TOPICS: readonly HelpTopic[] = [
  {
    title: 'Getting started',
    icon: 'compass',
    articles: [
      {
        question: 'Where should I begin?',
        answer:
          'Start from your role home. It highlights the next useful action and links to the areas available to you.',
        roles: ALL_ROLES,
      },
      {
        question: 'How do I update my profile or accessibility settings?',
        answer:
          'Open Profile & settings from the account menu. You can update your profile, notifications, display preferences, and privacy choices there.',
        roles: ALL_ROLES,
        keywords: 'text size contrast motion notifications password privacy',
      },
    ],
  },
  {
    title: 'Learning',
    icon: 'book-open',
    articles: [
      {
        question: 'Why is a lesson or quiz locked?',
        answer:
          'Some activities have prerequisites. Open Curriculum to see the required module or activity and complete it first.',
        roles: ['youth'],
        keywords: 'curriculum prerequisite module assignment',
      },
      {
        question: 'Where can I see feedback and badges?',
        answer:
          'Open My Journey for milestones and badges, or My Progress for quiz, attendance, and competency history.',
        roles: ['youth'],
        keywords: 'grade score review progress',
      },
      {
        question: 'How do I review a youth submission?',
        answer:
          'Open Reviews from the mentor navigation. Draft scores are preserved until you approve the work or request a revision.',
        roles: ['mentor'],
        keywords: 'grading rubric approve revision',
      },
    ],
  },
  {
    title: 'Program operations',
    icon: 'users',
    articles: [
      {
        question: 'How do I invite someone to Waypoint?',
        answer:
          'Open Invitations, choose the role and optional cohort context, then enter one or more email addresses. Links expire after 14 days.',
        roles: ['admin', 'mentor'],
        keywords: 'bulk resend revoke expired youth',
      },
      {
        question: 'How do I take attendance?',
        answer:
          'Open the assigned session from Today or Calendar. Attendance can be captured offline and will sync when connectivity returns.',
        roles: ['mentor', 'admin', 'volunteer'],
        keywords: 'present absent offline session roster',
      },
      {
        question: 'Where do I manage data-rights requests?',
        answer:
          'Program Administrators can review and fulfil requests from Privacy requests in the System navigation group.',
        roles: ['admin'],
        keywords: 'access correction removal consent retention',
      },
    ],
  },
  {
    title: 'Guardian and volunteer access',
    icon: 'shield',
    articles: [
      {
        question: 'What information can a guardian see?',
        answer:
          'Guardians see their own child’s shared progress, attendance, schedule, announcements, and consent record. Private reflections and quiz answers are never shown.',
        roles: ['guardian'],
        keywords: 'child privacy consent progress',
      },
      {
        question: 'Why can I only see one session roster?',
        answer:
          'Volunteer access is deliberately limited to assigned sessions and names-only attendance rosters.',
        roles: ['volunteer'],
        keywords: 'schedule youth records assigned',
      },
    ],
  },
  {
    title: 'Account and privacy',
    icon: 'lock',
    articles: [
      {
        question: 'What should I do if I cannot sign in?',
        answer:
          'Use Reset password from the sign-in page. If your account is locked or inactive, follow the recovery link shown on that page.',
        roles: ALL_ROLES,
        keywords: 'password locked inactive reset',
      },
      {
        question: 'How can I access, correct, or remove my information?',
        answer:
          'Open Privacy in Settings to review your stored account information and submit a trackable data-rights request.',
        roles: ALL_ROLES,
        keywords: 'data request withdraw consent',
      },
    ],
  },
];

export function helpTopicsFor(role: WRole, query = ''): HelpTopic[] {
  const needle = query.trim().toLocaleLowerCase();
  return HELP_TOPICS.map((topic) => ({
    ...topic,
    articles: topic.articles.filter((article) => {
      if (article.roles && !article.roles.includes(role)) return false;
      if (!needle) return true;
      return `${topic.title} ${article.question} ${article.answer} ${article.keywords ?? ''}`
        .toLocaleLowerCase()
        .includes(needle);
    }),
  })).filter((topic) => topic.articles.length > 0);
}
