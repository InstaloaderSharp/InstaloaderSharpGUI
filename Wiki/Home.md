# InstaloaderSharp GUI manual

Welcome to the InstaloaderSharp GUI manual. InstaloaderSharp GUI is a free Windows program with a graphical user interface for [InstaloaderSharp](https://github.com/InstaloaderSharp/InstaloaderSharp). It downloads pictures and videos from Instagram, along with their captions and other metadata: whole profiles, single posts, hashtags, locations, your feed, your saved posts, stories, highlights and reels. You don't need a command line.

Everything InstaloaderSharp's command line can do is available in the program, and every download gives exactly the same result as the matching InstaloaderSharp command. The GUI uses InstaloaderSharp as a library, so it can also offer things the console cannot: live progress, a profile preview, Instagram search, filters that are checked as you type, sign-in with dialogs for passwords and two-factor codes, and a list of saved sessions.

No previous knowledge of InstaloaderSharp or Instaloader is needed. If you are new, start with [Getting started](Getting-started), then read [Adding targets](Adding-targets) and [Signing in](Signing-in).

## Getting going

| Page | What it covers |
|---|---|
| [Getting started](Getting-started) | Requirements, getting both programs, the first download, updating a download |
| [The window](The-window) | The sidebar, the pages, the status bar, the account chip and how options are shown |
| [Adding targets](Adding-targets) | Typing and pasting names, `#hashtags` and Instagram links; feed, stories, saved posts, followees and metadata files; the target list |
| [Signing in](Signing-in) | Anonymous downloading, signing in with your password and two-factor code, using your browser's login, saved sessions |

## Downloading

| Page | What it covers |
|---|---|
| [Choosing what to download](Choosing-what-to-download) | Posts, profile pictures, stories, highlights, tagged posts, reels; pictures, videos, captions, metadata, comments, geotags |
| [Keeping an archive up to date](Keeping-an-archive-up-to-date) | Fast update, remembering the latest downloads, limiting the number of posts, scheduled downloads |
| [Filtering posts and stories](Filtering-posts-and-stories) | The post and story item filters, checking them as you type, the attribute and example menus |
| [File and folder names](File-and-folder-names) | The download folder, folder and file name patterns, caption file templates |
| [Running a download](Running-a-download) | Starting, the checks before starting, the Activity page, results, stopping and resuming |
| [Looking up and searching](Looking-up-and-searching) | Previewing profiles and hashtags, the story badge, searching Instagram |

## Reference

| Page | What it covers |
|---|---|
| [Settings reference](Settings-reference) | Every option on the Settings page |
| [Command line and the GUI](Command-line-and-the-GUI) | The equivalent command, argument files, importing command lines, every command-line option and where to find it |
| [Keyboard shortcuts](Keyboard-shortcuts) | All shortcuts, and using the program with the keyboard only |
| [Where data is stored](Where-data-is-stored) | Settings, sessions, latest stamps and resume files; what is never saved; starting over |
| [Compatibility with InstaloaderSharp](Compatibility-with-InstaloaderSharp) | Which InstaloaderSharp version to use, updating it, checking the versions |
| [Troubleshooting and FAQ](Troubleshooting-and-FAQ) | Common problems and their solutions, and how to tell GUI problems from InstaloaderSharp problems |
| [Glossary](Glossary) | Terms used in this manual |

## Project

| Page | What it covers |
|---|---|
| [Building and contributing](Building-and-contributing) | Building from source, unit tests, reporting bugs, contributing code |
| [How it works](How-it-works) | How the GUI drives the InstaloaderSharp library, for contributors and the curious |
| [Releases](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/releases) | Downloads of each version, and what changed in it |
| [InstaloaderSharp manual](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki) | The manual of the program that does the downloading: file formats, filter language, name placeholders and more |

## Conventions in this manual

- Names of pages, buttons, switches and fields are written in **bold**, as they appear in the program: **Start download**, **Fast update**.
- *Settings → Command line* means the **Command line** section of the **Settings** page.
- Keys are written like **Ctrl+Enter**.
- `YOUR-USERNAME` stands for your Instagram username, `profilename` for the username of the profile you want to download.
- Where an option corresponds to an InstaloaderSharp command-line option, the option is given in brackets, like this: **Fast update** (`--fast-update`). The descriptions in the program show the same.
- Times in file names are in UTC unless the text says otherwise.

> InstaloaderSharp GUI is in no way affiliated with, authorized, maintained or endorsed by Instagram or any of its affiliates or subsidiaries. This is an independent and unofficial project. Use at your own risk, and respect the privacy and copyright of the people whose posts you download.
