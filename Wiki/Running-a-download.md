This page explains what happens when you start a download, what the **Activity** page shows, and how to stop and continue a download.

## Contents

- [Starting a download](#starting-a-download)
- [Before the download starts](#before-the-download-starts)
- [The Activity page](#the-activity-page)
- [The output](#the-output)
- [Recent files](#recent-files)
- [Questions during a download](#questions-during-a-download)
- [When the download ends](#when-the-download-ends)
- [Stopping and resuming](#stopping-and-resuming)
- [Closing the program during a download](#closing-the-program-during-a-download)
- [Errors during a download](#errors-during-a-download)

## Starting a download

Click **Start download** in the status bar, or press **Ctrl+Enter** or **F5**, from any page. The program checks the options, saves your settings, and switches to the **Activity** page.

One download runs at a time. While it runs, **Start download** is replaced by **Stop**, and pressing **Ctrl+Enter** or **F5** shows the Activity page. You can keep using the other pages meanwhile, for example to look up a profile, but changes to targets and options only apply to the next download.

Every download does exactly what the matching InstaloaderSharp command does. The command is shown at the top of the output ("Equivalent command: …") and under *Settings → Command line*.

## Before the download starts

The options are checked the same way InstaloaderSharp's command line checks them.

**Problems that prevent starting** are listed in a dialog titled **Almost there**, and the page where they can be fixed is opened (the Account page for sign-in problems, otherwise the Download page):

| Message | Fix |
|---|---|
| *Add at least one target (a profile, #hashtag, post, location...) to download.* | Add a target ([Adding targets](Adding-targets)) |
| *Choose a folder to save the downloads to.* | Set the **Download folder** |
| *Enter your Instagram username on the Account page, or switch to anonymous downloading.* | See [Signing in](Signing-in) |
| *Choose the browser to import cookies from.* | Choose a **Browser** on the Account page |
| *"Pictures" off and "Fast update" cannot be used together.* | See [Combinations that are not allowed](Choosing-what-to-download#combinations-that-are-not-allowed) |
| *Captions (txt files) are turned off but a post or story item text template is set; that contradicts.* | Turn **Captions** on, or empty the templates under *Settings → Caption text files* |
| *Post filter: …* or *Story item filter: …* | Fix the filter ([Filtering posts and stories](Filtering-posts-and-stories)) |
| *Images of multi-image posts: …* | See [Images of multi-image posts](Choosing-what-to-download#images-of-multi-image-posts) |
| *Maximum count must be a whole number.* and other number errors | Fix the value |
| *JSON target not found: …* | Remove the target, or restore the file |

The same problems are also shown in the status bar and under the target list while you set up the download.

**Warnings** are listed in a dialog titled **Before you start**, where you can choose **Start download** anyway or **Go back**. For example: *Login is required to download stories - stories will be skipped.* See [Warnings about targets](Adding-targets#warnings-about-targets).

## The Activity page

| Part | Shows |
|---|---|
| **Items** | Posts and story items processed so far |
| **Already saved** | Items whose files existed already and were skipped |
| **Warnings**, **Errors** | Problems so far; the tiles turn amber or red when they are not zero |
| **Elapsed** | The running time |
| Current step | What is being done, for example "Retrieving posts from profile natgeo · profile 2 of 5" |
| Progress bars | A thin blue bar for the profiles when several are downloaded, and a bar for the items of the current target. The bar moves when the total is known and is animated when it is not, for example for hashtags and the feed |
| Current item | For example "12 of 240" and the files being saved |

The status bar and the taskbar button show the progress too, so you can follow the download from any page.

## The output

Under **Output** you see the complete output of InstaloaderSharp, exactly as the console would print it:

- normal output in white,
- warnings in amber,
- errors in red,
- messages from the GUI itself in blue, such as "▶ Starting download into …" and the equivalent command.

What each line means is explained in [Getting started](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Getting-started#your-first-download) in the InstaloaderSharp manual.

| Control | Does |
|---|---|
| **Problems only** | Shows only warnings and errors |
| **Auto-scroll** | Keeps the newest output in view. Turn it off to read older output while downloading |
| **Open folder** | Opens the download folder in Explorer |
| **Copy** | Copies the whole output (also the lines hidden by **Problems only**) |
| **Save…** | Saves the whole output as a text file |
| **Clear** | Empties the output |

The output is kept for the whole run of the program, across downloads. Very long output is shortened at the beginning, to keep the program fast.

## Recent files

When the window is wide enough, **Recent files** on the right shows the pictures and videos just saved, newest first, with thumbnails for pictures. Double-click a file to open it.

## Questions during a download

If the program needs something from you during a download, it asks in a dialog:

- your **password**, when you sign in with **Instagram login** and there is no valid session;
- a **two-factor code**, when your account uses two-factor authentication.

**Cancel** stops the download with *Login failed: Login cancelled.* See [Signing in](Signing-in).

## When the download ends

The status bar and the Activity page show the result:

| Result | Meaning | Exit code of the same command |
|---|---|---|
| **Download complete** | Everything was downloaded | 0 |
| **Completed with N problem(s)** | Finished, but errors or warnings occurred; see the output | 1 |
| **Stopped. Start again to resume where it left off.** | You clicked **Stop** | 5 |
| **Download aborted** | Instagram stopped the download, for example because of **Abort on HTTP status codes**, because it requires verification, or because it keeps redirecting a suspended or restricted account (see [Troubleshooting and FAQ](Troubleshooting-and-FAQ#rate-limits-and-blocks)) | 4 |
| **Login failed** | Signing in did not work; a dialog explains why | 3 |
| **Could not start** | The options were invalid; a dialog explains why | 2 |
| **Failed** | An unexpected error; see the output | 99 |

The exit codes are those of `InstaloaderSharp.exe`, see [Exit codes](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Command-line-options#exit-codes).

If the window is in the background when the download ends, its taskbar button flashes and a sound plays. This can be turned off with **Notify when finished** (*Settings → Behaviour*). While a download runs, the computer does not go to sleep, unless **Keep the computer awake** is turned off.

## Stopping and resuming

**Stop** (or **Shift+F5**) works like pressing Ctrl+C in InstaloaderSharp's console. The program finishes or cancels the current request, saves where it was, and stops. The status bar shows *Stopping…* meanwhile.

When a download of many posts stops early, whether because you stopped it, because of a connection error or because Instagram refused further requests, the position in the list of posts is saved as a **resume file** in the target's folder, for example `instagram\iterator_s8YiB9OL.json.xz`. Starting the same download again continues from that position, and the file is deleted when the download completes. Files that were being saved at that moment are not left half-written.

- Resume files expire after 29 days; then the download starts from the beginning.
- A download can only be resumed with the same account it was started with (or anonymously both times).
- Resuming can be turned off, and the file names changed, under *Settings → Resuming interrupted downloads*; see the [Settings reference](Settings-reference#resuming-interrupted-downloads).

See [Interrupting and resuming](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Interrupting-and-resuming) in the InstaloaderSharp manual.

## Closing the program during a download

If you close the window while a download runs, the program asks whether to stop it. **Stop and exit** stops the download cleanly, with resume information saved, and closes the program afterwards. **Keep running** cancels closing.

## Errors during a download

Errors with single items, such as a post that cannot be downloaded, do not stop the download. They are shown in red when they occur, counted in the **Errors** tile, and repeated under "Errors or warnings occurred:" at the end.

To find out whether a problem comes from the GUI or from InstaloaderSharp and Instagram, see [Troubleshooting and FAQ](Troubleshooting-and-FAQ#is-it-a-gui-problem).
