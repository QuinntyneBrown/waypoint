import { APIRequestContext, expect } from '@playwright/test';
import { registerUser, signInApi } from './api';

const MIB = 1024 * 1024;

/**
 * A stand-in MP4 for the Testing API: the "ftyp" box marker sits at offset 4 as in a real file.
 * The Testing-only fake transcoder rejects a head containing NOTVIDEO and fails one containing
 * FAILTRANSCODE; neither marker means a normal, probe-able video.
 */
export function fakeMp4(sizeBytes: number, marker = ''): Buffer {
  const bytes = Buffer.alloc(sizeBytes);
  bytes.write('\u0000\u0000\u0000\u0018ftypisom', 0, 'latin1');
  bytes.write(marker, 16, 'ascii');
  return bytes;
}

/**
 * Runs one pass of the video transcode worker, which is idle in the Testing environment. The pass is
 * limited to one lesson so parallel tests cannot advance each other's videos.
 */
export async function runVideoTranscodes(
  request: APIRequestContext,
  lessonId: string,
): Promise<number> {
  const res = await request.post('/api/_test/video-transcodes/run', { params: { lessonId } });
  expect(res.ok(), `transcode run failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  return ((await res.json()) as { processed: number }).processed;
}

/**
 * Uploads a small video to a lesson through the real chunked-upload API as an administrator and,
 * when asked, lets the worker publish it. Returns the new media id.
 */
export async function seedLessonVideo(
  request: APIRequestContext,
  lessonId: string,
  options: { ready: boolean },
): Promise<string> {
  const admin = await registerUser(request, 'ProgramAdministrator', 'lesson-video-seed');
  const { accessToken } = await signInApi(request, admin);
  const headers = { Authorization: `Bearer ${accessToken}` };
  const body = fakeMp4(2 * MIB);
  const base = `/api/lessons/${lessonId}/video-uploads`;

  const start = await request.post(base, {
    headers,
    data: { fileName: 'teaching.mp4', sizeBytes: body.length },
  });
  expect(start.ok(), `start upload failed: ${start.status()} ${await start.text()}`).toBeTruthy();
  const { uploadId } = (await start.json()) as { uploadId: string };

  const chunk = await request.put(`${base}/${uploadId}/chunks/0`, {
    headers: { ...headers, 'Content-Type': 'application/octet-stream' },
    data: body,
  });
  expect(chunk.ok(), `chunk failed: ${chunk.status()} ${await chunk.text()}`).toBeTruthy();

  const complete = await request.post(`${base}/${uploadId}/complete`, { headers });
  expect(
    complete.ok(),
    `complete failed: ${complete.status()} ${await complete.text()}`,
  ).toBeTruthy();
  const { id } = (await complete.json()) as { id: string };

  if (options.ready) await runVideoTranscodes(request, lessonId);
  return id;
}
