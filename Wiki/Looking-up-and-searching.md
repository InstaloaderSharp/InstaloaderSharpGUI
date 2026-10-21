The **Look up** page lets you check what you are about to download: preview a profile or hashtag, see whether a profile has a current story, and search Instagram for profiles, hashtags and places.

## Contents

- [Looking up a profile](#looking-up-a-profile)
- [Looking up a hashtag](#looking-up-a-hashtag)
- [The story badge](#the-story-badge)
- [From a preview to a download](#from-a-preview-to-a-download)
- [Searching Instagram](#searching-instagram)
- [Opening the page from the target list](#opening-the-page-from-the-target-list)
- [Look-up problems](#look-up-problems)

## Looking up a profile

Type a username or paste a profile's Instagram link into the box, and click **Look up** or press **Enter** (**Ctrl+F** goes to the box from any page). The preview shows:

- the profile picture, the name, `@username` and the profile's numeric ID;
- the number of posts, followers and accounts it follows;
- the biography and website;
- badges:

| Badge | Meaning |
|---|---|
| **Verified** | The account is verified by Instagram |
| **Private** | The account is private; you can only download it when signed in with an account that follows it |
| **Private · you follow** | The account is private, and the account you are signed in with follows it |
| A business category, or **Business** | A business account |
| **Story · 3 items** / **Has story · sign in to download** | The profile has a current story; see below |
| **Follows you** | The profile follows the account you are signed in with |

Looking up works anonymously too, but some profiles (for example age-restricted ones) are only found, and some details only shown, when you are signed in. The look-up uses your saved session or browser cookies, as set on the Account page, and never asks for a password.

## Looking up a hashtag

Enter `#hashtag` (or a hashtag's Instagram link) and click **Look up**. The preview shows the hashtag's picture, description and number of posts.

## The story badge

| Badge | Meaning |
|---|---|
| **Story · 3 items** | You are signed in, and the profile has a story right now with that many items. It is checked the same way a download fetches stories, so turning on **Stories** and downloading the profile gets these items. |
| **Has story · sign in to download** | You are not signed in, and the profile has a public story. Downloading stories requires that you are signed in. |
| (no badge) | The profile has no current story, or it could not be checked (for example because Instagram limits requests at the moment) |

Stories disappear after 24 hours, so a badge you saw yesterday may be gone today. When you are signed in, checking the story needs one extra request to Instagram.

## From a preview to a download

| Button | Does |
|---|---|
| **Add to download list** | Adds the profile or hashtag to the targets on the Download page |
| **Add accounts they follow** | Adds a [followees](Adding-targets#followees-of-a-profile) target for the profile: every account it follows (requires sign-in) |
| **Open on Instagram** | Opens the profile or hashtag in your web browser |

A confirmation, such as "Added natgeo to the download list.", appears under the box.

## Searching Instagram

Type one or more words and click **Search**, or press **Enter** when you typed several words. The results are listed under **Search results**:

| Results | Shown as | Up to |
|---|---|---|
| Profiles | `@username`, the full name, *verified*, *private* | 25 |
| Hashtags | `#hashtag` and its number of posts | 15 |
| Locations | the place's name and its ID, `%213385402` | 15 |

For each result, **Add** adds it to the download list (a location as a location target), and **Details** shows the preview of a profile or hashtag.

Searching requires that you are signed in, because Instagram only answers searches from signed-in accounts. Without a session, Search shows *Search failed: Searching Instagram requires login. - sign in on the Account page.* Locations are searched with a second request; if only that one fails, the profiles and hashtags are still shown.

## Opening the page from the target list

On the Download page, double-click a profile in the target list, or right-click it and choose **Look up profile**, to open its preview here.

## Look-up problems

| Message | Meaning |
|---|---|
| *Profile not found: …* | The username is wrong, the profile was renamed or deleted, it blocked you, or it is restricted and you are not signed in |
| *Instagram requires login for this. Sign in on the Account page.* | Instagram did not answer the anonymous request |
| *Single posts cannot be previewed here - add them to the download list directly.* | You entered a post's link or shortcode; only profiles and hashtags can be previewed |
| *Search failed: …* | See [Searching Instagram](#searching-instagram); for other errors, see [Troubleshooting and FAQ](Troubleshooting-and-FAQ#look-up-and-search) |

Search needs InstaloaderSharp 1.0.4 or later: older versions use Instagram's retired website search and fail with *404 Not Found*. Finding restricted profiles needs InstaloaderSharp 1.0.3 or later. See [Compatibility with InstaloaderSharp](Compatibility-with-InstaloaderSharp).
