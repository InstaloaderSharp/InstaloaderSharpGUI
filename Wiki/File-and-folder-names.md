This page explains where downloads are saved and how their folders and files are named, and how to change what is written into caption files.

## Contents

- [The download folder](#the-download-folder)
- [The default folder structure](#the-default-folder-structure)
- [Name patterns](#name-patterns)
- [Inserting placeholders](#inserting-placeholders)
- [Format specifications](#format-specifications)
- [Examples](#examples)
- [Sanitize paths](#sanitize-paths)
- [Caption file templates](#caption-file-templates)
- [Things to keep in mind](#things-to-keep-in-mind)

## The download folder

The **Download folder** is set at the bottom of the **Download** page, under **Save to**. Type a path or click **Browse…**; **Open** opens the folder in Explorer. The default is `Pictures\InstaloaderSharp` in your user folder. The folder is created when a download starts, if it does not exist.

All folder and file names below are relative to the download folder, unless a pattern contains an absolute path. On the command line, the download folder corresponds to the folder you run `InstaloaderSharp.exe` in.

## The default folder structure

Every target gets its own folder, named after the target:

```
Pictures\InstaloaderSharp\
    instagram\
        2025-09-20_00-00-36_UTC_profile_pic.jpg   profile picture
        id                                        the profile's numeric ID (used to detect renames)
        instagram_25025320.json.xz                profile metadata
        2026-10-08_17-10-31_UTC.jpg               picture, or thumbnail of a video
        2026-10-08_17-10-31_UTC.mp4               video
        2026-10-08_17-10-31_UTC.txt               caption
        2026-10-08_17-10-31_UTC.json.xz           post metadata
        2026-09-26_16-01-00_UTC_1.jpg             a post with several pictures gets
        2026-09-26_16-01-00_UTC_2.jpg               one numbered file per picture or video
    #sunset\
    ：feed\
```

- File names are the post's date and time in UTC. Each file's modification time is set to when the post was created.
- Characters that are not allowed in Windows file names are replaced by similar-looking characters. For example, `:feed` is saved in a folder named `：feed` (with a full-width colon).
- If a profile is renamed on Instagram, its stored `id` lets the program find it again, and the folder is renamed accordingly.

Every file is described in [Downloaded files](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Downloaded-files) in the InstaloaderSharp manual.

## Name patterns

Three patterns under *Settings → File naming* decide the names:

| Pattern | Default | Used for | Command line |
|---|---|---|---|
| **Folder name pattern** | `{target}` | The folder of posts, story items, profile pictures and metadata | `--dirname-pattern` |
| **File name pattern** | `{date_utc}_UTC` | The file names of posts and story items, without extension and without the `_1`, `_2` numbers of multi-image posts | `--filename-pattern` |
| **Title pattern** | `{date_utc}_UTC_{typename}` | The file names of profile pictures, hashtag pictures and highlight covers | `--title-pattern` |

Leave a pattern empty to use its default (shown in grey in the empty field). The **Title pattern** default becomes `{target}_{date_utc}_UTC_{typename}` when the folder name pattern contains neither `{target}` nor `{profile}`, so that the profile pictures of different profiles in a shared folder do not overwrite each other. See [Profile pictures and highlight covers](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-and-folder-names#profile-pictures-and-highlight-covers).

In a pattern, `{name}` is replaced by a property of the post. A `\` (or `/`) in the folder name pattern creates subfolders, and the folder name pattern may be an absolute path such as `D:\Instagram\{profile}`. The file name pattern is relative to the folder.

## Inserting placeholders

Click **Insert** next to a pattern for a list of common placeholders; the chosen one is inserted at the cursor:

| Placeholder | Replaced by |
|---|---|
| `{target}` | The target: profile name, `#hashtag`, `:feed`… |
| `{profile}` | The username of the post's owner |
| `{owner_username}`, `{owner_id}` | The owner's username and numeric ID |
| `{shortcode}` | The post's shortcode, as in its link |
| `{mediaid}` | The post's numeric media ID |
| `{typename}` | The kind of post, such as `GraphImage` or `GraphSidecar` |
| `{date_utc}`, `{date_local}` | The date and time, in UTC or in your local time |
| `{date_utc:%Y-%m-%d}`, `{date_utc:%Y}` | Formatted dates (see below) |
| `{filename}` | The original file name on Instagram's server |

All [post properties](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Properties-reference) can be used, for example `{likes}` or `{pcaption}`, a shortened single-line caption suitable for file names. See [Placeholders](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-and-folder-names#placeholders).

## Format specifications

After a colon, a placeholder can be formatted:

| Pattern | Gives |
|---|---|
| `{date_utc:%Y-%m-%d}` | `2026-10-08` |
| `{date_utc:%Y}\{date_utc:%m}` | `2026\10` (subfolders) |
| `{date_local:%Y-%m-%d_%H-%M}` | `2026-10-08_19-10` |
| `{likes:06d}` | `000150` |
| `{caption:.40}` | the first 40 characters of the caption |

Date codes include `%Y` year, `%m` month, `%d` day, `%H` hour, `%M` minute, `%S` second, `%A` weekday name and `%B` month name. See [Format specifications](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-and-folder-names#format-specifications).

## Examples

| You want | Folder name pattern | File name pattern |
|---|---|---|
| One folder per account, date and shortcode in the file name | `D:\Instagram\{profile}` | `{date_utc:%Y-%m-%d}_{shortcode}` |
| A subfolder per year, time and caption in the file name | `{target}\{date_utc:%Y}` | `{date_utc:%m-%d_%H%M}_{pcaption}` |
| Posts of a hashtag sorted by account | `{target}\{profile}` | (default) |
| Everything in one folder, account name in front | `D:\Instagram\all` | `{profile}_{date_utc}_UTC` |
| The original file names from Instagram | (default) | `{filename}` |

> Changing the patterns of an existing archive means that **Fast update** no longer finds the files downloaded before, so everything is downloaded again under the new names. Use **Remember latest downloads** if you reorganize; see [Keeping an archive up to date](Keeping-an-archive-up-to-date).

## Sanitize paths

**Sanitize paths** (*Settings → File naming*, `--sanitize-paths`) also makes names valid on Linux and macOS, for example for an archive on a network drive used by other systems. On Windows, invalid characters are always replaced.

## Caption file templates

Under *Settings → Caption text files*, two templates decide what is written into the `.txt` files:

| Template | Default | Command line |
|---|---|---|
| **Post text template** | `{caption}` (when empty) | `--post-metadata-txt` |
| **Story item text template** | none: no text file for story items (when empty) | `--storyitem-metadata-txt` |

Each line of the box becomes one line in the file, and **Insert** offers `{caption}`, `{likes}`, `{comments}`, `{date_utc}`, `{owner_username}`, `{shortcode}`, `{location}`, `{caption_hashtags}`, `{caption_mentions}`, `{tagged_users}` and `{video_view_count}`. For example:

```
{caption}
Posted {date_utc} - {likes} likes, {comments} comments
```

In caption files, values are written as they are; no characters are replaced. A template cannot be used while **Captions** is off on the Download page. See [Caption file templates](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-and-folder-names#caption-file-templates).

## Things to keep in mind

- **Keep names unique.** Two posts that get the same file name overwrite each other's caption and metadata, and the second one's pictures are skipped as already saved. Include `{date_utc}` with the time, `{shortcode}` or `{mediaid}`.
- **Keep the pattern when updating.** **Fast update** and the check for existing files look for the names the current pattern gives. If you change the pattern, existing downloads are not recognized and everything is downloaded again.
- **Long names.** Windows paths are limited to 260 characters unless long paths are enabled in Windows; the program supports long paths when they are. Prefer `{pcaption}` or `{caption:.40}` over `{caption}`.
- **Stories use the same patterns.** The file name pattern applies to story items too, so use placeholders that story items have as well (such as `{date_utc}`, `{shortcode}`, `{owner_username}`), or the story download fails.
- The copied command line (*Settings → Command line*) contains your patterns, so a scheduled task saves files with the same names.

See [Things to keep in mind](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-and-folder-names#things-to-keep-in-mind) in the InstaloaderSharp manual.
