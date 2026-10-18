import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url';
import {
  AssignmentDetail,
  BadgeDefinition,
  IdResult,
  ProgressionStage,
  PublishStatus,
  QuizDetail,
  SaveAssignment,
  SaveBadge,
  SaveLesson,
  SaveModule,
  SaveQuestion,
  SaveQuiz,
  SaveTrack,
  TrackSummary,
  TrackWorkspace,
  VideoUploadSession,
  VideoUploadStatus,
} from '../models/curriculum-authoring/curriculum-authoring';

@Injectable({ providedIn: 'root' })
export class CurriculumAuthoringService {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE_URL);

  tracks(): Observable<TrackSummary[]> {
    return this.http.get<TrackSummary[]>(this.url('/api/tracks'));
  }
  track(id: string): Observable<TrackWorkspace> {
    return this.http.get<TrackWorkspace>(this.url('/api/tracks/' + id));
  }
  createTrack(value: SaveTrack): Observable<IdResult> {
    return this.http.post<IdResult>(this.url('/api/tracks'), value);
  }
  publishTrack(id: string, status: PublishStatus): Observable<void> {
    return this.http.post<void>(this.url('/api/tracks/' + id + '/publish'), {
      status: this.publishStatusValue(status),
    });
  }

  createModule(value: SaveModule): Observable<IdResult> {
    return this.http.post<IdResult>(this.url('/api/modules'), value);
  }
  updateModule(id: string, value: SaveModule): Observable<void> {
    return this.http.put<void>(this.url('/api/modules/' + id), value);
  }
  reorderModules(trackId: string, orderedModuleIds: string[]): Observable<void> {
    return this.http.post<void>(this.url('/api/modules/reorder'), { trackId, orderedModuleIds });
  }
  publishModule(id: string, status: PublishStatus): Observable<void> {
    return this.http.post<void>(this.url('/api/modules/' + id + '/publish'), {
      status: this.publishStatusValue(status),
    });
  }

  createLesson(value: SaveLesson): Observable<IdResult> {
    return this.http.post<IdResult>(this.url('/api/lessons'), value);
  }
  updateLesson(id: string, value: SaveLesson): Observable<void> {
    return this.http.put<void>(this.url('/api/lessons/' + id), value);
  }
  reorderLessons(moduleId: string, orderedLessonIds: string[]): Observable<void> {
    return this.http.post<void>(this.url('/api/lessons/reorder'), {
      moduleId,
      orderedLessonIds,
    });
  }
  publishLesson(id: string, status: PublishStatus): Observable<void> {
    return this.http.post<void>(this.url('/api/lessons/' + id + '/publish'), {
      status: this.publishStatusValue(status),
    });
  }
  archiveLesson(id: string): Observable<void> {
    return this.http.delete<void>(this.url('/api/lessons/' + id));
  }
  uploadLessonMedia(lessonId: string, file: File): Observable<IdResult> {
    const body = new FormData();
    body.append('file', file, file.name);
    return this.http.post<IdResult>(this.url('/api/lessons/' + lessonId + '/media'), body);
  }
  removeLessonMedia(lessonId: string, mediaId: string): Observable<void> {
    return this.http.delete<void>(this.url('/api/lessons/' + lessonId + '/media/' + mediaId));
  }
  startVideoUpload(
    lessonId: string,
    fileName: string,
    sizeBytes: number,
  ): Observable<VideoUploadSession> {
    return this.http.post<VideoUploadSession>(
      this.url('/api/lessons/' + lessonId + '/video-uploads'),
      {
        fileName,
        sizeBytes,
      },
    );
  }
  videoUploadStatus(lessonId: string, uploadId: string): Observable<VideoUploadStatus> {
    return this.http.get<VideoUploadStatus>(this.videoUploadUrl(lessonId, uploadId));
  }
  /** Sends one chunk as a raw body; resending the same index is harmless. */
  uploadVideoChunk(
    lessonId: string,
    uploadId: string,
    index: number,
    chunk: Blob,
  ): Observable<void> {
    return this.http.put<void>(
      this.videoUploadUrl(lessonId, uploadId) + '/chunks/' + index,
      chunk,
      {
        headers: { 'Content-Type': 'application/octet-stream' },
      },
    );
  }
  completeVideoUpload(lessonId: string, uploadId: string): Observable<IdResult> {
    return this.http.post<IdResult>(this.videoUploadUrl(lessonId, uploadId) + '/complete', {});
  }
  retryVideoTranscode(lessonId: string, mediaId: string): Observable<void> {
    return this.http.post<void>(
      this.url('/api/lessons/' + lessonId + '/media/' + mediaId + '/retry-transcode'),
      {},
    );
  }
  private videoUploadUrl(lessonId: string, uploadId: string): string {
    return this.url('/api/lessons/' + lessonId + '/video-uploads/' + uploadId);
  }

  createQuiz(value: SaveQuiz): Observable<IdResult> {
    return this.http.post<IdResult>(this.url('/api/quizzes'), value);
  }
  quiz(id: string): Observable<QuizDetail> {
    return this.http.get<QuizDetail>(this.url('/api/quizzes/' + id));
  }
  updateQuiz(id: string, value: SaveQuiz): Observable<void> {
    return this.http.put<void>(this.url('/api/quizzes/' + id), value);
  }
  addQuestion(id: string, value: SaveQuestion): Observable<IdResult> {
    const questionType = {
      MultipleChoice: 1,
      TrueFalse: 2,
      MultipleAnswer: 3,
      ShortAnswer: 4,
    }[value.type];
    return this.http.post<IdResult>(this.url('/api/quizzes/' + id + '/questions'), {
      ...value,
      type: questionType,
    });
  }
  reorderQuestions(id: string, orderedQuestionIds: string[]): Observable<void> {
    return this.http.post<void>(this.url('/api/quizzes/' + id + '/questions/reorder'), {
      orderedQuestionIds,
    });
  }

  createAssignment(value: SaveAssignment): Observable<IdResult> {
    return this.http.post<IdResult>(this.url('/api/assignments'), value);
  }
  assignment(id: string): Observable<AssignmentDetail> {
    return this.http.get<AssignmentDetail>(this.url('/api/assignments/' + id));
  }
  updateAssignment(id: string, value: SaveAssignment): Observable<void> {
    return this.http.put<void>(this.url('/api/assignments/' + id), value);
  }
  publishAssignment(id: string, status: PublishStatus): Observable<void> {
    return this.http.post<void>(this.url('/api/assignments/' + id + '/publish'), {
      status: this.publishStatusValue(status),
    });
  }

  badges(): Observable<BadgeDefinition[]> {
    return this.http.get<BadgeDefinition[]>(this.url('/api/badges'));
  }
  createBadge(value: SaveBadge): Observable<IdResult> {
    return this.http.post<IdResult>(this.url('/api/badges'), {
      ...value,
      criteriaKind: value.criteriaKind === 'Auto' ? 1 : 2,
    });
  }
  archiveBadge(id: string): Observable<void> {
    return this.http.delete<void>(this.url('/api/badges/' + id));
  }
  awardBadge(id: string, youthUserId: string): Observable<void> {
    return this.http.post<void>(this.url('/api/badges/' + id + '/award'), { youthUserId });
  }

  progression(): Observable<ProgressionStage[]> {
    return this.http.get<ProgressionStage[]>(this.url('/api/progression'));
  }
  createStage(value: {
    name: string;
    description: string | null;
    order: number;
  }): Observable<IdResult> {
    return this.http.post<IdResult>(this.url('/api/progression/stages'), value);
  }
  updateStage(id: string, value: { name: string; description: string | null }): Observable<void> {
    return this.http.put<void>(this.url('/api/progression/stages/' + id), value);
  }
  reorderStages(orderedStageIds: string[]): Observable<void> {
    return this.http.post<void>(this.url('/api/progression/stages/reorder'), {
      orderedStageIds,
    });
  }
  addCompetency(stageId: string, name: string): Observable<IdResult> {
    return this.http.post<IdResult>(
      this.url('/api/progression/stages/' + stageId + '/competencies'),
      { name },
    );
  }
  removeCompetency(id: string): Observable<void> {
    return this.http.delete<void>(this.url('/api/progression/competencies/' + id));
  }

  private url(path: string): string {
    return this.base + path;
  }

  private publishStatusValue(status: PublishStatus): number {
    return status === 'Published' ? 3 : 1;
  }
}
