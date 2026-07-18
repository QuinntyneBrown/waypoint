import { expect, test } from '@playwright/test';
import { SignInPage, VolunteerAttendancePage, VolunteerTodayPage } from '../../pages';
import { registerUser, seedVolunteerSession, signInApi } from '../../support/api';

// Traces to: L2-VOLUNTEER-001, L2-ENGAGE-002, L2-ENGAGE-003, L2-NFR-004
test('a volunteer records names-only Present/Absent attendance with bulk action and tallies', async ({
  page,
  request,
}) => {
  const volunteer = await registerUser(request, 'Volunteer', 'vol-attendance');
  const seeded = await seedVolunteerSession(request, volunteer.userId);
  const auth = await signInApi(request, volunteer);

  await new SignInPage(page).signIn(volunteer);
  const today = new VolunteerTodayPage(page);
  await today.takeAttendanceLink.click();

  const attendance = new VolunteerAttendancePage(page);
  await expect(page).toHaveURL(VolunteerAttendancePage.urlPattern);
  await expect(attendance.heading).toBeVisible();
  await expect(attendance.rosterRows).toHaveCount(seeded.youth.length);
  for (const youth of seeded.youth) {
    await expect(attendance.rowFor(youth.name)).toBeVisible();
  }
  await expect(page.getByText(/at.risk|progress|safeguard/i)).toHaveCount(0);

  await attendance.allPresentButton.click();
  await attendance.markAbsent(seeded.youth[1].name);
  await expect(attendance.markedTally).toContainText(
    `${seeded.youth.length} of ${seeded.youth.length}`,
  );
  await expect(attendance.presentTally).toContainText(String(seeded.youth.length - 1));
  await expect(attendance.absentTally).toContainText('1');

  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/attendance saved/i);

  const roster = await request.get(`/api/volunteer/sessions/${seeded.sessionId}/roster`, {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
  });
  expect(roster.ok()).toBeTruthy();
  const saved = (await roster.json()) as { youthUserId: string; name: string; mark: string }[];
  expect(saved.filter((entry) => entry.mark === 'Present')).toHaveLength(seeded.youth.length - 1);
  expect(saved.filter((entry) => entry.mark === 'Absent')).toHaveLength(1);
});

test('offline volunteer attendance is owner-bound, queued, and automatically synced once', async ({
  page,
  context,
  request,
}) => {
  const volunteer = await registerUser(request, 'Volunteer', 'vol-offline-attendance');
  const seeded = await seedVolunteerSession(request, volunteer.userId);
  const auth = await signInApi(request, volunteer);

  await new SignInPage(page).signIn(volunteer);
  await new VolunteerTodayPage(page).takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await expect(attendance.rosterRows).toHaveCount(seeded.youth.length);

  await context.setOffline(true);
  await attendance.allPresentButton.click();
  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/saved offline.*1.*waiting to sync/i);

  const queued = await page.evaluate(() =>
    Object.entries(localStorage)
      .filter(([key]) => key.startsWith('wu.attendance.queue.'))
      .map(([, value]) => JSON.parse(value)),
  );
  expect(JSON.stringify(queued)).toContain(volunteer.userId);
  for (const youth of seeded.youth) {
    expect(JSON.stringify(queued)).not.toContain(youth.name);
  }

  await context.setOffline(false);
  await expect(attendance.syncStatus).toContainText(/all attendance changes synced/i);

  const roster = await request.get(`/api/volunteer/sessions/${seeded.sessionId}/roster`, {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
  });
  expect(roster.ok()).toBeTruthy();
  const saved = (await roster.json()) as { mark: string }[];
  expect(saved.every((entry) => entry.mark === 'Present')).toBeTruthy();
});

test('an offline attendance conflict is explicit and can be deterministically reconciled', async ({
  page,
  context,
  request,
}) => {
  const volunteer = await registerUser(request, 'Volunteer', 'vol-conflict');
  const administrator = await registerUser(request, 'ProgramAdministrator', 'attendance-admin');
  const seeded = await seedVolunteerSession(request, volunteer.userId);
  const adminAuth = await signInApi(request, administrator);

  await new SignInPage(page).signIn(volunteer);
  await new VolunteerTodayPage(page).takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await expect(attendance.rosterRows).toHaveCount(seeded.youth.length);

  await context.setOffline(true);
  await attendance.allPresentButton.click();
  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/waiting to sync/i);
  const queuedPayload = await page.evaluate(() =>
    Object.entries(localStorage)
      .filter(([key]) => key.startsWith('wu.attendance.queue.'))
      .map(([, value]) => JSON.parse(value)),
  );
  expect(JSON.stringify(queuedPayload)).toContain('"expectedVersion":"none"');

  const directorSave = await request.post('/api/attendance', {
    headers: {
      Authorization: `Bearer ${adminAuth.accessToken}`,
      'Idempotency-Key': crypto.randomUUID(),
    },
    data: {
      sessionId: seeded.sessionId,
      marks: [
        {
          youthId: seeded.youth[0].youthUserId,
          status: 'Absent',
          note: null,
        },
      ],
    },
  });
  expect(directorSave.ok()).toBeTruthy();

  await context.setOffline(false);
  await expect(attendance.syncStatus).toContainText(/conflicts with a newer server edit/i);
  await page.getByRole('button', { name: /apply my changes/i }).click();
  await expect(attendance.syncStatus).toContainText(/all attendance changes synced/i);

  const volunteerAuth = await signInApi(request, volunteer);
  const roster = await request.get(`/api/volunteer/sessions/${seeded.sessionId}/roster`, {
    headers: { Authorization: `Bearer ${volunteerAuth.accessToken}` },
  });
  const saved = (await roster.json()) as { mark: string }[];
  expect(saved.every((entry) => entry.mark === 'Present')).toBeTruthy();
});

test('unsaved attendance warns before route navigation', async ({ page, request }) => {
  const volunteer = await registerUser(request, 'Volunteer', 'vol-unsaved');
  const seeded = await seedVolunteerSession(request, volunteer.userId);

  await new SignInPage(page).signIn(volunteer);
  await new VolunteerTodayPage(page).takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await attendance.markAbsent(seeded.youth[0].name);

  page.once('dialog', async (dialog) => {
    expect(dialog.message()).toContain('unsaved attendance changes');
    await dialog.dismiss();
  });
  await page.getByRole('link', { name: /back to today/i }).click();
  await expect(page).toHaveURL(VolunteerAttendancePage.urlPattern);
});
