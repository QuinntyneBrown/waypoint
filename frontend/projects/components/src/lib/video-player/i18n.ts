import { InjectionToken } from '@angular/core';

/** Every user-visible or assistive-technology string the video player owns. */
export interface VideoPlayerStrings {
  regionLabel: (title: string | null) => string;
  connectingStatus: string;
  connecting: (title: string) => string;
  liveAnnounced: string;
  buffering: string;
  reconnecting: (attempt: number, max: number) => string;
  connectionLost: string;
  reconnected: string;
  waitingForSource: string;
  streamEnded: string;
  /** Words for a live duration; endedAfter and liveFor receive its result. */
  duration: (totalSeconds: number) => string;
  liveFor: (duration: string) => string;
  endedAfter: (duration: string) => string;
  controlsLabel: string;
  play: string;
  pause: string;
  paused: string;
  mute: string;
  unmute: string;
  volumeLabel: string;
  volumeValue: (volume: number) => string;
  muted: string;
  unmuted: (volume: number) => string;
  unmuteChip: string;
  dismiss: string;
  captions: string;
  captionsOn: string;
  captionsOff: string;
  fullscreen: string;
  exitFullscreen: string;
  fullscreenOn: string;
  fullscreenOff: string;
  backLive: string;
  live: string;
  liveBadge: string;
  goToLive: (seconds: number) => string;
  behindLive: (seconds: number) => string;
  retry: string;
  errorUnsupported: (mimeType: string) => string;
  errorUnauthorized: string;
  errorNotFound: string;
  errorConnection: string;
  errorSource: string;
  errorDecode: string;
  errorStalled: string;
  errorConfiguration: string;
  /** On-demand playback. */
  playing: string;
  seekLabel: string;
  /** Spoken position for the seek bar, e.g. "23 minutes 41 seconds of 1 hour 12 minutes". */
  seekValue: (elapsedSeconds: number, totalSeconds: number) => string;
  quality: (height: number) => string;
  renewingLink: string;
}

/** English defaults; consumers can replace any subset through VIDEO_PLAYER_I18N. */
export const DEFAULT_VIDEO_PLAYER_STRINGS: VideoPlayerStrings = {
  regionLabel: (title) => (title ? `Video player: ${title}` : 'Video player'),
  connectingStatus: 'Connecting…',
  connecting: (title) => `Connecting to ${title}.`,
  liveAnnounced: 'Live.',
  buffering: 'Buffering.',
  reconnecting: (attempt, max) => `Reconnecting… attempt ${attempt} of ${max}`,
  connectionLost: 'Connection lost. Reconnecting.',
  reconnected: 'Reconnected. Live.',
  waitingForSource: 'Waiting for the source…',
  streamEnded: 'Stream ended',
  duration: (totalSeconds) => formatDuration(totalSeconds),
  liveFor: (duration) => `Live for ${duration}`,
  endedAfter: (duration) => `Stream ended. It was live for ${duration}.`,
  controlsLabel: 'Player controls',
  play: 'Play',
  pause: 'Pause',
  paused: 'Paused.',
  mute: 'Mute',
  unmute: 'Unmute',
  volumeLabel: 'Volume',
  volumeValue: (volume) => `${volume}%`,
  muted: 'Muted.',
  unmuted: (volume) => `Unmuted, volume ${volume}%.`,
  unmuteChip: 'Unmute',
  dismiss: 'Dismiss',
  captions: 'Captions',
  captionsOn: 'Captions on.',
  captionsOff: 'Captions off.',
  fullscreen: 'Fullscreen',
  exitFullscreen: 'Exit fullscreen',
  fullscreenOn: 'Fullscreen.',
  fullscreenOff: 'Exited fullscreen.',
  backLive: 'Back live.',
  live: 'Live',
  liveBadge: 'LIVE',
  goToLive: (seconds) => `Go to live, ${seconds} seconds behind`,
  behindLive: (seconds) => `${seconds} seconds behind live. Press Live to catch up.`,
  retry: 'Retry',
  errorUnsupported: (mimeType) => `This browser can't play this stream (${mimeType}).`,
  errorUnauthorized: "You don't have access to this stream. Sign in again or ask the organiser.",
  errorNotFound: "This stream doesn't exist or is no longer available.",
  errorConnection: "The connection was lost and couldn't be restored.",
  errorSource: 'The video source stopped unexpectedly.',
  errorDecode: "The video couldn't be decoded.",
  errorStalled: 'The source stopped sending video.',
  errorConfiguration: 'Set either src or streamId on the video player, not both.',
  playing: 'Playing.',
  seekLabel: 'Seek',
  seekValue: (elapsed, total) => `${spokenTime(elapsed)} of ${spokenTime(total)}`,
  quality: (height) => `Quality ${height}p`,
  renewingLink: 'Renewing your video link',
};

/** Partial string overrides merged over the English defaults. */
export type VideoPlayerI18n = Partial<VideoPlayerStrings>;

/** Provides partial string overrides for every video player below the provider. */
export const VIDEO_PLAYER_I18N = new InjectionToken<VideoPlayerI18n>('VIDEO_PLAYER_I18N', {
  providedIn: 'root',
  factory: () => ({}),
});

/** Formats a live duration in words: "less than a minute", "1 minute", "2 hours 5 minutes". */
export function formatDuration(totalSeconds: number): string {
  const minutes = Math.floor(totalSeconds / 60);
  if (minutes < 1) return 'less than a minute';
  const plural = (count: number, unit: string) => `${count} ${unit}${count === 1 ? '' : 's'}`;
  const hours = Math.floor(minutes / 60);
  return hours
    ? `${plural(hours, 'hour')} ${plural(minutes % 60, 'minute')}`
    : plural(minutes, 'minute');
}

/** Spoken clock time: "1 hour 12 minutes 8 seconds"; zero parts are left out. */
export function spokenTime(totalSeconds: number): string {
  const seconds = Math.max(0, Math.floor(totalSeconds));
  const plural = (count: number, unit: string) => `${count} ${unit}${count === 1 ? '' : 's'}`;
  const parts = [
    [Math.floor(seconds / 3600), 'hour'],
    [Math.floor((seconds % 3600) / 60), 'minute'],
    [seconds % 60, 'second'],
  ] as const;
  const spoken = parts.filter(([count]) => count > 0).map(([count, unit]) => plural(count, unit));
  return spoken.length ? spoken.join(' ') : plural(0, 'second');
}
