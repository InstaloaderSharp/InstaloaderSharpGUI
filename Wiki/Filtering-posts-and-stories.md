Filters download only the posts or story items that match a condition: a date range, a minimum number of likes, a hashtag in the caption, only videos, and so on. The two filter boxes are in the *Which posts to download* section of the **Download** page.

## Contents

- [The two filters](#the-two-filters)
- [Writing a filter](#writing-a-filter)
- [Checking as you type](#checking-as-you-type)
- [The Attributes and Examples menus](#the-attributes-and-examples-menus)
- [Examples](#examples)
- [Missing values](#missing-values)
- [Filters and updates](#filters-and-updates)
- [The full language](#the-full-language)

## The two filters

| Filter | Applies to | Command line |
|---|---|---|
| **Post filter** | Posts of profiles, hashtags, locations, your feed and saved posts; tagged posts and reels | `--post-filter` |
| **Story item filter** | Story items of stories and highlights | `--storyitem-filter` |

Leave a filter empty to download everything. Posts that do not match are not downloaded; the output shows them as `… skipped`.

## Writing a filter

A filter is an expression in the syntax of Python. Its variables are properties of the post (or story item), for example:

| Filter | Downloads |
|---|---|
| `likes > 1000` | Posts with more than 1000 likes |
| `not is_video` | Only pictures |
| `date_utc >= datetime(2024, 1, 1)` | Posts from 2024 on |
| `'travel' in caption_hashtags` | Posts with `#travel` in the caption |

Some rules:

- Combine conditions with `and`, `or` and `not`, and use brackets where needed.
- Compare with `==`, `!=`, `<`, `<=`, `>`, `>=`; check membership with `in`.
- Write text in single quotes: `owner_username == 'natgeo'`. Hashtags in `caption_hashtags` are lowercase and without `#`.
- Create dates with `datetime(year, month, day)`, and compare them with `date_utc` (times in UTC).

Unlike on the command line, you do not need to put quotes around the whole filter.

## Checking as you type

A filter is checked with InstaloaderSharp's own parser shortly after you stop typing:

- **✓ Valid filter.** in green under the box: the filter is correct.
- A red message, for example *Invalid filter: foo not a Post attribute.* or *Invalid filter: invalid syntax (at position 7: end of expression)*, and a red border: the filter is wrong. The download cannot start until it is fixed or emptied.

The check finds unknown properties and syntax errors. Errors that depend on a post's values, such as a missing caption, can only appear while downloading (see [Missing values](#missing-values)).

## The Attributes and Examples menus

- **Attributes** lists every property you can use, with its type (`int`, `str`, `bool`, `datetime`, `list`, `float`). Click one to insert it at the cursor. The list is read from the InstaloaderSharp version you use, so it always matches it.
- **Examples** fills in a ready-made filter, which you can then change. **Clear filter** empties the box.

Examples offered for posts: `likes > 1000`, `not is_video`, `date_utc >= datetime(2024, 1, 1)`, `datetime(2023, 1, 1) <= date_utc < datetime(2024, 1, 1)`, `'travel' in caption_hashtags`, `viewer_has_liked`, `typename == 'GraphSidecar'` and `comments > 10 and likes > 100`. For story items: `not is_video`, `is_video` and `date_utc >= datetime(2024, 1, 1)`.

## Examples

| You want | Post filter |
|---|---|
| Posts of one year | `datetime(2023, 1, 1) <= date_utc < datetime(2024, 1, 1)` |
| Posts since a date | `date_utc >= datetime(2026, 6, 1)` |
| Popular posts | `likes > 1000` |
| Discussed posts | `comments > 10 and likes > 100` |
| Only videos | `is_video` |
| Only posts with several pictures | `typename == 'GraphSidecar'` |
| Posts with a hashtag | `'travel' in caption_hashtags` |
| Posts that mention someone | `'natgeo' in caption_mentions` |
| Posts in which someone is tagged | `'natgeo' in tagged_users` |
| Posts you liked (with **Your feed**, signed in) | `viewer_has_liked` |
| Long videos | `video_duration is not None and video_duration > 60` |
| A word in the caption | `caption is not None and 'recipe' in caption.lower()` |

| You want | Story item filter |
|---|---|
| Only pictures | `not is_video` |
| Only videos | `is_video` |
| Story items since a date | `date_utc >= datetime(2026, 6, 1)` |

## Missing values

Some properties can be missing for a post, for example `caption` for a post without a caption, or `video_duration` for a picture. In Python terms they are `None`.

- If a comparison fails because of a missing value (`video_duration > 60` for a picture), that post is skipped with an error, and the download continues.
- If a method of a missing value is called (`caption.lower()` for a post without a caption), the download stops with an error.

Guard properties that can be missing with `is not None`, as in the last two examples above. See [When a filter fails](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Filtering-posts-and-stories#when-a-filter-fails) in the InstaloaderSharp manual.

## Filters and updates

Skipped posts do not stop a **Fast update**: it stops only at a post that passes the filter and was downloaded before. With a filter that skips most posts, a fast update therefore still goes through many posts. **Remember latest downloads** stops at the first post older than the last download, whether it passes the filter or not. See [Keeping an archive up to date](Keeping-an-archive-up-to-date).

## The full language

The InstaloaderSharp manual describes everything filters can do:

- [Filtering posts and stories](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Filtering-posts-and-stories): the expression language, dates, text and lists, story filters.
- [Properties reference](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Properties-reference): every property of posts and story items, with its type and meaning.
