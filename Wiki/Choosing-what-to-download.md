On the **Download** page, two sections decide what is saved: *What to download of each profile* and *What to save of each post*. By default, the program saves posts with their pictures, videos, video thumbnails, captions and compressed metadata, and the profile picture.

## Contents

- [What to download of each profile](#what-to-download-of-each-profile)
- [What to save of each post](#what-to-save-of-each-post)
- [Images of multi-image posts](#images-of-multi-image-posts)
- [Captions and caption templates](#captions-and-caption-templates)
- [Examples](#examples)
- [Combinations that are not allowed](#combinations-that-are-not-allowed)

## What to download of each profile

These switches apply to every **profile** target (and to the profiles of a **followees** target):

| Switch | Default | Effect | Login | Command line |
|---|---|---|---|---|
| **Posts** | On | The profile's regular posts | | `--no-posts` when off |
| **Profile picture** | On | The profile picture. It is saved again only when it has changed since the last download. | | `--no-profile-pic` when off |
| **Stories** | Off | The profile's current stories, into the profile's folder | Required | `--stories` |
| **Highlights** | Off | The profile's story highlights, each into a subfolder named after the highlight, with its cover picture | Required | `--highlights` |
| **Tagged posts** | Off | Posts in which the profile is tagged, into the subfolder `：tagged` (with a full-width colon) | Recommended | `--tagged` |
| **Reels** | Off | The profile's reels, into the profile's folder | Recommended | `--reels` |

"Recommended" means that the program does not require you to be signed in, but Instagram may answer these requests only for signed-in users.

- To download **only the profile picture**, turn off everything except **Profile picture** (the console's `--profile-pic-only`).
- To download **only stories**, turn on **Stories** and turn off **Posts** (the console's `--stories-only`).
- Stories are only available for 24 hours, so download them regularly if you want to keep them. Highlights stay until the owner removes them.
- The [Look up](Looking-up-and-searching) page shows whether a profile has a current story before you download it.
- **Tagged posts** loads each tagged post with a request of its own, so it takes longer than the profile's own posts. Downloading all tagged posts needs InstaloaderSharp 1.0.5 or later; older versions stop after the first few. For updates, see [Tagged posts](Keeping-an-archive-up-to-date#tagged-posts).

These switches do not affect hashtag, location, feed, saved and single-post targets, which always download posts.

See [What is downloaded of each profile](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Choosing-what-to-download#what-is-downloaded-of-each-profile) in the InstaloaderSharp manual.

## What to save of each post

These options apply to every downloaded post, story item and reel:

| Option | Default | Effect | Command line |
|---|---|---|---|
| **Pictures** | On | Pictures. Turning this off also skips video thumbnails. It cannot be combined with **Fast update**. | `--no-pictures` when off |
| **Videos** | On | Videos | `--no-videos` when off |
| **Video thumbnails** | On | The thumbnail picture of each video | `--no-video-thumbnails` when off |
| **Captions** | On | A `.txt` file with the caption of each post | `--no-captions` when off |
| **Geotags** | Off | The post's location, with a Google Maps link, as `…_location.txt`. This may need one extra request per post. | `--geotags` |
| **Comments** | Off | The comments of each post as `…_comments.json`; existing files are updated. This needs one extra request per post, and sign-in. | `--comments` |
| **Metadata JSON** | On | A metadata file per post with likes, comment count, location and more. It is needed to re-download posts later with a [JSON target](Adding-targets#metadata-files). | `--no-metadata-json` when off |
| **Compress JSON** | On | Store metadata as compressed `.json.xz` instead of readable `.json`. Only available while **Metadata JSON** is on. | `--no-compress-json` when off |
| **Images of multi-image posts** | All | See below | `--slide` |

The files and their names are described in [Downloaded files](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Downloaded-files) in the InstaloaderSharp manual, and their formats in [File formats](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/File-formats).

## Images of multi-image posts

A post with several pictures or videos (a *sidecar*) gets one numbered file per picture or video. **Images of multi-image posts** downloads only some of them:

| Value | Downloads |
|---|---|
| (empty) | All pictures and videos |
| `2` | Only the second |
| `last` | Only the last |
| `2-4` | The second to the fourth |
| `3-last` | From the third to the last |

The value is checked as you type. An invalid value, such as `0`, `4-2` or `first`, is reported under the targets and keeps the download from starting: *Images of multi-image posts: Invalid data for --slide parameter.*

See [Parts of posts with several pictures](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Choosing-what-to-download#parts-of-posts-with-several-pictures).

## Captions and caption templates

By default, each post's `.txt` file contains its caption. What is written into the file can be changed under *Settings → Caption text files*, for example to add the number of likes; see [File and folder names](File-and-folder-names#caption-file-templates). Turning **Captions** off creates no text files at all, and cannot be combined with a caption template.

## Examples

| You want | Set |
|---|---|
| Everything of a profile, including stories and highlights | Sign in; turn on **Stories** and **Highlights** |
| Only the stories of some profiles, to collect them daily | Sign in; turn on **Stories**, turn off **Posts** and **Profile picture**; see [Keeping an archive up to date](Keeping-an-archive-up-to-date#scheduled-downloads) |
| Only pictures, no videos | Turn off **Videos** |
| Just the videos | Turn off **Pictures** (and leave **Fast update** off) |
| Readable metadata for your own tools | Turn off **Compress JSON** |
| As little as possible besides the media | Turn off **Captions** and **Metadata JSON** |
| The first picture of every multi-image post | **Images of multi-image posts**: `1` |

## Combinations that are not allowed

| Combination | Why |
|---|---|
| **Pictures** off with **Fast update** on | Fast update decides that a post was downloaded before by its picture file, which is not created. Use **Remember latest downloads** instead. |
| **Captions** off with a caption template | They contradict each other. |

The program shows these problems under the targets and in the status bar, and does not start until they are fixed. See [Combinations that are not allowed](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Choosing-what-to-download#combinations-that-are-not-allowed) in the InstaloaderSharp manual.
