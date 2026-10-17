import { CurriculumAuthoringService, VideoUploadSession, VideoUploadStatus } from '@waypoint/api';
import { catchError, concat, from, map, mergeMap, Observable, of, switchMap, tap } from 'rxjs';

export type VideoUploadEvent =
  { kind: 'progress'; sentBytes: number; totalBytes: number } | { kind: 'complete' };

/** Chunks in flight at once; the server accepts them in any order. */
const PARALLEL_CHUNKS = 3;

/**
 * Uploads a lesson video in chunks and completes it. A session remembered for the same lesson, file
 * name and size is resumed, so only the chunks the server has not received are sent again.
 */
export function uploadLessonVideo(
  api: CurriculumAuthoringService,
  lessonId: string,
  file: File,
): Observable<VideoUploadEvent> {
  return resolveSession(api, lessonId, file).pipe(
    switchMap(({ session, received }) => {
      const chunkSize = session.chunkSizeBytes;
      const sizeOf = (index: number) => Math.min(chunkSize, file.size - index * chunkSize);
      let sentBytes = received.reduce((total, index) => total + sizeOf(index), 0);
      const missing = Array.from({ length: session.chunkCount }, (_, index) => index).filter(
        (index) => !received.includes(index),
      );
      const start: VideoUploadEvent = { kind: 'progress', sentBytes, totalBytes: file.size };
      const chunks = from(missing).pipe(
        mergeMap(
          (index) =>
            api
              .uploadVideoChunk(
                lessonId,
                session.uploadId,
                index,
                file.slice(index * chunkSize, index * chunkSize + sizeOf(index)),
              )
              .pipe(
                map((): VideoUploadEvent => {
                  sentBytes += sizeOf(index);
                  return { kind: 'progress', sentBytes, totalBytes: file.size };
                }),
              ),
          PARALLEL_CHUNKS,
        ),
      );
      const complete = api.completeVideoUpload(lessonId, session.uploadId).pipe(
        tap(() => forgetSession(lessonId, file)),
        map((): VideoUploadEvent => ({ kind: 'complete' })),
      );
      return concat(of(start), chunks, complete);
    }),
  );
}

/** Drops a remembered session, for example after the server rejected the file. */
export function forgetSession(lessonId: string, file: File): void {
  try {
    sessionStorage.removeItem(sessionKey(lessonId, file));
  } catch {
    // Storage can be unavailable; the upload then simply cannot resume after a reload.
  }
}

function resolveSession(
  api: CurriculumAuthoringService,
  lessonId: string,
  file: File,
): Observable<{ session: VideoUploadSession; received: number[] }> {
  const startNew = () =>
    api.startVideoUpload(lessonId, file.name, file.size).pipe(
      tap((session) => rememberSession(lessonId, file, session.uploadId)),
      map((session) => ({ session, received: [] as number[] })),
    );
  const remembered = recallSession(lessonId, file);
  if (!remembered) return startNew();
  return api.videoUploadStatus(lessonId, remembered).pipe(
    map((status: VideoUploadStatus) => ({ session: status, received: status.receivedChunks })),
    catchError(() => startNew()),
  );
}

function sessionKey(lessonId: string, file: File): string {
  return `waypoint.videoUpload.${lessonId}.${file.size}.${file.name}`;
}

function rememberSession(lessonId: string, file: File, uploadId: string): void {
  try {
    sessionStorage.setItem(sessionKey(lessonId, file), uploadId);
  } catch {
    // See forgetSession.
  }
}

function recallSession(lessonId: string, file: File): string | null {
  try {
    return sessionStorage.getItem(sessionKey(lessonId, file));
  } catch {
    return null;
  }
}
