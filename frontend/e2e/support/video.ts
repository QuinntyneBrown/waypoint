import { APIRequestContext, expect } from '@playwright/test';

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

/** Runs one pass of the video transcode worker, which is idle in the Testing environment. */
export async function runVideoTranscodes(request: APIRequestContext): Promise<number> {
  const res = await request.post('/api/_test/video-transcodes/run');
  expect(res.ok(), `transcode run failed: ${res.status()} ${await res.text()}`).toBeTruthy();
  return ((await res.json()) as { processed: number }).processed;
}
