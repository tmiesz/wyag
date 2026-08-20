# Wyag

Wyag ("Write Yourself A Git") is a reimplementation of Git written in C#/.NET.

It is based on ["Write yourself a Git!"](https://wyag.thb.lt/) by Thibault Polge.

## Getting started

```bash
dotnet run --project src/Wyag.Cli/ -- init
```

This sets up a .git directory - fully readable by real Git.
From there you may:

```bash
echo "hello world" > helloworld.txt
dotnet run --project src/Wyag.Cli/ -- add helloworld.txt
dotnet run --project src/Wyag.Cli/ -- commit -m "hello world"
dotnet run --project src/Wyag.Cli/ -- log
```

## Commands

`add`, `cat-file`, `check-ignore`, `checkout`, `commit`, `hash-object`, `init`, `log`, `ls-files`, `ls-tree`, `rev-parse`, `rm`, `show-ref`, `status`, `tag`

## Project layout

```
src/
├── Wyag.Cli/         # entry point, wires up commands via DI
└── Wyag.Core/        # everything else
    ├── Commands/     # one class per CLI command
    ├── Objects/      # blobs, trees, commits, tags, the object store
    ├── Refs/         # branches, tags, ref resolution
    ├── Index/        # the staging area
    ├── Ignore/       # .gitignore handling
    ├── Status/       # status logic
    ├── Compression/  # zlib (de)compression for object storage
    └── IO/           # filesystem abstraction
```
