Public profiles and single public posts can be downloaded without an account. Everything else needs you to be signed in to Instagram. This page explains the **Account** page.

## Contents

- [What needs sign-in](#what-needs-sign-in)
- [The Account page](#the-account-page)
- [Downloading anonymously](#downloading-anonymously)
- [Signing in with your Instagram account](#signing-in-with-your-instagram-account)
- [Two-factor authentication](#two-factor-authentication)
- [Using your browser's login](#using-your-browsers-login)
- [Checking the session and signing out](#checking-the-session-and-signing-out)
- [Saved sessions](#saved-sessions)
- [Several accounts](#several-accounts)
- [Keeping your account safe](#keeping-your-account-safe)
- [Sign-in problems](#sign-in-problems)

## What needs sign-in

| Needs sign-in | Works without |
|---|---|
| Private profiles you follow | Public profiles: their posts and profile pictures |
| Stories and highlights | Single public posts |
| Hashtags and locations | Geotags of posts |
| Your feed, your saved posts, followed stories | Metadata files of public posts |
| Followees of a profile | |
| Comments | |
| Searching Instagram (Look up page) | |

When you are signed in, pictures and videos are also downloaded in higher quality (through Instagram's app interface; see **Use iPhone API** in the [Settings reference](Settings-reference#connection)), and Instagram limits you less than anonymous visitors.

Options and targets that need sign-in are marked **Requires login**, and the program warns before starting if you use them anonymously.

> Downloading in large amounts can get an account temporarily restricted by Instagram. Consider using a secondary account.

## The Account page

At the top, a card shows the current state: your profile picture (after the session has been checked) or initial, **@username**, and whether a saved session exists and has been verified. It has the buttons **Check session** and **Sign out**.

Under **Sign-in method**, choose how the program accesses Instagram:

| Method | When to use it | Command-line equivalent |
|---|---|---|
| **Anonymous** | Public profiles and posts only | (no option) |
| **Instagram login** | You sign in with your username and password, once | `--login` |
| **Browser cookies** | Signing in with a password does not work, or you prefer to sign in in your browser | `--load-cookies` |

Only the fields of the chosen method are shown. The account chip at the bottom of the sidebar always shows the current method (see [The window](The-window#the-sidebar)).

## Downloading anonymously

With **Anonymous**, no account is used. Instagram limits anonymous access heavily: if you see errors such as 401, 403 or 429, or "Login required", sign in.

## Signing in with your Instagram account

1. Choose **Instagram login**.
2. Enter your **Username**: your Instagram username, not your e-mail address or phone number.
3. Click **Sign in**.
4. Enter your password when asked (and a two-factor code, if your account uses one).

When the sign-in succeeds, a message shows where the **session** was saved, and the account chip shows a green dot. From then on, downloads reuse the session without asking for anything, until it expires.

| Field | Meaning |
|---|---|
| **Username** | Your Instagram username |
| **Password** | Optional. You can type it here instead of in the dialog. It is only needed when there is no valid saved session, is kept in memory only, and is **never written to disk** |
| **Session file** | Optional. Where the session is stored. Leave it empty for the default, `%LOCALAPPDATA%\Instaloader\session-USERNAME`, which InstaloaderSharp and Instaloader use as well (`--sessionfile`) |
| **Sign in** | Signs in now and saves the session |

You do not have to click **Sign in**: if there is no valid session when a download starts, you are asked for the password then. If a password or code is not accepted, you are asked again; **Cancel** stops the download with *Login failed: Login cancelled.*

## Two-factor authentication

If your account uses two-factor authentication, a dialog asks for the verification code from your authenticator app or text message after the password. Enter it and click **Verify**.

Instagram's two-factor sign-in for programs is unreliable, which the output also warns about. If it does not work, use your browser's login instead.

## Using your browser's login

1. Sign in to instagram.com in a supported browser. Firefox works best.
2. On the Account page, choose **Browser cookies** and select the **Browser**: Brave, Chrome, Chromium, Edge, Firefox, LibreWolf, Opera, Opera GX, Safari or Vivaldi.
3. Click **Import cookies**.

The program reads the browser's Instagram cookies, checks which account they belong to, and saves them as a session. It then offers to **switch to "Instagram login"** with that saved session. This is recommended: downloads then no longer read the browser, which can fail while the browser is open. If you choose **Keep browser cookies** instead, the cookies are imported again every time a download starts.

| Field | Meaning |
|---|---|
| **Browser** | The browser to read (`--load-cookies`) |
| **Cookie file** | Optional. The cookie database of a specific browser profile, for example a Firefox `cookies.sqlite` file (`--cookiefile`). Leave empty for the browser's default profile |
| **Import cookies** | Reads, checks and saves the session now |

- Chromium-based browsers (Chrome, Edge, Brave, Opera, Vivaldi) may need to be closed while their cookies are read.
- Recent Chrome versions encrypt cookies in a way that other programs cannot decrypt. Use Firefox or LibreWolf if Chrome does not work.
- The imported session belongs to the browser: signing out in the browser ends it.

See [Importing the login from your web browser](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Logging-in#importing-the-login-from-your-web-browser) in the InstaloaderSharp manual for the details of each browser.

## Checking the session and signing out

- **Check session** asks Instagram whether the saved session (or the browser's cookies) is still valid. If it is, you see *Signed in as @username* and your profile picture appears on the page. If not, sign in again.
- **Sign out** deletes the saved session file (after asking) and switches to **Anonymous**. You will need your password, and possibly a two-factor code, to sign in again.

Sessions end when you change your password, sign out of all devices, or Instagram ends them. A download with an expired session asks for the password again.

## Saved sessions

The **Saved sessions** list shows every session file in `%LOCALAPPDATA%\Instaloader`, newest first, including sessions created with InstaloaderSharp or Instaloader on the command line.

| Button | Does |
|---|---|
| **Use** | Switches to **Instagram login** with that account and session |
| **Delete** | Deletes the session file (after asking) |
| **Refresh** | Reloads the list |
| **Open folder** | Opens the session folder in Explorer |

Sessions saved by the GUI work with `InstaloaderSharp.exe --login YOUR-USERNAME` and with Instaloader, and the other way round.

## Several accounts

Every account has its own session file. To switch accounts, click **Use** on its saved session, or change the **Username** and sign in. Downloads always use the account shown on the Account page.

## Keeping your account safe

- The program never saves your password. Only the session is saved, as InstaloaderSharp and Instaloader do.
- A session file still gives access to your account. Keep `%LOCALAPPDATA%\Instaloader` private, and never post session files, cookies or two-factor codes, for example in bug reports.
- Use a secondary account for large downloads, and avoid using the account heavily in the app or browser while downloading.

See [Keeping your account safe](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Logging-in#keeping-your-account-safe) in the InstaloaderSharp manual.

## Sign-in problems

| Problem | Solution |
|---|---|
| The password is not accepted, although it is correct | Make sure you entered your username, not your e-mail address. If Instagram refuses anyway, use your browser's login. |
| *Checkpoint required. Point your browser to …* | Instagram wants to confirm that it is you. Open the link in a browser where you are signed in to the same account, confirm, and try again, or use your browser's login. |
| Importing browser cookies fails, or finds no cookies | Close the browser and try again, use Firefox, and check that you are signed in to instagram.com in that browser. For a non-default browser profile, choose its **Cookie file**. |
| It imports the wrong account | The browser's default profile is signed in to another account. Sign in with the right account there, or choose the right **Cookie file**. |
| The session stopped working | Sign in again, or import fresh cookies. |
| Two-factor authentication does not work | Use your browser's login. |

See also [Troubleshooting and FAQ](Troubleshooting-and-FAQ#signing-in) and [Login problems](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Logging-in#login-problems) in the InstaloaderSharp manual.
