This page takes you from nothing to your first download, and shows how to update a download later.

## Contents

- [Requirements](#requirements)
- [Getting the program](#getting-the-program)
- [Starting the program](#starting-the-program)
- [Your first download](#your-first-download)
- [Updating a download](#updating-a-download)
- [Downloading more than public profiles](#downloading-more-than-public-profiles)
- [Getting help](#getting-help)
- [Next steps](#next-steps)

## Requirements

- Windows 10 version 1607 or later, or Windows 11. The dark title bar needs Windows 10 version 1809 or later; on older versions the program still works, with a light title bar.
- .NET Framework 4.8.1. It is included in recent Windows 11 builds; on Windows 10 it is a free download from Microsoft.
- [InstaloaderSharp](https://github.com/InstaloaderSharp/InstaloaderSharp), which does the actual downloading. It is a separate download (see below).

Nothing else is needed: no Python, no other runtime, no installer.

## Getting the program

InstaloaderSharp GUI consists of two files that must be in the **same folder**:

| File | Where to get it |
|---|---|
| `InstaloaderSharpGUI.exe` | `InstaloaderSharpGUI-<version>-win.zip` from the [InstaloaderSharp GUI releases](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/releases) |
| `InstaloaderSharp.exe` | `InstaloaderSharp-<version>-win.zip` from the [InstaloaderSharp releases](https://github.com/InstaloaderSharp/InstaloaderSharp/releases) |

1. Download the latest InstaloaderSharp GUI release.
2. Download the InstaloaderSharp version that the GUI release says it has been tested with. It is named at the top of the [README](https://github.com/InstaloaderSharp/InstaloaderSharpGUI#readme) and in the release notes. Only use a later InstaloaderSharp version if you run into problems; see [Compatibility with InstaloaderSharp](Compatibility-with-InstaloaderSharp).
3. Extract both `.exe` files into one folder of your choice, for example `C:\Tools\InstaloaderSharp`.

To build the program yourself instead, see [Building and contributing](Building-and-contributing).

> Windows SmartScreen may warn about the downloaded programs because they are not signed. Click **More info → Run anyway** if you downloaded them from the official releases pages.

## Starting the program

Double-click `InstaloaderSharpGUI.exe`. The window opens on the **Download** page. [The window](The-window) explains its parts.

If `InstaloaderSharp.exe` is not in the same folder, the program says so and closes:

```
InstaloaderSharp.exe was not found.

Place InstaloaderSharpGUI.exe in the same folder as InstaloaderSharp.exe:
C:\Tools\InstaloaderSharp\
```

## Your first download

1. On the **Download** page, type the name of a public profile, for example `instagram`, into the box under **Targets**, and press **Enter** (or click **Add**). While you type, the line under the box shows what was recognized ("Press Enter to add profile: instagram").
2. Check the **Download folder** at the bottom of the page (under **Save to**). The default is `Pictures\InstaloaderSharp`; click **Browse…** to choose another one.
3. Click **Start download** at the bottom right, or press **Ctrl+Enter**.

The program switches to the **Activity** page, where you can follow the download: counters, progress bars, the complete output and, in a wide window, thumbnails of the files just saved. All posts of the profile (pictures, videos, captions and metadata) and its profile picture are saved into a folder named `instagram` inside your download folder. See [Running a download](Running-a-download) for everything the Activity page shows.

A large profile takes a long time, because the program waits between requests so that Instagram does not block it (see [Rate limits and connections](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Rate-limits-and-connections) in the InstaloaderSharp manual).

Press **Stop** at any time. The download can be continued later from where it stopped; see [Running a download](Running-a-download#stopping-and-resuming).

You can also paste an Instagram link instead of a name. A link to a post, such as `https://www.instagram.com/p/DdwalmrkTSK/`, adds that single post. See [Adding targets](Adding-targets) for everything else you can download.

## Updating a download

Click **Start download** again later and only new posts are downloaded; files that already exist are skipped. Turn on **Fast update** (under *Which posts to download*) to stop as soon as the first already-downloaded post is reached, which makes updates much faster.

The program remembers your targets, options and download folder, so the next time you start it you only need to click **Start download**. See [Keeping an archive up to date](Keeping-an-archive-up-to-date) for more ways to update downloads, including scheduled updates.

## Downloading more than public profiles

Without an account you can download public profiles and single public posts. Stories, highlights, private profiles you follow, hashtags, locations, your feed and your saved posts need you to be signed in.

Open the **Account** page and choose a sign-in method:

- **Instagram login**: enter your username and click **Sign in**. You are asked for your password, and for a two-factor code if your account uses one. The login is saved as a session and reused afterwards.
- **Browser cookies**: if signing in with a password does not work, sign in to instagram.com in Firefox (or another supported browser), choose that browser and click **Import cookies**.

See [Signing in](Signing-in) for details.

> Downloading in large amounts can get an account temporarily restricted by Instagram. Consider using a secondary account.

## Getting help

- Every option has a short description in the program, and the buttons have tooltips.
- This manual explains every page and option. [Troubleshooting and FAQ](Troubleshooting-and-FAQ) covers common problems.
- The [InstaloaderSharp manual](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki) explains what happens behind the scenes: the downloaded files, filters, name templates and Instagram's limits.

## Next steps

- [Choosing what to download](Choosing-what-to-download): leave out videos, add comments or geotags, download stories and highlights.
- [File and folder names](File-and-folder-names): organize downloads in your own folder structure.
- [Filtering posts and stories](Filtering-posts-and-stories): download only posts that match a condition, such as a date range or a hashtag.
