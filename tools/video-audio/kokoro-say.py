#!/usr/bin/env python3
"""Speaks one paragraph with the offline Kokoro voice, for generate-audio.mjs.

    python kokoro-say.py <kokoro-dir> <voice> <speed> <text-file> <out.wav>
    python kokoro-say.py --batch <kokoro-dir> <voice> <speed> <jobs.json>

Batch mode loads the model once and speaks every {"text_file", "out"} job in
jobs.json, which is much faster than one process per paragraph.

<kokoro-dir> holds model.onnx and voices/<voice>.bin, both downloaded from
https://huggingface.co/onnx-community/Kokoro-82M-v1.0-ONNX (Apache-2.0). The
voices are packed once into voices.npz, which kokoro-onnx reads.
"""
import json
import sys
from pathlib import Path

import numpy as np
import soundfile as sf
from kokoro_onnx import Kokoro


def pack_voices(voices_dir: Path, packed: Path) -> None:
    """Packs each voices/<name>.bin (510 style rows of 256 floats) into one npz."""
    styles = {p.stem: np.fromfile(p, dtype=np.float32).reshape(-1, 1, 256) for p in sorted(voices_dir.glob("*.bin"))}
    if not styles:
        sys.exit(f"no voices found in {voices_dir}")
    np.savez(packed, **styles)


def main() -> None:
    batch = len(sys.argv) == 6 and sys.argv[1] == "--batch"
    if batch:
        folder, voice, speed, jobs_file = sys.argv[2:]
    elif len(sys.argv) == 6:
        folder, voice, speed, text_file, out = sys.argv[1:]
    else:
        sys.exit(__doc__)
    folder = Path(folder)
    model = folder / "model.onnx"
    if not model.exists():
        sys.exit(f"Kokoro model not found: {model} (see the header of generate-audio.mjs)")
    voices_dir = folder / "voices"
    packed = folder / "voices.npz"
    newest_voice = max((p.stat().st_mtime for p in voices_dir.glob("*.bin")), default=0)
    if not packed.exists() or packed.stat().st_mtime < newest_voice:
        pack_voices(voices_dir, packed)

    kokoro = Kokoro(str(model), str(packed))
    jobs = json.loads(Path(jobs_file).read_text(encoding="utf-8")) if batch else [{"text_file": text_file, "out": out}]
    for job in jobs:
        text = Path(job["text_file"]).read_text(encoding="utf-8")
        samples, rate = kokoro.create(text, voice=voice, speed=float(speed), lang="en-us")
        sf.write(job["out"], samples, rate)


if __name__ == "__main__":
    main()
