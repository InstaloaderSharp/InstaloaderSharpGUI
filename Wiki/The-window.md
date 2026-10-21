This page explains the parts of the program's window and how options are presented.

## Contents

- [Overview](#overview)
- [The sidebar](#the-sidebar)
- [The pages](#the-pages)
- [The status bar](#the-status-bar)
- [The taskbar button](#the-taskbar-button)
- [How options are shown](#how-options-are-shown)
- [Dialogs](#dialogs)
- [Display scaling and window size](#display-scaling-and-window-size)

## Overview

```
┌──────────────┬──────────────────────────────────────────────────────┐
│ Logo         │  Page title                                          │
│              │  Short description of the page                       │
│ Download     │                                                      │
│ Activity     │  Sections with options ...                           │
│ Look up      │                                                      │
│ Account      │                                                      │
│ Settings     │                                                      │
│              ├──────────────────────────────────────────────────────┤
│ Account chip │  Status bar: state · details   [progress] [Start]    │
└──────────────┴──────────────────────────────────────────────────────┘
```

The window has a dark theme. Text has an accessible contrast, and the control that has the keyboard focus is outlined.

## The sidebar

The sidebar on the left switches between the five pages. Click a page, use **Ctrl+1** to **Ctrl+5**, or select the sidebar with **Tab** and use the **↑** and **↓** keys.

While a download runs, **Activity** shows a **Live** badge. When a download ends while you are on another page, the badge shows **Done**, or the number of errors and warnings, until you open the Activity page.

At the top of the sidebar are the logo and the GUI's version. At the bottom is the **account chip**: it shows how the program accesses Instagram:

| The chip shows | Meaning |
|---|---|
| **Anonymous**, *Not signed in* | Downloads are anonymous |
| **@username**, *Instagram login* | You sign in with your Instagram account; the session has not been checked yet in this run |
| **@username**, *Signed in* (green dot) | The session was verified, by **Sign in**, **Check session** or a successful download |
| **Browser cookies**, *Cookies from Firefox* | The login is imported from a browser when a download starts |

Click the chip to open the **Account** page.

## The pages

| Page | What it is for | Manual page |
|---|---|---|
| **Download** | What to download (targets), what to include, which posts and where to save | [Adding targets](Adding-targets), [Choosing what to download](Choosing-what-to-download), [Keeping an archive up to date](Keeping-an-archive-up-to-date), [Filtering posts and stories](Filtering-posts-and-stories) |
| **Activity** | Progress, counters, recent files and the complete output of the current download | [Running a download](Running-a-download) |
| **Look up** | Previewing profiles and hashtags, and searching Instagram | [Looking up and searching](Looking-up-and-searching) |
| **Account** | Sign-in method, checking the session, saved sessions | [Signing in](Signing-in) |
| **Settings** | Advanced options: naming, caption files, resuming, connection, behaviour, the command line, about | [Settings reference](Settings-reference), [Command line and the GUI](Command-line-and-the-GUI) |

Pages scroll when they are taller than the window. Changes take effect immediately; there is no Save or Apply button. Options are remembered between runs (see [Where data is stored](Where-data-is-stored)).

## The status bar

The status bar at the bottom is always visible, whatever page you are on.

When no download runs, it shows whether you can start, and a summary of what will happen:

| Status | Meaning |
|---|---|
| **Ready to download** | Everything is set; the line below shows the number of targets, the sign-in method and the download folder |
| **Add something to download** | The target list is empty |
| A problem, in amber | Something must be fixed first, for example an invalid filter; see [Running a download](Running-a-download#before-the-download-starts) |

The **Start download** button is on the right (**Ctrl+Enter** or **F5**).

While a download runs, the status bar shows the current step ("Retrieving posts from profile natgeo"), the progress ("Profile 2 of 5 · 12 of 240") and the number of errors so far, with a progress bar and the **Stop** button (**Shift+F5**). After the download, it shows the result, such as **Download complete** or **Completed with 2 problem(s)**.

## The taskbar button

The program's taskbar button shows the download's progress, like a file copy in Explorer: green while it runs, red when the download ended with errors, yellow while it is stopping.

## How options are shown

Most options are rows in a card:

- The **title** says what the option does, and the **description** underneath explains it. Where an option corresponds to a command-line option of InstaloaderSharp, the description names it, for example `(--fast-update)`.
- A **Requires login** badge marks options that only work when you are signed in.
- **Switches** show their state as **On** or **Off**. Clicking anywhere on the row flips the switch.
- Options that depend on another option are greyed out when they do not apply, for example **Compress JSON** when **Metadata JSON** is off.
- Text fields show a grey hint when empty, usually the default value. A field with an invalid value gets a red border, and its row explains why.

## Dialogs

Messages and questions appear in dark dialogs. The button on the right is the main answer (**Enter**); **Esc** cancels. When the program needs your password or a two-factor code, it asks in such a dialog, never in a console window.

If something unexpected goes wrong, a dialog titled **Something went wrong** shows the error and its details. Please include that text when you report a bug (see [Building and contributing](Building-and-contributing#reporting-bugs)).

## Display scaling and window size

The program adapts to Windows' display scaling (for example 125 % or 150 %). The window's size and position, and whether it was maximized, are remembered. On the Activity page, the list of recent files is shown when the window is wide enough.
