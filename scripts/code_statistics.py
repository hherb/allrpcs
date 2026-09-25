#!/usr/bin/env python3
"""Report code, documentation/comment, blank-line, and file-size statistics."""

from __future__ import annotations

import argparse
import subprocess
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class Syntax:
    """Describes the comment syntax and display name for one source type."""

    name: str
    line_comment: str | None = None
    block_comment: tuple[str, str] | None = None


@dataclass(frozen=True)
class Statistics:
    """Contains the measurements collected for one file or group of files."""

    files: int = 0
    bytes: int = 0
    lines: int = 0
    code: int = 0
    documentation: int = 0
    blank: int = 0

    def __add__(self, other: Statistics) -> Statistics:
        """Combines two independent measurements."""

        return Statistics(
            files=self.files + other.files,
            bytes=self.bytes + other.bytes,
            lines=self.lines + other.lines,
            code=self.code + other.code,
            documentation=self.documentation + other.documentation,
            blank=self.blank + other.blank,
        )


SYNTAXES = {
    ".cs": Syntax("C#", "//", ("/*", "*/")),
    ".py": Syntax("Python", "#"),
    ".sh": Syntax("Shell", "#"),
    ".csproj": Syntax("MSBuild", block_comment=("<!--", "-->")),
    ".props": Syntax("MSBuild", block_comment=("<!--", "-->")),
    ".sln": Syntax("Solution", "#"),
    ".godot": Syntax("Godot config", ";"),
    ".tscn": Syntax("Godot scene", ";"),
    ".cfg": Syntax("Config", ";"),
    ".yml": Syntax("YAML", "#"),
    ".yaml": Syntax("YAML", "#"),
    ".json": Syntax("JSON"),
}


def parse_arguments() -> argparse.Namespace:
    """Parses command-line arguments."""

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "root",
        nargs="?",
        type=Path,
        default=Path(__file__).resolve().parent.parent,
        help="project root (default: the parent of this script's directory)",
    )
    parser.add_argument(
        "--summary-only",
        action="store_true",
        help="omit the per-file table",
    )
    return parser.parse_args()


def source_paths(root: Path) -> list[Path]:
    """Returns supported tracked and non-ignored source files below root."""

    command = [
        "git",
        "-C",
        str(root),
        "ls-files",
        "-z",
        "--cached",
        "--others",
        "--exclude-standard",
    ]
    try:
        result = subprocess.run(command, check=True, capture_output=True)
        relative_paths = result.stdout.decode("utf-8", errors="surrogateescape").split("\0")
        paths = (root / relative for relative in relative_paths if relative)
    except (FileNotFoundError, subprocess.CalledProcessError):
        paths = root.rglob("*")

    return sorted(
        (path for path in paths if path.is_file() and path.suffix.lower() in SYNTAXES),
        key=lambda path: path.relative_to(root).as_posix(),
    )


def line_kind(line: str, syntax: Syntax, in_block: bool) -> tuple[str, bool]:
    """Classifies one physical line and returns its resulting block-comment state."""

    if not line.strip():
        return "blank", in_block

    has_code = line.startswith("#!")
    has_documentation = False
    quote: str | None = None
    escaped = False
    index = 0

    while index < len(line):
        if in_block:
            has_documentation = True
            assert syntax.block_comment is not None
            end_index = line.find(syntax.block_comment[1], index)
            if end_index < 0:
                break
            index = end_index + len(syntax.block_comment[1])
            in_block = False
            continue

        character = line[index]
        if quote is not None:
            has_code = True
            if escaped:
                escaped = False
            elif character == "\\":
                escaped = True
            elif character == quote:
                quote = None
            index += 1
            continue

        if character in {'"', "'"}:
            has_code = True
            quote = character
            index += 1
            continue

        if syntax.line_comment and line.startswith(syntax.line_comment, index):
            has_documentation = True
            break

        if syntax.block_comment and line.startswith(syntax.block_comment[0], index):
            has_documentation = True
            in_block = True
            index += len(syntax.block_comment[0])
            continue

        if not character.isspace():
            has_code = True
        index += 1

    return ("code" if has_code else "documentation" if has_documentation else "blank"), in_block


def measure(path: Path, syntax: Syntax) -> Statistics:
    """Measures one UTF-8-compatible source file."""

    content = path.read_bytes()
    lines = content.decode("utf-8", errors="replace").splitlines()
    counts = defaultdict(int)
    in_block = False
    for line in lines:
        kind, in_block = line_kind(line, syntax, in_block)
        counts[kind] += 1

    return Statistics(
        files=1,
        bytes=len(content),
        lines=len(lines),
        code=counts["code"],
        documentation=counts["documentation"],
        blank=counts["blank"],
    )


def print_table(title: str, rows: list[tuple[str, Statistics]]) -> None:
    """Prints one aligned statistics table."""

    name_width = max(len("File/type"), *(len(name) for name, _ in rows))
    print(title)
    print(
        f"{'File/type':<{name_width}}  {'Files':>5}  {'kB':>10}  {'Lines':>8}  "
        f"{'Code':>8}  {'Docs':>8}  {'Blank':>8}"
    )
    print("-" * (name_width + 65))
    for name, stats in rows:
        print(
            f"{name:<{name_width}}  {stats.files:>5}  {stats.bytes / 1000:>10.2f}  "
            f"{stats.lines:>8}  {stats.code:>8}  {stats.documentation:>8}  {stats.blank:>8}"
        )
    print()


def main() -> None:
    """Collects and prints project source statistics."""

    arguments = parse_arguments()
    root = arguments.root.resolve()
    if not root.is_dir():
        raise SystemExit(f"Project root is not a directory: {root}")

    file_rows: list[tuple[str, Statistics]] = []
    language_totals: dict[str, Statistics] = defaultdict(Statistics)
    total = Statistics()

    for path in source_paths(root):
        syntax = SYNTAXES[path.suffix.lower()]
        statistics = measure(path, syntax)
        file_rows.append((path.relative_to(root).as_posix(), statistics))
        language_totals[syntax.name] += statistics
        total += statistics

    if not file_rows:
        raise SystemExit(f"No supported source files found below {root}")

    if not arguments.summary_only:
        print_table("Files", file_rows)
    summary_rows = sorted(language_totals.items())
    summary_rows.append(("TOTAL", total))
    print_table("Summary", summary_rows)
    print("kB uses 1,000 bytes. Docs are comment-only lines; mixed code/comment lines count as code.")


if __name__ == "__main__":
    main()
