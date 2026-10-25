# InstaloaderSharp GUI

A Graphical User Interface program for [InstaloaderSharp](https://github.com/InstaloaderSharp/InstaloaderSharp). It downloads pictures and videos along with their
captions and other metadata from Instagram: whole profiles, single posts, hashtags, locations, your feed, your
saved posts, stories and highlights. It does this without a command line.

This version of InstaloaderSharp GUI has been tested for compatibility with InstaloaderSharp 1.1.0, but might be compatible with later versions.

Everything that InstaloaderSharp's command line can do is available in the GUI. The GUI does not start
`InstaloaderSharp.exe` as a separate process. It uses it as a library, so you also get things the console
cannot offer: live progress bars, a profile preview, Instagram search, filters that are checked as you type,
sign-in with dialogs for passwords and two-factor codes, and a list of saved sessions.

No previous knowledge of InstaloaderSharp or Instaloader is needed. This document covers everything from a first download to the
advanced options.

The full manual is in the [wiki](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/wiki/Home).

> [!NOTE]  
> InstaloaderSharp GUI requires [InstaloaderSharp](https://github.com/InstaloaderSharp/InstaloaderSharp/releases) to work.
> Download the version listed above that has been tested for compatibility.

> [!IMPORTANT]  
> Only download and test with later versions of InstaloaderSharp if you are experiencing problems.

* [Requirements and building](#requirements-and-building)
* [Quick start](#quick-start)
* [The window](#the-window)
* [Adding targets](#adding-targets)
* [Choosing what to download](#choosing-what-to-download)
* [Choosing which posts to download](#choosing-which-posts-to-download)
* [Where files go](#where-files-go)
* [Signing in](#signing-in)
* [Running a download](#running-a-download)
* [Looking up profiles and searching Instagram](#looking-up-profiles-and-searching-instagram)
* [Settings](#settings)
* [Using the command line alongside the GUI](#using-the-command-line-alongside-the-gui)
* [Keyboard shortcuts](#keyboard-shortcuts)
* [Option reference](#option-reference)
* [Where the GUI stores its data](#where-the-gui-stores-its-data)
* [Tips and troubleshooting](#tips-and-troubleshooting)
* [How it works](#how-it-works)
* [Disclaimer](#disclaimer)

## Requirements and building

* Windows 10 version 1607 or later (the same as InstaloaderSharp). The dark title bar needs Windows 10
  version 1809 or later; on older versions the program still works with a light title bar.
* .NET Framework 4.8.1.
* `InstaloaderSharp.exe` in the same folder as `InstaloaderSharpGUI.exe`.

The repository expects a compiled `InstaloaderSharp.exe` in the `InstaloaderSharp` folder next to the solution:

```
InstaloaderSharpGUI.sln
InstaloaderSharp\InstaloaderSharp.exe      ← referenced as a library
InstaloaderSharpGUI\                      ← the GUI project
InstaloaderSharpGUI.Tests\                ← the unit tests
```

Open `InstaloaderSharpGUI.sln` in Visual Studio (2015 or later) and build, or run:

```
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" InstaloaderSharpGUI.sln /p:Configuration=Release
```

The result is in `InstaloaderSharpGUI\bin\Release\`: `InstaloaderSharpGUI.exe` and a copy of
`InstaloaderSharp.exe`. No NuGet packages or other dependencies are used. Copy both files (and optionally
`InstaloaderSharpGUI.exe.config`) wherever you like, as long as they stay in the same folder. If
`InstaloaderSharp.exe` is missing, the GUI says so at start-up instead of failing.

To update InstaloaderSharp, replace `InstaloaderSharp\InstaloaderSharp.exe` and rebuild. A newer
`InstaloaderSharp.exe` with the same API can also simply be copied over the one next to the GUI.

The solution also contains the unit tests (`InstaloaderSharpGUI.Tests`), which need Visual Studio's MSTest
framework; see [CONTRIBUTING.md](CONTRIBUTING.md#unit-tests). Without Visual Studio, build only the program
with `InstaloaderSharpGUI\InstaloaderSharpGUI.csproj` instead of `InstaloaderSharpGUI.sln` in the command above.

Found a bug or want to help? See [CONTRIBUTING.md](CONTRIBUTING.md), which also explains how to check whether a
problem comes from the GUI or from InstaloaderSharp.

## Quick start

1. Start `InstaloaderSharpGUI.exe`.
2. On the **Download** page, type a profile name, for example `instagram`, into the box under **Targets** and
   press **Enter**.
3. Press **Start download** at the bottom right (or **Ctrl+Enter**).

The program switches to the **Activity** page, where you can follow the progress. All posts of the public
profile (pictures, videos, captions and metadata) and its profile picture are saved into a folder named
`instagram` inside your download folder. The default download folder is `Pictures\InstaloaderSharp`.

Start the same download again later and only new posts are downloaded, while existing files are skipped.
Turn on **Fast update** (under *Which posts to download*) to stop as soon as the first already-downloaded post
is reached, which makes updates much faster.

Press **Stop** at any time. The download can be continued later from where it stopped; see
[Stopping and resuming](#stopping-and-resuming).

## The window

| Area | What it is for |
| --- | --- |
| **Sidebar**, left | Navigation between the five pages: **Download**, **Activity**, **Look up**, **Account** and **Settings**. While a download runs, *Activity* shows a **Live** badge. After a download it shows **Done** or the number of problems until you open the page. |
| **Account chip**, bottom left | Shows how you access Instagram (*Anonymous*, *@username* or browser cookies). A green dot means the session has been verified. Click it to open the Account page. |
| **Status bar**, bottom | Always visible. Shows whether everything is ready to start, or what is wrong ("Add something to download", a validation error, …). During a download it shows the current step, a progress bar and the **Stop** button. When idle it shows **Start download**. |
| **Taskbar button** | Shows the download progress, like a file copy in Explorer. It turns red if the download ended with errors. |

Every option has a short description under its name. Options that only work when signed in have a
**Requires login** badge. Hover over buttons for more help. Clicking anywhere on a row with a switch flips the
switch.

## Adding targets

A *target* is something to download: a profile, a hashtag, a post, and so on. Targets are added at the top of
the **Download** page.

### Typing or pasting

Type into the target box and press **Enter** (or click **Add**). While you type, the line underneath shows what
was recognised ("Press Enter to add profile: natgeo") or what is wrong. Anything that could not be understood
stays in the box, so you can correct it.

With **Auto-detect** selected (the default), the following are understood:

| You enter | Added as | Downloads | Login required |
| --- | --- | --- | --- |
| `natgeo`, `@natgeo` or `https://www.instagram.com/natgeo/` | Profile | All posts and the profile picture of a profile. Private profiles require that you are signed in and follow them. | Only for private profiles |
| `#sunset` or `https://www.instagram.com/explore/tags/sunset/` | Hashtag | Posts with that hashtag. | Yes |
| `%213385402` or `https://www.instagram.com/explore/locations/213385402/…` | Location | Posts tagged with that location. The ID is the number in the location's link. | Yes |
| `-DdwalmrkTSK` or a link to a post, reel or video (`/p/…`, `/reel/…`, `/reels/…`, `/tv/…`) | Post | A single post. | No (unless the post is private) |
| `https://www.instagram.com/stories/natgeo/…` | Profile | The profile (turn on **Stories** to get its stories). | Only for private profiles |
| `:feed`, `:stories`, `:saved` | Feed / Stories / Saved | Your feed, the stories of all accounts you follow, or your saved posts. | Yes |
| a path ending in `.json` or `.json.xz` | JSON file | Re-downloads a post or story item from a metadata file saved earlier. | Depends |

You can paste many targets at once, one per line or separated by spaces, commas or semicolons, and they are
all added. Links may contain tracking parameters like `?igsh=…`; these are ignored.

> **Note:** In the target box, `@name` means the *profile* `name`, as it does on Instagram. To download every
> account a profile follows (the console's `@profile` target), choose **Followees of** in the type list.

### Choosing the type explicitly

The list in front of the target box sets how the input is read: **Auto-detect**, **Profile**, **Hashtag**,
**Location ID**, **Single post**, **Followees of** or **JSON file**. With an explicit type you can leave out
the prefix: for example, choose *Hashtag* and type `sunset`.

**Followees of** adds an `@profile` target, which downloads all profiles that the given profile follows. It
requires login.

### Quick buttons

| Button | Adds |
| --- | --- |
| **Your feed** | `:feed`: the posts of your own feed. Posts that Instagram suggests from other accounts are skipped. |
| **Followed stories** | `:stories`: the current stories of all accounts you follow. |
| **Saved posts** | `:saved`: the posts you marked as saved. |
| **JSON file…** | One or more `.json` / `.json.xz` metadata files, to re-download those posts or story items. |
| **Import list…** | Targets from a text file, one per line, in console syntax. Here `@profile` means followees, as on the command line. Lines with options (`--…`) are skipped and listed. To import options as well, use [Import command line](#using-the-command-line-alongside-the-gui). |
| **Clear all** | Removes all targets (after asking). |

### The target list

Each target is shown with a coloured badge for its type, plus extra details such as "requires login" or the
post's link.

* Click **×** on a row, or select rows and press **Delete**, to remove them.
* **Ctrl+A** selects all, **Ctrl+C** copies the selected targets, and **Ctrl+V** pastes and adds targets from
  the clipboard.
* Double-click a profile to preview it on the [Look up](#looking-up-profiles-and-searching-instagram) page.
* Right-click for **Look up profile**, **Open on Instagram**, **Copy**, **Paste targets** and **Remove**.

Under the list, warnings appear as soon as the options and targets do not fit together, for example "Feed,
stories, saved posts, followees and locations require login."

## Choosing what to download

### What to download of each profile

| Switch | Default | Effect |
| --- | --- | --- |
| **Posts** | On | The profile's regular posts. |
| **Profile picture** | On | The profile picture. It is saved again only when it changes. |
| **Stories** | Off | The profile's current stories. Requires login. If you are not signed in, stories are skipped with a warning. |
| **Highlights** | Off | The profile's story highlights. Each highlight gets a subfolder. Requires login. |
| **Tagged posts** | Off | Posts in which the profile is tagged, saved in a `：tagged` subfolder (with a full-width colon). |
| **Reels** | Off | The profile's reels. |

To download only the profile picture, turn off everything except **Profile picture**. To download only
stories, turn on **Stories** and turn off **Posts**.

### What to save of each post

| Switch / field | Default | Effect |
| --- | --- | --- |
| **Pictures** | On | Pictures. Turning this off also skips video thumbnails. It cannot be combined with *Fast update*. |
| **Videos** | On | Videos. |
| **Video thumbnails** | On | The thumbnail picture of each video. |
| **Captions** | On | A `.txt` file with the caption. Its content can be changed under [Settings → Caption text files](#caption-text-files). |
| **Geotags** | Off | The post's location, with a Google Maps link, as `…_location.txt`. This may need an extra request per post. |
| **Comments** | Off | The comments of each post as `…_comments.json`. Existing files are updated. Requires login and an extra request per post. |
| **Metadata JSON** | On | A metadata file per post (likes, comment count, location, …). It is needed to re-download posts later through a JSON target. |
| **Compress JSON** | On | Store metadata as compressed `.json.xz` instead of readable `.json`. This is only available while *Metadata JSON* is on. |
| **Images of multi-image posts** | All | For posts with several pictures ("sidecars"), download only some of them: `2` (the second), `last`, `2-4` or `3-last`. Leave empty for all. |

## Choosing which posts to download

| Option | Effect |
| --- | --- |
| **Fast update** | For every target, stop at the first item that was already downloaded. Up to three pinned posts at the top of a profile are taken into account. Ideal for keeping an archive up to date. |
| **Maximum number of posts** | Download at most this many posts. This applies to hashtags, locations, your feed and saved posts. Leave empty for no limit. |
| **Remember latest downloads** | Store the date of the newest downloaded post, story, reel and so on of every profile in a small database, and only download newer items next time. Unlike *Fast update*, this works even if you delete or move the downloaded files. When it is turned on, a **Stamps file** field appears. Leave it empty for the default file `%LOCALAPPDATA%\Instaloader\latest-stamps.ini`, or choose another. |
| **Post filter** | Only download posts for which an expression is true. See below. |
| **Story item filter** | The same for stories and highlights. |

### Filters

A filter is an expression in Python syntax. It uses post properties as variables, comparison operators
(`==`, `!=`, `<`, `<=`, `>`, `>=`), `and`, `or`, `not`, `in`, arithmetic, strings in single quotes and lists.
`datetime(year, month, day)` creates a date to compare with. Examples:

```
likes > 1000
not is_video
date_utc >= datetime(2024, 1, 1)
datetime(2023, 1, 1) <= date_utc < datetime(2024, 1, 1)
'travel' in caption_hashtags and likes > 100
viewer_has_liked
typename == 'GraphSidecar'
```

The filter is checked by InstaloaderSharp's own parser **while you type**. A green "✓ Valid filter" or a red
error message (for example "foo not a Post attribute") appears under the box, and an invalid filter keeps you
from starting.

* **Attributes** inserts a property at the cursor. The list is read from the library, so it always matches the
  InstaloaderSharp version you use, and shows each property's type (`int`, `str`, `bool`, `datetime`, `list`).
* **Examples** fills in a ready-made filter, or clears it.

Useful post properties:

| Property | Meaning |
| --- | --- |
| `date_utc`, `date_local` | When the post was created (UTC, or your local time). Compare `date_utc` with `datetime(...)`. |
| `likes`, `comments` | Number of likes and comments. |
| `is_video`, `typename` | Whether it is a video; the kind of post: `'GraphImage'`, `'GraphVideo'` or `'GraphSidecar'`. |
| `mediacount` | Number of pictures/videos in the post. |
| `caption`, `caption_hashtags`, `caption_mentions` | The caption text, and lists of its hashtags (lowercase, without `#`) and mentioned users. |
| `tagged_users` | List of users tagged in the post. |
| `owner_username`, `owner_id` | The account that posted it. |
| `video_duration`, `video_view_count` | For videos: length in seconds and number of views. |
| `viewer_has_liked` | Whether you liked the post (when signed in). |

Story item properties include `date_utc`, `date_local`, `expiring_utc`, `is_video`, `typename`, `caption`,
`caption_hashtags`, `caption_mentions`, `owner_username`, `owner_id`, `shortcode` and `mediaid`. See the
[InstaloaderSharp README](https://github.com/InstaloaderSharp/InstaloaderSharp/blob/main/README.md#filtering-posts)
for more details.

## Where files go

Choose the download folder under **Save to** at the bottom of the Download page (**Browse…**). **Open** opens
it in Explorer. The folder is created if it does not exist.

Inside it, every target gets its own folder named after the target. For a profile:

```
instagram\
    2025-09-20_00-00-36_UTC_profile_pic.jpg   profile picture
    id                                        the profile's numeric ID (used to detect renames)
    instagram_25025320.json.xz                profile metadata
    2026-10-08_17-10-31_UTC.jpg               picture, or thumbnail of a video
    2026-10-08_17-10-31_UTC.mp4               video
    2026-10-08_17-10-31_UTC.txt               caption
    2026-10-08_17-10-31_UTC.json.xz           post metadata
    2026-09-26_16-01-00_UTC_1.jpg             a post with several pictures gets
    2026-09-26_16-01-00_UTC_2.jpg               one numbered file per picture/video
```

* File names are the post's date and time in UTC. Each file's modification time is set to when the post was
  created.
* Characters that are not allowed in Windows file names are replaced by similar-looking characters. For
  example, `:feed` is saved in a folder named `：feed` (with a full-width colon).
* If a profile is renamed on Instagram, its stored `id` lets the program find it again, and the folder is
  renamed accordingly.

Folder and file names can be changed under [Settings → File naming](#file-naming).

## Signing in

Public profiles and single public posts can be downloaded without an account. Signing in is needed for
private profiles you follow, stories, highlights, hashtags, locations, followees, your feed and saved posts,
and comments. When signed in, you also get higher-quality pictures and videos.

> **Note:** Downloading in large amounts can get an account temporarily restricted by Instagram. Consider
> using a secondary account.

Open the **Account** page (or click the account chip at the bottom left). The card at the top shows the
current state: who you are signed in as, whether a saved session exists and whether it was verified, with
**Check session** and **Sign out** buttons.

Choose the **sign-in method**:

### Anonymous

No account is used. Instagram limits anonymous access heavily. If you see errors such as 401, 403 or 429, or
"Login required", sign in.

### Instagram login

| Field | Meaning |
| --- | --- |
| **Username** | Your Instagram username (not your e-mail address). |
| **Password** | Optional. Only needed when there is no valid saved session. It is kept in memory only and is **never written to disk**. If you leave it empty, you are asked for it when needed. |
| **Session file** | Optional. Where the session is stored. The default is `%LOCALAPPDATA%\Instaloader\session-USERNAME`. |
| **Sign in** | Signs in now and saves the session, so that downloads can start without asking again. |

If your account uses **two-factor authentication**, a dialog asks for the code. If a password or code is
rejected, you are asked again.

After a successful sign-in, a **session file** is saved. It contains the session cookie, not your password,
but it still grants access to your account, so keep it private. As long as the session is valid, downloads use
it without asking for anything. If no valid session exists when a download starts, you are asked for the
password (and the 2FA code) at that point.

### Browser cookies

If signing in with a password does not work (Instagram sometimes asks for extra verification), sign in to
instagram.com in a web browser and use its cookies instead.

| Field | Meaning |
| --- | --- |
| **Browser** | Brave, Chrome, Chromium, Edge, Firefox, LibreWolf, Opera, Opera GX, Safari or Vivaldi. |
| **Cookie file** | Optional. The cookie database of a specific browser profile, for example a Firefox `cookies.sqlite`. |
| **Import cookies** | Reads the cookies now, verifies them and saves them as a session. |

After importing, you are offered to **switch to "Instagram login" with the saved session** (recommended).
From then on, the browser does not need to be read again for every download. If you keep using browser
cookies, they are imported every time a download starts.

Chromium-based browsers may need to be closed while their cookies are read. Firefox usually works best,
because recent Chrome versions encrypt cookies in a way that other programs cannot decrypt.

### Checking and signing out

* **Check session** verifies with Instagram that the saved session (or the browser's cookies) is still valid,
  and shows your profile picture.
* **Sign out** deletes the saved session file (after asking) and switches to anonymous.

### Saved sessions

The list at the bottom of the Account page shows every session file in the default location, including
those created with InstaloaderSharp or Instaloader on the command line. **Use** switches to that account and
**Delete** removes the session. **Refresh** reloads the list and **Open folder** opens the session folder.

## Running a download

Press **Start download** (or **Ctrl+Enter** / **F5**).

Before starting, the options are checked the same way the command line checks them:

* **Problems that prevent starting** are listed in an "Almost there" dialog, and the page where they can be
  fixed is opened. Examples: no targets, no download folder, an invalid filter, *Pictures* off together with
  *Fast update*, or a missing username.
* **Warnings** are listed in a "Before you start" dialog, where you can choose **Start download** anyway or
  **Go back**. An example is stories requested without signing in.

Your settings are saved and the **Activity** page opens.

### The Activity page

| Part | Shows |
| --- | --- |
| **Items**, **Already saved**, **Warnings**, **Errors**, **Elapsed** | Counters for posts and story items processed, files that already existed, problems so far, and the running time. Warning and error tiles turn amber or red when they are not zero. |
| **Current step** | What is being done, e.g. "Retrieving posts from profile natgeo · profile 2 of 5". |
| **Progress bars** | A thin blue bar for the profiles when several are downloaded, and a bar for the items of the current target. It moves when the total is known and is animated when it is not (e.g. hashtags). |
| **Current item** | E.g. "12 of 240" and the file being saved. |
| **Output** | The complete output of InstaloaderSharp, exactly as the console would print it: normal output in white, warnings in amber, errors in red and messages from the GUI in blue. It includes the equivalent command line of the download. |
| **Recent files** | Thumbnails of the pictures and videos just downloaded (shown when the window is wide enough). Double-click one to open it. |

Output tools:

* **Problems only** shows only warnings and errors.
* **Auto-scroll** keeps the newest output in view. Turn it off to read older output while downloading.
* **Open folder** opens the download folder, **Copy** copies the whole output, **Save…** saves it as a text
  file, and **Clear** empties it.

Errors with single items do not stop the download. They are shown when they occur and repeated at the end.

### When the download ends

The status bar and Activity page show the result:

| Result | Meaning |
| --- | --- |
| **Download complete** | Everything was downloaded. |
| **Completed with N problem(s)** | Finished, but errors or warnings occurred; see the output. |
| **Stopped** | You pressed Stop. Start again to resume. |
| **Download aborted** | Instagram stopped the download (e.g. because of *Abort on HTTP status codes*, or because it requires verification). |
| **Login failed** | Signing in did not work; a dialog explains why. |
| **Could not start** | The options were invalid; a dialog explains why. |

If the window is in the background, its taskbar button flashes and a sound plays (this can be turned off under
*Settings → Behaviour*).

### Stopping and resuming

**Stop** (or **Shift+F5**) works like Ctrl+C on the command line. The current request is finished and the
position in the list of posts is saved as a resume file (`iterator_XXXXXXXX.json.xz` in the target's
folder). Starting the same download again continues from that position, and the file is deleted when the
download completes. Resume files expire after 29 days. The behaviour can be changed under
[Settings → Resuming interrupted downloads](#resuming-interrupted-downloads).

If you close the window while a download is running, you are asked whether to stop it. The download is then
stopped cleanly (with resume information saved) before the program exits.

Changes to targets and options while a download runs apply to the next download.

## Looking up profiles and searching Instagram

The **Look up** page lets you check what you are about to download.

* **Look up** (or **Enter**) shows a **profile** or **hashtag**. You can enter a username, `#hashtag` or an
  Instagram link.
  * For a profile you see its picture, name, ID, biography and website; the numbers of posts, followers and
    following; and badges such as *Verified*, *Private* (or *Private · you follow*), the business category,
    *Follows you*, and the current story. When you are signed in, *Story · 3 items* means the profile has a
    story right now that **Stories** would download (it is checked the same way a download fetches it). When
    you are not signed in, *Has story · sign in to download* means the profile has a public story, which can
    only be downloaded when signed in. Without a current story, no story badge is shown.
  * For a hashtag you see its picture, description and number of posts.
* **Search** (or **Enter** when you typed several words) searches Instagram for matching **profiles**,
  **hashtags** and **locations**. For each result, **Add** adds it to the download list and **Details**
  shows it. Search requires that you are signed in (Instagram only answers searches from signed-in accounts).

From a preview:

| Button | Does |
| --- | --- |
| **Add to download list** | Adds the profile or hashtag as a target. |
| **Add accounts they follow** | Adds an `@profile` (followees) target. |
| **Open on Instagram** | Opens it in your web browser. |

You can also double-click a profile in the target list, or right-click it and choose **Look up profile**.
Look-ups use your saved session when you are signed in, and never ask for a password.

## Settings

The **Settings** page contains the advanced options. The defaults work well for most people. The matching
command line option is shown in parentheses after each description.

### File naming

| Option | Default | Used for |
| --- | --- | --- |
| **Folder name pattern** | `{target}` | The folder for posts. `{target}` is the target (profile name, `#hashtag`, `:feed`, …) and `{profile}` is the post owner's username. A `\` creates subfolders, e.g. `{target}\{date_utc:%Y}`. An absolute path like `D:\Instagram\{profile}` is allowed. |
| **File name pattern** | `{date_utc}_UTC` | File names of posts and stories, without extension. |
| **Title pattern** | `{date_utc}_UTC_{typename}` | File names of profile pictures, hashtag pictures and highlight covers. |
| **Sanitize paths** | Off | Also make names valid on Linux and macOS. |

**Insert** adds a placeholder at the cursor: `{target}`, `{profile}`, `{owner_username}`, `{owner_id}`,
`{shortcode}`, `{mediaid}`, `{typename}`, `{date_utc}`, `{date_local}`, `{filename}`, and
formatted dates. All [post properties](#filters) can be used.
Format specifications can follow a colon:

* dates: `{date_utc:%Y-%m-%d}` gives `2026-10-08`. The codes are `%Y` year, `%m` month, `%d` day, `%H` hour,
  `%M` minute, `%S` second, `%A` weekday name and `%B` month name.
* numbers: `{likes:06d}` gives a zero-padded `000150`.
* text: `{caption:.40}` cuts the caption after 40 characters. `{pcaption}` is a shortened single-line caption,
  suitable for file names.

### Caption text files

| Option | Effect |
| --- | --- |
| **Post text template** | What to write into each post's `.txt` file. Empty means the caption. Each line of the box becomes one line in the file, e.g. `{caption}` and then `Posted {date_utc} - {likes} likes`. |
| **Story item text template** | The same for stories. Empty means that no text file is written. |

**Insert** offers `{caption}`, `{likes}`, `{comments}`, `{date_utc}`, `{owner_username}`, `{shortcode}`,
`{location}`, `{caption_hashtags}`, `{caption_mentions}`, `{tagged_users}` and `{video_view_count}`. A template
cannot be combined with *Captions* turned off.

### Resuming interrupted downloads

| Option | Default | Effect |
| --- | --- | --- |
| **Resume interrupted downloads** | On | Save the position when a download is stopped, and continue from there next time. When turned off, resume files are neither used nor created. |
| **Resume file prefix** | `iterator` | Name prefix of the resume files. |
| **Use aged resume files** | Off | Also resume from resume files that are older than the last successful download. |

### Connection

| Option | Default | Effect |
| --- | --- | --- |
| **Pause between requests** | On | Wait between requests to respect Instagram's rate limits. Turning this off greatly increases the risk of being blocked. |
| **Use iPhone API** | On | When signed in, download higher-quality pictures and videos through Instagram's mobile API. |
| **Maximum connection attempts** | 3 | Attempts for failing requests. 0 retries forever. |
| **Request timeout** | 300 | Seconds to wait for a response. |
| **Abort on HTTP status codes** | None | Stop immediately if Instagram answers with one of these codes, e.g. `302,400,429`. |
| **User agent** | InstaloaderSharp's default | The browser identification sent to Instagram. |

### Behaviour

| Option | Default | Effect |
| --- | --- | --- |
| **Quiet mode** | Off | Only errors are shown in the output, with no progress messages. The progress bars and counters then have less to go on. |
| **Keep the computer awake** | On | Prevent Windows from going to sleep while a download is running. |
| **Notify when finished** | On | Flash the taskbar button and play a sound when a download ends while the window is in the background. |

### Command line

See [Using the command line alongside the GUI](#using-the-command-line-alongside-the-gui).

### About

Shows the versions of the GUI and of the InstaloaderSharp library, links to both projects on GitHub, the data
folders (with a button to open them) and a link to this documentation.

## Using the command line alongside the GUI

The GUI and `InstaloaderSharp.exe` share the same session files, latest-stamps file, resume files and download
folders, so you can switch between them at any time.

Under **Settings → Command line**:

| Button | Does |
| --- | --- |
| *(the box)* | Always shows the InstaloaderSharp command line that matches your current targets and options. |
| **Copy** | Copies that command, e.g. to run the same download from a script or scheduled task. |
| **Save args file…** | Saves the options and targets as an argument file (one argument per line). Run it with `InstaloaderSharp.exe +args.txt`. |
| **Import command line…** | Paste an InstaloaderSharp or Instaloader command line; all its options and targets are applied. Options it does not mention are reset to their defaults. The download folder, *Keep the computer awake* and *Notify when finished* are kept. |
| **Import args file…** | Applies an argument file (`+args.txt`), including any files it refers to. |
| **Reset to defaults** | Resets all options. Your targets, download folder and sign-in method are kept. |

Imported command lines are read by InstaloaderSharp's own argument parser, so everything is understood exactly
as on the console: abbreviated options, `--option=value`, combined short options, `--` before a shortcode
target, and so on. Invalid command lines are rejected with the parser's error message, e.g. "unrecognized
arguments: --bogus-option".

## Keyboard shortcuts

| Keys | Action |
| --- | --- |
| **Ctrl+Enter**, **F5** | Start download (while one is running: show the Activity page). |
| **Shift+F5** | Stop the download. |
| **Ctrl+L** | Go to the target box on the Download page. |
| **Ctrl+F** | Go to the search box on the Look up page. |
| **Ctrl+1** … **Ctrl+5** | Download, Activity, Look up, Account, Settings. |
| **Enter** | In the target box: add. In the Look up box: look up, or search when several words are entered. |
| **Delete**, **Ctrl+A**, **Ctrl+C**, **Ctrl+V** | In the target list: remove, select all, copy and paste targets. |
| **↑** / **↓** | In the sidebar: change page. |
| **←** / **→** | In the sign-in method selector: change method. |
| **Space** | Toggle the focused switch. |
| **Tab** | Move between controls. The focused control is outlined. |

## Option reference

Every InstaloaderSharp command line option and where to find it in the GUI:

| Command line | GUI |
| --- | --- |
| `profile`, `@profile`, `"#hashtag"`, `%location_id`, `:feed`, `:stories`, `:saved`, `-- -shortcode`, `file.json[.xz]` | Download → Targets |
| `+args.txt` | Settings → Command line → Import args file… |
| `--profile-pic-only`, `-P` | Download → only *Profile picture* on |
| `--no-posts` | Download → *Posts* off |
| `--no-profile-pic` | Download → *Profile picture* off |
| `--stories`, `-s` | Download → *Stories* |
| `--stories-only` | Download → *Stories* on, *Posts* off |
| `--highlights` | Download → *Highlights* |
| `--tagged` | Download → *Tagged posts* |
| `--reels` | Download → *Reels* |
| `--no-pictures` | Download → *Pictures* off |
| `--no-videos`, `-V` | Download → *Videos* off |
| `--no-video-thumbnails` | Download → *Video thumbnails* off |
| `--geotags`, `-G` | Download → *Geotags* |
| `--comments`, `-C` | Download → *Comments* |
| `--no-captions` | Download → *Captions* off |
| `--no-metadata-json` | Download → *Metadata JSON* off |
| `--no-compress-json` | Download → *Compress JSON* off |
| `--slide` | Download → *Images of multi-image posts* |
| `--post-metadata-txt` | Settings → Caption text files → *Post text template* |
| `--storyitem-metadata-txt` | Settings → Caption text files → *Story item text template* |
| `--fast-update`, `-F` | Download → *Fast update* |
| `--latest-stamps [FILE]` | Download → *Remember latest downloads* (+ *Stamps file*) |
| `--post-filter`, `--only-if` | Download → *Post filter* |
| `--storyitem-filter` | Download → *Story item filter* |
| `--count`, `-c` | Download → *Maximum number of posts* |
| `--login`, `-l` | Account → Instagram login → *Username* |
| `--password`, `-p` | Account → Instagram login → *Password* (or asked when needed) |
| `--sessionfile`, `-f` | Account → Instagram login → *Session file* |
| `--load-cookies`, `-b` | Account → Browser cookies → *Browser* |
| `--cookiefile`, `-B` | Account → Browser cookies → *Cookie file* |
| `--dirname-pattern` | Settings → File naming → *Folder name pattern* |
| `--filename-pattern` | Settings → File naming → *File name pattern* |
| `--title-pattern` | Settings → File naming → *Title pattern* |
| `--sanitize-paths` | Settings → File naming → *Sanitize paths* |
| `--no-resume` | Settings → Resuming → *Resume interrupted downloads* off |
| `--resume-prefix` | Settings → Resuming → *Resume file prefix* |
| `--use-aged-resume-files` | Settings → Resuming → *Use aged resume files* |
| `--no-sleep` | Settings → Connection → *Pause between requests* off |
| `--no-iphone` | Settings → Connection → *Use iPhone API* off |
| `--max-connection-attempts` | Settings → Connection → *Maximum connection attempts* |
| `--request-timeout` | Settings → Connection → *Request timeout* |
| `--abort-on` | Settings → Connection → *Abort on HTTP status codes* |
| `--user-agent` | Settings → Connection → *User agent* |
| `--quiet`, `-q` | Settings → Behaviour → *Quiet mode* |
| `--version` | Settings → About |
| `--help`, `-h` | This document; the descriptions under each option in the program |

## Where the GUI stores its data

| What | Where |
| --- | --- |
| GUI settings: all options, targets, the download folder, the sign-in method and username, and the window position | `%LOCALAPPDATA%\InstaloaderSharpGUI\settings.ini` (saved when a download starts and when the program closes) |
| Session files and the latest-stamps file (shared with InstaloaderSharp and Instaloader) | `%LOCALAPPDATA%\Instaloader\` |
| Resume files | In the target's download folder |

Your **password is never saved**. It is only kept in memory while the program runs. To start over with
default settings, close the program and delete `settings.ini`.

## Tips and troubleshooting

* **"Too many queries" / "Please wait a few minutes" / HTTP 429 or 401:** Instagram limits how many requests
  can be made. The program waits automatically between requests, and when a limit is hit it waits longer and
  retries. Do not run several downloads at the same time (also not with the command line), keep *Pause between
  requests* on, and avoid using Instagram apps with the same account while downloading.
* **"Login required" / "Redirected to login page":** Instagram refused the request anonymously. Sign in on
  the Account page. If you are signed in, your session has expired: press **Check session**, then sign in
  again or re-import your browser cookies.
* **"Checkpoint required":** open the given link in a browser, confirm the sign-in there, then try again, or
  use browser cookies.
* **"Profile ... does not exist":** check the spelling on the Look up page. The profile may have been
  renamed or deleted, or it may have blocked you. When signed in, the program tries again anonymously in that
  case.
* **Importing browser cookies fails:** close the browser and try again, or use Firefox. If you use a
  non-default browser profile, choose its cookie file.
* **The filter box is red:** the message under it says why, for example a misspelled property. Use
  **Attributes** to insert valid names.
* **Nothing seems to happen:** the Activity page shows the full output. Turn on **Problems only** to see just
  the warnings and errors.

## How it works

`InstaloaderSharp.exe` is referenced as a .NET library. A download creates an `Instaloader` object with your
options and calls `Program.MainImpl`, the same code the console program runs after reading its command line.
Results are therefore identical to the command line, and the equivalent command is shown for every download.

* InstaloaderSharp writes its progress to the console. The GUI redirects `Console.Out` and `Console.Error`,
  shows the text in the Activity log and reads the progress counters from it.
* Signing in uses the library's own login, two-factor, session and browser cookie functions. Password and code
  prompts are shown as dialogs instead of console questions. If the library ever asks for console input, it
  appears as a dialog as well.
* **Stop** raises the library's interrupt (the equivalent of Ctrl+C), so resume information is saved.
* Filters are validated with the library's expression parser, and the list of filter attributes is read from
  the library's `Post` and `StoryItem` classes.
* Look up uses `Profile`, `Hashtag` and `TopSearchResults`. Command line import uses InstaloaderSharp's own
  argument parser.

The GUI is written in C# 6 for .NET Framework 4.8.1 with Windows Forms. It uses no NuGet packages: all
controls (switches, buttons, inputs, navigation, progress bars, dialogs) are drawn by the program itself in a
dark theme with accessible contrast, visible keyboard focus and DPI scaling.

## Disclaimer

InstaloaderSharp GUI is in no way affiliated with, authorized, maintained or endorsed by Instagram or any of its
affiliates or subsidiaries. This is an independent and unofficial project. Use at your own risk.
