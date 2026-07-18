import { expect, test } from '@playwright/test';
import { MentorYouthDetailPage, SignInPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-MENTOR-002, L2-AUTH-010
test('mentor sees the complete privacy-scoped record for an assigned youth', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/youth/${seeded.youthId}`);

  const detail = new MentorYouthDetailPage(page);
  await expect(detail.heading).toHaveText(seeded.youthName);
  await expect(detail.atRiskBanner).toContainText('Follow up after two missed sessions.');
  await expect(detail.atRiskBanner).toContainText(seeded.mentorName);

  await expect(detail.panel('Profile')).toContainText(seeded.youthEmail);
  await expect(detail.panel('Profile')).toContainText(seeded.guardianName);
  await expect(detail.panel('Profile')).toContainText(seeded.guardianEmail);

  await detail.tab('Progress').click();
  await expect(detail.panel('Progress')).toContainText('1 of 2 lessons complete');
  await detail.tab('Attendance').click();
  await expect(detail.panel('Attendance')).toContainText(seeded.progressPresentSessionTitle);
  await detail.tab('Check-ins').click();
  await expect(detail.panel('Check-ins')).toContainText('1:1');
  await detail.tab('Submissions').click();
  await expect(detail.panel('Submissions')).toContainText(seeded.mentorReviewTitle);
  await detail.tab('Competencies').click();
  await expect(detail.panel('Competencies')).toContainText(seeded.progressCompetencyName);

  await expect(page.getByText('PRIVATE PRAYER DETAIL', { exact: false })).toHaveCount(0);
});
