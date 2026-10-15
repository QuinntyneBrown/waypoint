import { helpTopicsFor } from './help-content';

describe('helpTopicsFor', () => {
  it('shows role-specific invitation guidance only to staff who can invite', () => {
    const admin = helpTopicsFor('admin').flatMap((topic) => topic.articles);
    const youth = helpTopicsFor('youth').flatMap((topic) => topic.articles);
    expect(admin.some((article) => article.question.includes('invite'))).toBe(true);
    expect(youth.some((article) => article.question.includes('invite'))).toBe(false);
  });

  it('searches questions, answers, and keywords without keeping empty topics', () => {
    const results = helpTopicsFor('volunteer', 'offline');
    expect(results).toHaveLength(1);
    expect(results[0].articles[0].question).toContain('attendance');
  });
});
