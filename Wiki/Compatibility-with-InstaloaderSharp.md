InstaloaderSharp GUI needs `InstaloaderSharp.exe`, which does the actual downloading. This page explains which version to use, how to update it, and what to do when the two do not fit together.

## Contents

- [Which InstaloaderSharp version to use](#which-instaloadersharp-version-to-use)
- [Checking the versions](#checking-the-versions)
- [Updating InstaloaderSharp](#updating-instaloadersharp)
- [Features that need a newer InstaloaderSharp](#features-that-need-a-newer-instaloadersharp)
- [When the versions do not fit together](#when-the-versions-do-not-fit-together)
- [How compatibility is checked](#how-compatibility-is-checked)

## Which InstaloaderSharp version to use

Every InstaloaderSharp GUI release is tested with one InstaloaderSharp version, which is named at the top of the [README](https://github.com/InstaloaderSharp/InstaloaderSharpGUI#readme) and in the release notes. **Use that version.** Download it from the [InstaloaderSharp releases](https://github.com/InstaloaderSharp/InstaloaderSharp/releases), and put `InstaloaderSharp.exe` in the same folder as `InstaloaderSharpGUI.exe`.

Later InstaloaderSharp versions usually work too, but have not been tested with your GUI version. Only use a later version if you run into problems that a newer InstaloaderSharp fixes, for example after a change on Instagram's side. Older versions are not recommended: features of the GUI may not work with them (see [below](#features-that-need-a-newer-instaloadersharp)).

## Checking the versions

*Settings → About* shows both versions:

- **InstaloaderSharp GUI** *version*: the GUI.
- **InstaloaderSharp** *version*: the `InstaloaderSharp.exe` that is actually loaded, read from the file itself.

The GUI's version is also shown at the top of the sidebar, under the logo. The first line of the Activity page's output names the InstaloaderSharp version too: *InstaloaderSharp GUI ready - using the InstaloaderSharp library.*

Please include both versions in bug reports.

## Updating InstaloaderSharp

1. Close InstaloaderSharp GUI.
2. Download the new `InstaloaderSharp-<version>-win.zip` from the [InstaloaderSharp releases](https://github.com/InstaloaderSharp/InstaloaderSharp/releases).
3. Replace `InstaloaderSharp.exe` next to `InstaloaderSharpGUI.exe` with the new one.
4. Start the GUI and check the version under *Settings → About*.

Your settings, sessions and downloads are not affected. To go back, put the previous `InstaloaderSharp.exe` back.

## Features that need a newer InstaloaderSharp

Some features depend on fixes in InstaloaderSharp:

| Feature | Needs InstaloaderSharp |
|---|---|
| **Search** on the Look up page (Instagram retired the website search that older versions use; they fail with *404 Not Found*) | 1.0.4 or later |
| Downloading **Reels** of some profiles; finding age-restricted and other restricted profiles when signed in | 1.0.3 or later |
| Downloading posts of profiles when signed in, for accounts affected by an Instagram change in October 2026 (older versions download no posts and report a redirect to Instagram's home page) | 1.0.2 or later |
| Paths longer than 260 characters, if enabled in Windows; names with formatted dates such as `{date_utc:%c}` | 1.0.1 or later |

See the [InstaloaderSharp releases](https://github.com/InstaloaderSharp/InstaloaderSharp/releases) for everything that changed in each version.

## When the versions do not fit together

| What you see | Cause | Solution |
|---|---|---|
| *InstaloaderSharp.exe was not found.* when starting | `InstaloaderSharp.exe` is not in the same folder as `InstaloaderSharpGUI.exe` | Put it there; see [Getting started](Getting-started#getting-the-program) |
| **Something went wrong**, or a download that ends with **Failed** right away, mentioning `MissingMethodException`, `MissingFieldException`, `TypeLoadException` or `FileLoadException` | The InstaloaderSharp version is too old or too new for this GUI version: something the GUI uses has changed | Use the InstaloaderSharp version the GUI was tested with |
| *The command line could not be imported: This version of InstaloaderSharp does not expose its argument parser.* | This InstaloaderSharp version changed its internal argument parser | Use the tested version; downloads still work, only importing command lines is affected |
| A feature from the table above does not work | The InstaloaderSharp version is older than required | Update InstaloaderSharp |

If a problem persists with the tested versions, see [Troubleshooting and FAQ](Troubleshooting-and-FAQ#is-it-a-gui-problem) to find out whether it comes from the GUI or from InstaloaderSharp.

## How compatibility is checked

`InstaloaderSharp.exe` is loaded as a library, and Windows does not require a specific version of it. A newer `InstaloaderSharp.exe` works as long as the parts of it that the GUI uses have not changed.

For every new InstaloaderSharp version, the GUI is checked as described in [Building and contributing](Building-and-contributing#testing-with-a-new-instaloadersharp-version): the changes are reviewed for anything the GUI uses, the existing GUI build is run with the new version, the GUI is rebuilt against it, and all unit tests are run. New command-line options are added to the GUI when they appear. See [How it works](How-it-works) for what the GUI uses of InstaloaderSharp.
