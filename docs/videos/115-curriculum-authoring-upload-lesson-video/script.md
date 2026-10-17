# 115 · Uploading a lesson video

## Welcome

Authors record their own lessons, and a recording can run two minutes or three hours. This video follows one recording from the lesson editor to a set of files that any browser can stream, using only Waypoint's own server and one open source tool. It explains the design for this feature. The screens are the existing mocks and the design diagrams, because the feature is not built yet.

[pause 2s]

## Why a new path

Today the lesson editor has an attach media button. It accepts an MP4 file of up to twenty five megabytes and stores the whole file inside the database. That is fine for a handout, and it cannot work for a long recording. A three hour video can be many gigabytes, and the server would hold all of it in memory to receive it and again to send it. So video gets its own path, and documents keep the old one.

[pause 2s]

## Uploading in chunks

The editor cuts the file into chunks of eight MiB and sends up to three at a time. First, it asks the server to start an upload session. The server checks that the lesson exists, that the file is under the configured size limit, and that the disk has room for about one and a half times the file.

Each chunk is written straight to disk at its own position in the file, so sending a chunk twice does no harm. If the page reloads or the network drops, the editor asks which chunks have arrived and sends only the missing ones. That is how a three hour upload survives a bad afternoon. Because chunks go through the normal Angular HTTP client, an expired access token is refreshed in the middle of a long upload.

[pause 2s]

## Checking the file

When every chunk is in, the editor asks the server to complete the upload. The server checks that the length is exact, checks that the first bytes look like an MP4 file, and then runs ffprobe once, right away. A file that is not a real video is rejected in seconds, not hours later.

If the probe succeeds, the server moves the file into the sources folder, creates the media record with the status queued, adds a transcode job, and writes an audit entry. All of that is saved in one unit of work.

[pause 2s]

## The transcode worker

A background worker inside the API process picks up the job. It claims the job with a lease of five minutes, and it keeps extending that lease while the work goes on, so a crashed server is noticed within five minutes.

The worker runs ffmpeg once. The source is decoded one time and split into up to four renditions: 1080p, 720p, 480p, and 360p, never larger than the source. Video is H.264, audio is stereo AAC, and key frames fall every two seconds so players can switch renditions cleanly.

The output is cut into six second segments in the fragmented MP4 format, with a master playlist that ties the renditions together, and a poster image. Everything is written to a work folder. Only when ffmpeg succeeds is that folder moved, in one step, to the published folder. A viewer never sees half a video.

On a machine with eight modern cores, a three hour recording should finish in roughly forty to sixty minutes. The real figure for the deployment host is still to be supplied.

[pause 2s]

## When something fails

A job has a time limit of thirty minutes or three times the length of the video, whichever is longer. If ffmpeg fails or runs out of time, the worker stops it, deletes the work folder, and records the reason. After two attempts the media is marked failed, and the author can press retry.

Deleting a video moves its files into a trash folder after the database change is committed. An hourly sweep clears the trash, stale partial uploads, and orphaned folders.

[pause 2s]

## What the author sees

The author sees four states: queued, processing with a percentage, ready with the duration, or failed with a retry button. The editor checks every five seconds while a video is queued or processing.

The current mock shows the media list with a video row and an attach button. It does not show the progress or failure states, so those still need to be drawn before the feature is built.

[pause 2s]

## Things to remember

Upload in resumable chunks. Reject bad files at completion. Transcode once into a ladder of renditions. Publish in one atomic move. The next video shows how a youth streams the result.
