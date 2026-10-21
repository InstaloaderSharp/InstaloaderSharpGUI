This page lists every file InstaloaderSharp GUI reads or writes, besides the downloads themselves, and how to start over or move to another computer.

## Contents

- [Overview](#overview)
- [The GUI's settings](#the-guis-settings)
- [Sessions](#sessions)
- [The latest-stamps file](#the-latest-stamps-file)
- [Files in the download folder](#files-in-the-download-folder)
- [What is never saved](#what-is-never-saved)
- [Starting over](#starting-over)
- [Moving to another computer](#moving-to-another-computer)

## Overview

| What | Where | Written by |
|---|---|---|
| The GUI's settings | `%LOCALAPPDATA%\InstaloaderSharpGUI\settings.ini` | The GUI only |
| Session files | `%LOCALAPPDATA%\Instaloader\session-USERNAME` (or the **Session file** you chose) | The GUI, InstaloaderSharp and Instaloader |
| The latest-stamps file | `%LOCALAPPDATA%\Instaloader\latest-stamps.ini` (or the **Stamps file** you chose) | The GUI, InstaloaderSharp and Instaloader |
| Downloads, metadata, resume files | The download folder | The GUI, InstaloaderSharp and Instaloader |

`%LOCALAPPDATA%` is usually `C:\Users\YOUR-NAME\AppData\Local`. Type it into Explorer's address bar to open it, or use *Settings → About → Data folders → Open*.

The program files themselves (`InstaloaderSharpGUI.exe`, `InstaloaderSharp.exe`) write nothing into their own folder, so they can be kept in a read-only folder.

## The GUI's settings

`settings.ini` stores:

- all options of the Download, Account and Settings pages,
- the targets,
- the download folder,
- the sign-in method, the username, the browser, and the session and cookie file paths,
- the window's position and size.

It is saved when a download starts and when the program closes, and read when the program starts. It is a plain text file with one `name=value` line per setting. It contains no password and no session, but it does contain your username, targets and folders, so remove those before sharing it.

## Sessions

A session file contains the login cookie of an account. It is created by **Sign in** and **Import cookies** on the Account page, and updated after downloads. The GUI, `InstaloaderSharp.exe` and Instaloader use the same files, in the same format (see [Session files](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-formats#session-files) in the InstaloaderSharp manual).

The **Saved sessions** list on the Account page shows the session files in the default folder, and **Delete** or **Sign out** removes them. See [Signing in](Signing-in#saved-sessions).

> A session file gives access to your account. Keep it private, and never post it, for example in a bug report.

## The latest-stamps file

With **Remember latest downloads** on, the time of the newest downloaded item of each profile is stored in the stamps file. See [Keeping an archive up to date](Keeping-an-archive-up-to-date#remember-latest-downloads) and [Latest-stamps file](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-formats#latest-stamps-file).

## Files in the download folder

Besides pictures, videos, captions and metadata, the download folder may contain:

| File | Meaning |
|---|---|
| `id` in a profile's folder | The profile's numeric ID, used to recognize renamed profiles |
| `iterator_XXXXXXXX.json.xz` | A resume file of an interrupted download; see [Running a download](Running-a-download#stopping-and-resuming) |
| `*.temp` | A file that was being downloaded; it is renamed when complete |

See [Downloaded files](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Downloaded-files) and [File formats](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-formats) in the InstaloaderSharp manual.

## What is never saved

- **Passwords.** A password you type is kept in memory only while the program runs, and forgotten when it closes.
- **Two-factor codes.**
- The output of downloads, unless you save it with **Save…** on the Activity page.

## Starting over

| To reset | Do |
|---|---|
| All options, keeping targets, folder and sign-in | *Settings → Command line → Reset to defaults* |
| Everything in the GUI | Close the program and delete `%LOCALAPPDATA%\InstaloaderSharpGUI\settings.ini` |
| A sign-in | *Account → Sign out*, or **Delete** in **Saved sessions** |
| What **Remember latest downloads** knows | Delete the stamps file (the next download fetches everything again) |

## Moving to another computer

1. Install both programs on the new computer; see [Getting started](Getting-started#getting-the-program).
2. Copy `%LOCALAPPDATA%\InstaloaderSharpGUI\settings.ini` to the same place on the new computer, and the download folder if you want to keep updating it.
3. Copy `%LOCALAPPDATA%\Instaloader` too, to keep your sessions and latest stamps; otherwise sign in again.
4. Start the program and check the **Download folder** and the Account page, since paths may differ.

Alternatively, save an argument file with *Settings → Command line → Save args file…* and import it on the new computer with **Import args file…**.
