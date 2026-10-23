This page describes every option on the **Settings** page. The defaults work well for most people. Every option except those under *Behaviour* corresponds to an InstaloaderSharp command-line option, which is named in brackets in the program and below.

## Contents

- [File naming](#file-naming)
- [Caption text files](#caption-text-files)
- [Resuming interrupted downloads](#resuming-interrupted-downloads)
- [Connection](#connection)
- [Behaviour](#behaviour)
- [Command line](#command-line)
- [About](#about)

## File naming

| Option | Default | Effect | Command line |
|---|---|---|---|
| **Folder name pattern** | `{target}` | Name of the folder posts are stored in; may contain subfolders and an absolute path | `--dirname-pattern` |
| **File name pattern** | `{date_utc}_UTC` | File names of posts and story items, relative to the folder | `--filename-pattern` |
| **Title pattern** | `{date_utc}_UTC_{typename}` | File names of profile pictures, hashtag pictures and highlight covers | `--title-pattern` |
| **Sanitize paths** | Off | Also make names valid on Linux and macOS | `--sanitize-paths` |

**Insert** next to each pattern inserts a placeholder at the cursor. See [File and folder names](File-and-folder-names).

## Caption text files

| Option | Default | Effect | Command line |
|---|---|---|---|
| **Post text template** | empty: the caption | What to write into each post's `.txt` file; each line of the box becomes one line in the file | `--post-metadata-txt` (once per line) |
| **Story item text template** | empty: no file | What to write into each story item's `.txt` file | `--storyitem-metadata-txt` (once per line) |

These templates cannot be used while **Captions** is off on the Download page. See [Caption file templates](File-and-folder-names#caption-file-templates).

## Resuming interrupted downloads

| Option | Default | Effect | Command line |
|---|---|---|---|
| **Resume interrupted downloads** | On | When a download stops early, save where it was in a resume file, and continue from there next time | `--no-resume` when off |
| **Resume file prefix** | `iterator` | Name prefix of the resume files | `--resume-prefix` |
| **Use aged resume files** | Off | Also resume from resume files that are older than the last successful download | `--use-aged-resume-files` |

The last two are only available while resuming is on. See [Running a download](Running-a-download#stopping-and-resuming) and [Interrupting and resuming](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Interrupting-and-resuming) in the InstaloaderSharp manual.

## Connection

| Option | Default | Effect | Command line |
|---|---|---|---|
| **Pause between requests** | On | Wait between requests to respect Instagram's rate limits. Turning this off greatly increases the risk of being blocked | `--no-sleep` when off |
| **Use iPhone API** | On | When signed in, download higher-quality pictures and videos through Instagram's app interface | `--no-iphone` when off |
| **Maximum connection attempts** | `3` | Attempts for a failing request before it is given up; `0` retries forever | `--max-connection-attempts` |
| **Request timeout** | `300` | Seconds to wait for an answer from Instagram | `--request-timeout` |
| **Abort on HTTP status codes** | none | Comma-separated HTTP status codes that stop the download immediately, for example `302,400,429` | `--abort-on` |
| **User agent** | InstaloaderSharp's default (shown in grey) | The browser identification sent to Instagram | `--user-agent` |

The values are checked before a download starts, and a problem is shown in the status bar right away: **Maximum connection attempts** must be a whole number of 0 or more, **Request timeout** a positive number of seconds (decimals with a point, `12.5`), and **Abort on HTTP status codes** a list of codes between 100 and 599.

Keep **Pause between requests** on. See [Rate limits and connections](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Rate-limits-and-connections) in the InstaloaderSharp manual for how the program paces requests, retries and what each status code means.

## Behaviour

| Option | Default | Effect | Command line |
|---|---|---|---|
| **Quiet mode** | Off | Only show errors in the output, no progress messages. The progress bars and counters on the Activity page then have little to go on | `--quiet` |
| **Keep the computer awake** | On | Prevent Windows from going to sleep while a download is running | (GUI only) |
| **Notify when finished** | On | Flash the taskbar button and play a sound when a download ends while the window is in the background | (GUI only) |

In quiet mode, the GUI still asks for a password or two-factor code when needed (InstaloaderSharp's console would fail instead).

## Command line

| Button | Does |
|---|---|
| *(the box)* | Shows the InstaloaderSharp command that does the same as your current targets and options |
| **Copy** | Copies the command |
| **Save args file…** | Saves the options and targets as an InstaloaderSharp argument file |
| **Import command line…** | Applies an InstaloaderSharp or Instaloader command line |
| **Import args file…** | Applies an argument file (`+args.txt`) |
| **Reset to defaults** | Resets all options; your targets, download folder and sign-in method are kept |

See [Command line and the GUI](Command-line-and-the-GUI).

## About

| Row | Shows |
|---|---|
| **InstaloaderSharp GUI** *version* | The GUI's version, with a **GitHub** button for its repository |
| **InstaloaderSharp** *version* | The version of the `InstaloaderSharp.exe` that is actually loaded, with a **GitHub** button |
| **Data folders** | Where sessions and latest stamps (`%LOCALAPPDATA%\Instaloader`) and the GUI's settings (`%LOCALAPPDATA%\InstaloaderSharpGUI`) are stored, with an **Open** button |
| **Documentation** | Opens this manual, the [InstaloaderSharp GUI wiki](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/wiki) |

Please include both versions when you report a problem; see [Compatibility with InstaloaderSharp](Compatibility-with-InstaloaderSharp#checking-the-versions).
