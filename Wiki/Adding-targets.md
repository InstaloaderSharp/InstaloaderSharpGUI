A *target* is something to download: a profile, a hashtag, a single post, a location, your feed, and so on. Targets are added at the top of the **Download** page, under **Targets**. Any number of targets can be downloaded in one go.

## Contents

- [Overview](#overview)
- [Typing or pasting a target](#typing-or-pasting-a-target)
- [Instagram links](#instagram-links)
- [Choosing the type](#choosing-the-type)
- [Your feed, followed stories and saved posts](#your-feed-followed-stories-and-saved-posts)
- [Followees of a profile](#followees-of-a-profile)
- [Metadata files](#metadata-files)
- [Importing a list](#importing-a-list)
- [The target list](#the-target-list)
- [Warnings about targets](#warnings-about-targets)

## Overview

| Target | Shown as | Downloads | Login required | More in the InstaloaderSharp manual |
|---|---|---|---|---|
| `profilename` | Profile | All posts and the profile picture of a profile (and, if you choose, its stories, highlights, tagged posts and reels). Private profiles require that you are signed in and follow them. | Only for private profiles | [Profiles](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#profiles) |
| `#hashtag` | Hashtag | Posts with that hashtag | Yes | [Hashtags](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#hashtags) |
| `%location_id` | Location | Posts tagged with a location, given by its numeric ID | Yes | [Locations](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#locations) |
| `-shortcode` | Post | A single post | No (unless the post is private) | [Single posts](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#single-posts) |
| `:feed` | Feed | Posts of your feed | Yes | [Your feed](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#your-feed) |
| `:stories` | Stories | Current stories of all accounts you follow | Yes | [Stories of the accounts you follow](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#stories-of-the-accounts-you-follow) |
| `:saved` | Saved | Posts you marked as saved | Yes | [Your saved posts](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#your-saved-posts) |
| `@profilename` | Followees | All profiles that `profilename` follows | Yes | [Followees of a profile](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#followees-of-a-profile) |
| `file.json` / `file.json.xz` | JSON file | Re-downloads a post or story item from a metadata file saved earlier | Depends | [Metadata files](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Targets#metadata-files) |

The first column shows how the target is written on InstaloaderSharp's command line. In the program you can usually type it the same way, or simply paste an Instagram link.

## Typing or pasting a target

Type into the target box and press **Enter**, or click **Add**.

While you type, the line under the box shows what will be added, for example "Press Enter to add hashtag: #sunset", or what is wrong, for example "A hashtag may only contain letters, numbers and underscores." Anything that could not be understood stays in the box when you press **Enter**, so you can correct it, while everything else is added.

You can add many targets at once: paste them one per line, or separated by spaces, commas or semicolons. For example, pasting

```
natgeo, nasa
#sunset
https://www.instagram.com/p/DdwalmrkTSK/
```

adds four targets. Quotes and angle brackets around a target are removed, so links copied from a message (`<https://…>`) work too.

With **Auto-detect** selected (the default), the type is recognized from the first character:

| Starts with | Becomes |
|---|---|
| a letter, digit, `.` or `_` | Profile |
| `@` | Profile (as on Instagram; see [Followees of a profile](#followees-of-a-profile)) |
| `#` | Hashtag |
| `%` | Location |
| `-` | Single post (shortcode) |
| `:` | `:feed`, `:stories` or `:saved` (upper or lower case) |
| ends with `.json` or `.json.xz` | JSON file |

## Instagram links

Links copied from the Instagram website or app are understood, with or without `https://`, `www.` or tracking parameters such as `?igsh=…`:

| Link | Added as |
|---|---|
| `instagram.com/natgeo/` | Profile `natgeo` |
| `instagram.com/p/DdwalmrkTSK/` | Post `-DdwalmrkTSK` |
| `instagram.com/reel/…/`, `instagram.com/reels/…/`, `instagram.com/tv/…/` | Post |
| `instagram.com/natgeo/p/…/`, `instagram.com/natgeo/reel/…/` | Post |
| `instagram.com/explore/tags/sunset/` | Hashtag `#sunset` |
| `instagram.com/explore/locations/213385402/paris-france/` | Location `%213385402` |
| `instagram.com/stories/natgeo/…/` | Profile `natgeo` (turn on **Stories** to get its stories) |

Links that do not point to something downloadable, such as `instagram.com/accounts/login/` or `instagram.com/direct/inbox/`, are rejected with "This link does not point to a profile, post, hashtag or location."

## Choosing the type

The list in front of the target box sets how the input is read:

| Type | Example input | Added as |
|---|---|---|
| **Auto-detect** | anything in the tables above | as recognized |
| **Profile** | `natgeo` or `@natgeo` | `natgeo` |
| **Hashtag** | `sunset` | `#sunset` |
| **Location ID** | `213385402` | `%213385402` |
| **Single post** | `DdwalmrkTSK` or the post's link | `-DdwalmrkTSK` |
| **Followees of** | `natgeo` or the profile's link | `@natgeo` |
| **JSON file** | a file path | the full path of the file |

With an explicit type, you can leave out the prefix, and the input is checked against that type. A location ID, for example, must be a number.

## Your feed, followed stories and saved posts

The buttons under the target box add the special targets that need no name:

| Button | Adds | Downloads |
|---|---|---|
| **Your feed** | `:feed` | The posts of your feed, as Instagram shows them to you. Posts that Instagram suggests from accounts you do not follow are skipped. |
| **Followed stories** | `:stories` | The current stories of all accounts you follow |
| **Saved posts** | `:saved` | The posts you marked as saved |

All three require that you are signed in. **Maximum number of posts** (see [Keeping an archive up to date](Keeping-an-archive-up-to-date#limiting-the-number-of-posts)) limits how many posts of the feed and saved posts are downloaded.

## Followees of a profile

A *followees* target downloads every profile that a profile follows. To add one, choose **Followees of** in the type list and enter the profile's name or link; it is shown in the list as **Followees** with the detail "every account natgeo follows · requires login". On the Look up page, **Add accounts they follow** does the same.

On InstaloaderSharp's command line this target is written `@profilename`. In the target box, however, `@natgeo` adds the *profile* natgeo, because that is what `@name` means on Instagram. Only **Import list…** and imported command lines use the command-line meaning.

> A followees target can mean hundreds of profiles. Consider the size before starting, and see [Rate limits and connections](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Rate-limits-and-connections).

## Metadata files

When **Metadata JSON** is on, every downloaded post gets a `.json` or `.json.xz` metadata file. Such a file can be used as a target to download the post again, for example after deleting its pictures:

- click **JSON file…** and select one or more files, or
- type or paste the file's path into the target box.

The list shows the file name, with its folder as the detail. The files are saved next to the metadata file. Relative paths are turned into full paths when the target is added, so it does not matter which folder the download runs in. Profile metadata files cannot be used as targets; add the profile by its name instead.

## Importing a list

**Import list…** adds targets from a text file with one target per line, written as on InstaloaderSharp's command line (`profilename`, `@profilename` for followees, `#hashtag`, `%123`, `-shortcode`, `:feed`, file paths). Lines with options (`--…`) and lines that are not valid targets are skipped and listed afterwards.

To import a file with options as well, such as an InstaloaderSharp argument file (`+args.txt`), use *Settings → Command line → Import args file…*; see [Command line and the GUI](Command-line-and-the-GUI).

## The target list

Each target is shown with an icon and a coloured badge for its type, its name, and details such as "requires login" or the post's link. The list grows with its content and scrolls when there are many targets. Adding a target that is already in the list does nothing.

| To | Do |
|---|---|
| Remove a target | Click **×** on its row, or select it and press **Delete** |
| Select several targets | **Ctrl+click**, **Shift+click**, or **Ctrl+A** for all |
| Copy targets | Select them and press **Ctrl+C** (copied in command-line syntax, one per line) |
| Paste targets | **Ctrl+V** in the list, or paste into the target box |
| Preview a profile | Double-click it, or right-click → **Look up profile** |
| Open a target on Instagram | Right-click → **Open on Instagram** (for a JSON file: opens its folder) |
| Remove all targets | **Clear all** (asks first) |

Changes to the list while a download runs apply to the next download.

## Warnings about targets

Under the target list, the program shows problems as soon as targets and options do not fit together:

| Message | Meaning |
|---|---|
| *Feed, stories, saved posts, followees and locations require login.* | Such a target is in the list, but you are downloading anonymously. Sign in on the Account page. |
| *Login is required to download stories - stories will be skipped.* | **Stories** is on, but you are not signed in. |
| *Highlights and comments require login and will probably fail when downloading anonymously.* | **Highlights** or **Comments** is on, but you are not signed in. |
| *JSON target not found: …* | A metadata file in the list no longer exists. |
| *:feed-all and :feed-liked were removed. …* | These old Instaloader targets no longer exist; use `:feed` and the post filter `viewer_has_liked`. |

Warnings (amber) let you start anyway after asking; errors (red) must be fixed first. See [Running a download](Running-a-download#before-the-download-starts).
