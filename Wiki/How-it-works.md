This page explains how InstaloaderSharp GUI drives the InstaloaderSharp library. It is meant for contributors and for anyone who wants to know why a download in the GUI behaves exactly like the matching command. You don't need it to use the program.

## Contents

- [Overview](#overview)
- [Starting up](#starting-up)
- [From options to a download](#from-options-to-a-download)
- [Signing in](#signing-in)
- [Output and progress](#output-and-progress)
- [Questions from the library](#questions-from-the-library)
- [Stopping](#stopping)
- [Checking options](#checking-options)
- [The command line](#the-command-line)
- [The Look up page](#the-look-up-page)
- [What the GUI uses of InstaloaderSharp](#what-the-gui-uses-of-instaloadersharp)

## Overview

```
┌──────────────────── InstaloaderSharpGUI.exe ────────────────────┐
│  Pages ──► JobOptions ──► DownloadEngine (background thread)    │
│    ▲                          │                                 │
│    │                          ▼                                 │
│  Activity page ◄── ConsoleRouter ◄── Console.Out / Console.Error│
│                          │                                      │
│  Dialogs ◄───────────────┘◄── Console.In                        │
└──────────────────────────────┬──────────────────────────────────┘
                               │ library calls
┌──────────────────────────────▼──────────────────────────────────┐
│  InstaloaderSharp.exe: Instaloader, Program.MainImpl, Profile,  │
│  Hashtag, TopSearchResults, LatestStamps, ...                   │
└─────────────────────────────────────────────────────────────────┘
```

`InstaloaderSharp.exe` is referenced as a .NET library. The GUI never starts it as a separate process; it calls the same code that the console program runs after reading its command line. That is why results are identical to the command line.

## Starting up

`Program.Main` checks that `InstaloaderSharp.exe` is next to `InstaloaderSharpGUI.exe` before any InstaloaderSharp type is used, and shows a message if it is missing. It then installs the console router (see [Output and progress](#output-and-progress)), loads `settings.ini` and opens the main window.

## From options to a download

All options of the pages are kept in one `JobOptions` object, which is saved to `settings.ini`. When a download starts, a copy of it is handed to the `DownloadEngine`, which runs on a background thread so that the window stays responsive:

1. The working directory is set to the download folder (InstaloaderSharp saves relative to it, as on the console) and restored afterwards. If **Keep the computer awake** is on, Windows is told not to sleep.
2. `Library.CreateLoader` creates an `Instaloader` with the options, exactly as `Program.MainWithArgs` maps the command-line options to its constructor.
3. `Library.SignIn` signs in, if a sign-in method is chosen (see below).
4. `Program.MainImpl` is called with the loader, the targets and the remaining options. It does the actual work: it resolves targets, downloads hashtags, locations, the feed and single posts, collects profiles and calls `DownloadProfiles`.
5. The loader is closed, which prints the "Errors or warnings occurred" summary, and the result is mapped to InstaloaderSharp's exit codes in the same way as `MainWithArgs`.

The checks that `MainWithArgs` does before creating the loader, such as contradicting options and invalid counts, are repeated by the engine with the same messages.

## Signing in

`Library.SignIn` follows InstaloaderSharp's handling of `--login` and `--load-cookies`:

- **Instagram login**: `LoadSessionFromFile`, then `TestLogin`; if there is no valid session, `Login` with the password, and `TwoFactorLogin` when Instagram asks for a code. Passwords and codes are asked for in dialogs instead of on the console.
- **Browser cookies**: `Program.ImportSession`, which reads the browser's cookies and checks them.

After a download, `MainImpl` saves the session as on the console. **Sign in** and **Import cookies** on the Account page do the same steps immediately and call `SaveSessionToFile`.

## Output and progress

InstaloaderSharp reports everything through `Console.Out` and `Console.Error`. At start-up, the GUI replaces both with writers of its `ConsoleRouter`, which queue the text. A timer on the UI thread takes the queued text ten times a second and:

- appends it to the output on the Activity page, coloured by stream (warnings are error lines that start with "Warning");
- feeds it to the `ActivityTracker`, which reads the progress from InstaloaderSharp's messages: `[  12/240] ` before each item, `[2/5] Downloading profile …`, `Retrieving … from profile …`, `… exists`, and the error lines.

The counters, progress bars, status bar and taskbar progress are drawn from the tracker.

## Questions from the library

`Console.In` is replaced too. If InstaloaderSharp ever reads a line from the console, for example to ask for a password, the router shows a dialog with the text the library printed before reading as the question. Cancelling acts like Ctrl+C.

## Stopping

**Stop** calls `Interrupt.Raise()`, which is what Ctrl+C does in InstaloaderSharp's console. At the next interruption point, the library throws `KeyboardInterruptException`, saves resume information, and the download ends with *Stopped*.

## Checking options

`JobOptions.Validate` repeats the checks of InstaloaderSharp's command line, with its messages, so that problems are shown before starting:

- filters are compiled with InstaloaderSharp's own parser (`Program.FilterstrToFilterfunc`);
- the list of filter attributes is read from the public properties of InstaloaderSharp's `Post` and `StoryItem` classes;
- **Abort on HTTP status codes** is parsed with `Program.HttpStatusCodeList`;
- **Images of multi-image posts** is checked the way the `Instaloader` constructor parses it.

## The command line

- `JobOptions.ToArgs` turns the options into InstaloaderSharp's command-line arguments, for the equivalent command and argument files.
- Imported command lines are split with Windows' own rules (`CommandLineToArgvW`) and parsed with InstaloaderSharp's argument parser (`Program.CreateParser`), so they are understood exactly as on the console. `JobOptions.ApplyParsedArgs` maps the result to the options as `MainWithArgs` does.

Unit tests check that every option survives the round trip from the GUI to arguments, through the parser, and back.

## The Look up page

| Feature | InstaloaderSharp |
|---|---|
| Profile preview | `Profile.FromUsername` and its properties |
| Hashtag preview | `Hashtag.FromName` |
| Story badge | `Instaloader.GetStories` when signed in (the same request a download uses), `Profile.HasPublicStory` when not |
| Search | `TopSearchResults`: `GetProfiles`, `GetHashtags`, `GetLocations` |
| Pictures | `InstaloaderContext.GetRaw` through the same connection |

Look-ups use a separate loader that loads the saved session (or imports the browser cookies) and never asks for input.

## What the GUI uses of InstaloaderSharp

When InstaloaderSharp changes any of these, the GUI must be checked (see [Compatibility with InstaloaderSharp](Compatibility-with-InstaloaderSharp)):

| Area | Used |
|---|---|
| Downloading | The `Instaloader` constructor and its properties, `Close`, `HasStoredErrors`; `Program.MainImpl` and `Program.ExitCode` |
| Signing in | `LoadSessionFromFile`, `SaveSessionToFile`, `TestLogin`, `Login`, `TwoFactorLogin`, `Program.ImportSession`, `InstaloaderContext.IsLoggedIn`, `Username`, `Log`, `Error` |
| Exceptions | `InstaloaderException`, `InvalidArgumentException`, `LoginException`, `TwoFactorAuthRequiredException`, `BadCredentialsException`, `LoginRequiredException`, `ProfileNotExistsException`, `KeyboardInterruptException` |
| Checking options | `Program.FilterstrToFilterfunc`, `Program.HttpStatusCodeList`, the properties of `Post` and `StoryItem` |
| Command line | `Program.CreateParser` (internal; used through reflection, so only importing is affected if it changes) and `Cli.ArgParser.Parse` |
| Defaults and files | `Instaloader.GetDefaultSessionFilename`, `Instaloader.GetDefaultStampsFilename`, `InstaloaderContext.DefaultUserAgent` |
| Look up | `Profile`, `Hashtag`, `TopSearchResults`, `PostLocation`, `Instaloader.GetStories`, `Story.Itemcount`, `InstaloaderContext.GetRaw` |
| Stopping | `Util.Interrupt.Raise`, `Interrupt.Check` |
| Version | The assembly version of `InstaloaderSharp.exe` |

The library's API is documented in [Library use](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Library-use) in the InstaloaderSharp manual.
