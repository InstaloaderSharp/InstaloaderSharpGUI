Common problems and their solutions. If your problem is not here, see the [InstaloaderSharp troubleshooting page](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Troubleshooting-and-FAQ), which covers the downloading itself in more detail.

## Contents

- [Is it a GUI problem?](#is-it-a-gui-problem)
- [Installation and start-up](#installation-and-start-up)
- [Signing in](#signing-in)
- [Rate limits and blocks](#rate-limits-and-blocks)
- [Profiles and posts](#profiles-and-posts)
- [Stories, highlights, feed and saved posts](#stories-highlights-feed-and-saved-posts)
- [Look up and search](#look-up-and-search)
- [Starting a download](#starting-a-download)
- [Files and folders](#files-and-folders)
- [Updates and resuming](#updates-and-resuming)
- [The window](#the-window)
- [General questions](#general-questions)
- [Getting help](#getting-help)

## Is it a GUI problem?

The GUI does not download anything itself: it passes your options to InstaloaderSharp, exactly as `InstaloaderSharp.exe` would receive them. To find out where a problem comes from:

1. Go to *Settings → Command line* and click **Copy**.
2. Open a command prompt in your download folder and run the copied command with the `InstaloaderSharp.exe` that is next to `InstaloaderSharpGUI.exe`.
3. Compare:

| `InstaloaderSharp.exe` shows… | The problem is probably in… | Report it to |
|---|---|---|
| the same error or the same wrong result | InstaloaderSharp, or a change on Instagram's side | [InstaloaderSharp issues](https://github.com/InstaloaderSharp/InstaloaderSharp/issues) |
| no problem | the GUI | [InstaloaderSharp GUI issues](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues) |

As a rule of thumb, errors in the output while downloading, missing or wrong files, and sign-in failures usually come from InstaloaderSharp or Instagram. Problems with the window, options without effect, wrong messages before starting, importing command lines, the Activity page and the Look up page usually come from the GUI.

## Installation and start-up

**The program says *InstaloaderSharp.exe was not found.***
Put `InstaloaderSharp.exe` into the same folder as `InstaloaderSharpGUI.exe`. It is a separate download; see [Getting started](Getting-started#getting-the-program).

**Windows says that .NET Framework 4.8.1 is missing.**
Install .NET Framework 4.8.1 from Microsoft. It is a free runtime and is included in recent Windows 11 builds.

**Windows SmartScreen or my antivirus warns about the program.**
The programs are not signed, which triggers SmartScreen. Click **More info → Run anyway** if you downloaded them from the official releases pages. You can also [build them yourself](Building-and-contributing#building-from-source).

**A dialog *Something went wrong* appears.**
Something unexpected failed. If it mentions `MissingMethodException` or a similar error, the InstaloaderSharp version does not fit the GUI version; see [Compatibility with InstaloaderSharp](Compatibility-with-InstaloaderSharp#when-the-versions-do-not-fit-together). Otherwise, please [report it](Building-and-contributing#reporting-bugs) with the dialog's text.

**The title bar is light, not dark.**
The dark title bar needs Windows 10 version 1809 or later.

## Signing in

**The password is not accepted, although it is correct.**
Enter your Instagram username on the Account page, not your e-mail address or phone number. If Instagram refuses the sign-in anyway, sign in to instagram.com in Firefox and use **Browser cookies**; see [Signing in](Signing-in#using-your-browsers-login).

***Checkpoint required. Point your browser to …***
Instagram wants to confirm that it is you. Open the link in a browser where you are signed in to the same account, follow the instructions, and try again. Or use **Browser cookies**.

**Importing browser cookies finds no cookies, or is not signed in.**
Sign in to instagram.com in that browser first. Close Chromium-based browsers (Chrome, Edge, Brave, Opera, Vivaldi) before importing. Recent Chrome versions encrypt cookies so that only Chrome can read them; use Firefox or LibreWolf.

**It imports the wrong account.**
The browser's default profile is signed in to another account. Sign in with the right account there, or choose the browser profile's **Cookie file**.

**The session stopped working.**
Sessions end when you sign out in the browser you imported them from, change your password, or Instagram ends them. Click **Sign in** again (or **Import cookies**). **Check session** on the Account page tells you whether a session is still valid.

**Two-factor authentication does not work.**
Instagram's two-factor sign-in for programs is unreliable. Use **Browser cookies** instead.

**The account chip shows a grey dot although I am signed in.**
The green dot only appears once the session has been verified in this run of the program: by **Sign in**, **Check session** or a successful download.

## Rate limits and blocks

***Too many queries in the last time. Need to wait …***
The program is pacing its requests to stay within Instagram's limits. This is normal for large downloads; it continues by itself.

***429 - Too Many Requests*, *Please wait a few minutes before you try again.* (401)**
Instagram limited the account or the connection. The program waits and retries. If it keeps happening, do not run several downloads at the same time (also not from the command line), keep **Pause between requests** on, avoid using the account heavily in the app or browser meanwhile, and stop for a while: hours rather than minutes.

See [Rate limits and blocks](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Troubleshooting-and-FAQ#rate-limits-and-blocks) and [Rate limits and connections](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Rate-limits-and-connections) in the InstaloaderSharp manual.

## Profiles and posts

***Profile … does not exist***
Check the spelling on the [Look up](Looking-up-and-searching) page. The profile may have been renamed or deleted, or it may have blocked you. Some profiles, for example age-restricted ones, are only found when you are signed in (with InstaloaderSharp 1.0.3 or later). When signed in, the program also tries again anonymously, in case you are blocked.

**A private profile downloads nothing.**
You must be signed in with an account that follows the profile. The Look up page shows **Private · you follow** if it does.

**Only some posts are downloaded.**
Check the **Post filter**, **Maximum number of posts** (hashtags, locations, feed and saved posts only), **Fast update** and **Remember latest downloads**: each of them can stop or skip posts on purpose.

**Signed in, no posts are downloaded, and the output mentions a redirect to `https://www.instagram.com/`.**
Use InstaloaderSharp 1.0.2 or later; see [Compatibility with InstaloaderSharp](Compatibility-with-InstaloaderSharp#features-that-need-a-newer-instaloadersharp).

## Stories, highlights, feed and saved posts

***Login is required to download stories - stories will be skipped.***
Stories, highlights, the feed and saved posts need you to be signed in; see [Signing in](Signing-in).

**Stories are on, but no story is downloaded.**
The profile has no current story; stories disappear after 24 hours. When you are signed in, the [Look up](Looking-up-and-searching#the-story-badge) page shows **Story · N items** when there is one. If it does and the download still gets nothing, check with `InstaloaderSharp.exe` (see [above](#is-it-a-gui-problem)).

**The feed contains fewer posts than in the app.**
Posts that Instagram suggests from accounts you do not follow are skipped.

## Look up and search

***Search failed: Searching Instagram requires login. - sign in on the Account page.***
Instagram only answers searches from signed-in accounts. Sign in on the Account page.

***Search failed: JSON Query to web/search/topsearch/: 404 Not Found***
Your InstaloaderSharp version uses Instagram's retired website search. Use InstaloaderSharp 1.0.4 or later.

**Search finds profiles and hashtags, but no locations.**
Locations are searched with a separate request, which failed; the output on the Activity page shows why. Try again later.

***Single posts cannot be previewed here***
Only profiles and hashtags can be previewed. Add posts to the download list directly.

## Starting a download

**Start download shows *Almost there* with a list of problems.**
Each problem says what to fix; the page where it can be fixed opens. See [Before the download starts](Running-a-download#before-the-download-starts).

**The status bar shows a problem in amber, although everything looks right.**
The message names the option. A common one is a filter or a number field on the Settings page; see the [Settings reference](Settings-reference).

***"Pictures" off and "Fast update" cannot be used together.***
Fast update recognizes downloaded posts by their pictures. Use **Remember latest downloads** instead, or turn **Pictures** back on.

**A filter is red.**
The message under it says why, for example a misspelled property. Use **Attributes** to insert valid names; see [Filtering posts and stories](Filtering-posts-and-stories).

## Files and folders

**Where are my downloads?**
In the **Download folder** shown under **Save to** on the Download page, and in the status bar. **Open** opens it; each target has its own folder. The default is `Pictures\InstaloaderSharp`.

**Why is the folder called `：feed` / why do names contain odd characters?**
Characters that are not allowed in Windows file names are replaced by similar-looking ones; see [File and folder names](File-and-folder-names#the-default-folder-structure).

**After changing a name pattern, everything is downloaded again.**
Existing files are only recognized under the names the current pattern gives. Change patterns before the first download, or use **Remember latest downloads**.

**Can I use the same folders with InstaloaderSharp or Instaloader on the command line?**
Yes. All three write the same files and names; see [Interoperability with Instaloader](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Interoperability-with-Instaloader).

## Updates and resuming

**An update downloads everything again.**
The files are not where the current download folder and patterns expect them, for example after moving the folder. Choose the right **Download folder**, or use **Remember latest downloads**, which does not depend on the files.

**A stopped download starts from the beginning.**
Resume files expire after 29 days, and only work with the same account. Check also that **Resume interrupted downloads** is on (*Settings → Resuming interrupted downloads*).

## The window

**Text or controls look too small or too large.**
The program follows Windows' display scaling. If you changed the scaling while it was running, restart it.

**The Recent files list on the Activity page is missing.**
It is shown when the window is wide enough. Make the window wider or maximize it.

**The output stops scrolling.**
**Auto-scroll** is off. Turn it on, above the output.

## General questions

**Is it free? Does it collect data?**
It is free and open source under the MIT License. It only talks to Instagram, and only for what you start: downloads, **Sign in**, **Check session**, **Look up** and **Search**.

**Does the GUI need to be open for scheduled downloads?**
No. Scheduled downloads run `InstaloaderSharp.exe` with an argument file saved from the GUI; see [Scheduled downloads](Keeping-an-archive-up-to-date#scheduled-downloads).

**Can I run several downloads at once?**
One download runs at a time. Add all targets to the list instead; they are downloaded one after the other, which is also gentler on Instagram's limits.

**Can it upload, like or follow?**
No. InstaloaderSharp and the GUI only download.

## Getting help

- Read [Is it a GUI problem?](#is-it-a-gui-problem) and report the problem to the right project, with both versions (*Settings → About*), the copied command and the output (*Activity → Copy* or **Save…**). See [Reporting bugs](Building-and-contributing#reporting-bugs).
- Never post passwords, session files, cookies or two-factor codes.
