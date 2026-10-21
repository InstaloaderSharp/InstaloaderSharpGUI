Most people download a profile once and then update it from time to time. This page explains how to make updates fast, and how to run them automatically.

## Contents

- [Starting the same download again](#starting-the-same-download-again)
- [Fast update](#fast-update)
- [Remember latest downloads](#remember-latest-downloads)
- [Fast update or latest downloads?](#fast-update-or-latest-downloads)
- [Limiting the number of posts](#limiting-the-number-of-posts)
- [Keeping a list of profiles](#keeping-a-list-of-profiles)
- [Scheduled downloads](#scheduled-downloads)

## Starting the same download again

The program remembers your targets, options and download folder. To update a download, start the program and click **Start download**. Files that already exist are skipped (the output shows `… exists`, and the **Already saved** counter on the Activity page counts them), and only new posts are downloaded.

Without the options below, the program still goes through every post of every target to check which ones exist, which takes long for big profiles.

## Fast update

**Fast update** (*Which posts to download*, `--fast-update`): for every target, stop at the first post that was already downloaded. Up to three pinned posts at the top of a profile are taken into account. This makes updates much faster, and is recommended when the download folders are your archive.

Fast update cannot be combined with **Pictures** turned off, because it recognizes downloaded posts by their picture files.

## Remember latest downloads

**Remember latest downloads** (`--latest-stamps`): the program stores the time of the newest post, story, tagged post and reel downloaded for every profile in a small database, and only downloads newer items next time. Unlike **Fast update**, this works even if you delete or move the downloaded files.

When you turn it on, a **Stamps file** field appears. Leave it empty to use the default file, `%LOCALAPPDATA%\Instaloader\latest-stamps.ini`, which InstaloaderSharp and Instaloader use as well, or choose another file with **Browse…**.

See [Latest stamps](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Keeping-an-archive-up-to-date#latest-stamps) in the InstaloaderSharp manual.

## Fast update or latest downloads?

| | **Fast update** | **Remember latest downloads** |
|---|---|---|
| Decides by | Files in the download folder | Times stored in the stamps file |
| Downloads again after you delete files | Yes | No |
| Works if you move or reorganize downloads | No | Yes |
| Works with **Pictures** off | No | Yes |
| Works for hashtags, locations, feed, saved posts | Yes | No |
| Re-downloads the profile picture | Only if missing (or larger, when signed in) | Only if it changed |

Use **Remember latest downloads** if you process or move the downloaded files afterwards; use **Fast update** if the download folders are your archive. Both can be on together.

## Limiting the number of posts

**Maximum number of posts** (`--count`) downloads at most this many posts. It applies to hashtags, locations, your feed and saved posts, which can otherwise be very long. Leave it empty for no limit. It must be a whole number.

For profiles, use **Fast update** or a [post filter](Filtering-posts-and-stories) with a date instead, for example `date_utc >= datetime(2026, 1, 1)`.

## Keeping a list of profiles

The target list itself is your list: it is saved with your settings. To keep several lists, for example "daily" and "weekly", save each as an argument file with *Settings → Command line → Save args file…*, and load one with **Import args file…** when needed. See [Command line and the GUI](Command-line-and-the-GUI#argument-files).

A plain text file with one target per line can also be added with **Import list…** on the Download page.

## Scheduled downloads

InstaloaderSharp GUI has no scheduler of its own, and does not need to be running for scheduled updates. Instead, Windows' Task Scheduler runs `InstaloaderSharp.exe` with the same options as your download in the GUI:

1. Set up the download in the GUI as you want it, and sign in with **Instagram login**, so that a session file exists. Browser cookies are not suitable for scheduled tasks, because the browser may be open or locked while the task runs.
2. Turn on **Fast update** or **Remember latest downloads**, so that each run only fetches new posts.
3. Go to *Settings → Command line*, click **Save args file…**, and save the file into your download folder, for example `D:\Instagram\args.txt`.
4. Create a batch file, for example `D:\Instagram\update.cmd`:

   ```bat
   @echo off
   cd /d D:\Instagram
   C:\Tools\InstaloaderSharp\InstaloaderSharp.exe --quiet +args.txt >> update.log 2>&1
   ```

   `cd /d` makes the downloads go to `D:\Instagram`, the folder you chose as download folder in the GUI. `--quiet` prints only errors and never waits for input, and `>> update.log 2>&1` collects all output in a log file.

5. Create the task with **Task Scheduler → Create Basic Task**, or in a command prompt:

   ```
   schtasks /create /tn "Instagram archive" /tr "D:\Instagram\update.cmd" /sc daily /st 03:00
   ```

Because the GUI and the scheduled task use the same session file, download folder and stamps file, you can still open the GUI at any time, for example to check a profile on the Look up page or to download something extra.

Check the log and the task's **Last Run Result** from time to time: 0 means success, 1 that some items failed, and 3 that the session has expired and you need to sign in again (use **Sign in** on the GUI's Account page). The details are in [Scheduled downloads](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Keeping-an-archive-up-to-date#scheduled-downloads) and [Exit codes](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Command-line-options#exit-codes) in the InstaloaderSharp manual.

> Do not schedule several downloads with the same account at the same time, and do not run them too often: once or twice a day is enough for posts. Stories need a daily run to catch everything, since they disappear after 24 hours.
