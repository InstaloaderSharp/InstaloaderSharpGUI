InstaloaderSharp GUI and `InstaloaderSharp.exe` work together: every download in the GUI corresponds to an InstaloaderSharp command, and existing commands can be imported into the GUI. This page explains *Settings → Command line* and lists where every command-line option is found in the GUI.

## Contents

- [The equivalent command](#the-equivalent-command)
- [What the command contains](#what-the-command-contains)
- [Argument files](#argument-files)
- [Importing a command line](#importing-a-command-line)
- [Importing an argument file](#importing-an-argument-file)
- [Resetting the options](#resetting-the-options)
- [Sharing data between the GUI and the command line](#sharing-data-between-the-gui-and-the-command-line)
- [Option reference](#option-reference)

## The equivalent command

The box under *Settings → Command line* always shows the InstaloaderSharp command that does the same as your current targets and options, for example:

```
InstaloaderSharp --stories --fast-update --login=YOUR-USERNAME natgeo "#sunset"
```

It changes as you change options. **Copy** copies it, so you can run the same download from a command prompt, a batch file or a scheduled task (see [Scheduled downloads](Keeping-an-archive-up-to-date#scheduled-downloads)). The command is also printed at the top of the output of every download on the Activity page.

To run it, open a command prompt in your download folder and use the full path of `InstaloaderSharp.exe` if it is not in your `PATH`:

```
cd /d "%USERPROFILE%\Pictures\InstaloaderSharp"
"C:\Tools\InstaloaderSharp\InstaloaderSharp.exe" --fast-update natgeo
```

## What the command contains

- Only options that differ from InstaloaderSharp's defaults are included. With default options, the command is just `InstaloaderSharp` and the targets.
- The **download folder** is not part of the command: InstaloaderSharp saves into the folder it is run in.
- Your **password** is never included. With **Instagram login**, the command contains `--login=YOUR-USERNAME`; the saved session is used, or the password is asked for.
- The GUI-only options **Keep the computer awake** and **Notify when finished** have no command-line equivalent.
- Arguments that contain spaces or special characters are quoted, for example `"#sunset"` and `"--post-filter=likes > 100"`.
- Targets that start with `-` (single posts) come after `--`, as the command line requires. When **Remember latest downloads** is on without a stamps file, `--` is placed before the targets too, so that the first target is not read as the stamps file.
- Several caption template lines become several `--post-metadata-txt` options.
- Metadata file targets are written with their full path.

## Argument files

**Save args file…** saves the options and targets as an InstaloaderSharp argument file: a text file with one argument per line, for example:

```
--stories
--fast-update
--login=YOUR-USERNAME
natgeo
#sunset
```

Run it with:

```
InstaloaderSharp.exe +args.txt
```

Relative paths in the file are resolved from the folder you run it in. Argument files are useful to keep several target lists, to schedule downloads, and to share a setup. See [Reading options from a file](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Command-line-options#reading-options-from-a-file) in the InstaloaderSharp manual.

## Importing a command line

**Import command line…** applies an InstaloaderSharp or Instaloader command line to the GUI. Paste it into the dialog and click **Import**:

- The command is read by InstaloaderSharp's own argument parser, so everything is understood exactly as on the console: abbreviated options (`--fast` for `--fast-update`), `--option=value`, combined short options (`-sF`), `--` before shortcodes, and `+file` arguments.
- The program name at the start (`InstaloaderSharp`, `InstaloaderSharp.exe`, `instaloader`, or a full path) is ignored.
- All options and targets of the command are applied. **Options that the command does not mention are reset to their defaults.** Your download folder and the *Behaviour* options **Keep the computer awake** and **Notify when finished** are kept.
- `--login` switches the sign-in method to **Instagram login**, `--load-cookies` to **Browser cookies**, and a command without either to **Anonymous**.
- `--profile-pic-only` and `--stories-only` become the matching switches; see [Choosing what to download](Choosing-what-to-download#what-to-download-of-each-profile).
- An invalid command line is not applied; the parser's error is shown, for example *instaloader: error: unrecognized arguments: --bogus-option*.

Importing is not possible while a download runs.

## Importing an argument file

**Import args file…** applies an argument file, such as one saved with **Save args file…** or written for InstaloaderSharp or Instaloader. Argument files that refer to other argument files (`+more.txt` inside the file) are read too, relative to the file's folder. Everything else works as for [importing a command line](#importing-a-command-line).

To add only the targets of a plain list, use **Import list…** on the Download page instead.

## Resetting the options

**Reset to defaults** resets every option to its default, after asking. Your targets, download folder and sign-in method (with username, session file, browser and cookie file) are kept.

## Sharing data between the GUI and the command line

The GUI and `InstaloaderSharp.exe` (and Instaloader) use the same files, so you can switch between them at any time:

| Shared | Where |
|---|---|
| Session files | `%LOCALAPPDATA%\Instaloader\session-USERNAME` |
| The latest-stamps file | `%LOCALAPPDATA%\Instaloader\latest-stamps.ini` |
| Downloads, metadata, resume files | The download folder |

A session created with **Sign in** in the GUI works with `InstaloaderSharp.exe --login YOUR-USERNAME`, and the other way round. See [Where data is stored](Where-data-is-stored).

## Option reference

Every InstaloaderSharp command-line option, and where to find it in the GUI. The options are explained in [Command-line options](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Command-line-options) in the InstaloaderSharp manual.

| Command line | GUI |
|---|---|
| `profile`, `@profile`, `"#hashtag"`, `%location_id`, `:feed`, `:stories`, `:saved`, `-- -shortcode`, `file.json[.xz]` | Download → Targets ([Adding targets](Adding-targets)) |
| `+args.txt` | Settings → Command line → **Import args file…** |
| `--profile-pic-only`, `-P` | Download → only **Profile picture** on |
| `--no-posts` | Download → **Posts** off |
| `--no-profile-pic` | Download → **Profile picture** off |
| `--stories`, `-s` | Download → **Stories** |
| `--stories-only` | Download → **Stories** on, **Posts** off |
| `--highlights` | Download → **Highlights** |
| `--tagged` | Download → **Tagged posts** |
| `--reels` | Download → **Reels** |
| `--no-pictures` | Download → **Pictures** off |
| `--no-videos`, `-V` | Download → **Videos** off |
| `--no-video-thumbnails` | Download → **Video thumbnails** off |
| `--geotags`, `-G` | Download → **Geotags** |
| `--comments`, `-C` | Download → **Comments** |
| `--no-captions` | Download → **Captions** off |
| `--no-metadata-json` | Download → **Metadata JSON** off |
| `--no-compress-json` | Download → **Compress JSON** off |
| `--slide` | Download → **Images of multi-image posts** |
| `--post-metadata-txt` | Settings → Caption text files → **Post text template** |
| `--storyitem-metadata-txt` | Settings → Caption text files → **Story item text template** |
| `--fast-update`, `-F` | Download → **Fast update** |
| `--latest-stamps [FILE]` | Download → **Remember latest downloads** (and **Stamps file**) |
| `--post-filter`, `--only-if` | Download → **Post filter** |
| `--storyitem-filter` | Download → **Story item filter** |
| `--count`, `-c` | Download → **Maximum number of posts** |
| `--login`, `-l` | Account → Instagram login → **Username** |
| `--password`, `-p` | Account → Instagram login → **Password** (or asked when needed) |
| `--sessionfile`, `-f` | Account → Instagram login → **Session file** |
| `--load-cookies`, `-b` | Account → Browser cookies → **Browser** |
| `--cookiefile`, `-B` | Account → Browser cookies → **Cookie file** |
| `--dirname-pattern` | Settings → File naming → **Folder name pattern** |
| `--filename-pattern` | Settings → File naming → **File name pattern** |
| `--title-pattern` | Settings → File naming → **Title pattern** |
| `--sanitize-paths` | Settings → File naming → **Sanitize paths** |
| `--no-resume` | Settings → Resuming → **Resume interrupted downloads** off |
| `--resume-prefix` | Settings → Resuming → **Resume file prefix** |
| `--use-aged-resume-files` | Settings → Resuming → **Use aged resume files** |
| `--no-sleep` | Settings → Connection → **Pause between requests** off |
| `--no-iphone` | Settings → Connection → **Use iPhone API** off |
| `--max-connection-attempts` | Settings → Connection → **Maximum connection attempts** |
| `--request-timeout` | Settings → Connection → **Request timeout** |
| `--abort-on` | Settings → Connection → **Abort on HTTP status codes** |
| `--user-agent` | Settings → Connection → **User agent** |
| `--quiet`, `-q` | Settings → Behaviour → **Quiet mode** |
| `--version` | Settings → About |
| `--help`, `-h` | This manual, and the descriptions under each option in the program |
| (the folder the command runs in) | Download → Save to → **Download folder** |
