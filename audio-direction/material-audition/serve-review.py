#!/usr/bin/env python3
"""Serve this local audition with byte ranges so native audio can seek without downloading complete mixes."""
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parent


class ReviewHandler(SimpleHTTPRequestHandler):
    """Expose only files inside the external review package, bound to the local loopback interface."""

    def __init__(self, *args, **kwargs):
        """Keep document resolution independent of the shell working directory."""
        super().__init__(*args, directory=str(ROOT), **kwargs)

    def send_head(self):
        """Return a correct single byte range for the browser's native media request."""
        path = Path(self.translate_path(self.path)).resolve()
        if path.is_dir():
            path = path / 'index.html'
        if not path.is_relative_to(ROOT) or not path.is_file():
            self.send_error(404)
            return None
        size = path.stat().st_size
        begin, end = 0, size - 1
        requested = self.headers.get('Range')
        if requested:
            match = re.fullmatch(r'bytes=(\d*)-(\d*)', requested)
            if not match or not any(match.groups()):
                self.send_error(416)
                return None
            left, right = match.groups()
            if left:
                begin = int(left)
                end = min(int(right), end) if right else end
            else:
                begin = max(0, size - int(right))
            if begin > end or begin >= size:
                self.send_response(416)
                self.send_header('Content-Range', f'bytes */{size}')
                self.send_header('Content-Length', '0')
                self.end_headers()
                return None
        source = path.open('rb')
        source.seek(begin)
        self.byte_limit = end - begin + 1
        self.send_response(206 if requested else 200)
        self.send_header('Content-Type', self.guess_type(str(path)))
        self.send_header('Accept-Ranges', 'bytes')
        self.send_header('Content-Length', str(self.byte_limit))
        self.send_header('Last-Modified', self.date_time_string(path.stat().st_mtime))
        if requested:
            self.send_header('Content-Range', f'bytes {begin}-{end}/{size}')
        self.end_headers()
        return source

    def copyfile(self, source, output):
        """Stream only the requested bytes; a browser may cancel metadata preloading normally."""
        remaining = self.byte_limit
        try:
            while remaining:
                chunk = source.read(min(65536, remaining))
                if not chunk:
                    break
                output.write(chunk)
                remaining -= len(chunk)
        except (BrokenPipeError, ConnectionResetError):
            pass


if __name__ == '__main__':
    ThreadingHTTPServer(('127.0.0.1', 8777), ReviewHandler).serve_forever()
