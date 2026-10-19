# Contributing to InstaloaderSharp GUI

Thank you for your interest in InstaloaderSharp GUI! Everything about the project, including bug reports,
feature requests, questions and code contributions, is handled through the GitHub repository:

**https://github.com/InstaloaderSharp/InstaloaderSharpGUI**

InstaloaderSharp GUI is the graphical front end of [InstaloaderSharp](https://github.com/InstaloaderSharp/InstaloaderSharp),
which does the actual downloading. Many problems that show up in the GUI come from InstaloaderSharp or from
Instagram. Please read [Is it a GUI problem?](#is-it-a-gui-problem) before reporting, so that your report goes to
the right project.

* [Is it a GUI problem?](#is-it-a-gui-problem)
* [Reporting a bug](#reporting-a-bug)
* [Reporting a security issue](#reporting-a-security-issue)
* [Suggesting a feature](#suggesting-a-feature)
* [Asking a question](#asking-a-question)
* [Contributing code](#contributing-code)
* [Project guidelines](#project-guidelines)
* [Testing your changes](#testing-your-changes)
* [Submitting a pull request](#submitting-a-pull-request)
* [License](#license)

## Is it a GUI problem?

The GUI does not download anything itself. It passes your options to the InstaloaderSharp library, exactly as
`InstaloaderSharp.exe` would receive them on the command line. That makes it easy to find out where a problem
comes from:

1. Go to **Settings → Command line** and press **Copy**. You can also find the command at the top of the output
   on the **Activity** page ("Equivalent command: …").
2. Open a command prompt in your download folder and run that command with the `InstaloaderSharp.exe` that is
   next to `InstaloaderSharpGUI.exe`. If you were signed in with a password, the session file is shared, so
   `--login` works without the password. With browser cookies, the browser is read again.
3. Compare the results:

| `InstaloaderSharp.exe` shows… | Then the problem is probably in… | Report it to |
| --- | --- | --- |
| the same error or the same wrong result | InstaloaderSharp, or a change on Instagram's side | [InstaloaderSharp issues](https://github.com/InstaloaderSharp/InstaloaderSharp/issues) |
| no problem | the GUI | [InstaloaderSharp GUI issues](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues) |

As a rule of thumb:

* **Usually InstaloaderSharp:** errors in the output while downloading (`KeyError`, `JSON Query`, "Login
  required", `400 Bad Request`, …), missing or wrong files, wrong file names or dates, profiles or posts that
  cannot be found, sign-in or two-factor failures, browser cookies that cannot be read, filters that behave
  differently than documented.
* **Usually the GUI:** the window, layout, colours or keyboard navigation; an option in the GUI that has no
  effect or the wrong effect (the copied command does not match what you chose); wrong validation messages;
  problems with importing command lines or argument files; progress, counters or the log on the Activity page;
  the Look up page; settings that are not remembered; crashes of the program itself ("Something went wrong").

If you are not sure, report it here and say what you tried; it can be moved.

## Reporting a bug

Report bugs in the [issue tracker](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues).

1. **Search first.** Check the [open and closed issues](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues?q=is%3Aissue)
   for the same problem. If you find one, add any new details as a comment instead of opening a duplicate.
2. **Make sure you use the latest versions** of both InstaloaderSharp GUI and InstaloaderSharp. Many problems are
   caused by changes on Instagram's side and may already be fixed in a newer InstaloaderSharp.
3. **Check whether `InstaloaderSharp.exe` shows the same problem** (see [above](#is-it-a-gui-problem)).
4. **Open a new issue** with a short, descriptive title, e.g. "Post filter is not saved when switching pages"
   rather than "It doesn't work".

Please include:

* The InstaloaderSharp GUI and InstaloaderSharp versions (both are shown under **Settings → About**) and your
  Windows version.
* What you did, step by step: the page, the options you changed and the buttons you pressed.
* The equivalent command line (**Settings → Command line → Copy**).
* The complete output from the **Activity** page (**Copy** or **Save…**), including the "Errors or warnings
  occurred" summary at the end, and the result shown in the status bar (e.g. "Completed with 2 problem(s)").
* What you expected to happen and what happened instead. For display problems, a screenshot helps a lot, and
  please mention your display scaling (e.g. 150 %).
* How you were signed in (anonymous, Instagram login, or browser cookies with which browser).
* Whether the problem is reproducible, and whether `InstaloaderSharp.exe` with the same command shows it too.
* For a crash ("Something went wrong"), the full message including the stack trace.

> **Never post passwords, session files, cookie values or two-factor codes.** Session files
> (`%LOCALAPPDATA%\Instaloader\session-*`) and browser cookies give full access to your Instagram account.
> The GUI's own settings file (`%LOCALAPPDATA%\InstaloaderSharpGUI\settings.ini`) contains no password, but it
> does contain your username, your targets and your folders. Before posting output, a command line or settings,
> remove or replace anything private, such as the usernames of private accounts. A public profile or post that
> shows the problem is the most helpful example.

### Rate limits and blocked accounts

Messages like *"Please wait a few minutes before you try again"*, HTTP 429 or 401, *"Checkpoint required"* or
redirects to `/terms/unblock/` come from Instagram restricting the account or IP address, not from a bug in the
GUI. Before reporting them, see "Tips and troubleshooting" in the [README](README.md#tips-and-troubleshooting).
Waiting, using the account normally in a browser for a while, or importing fresh browser cookies on the
**Account** page usually helps.

## Reporting a security issue

If you find a security problem, for example in how passwords, session files or cookies are handled, **do not
open a public issue.** Report it privately through the repository's
[Security tab](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/security) ("Report a vulnerability"), so
that it can be fixed before it is disclosed. If the problem is in InstaloaderSharp itself, report it there in the
same way.

## Suggesting a feature

Feature requests are welcome as [issues](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues/new).
Describe what you would like to do and why (the use case), not only the proposed solution.

* **Features of the user interface** (a new page, a better way to add targets, a setting that should be easier to
  find, …) belong here.
* **New download features or options** must exist in InstaloaderSharp first, because the GUI only offers what the
  library can do. Request them in the [InstaloaderSharp repository](https://github.com/InstaloaderSharp/InstaloaderSharp/issues);
  once they are released, the GUI will offer them too.

Please keep in mind that InstaloaderSharp and the GUI only download data; features that act on Instagram
(liking, following, posting, …) are out of scope.

## Asking a question

For questions about using the GUI, first check the [README](README.md), which describes every page and option.
Every option also has a description in the program. If that does not answer your question, open an
[issue](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues) and describe what you are trying to do.

## Contributing code

Bug fixes, usability and accessibility improvements, documentation improvements and new features are all
welcome. For larger changes, please open an issue first to discuss the approach, so that no work is wasted.

### Setting up

1. Fork the repository on GitHub and clone your fork:

   ```
   git clone https://github.com/YOUR-USERNAME/InstaloaderSharpGUI.git
   ```

2. Create a branch for your change from `main`, with a descriptive name:

   ```
   git checkout -b fix-filter-not-saved
   ```

3. Put a compiled `InstaloaderSharp.exe` into the `InstaloaderSharp` folder of the repository (next to
   `InstaloaderSharpGUI.sln`). It is not part of the repository. Use the version the GUI currently targets (see
   the [README](README.md)): download it from the
   [InstaloaderSharp releases](https://github.com/InstaloaderSharp/InstaloaderSharp/releases), or build it from
   the [InstaloaderSharp source](https://github.com/InstaloaderSharp/InstaloaderSharp).

4. Build the solution, either in Visual Studio (2015 or later) or with:

   ```
   "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" InstaloaderSharpGUI.sln /p:Configuration=Release
   ```

   You need the .NET Framework 4.8.1 developer pack (reference assemblies). The build must finish without
   errors and warnings. The unit test project needs Visual Studio's MSTest framework (see
   [Unit tests](#unit-tests)). Without Visual Studio, build only the program with
   `InstaloaderSharpGUI\InstaloaderSharpGUI.csproj` instead of the solution.

### Repository layout

| Path | Contents |
| --- | --- |
| `InstaloaderSharpGUI/Program.cs`, `MainForm.cs` | Start-up (checks for `InstaloaderSharp.exe`), the main window: sidebar, status bar, keyboard shortcuts, starting and stopping downloads |
| `InstaloaderSharpGUI/Core/JobOptions.cs` | All options, their validation, the mapping to command line arguments (and back) and saving them |
| `InstaloaderSharpGUI/Core/DownloadEngine.cs` | Runs a download on a background thread through the library (`Program.MainImpl`), and reads progress from its output (`ActivityTracker`) |
| `InstaloaderSharpGUI/Core/Library.cs` | Helpers around the library: creating an `Instaloader`, signing in, sessions, command line import, filter attributes; `settings.ini` |
| `InstaloaderSharpGUI/Core/ConsoleRouter.cs` | Captures the library's console output for the Activity page and turns its console input into dialogs |
| `InstaloaderSharpGUI/Core/Targets.cs` | Understanding what the user types or pastes: names, `#hashtags`, links, … |
| `InstaloaderSharpGUI/Pages/` | The pages: Download, Activity, Look up, Account, Settings, and `PageBase` with the helpers that bind controls to options |
| `InstaloaderSharpGUI/UI/` | The dark theme (`Theme.cs`) and the custom controls: buttons, switches, inputs, cards, setting rows, navigation, dialogs, log |
| `InstaloaderSharpGUI.Tests/` | Unit tests |

## Project guidelines

* **C# 6 and .NET Framework 4.8.1.** The code must compile with the C# 6 compiler of Visual Studio 2015 /
  MSBuild 14. Do not use newer language features such as tuples, `out var`, local functions, pattern
  matching (`is T x`), `switch` expressions, `??=` or `default` literals.
* **No third-party dependencies.** The GUI uses no NuGet packages. Use the .NET Framework (Windows Forms) and
  Windows APIs only. The only other file it needs at runtime is `InstaloaderSharp.exe`.
* **Use the library, do not duplicate it.** Downloading, signing in, parsing command lines, filters, file names
  and so on are done by InstaloaderSharp. Call it instead of reimplementing its logic, and never start
  `InstaloaderSharp.exe` as a separate process. If something cannot be done through the library, change
  InstaloaderSharp first.
* **Keep every command line option available.** The GUI offers everything the command line can do. When
  InstaloaderSharp gets a new option, add it to `JobOptions` (including `ToArgs`, `ApplyParsedArgs` and
  validation), give it a control with a description that names the command line option, and add it to the
  README, including its "Option reference" table. An imported command line must produce the same command again.
* **Results must match the command line.** A download from the GUI must behave exactly like the equivalent
  command shown under *Settings → Command line*. Validation should match InstaloaderSharp's checks and its
  messages.
* **Never store passwords, and do not contact Instagram unasked.** Passwords are kept in memory only. Requests to
  Instagram happen only when the user starts something (a download, *Sign in*, *Check session*, *Look up*,
  *Search*), not when the program starts or when an option changes.
* **Keep the window responsive.** Library calls that may contact Instagram run on a background thread
  (`PageBase.RunBackground`, `DownloadEngine`); results are shown on the UI thread.
* **Follow the design of the user interface.** Use the colours and fonts from `Theme` (and check the contrast of
  new colours: at least 4.5:1 for text), scale every pixel size with `Theme.S(...)`, and reuse the existing
  controls (`SettingRow`, `ToggleSwitch`, `DarkTextBox`, `FlatButton`, …). Every option gets a short description,
  everything must work with the keyboard, and new controls need an accessible name. Prefer clear wording without
  technical terms in the interface; put the technical details in the description and the README.
* **Add new source files to the project file.** `InstaloaderSharpGUI.csproj` and
  `InstaloaderSharpGUI.Tests.csproj` list their files explicitly. Visual Studio does this automatically; if you
  use another editor, add a `<Compile Include="..." />` entry. The GUI's forms are created in code, not with the
  Windows Forms designer.
* **Follow the existing code style.** Match the surrounding code: four-space indentation, braces on their own
  lines, PascalCase names, and XML documentation comments on classes and important members. Explain *why* in
  comments where the code works around Windows Forms or library behaviour.
* **Do not commit downloads or personal data.** Never commit session files, cookies, `settings.ini`, downloaded
  media, metadata of private accounts, credentials or `InstaloaderSharp.exe`, also not in tests or examples.

## Testing your changes

### Unit tests

The solution contains a unit test project, `InstaloaderSharpGUI.Tests` (MSTest). The tests run offline. They make
no requests to Instagram and need no account. They do not repeat what the
[InstaloaderSharp tests](https://github.com/InstaloaderSharp/InstaloaderSharp/tree/main/InstaloaderSharp.Tests)
already cover, but test the GUI's own code and how it uses the library:

* Understanding typed and pasted targets, including all kinds of Instagram links (`TargetsTests`).
* Option validation, the mapping of every option to command line arguments, and saving options
  (`JobOptionsTests`, `SettingsStoreTests`).
* Importing command lines and argument files through InstaloaderSharp's own parser, and that every option
  survives a round trip (`CommandLineTests`).
* Creating an `Instaloader` from the options, sessions, signing in without a session, and filter attributes
  (`LibraryTests`).
* Capturing console output and answering console input with prompts (`ConsoleRouterTests`), and reading progress
  from the output (`ActivityTrackerTests`).
* Downloads that end before any request to Instagram: invalid targets and options, cancelled sign-in
  (`DownloadEngineTests`).
* The custom controls, the layout, colour contrast, and the binding of the pages to the options (`ControlTests`,
  `PageTests`).

The test project needs Visual Studio's MSTest framework, which is included with Visual Studio 2015 or later.
To run the tests, build the solution in Visual Studio, open **Test Explorer** from the **Test** menu and choose
**Run All**. On the command line, run:

```
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" InstaloaderSharpGUI.sln /p:Configuration=Debug
"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" InstaloaderSharpGUI.Tests\bin\Debug\InstaloaderSharpGUI.Tests.dll
```

Use the `vstest.console.exe` of your Visual Studio version (for newer versions, it is under
`Common7\IDE\Extensions\TestPlatform`). The test runner does not load tests from a network drive ("Operation is
not supported", `0x80131515`); in that case, clone the repository to a local drive or copy the
`InstaloaderSharpGUI.Tests\bin\Debug` folder to one and run the tests there.

All tests must pass before you submit a pull request. When you fix a bug or add a feature, add a test for it where
possible. When writing tests:

* Never use the real user data. Use `TempDirectory` for files, `IsolatedAppData` to point `%LOCALAPPDATA%` (where
  InstaloaderSharp keeps sessions and latest stamps) to a temporary folder, and the path overloads of
  `SettingsStore` instead of the real `settings.ini`.
* Use `RouterScope` when the code writes to the console, and `Sta.Run` for tests that create Windows Forms
  controls.
* Do not test what InstaloaderSharp's own tests cover (file formats, the expression evaluator, the HTTP client,
  …); test how the GUI uses it.
* Do not contact Instagram. Test against Instagram by hand instead (see below).

Add new test files to `InstaloaderSharpGUI.Tests/InstaloaderSharpGUI.Tests.csproj` like source files.

### Testing with Instagram

Unit tests cannot detect changes on Instagram's side or how the window looks, so please also test your change by
hand and describe how in the pull request:

* Build both Debug and Release without warnings, and run the unit tests.
* Try the pages and options affected by your change, with the mouse and with the keyboard. For visual changes,
  also check another display scaling (e.g. 150 %), a small window and a maximised one.
* Download something small that uses your change: a single public post, a public profile with
  **Maximum number of posts** set to a few, or **Posts** turned off. For features that need it, sign in.
* Compare with InstaloaderSharp: copy the equivalent command (**Settings → Command line → Copy**), run it with
  `InstaloaderSharp.exe` in an empty folder, and check that both produce the same files.
* If your change touches sign-in or sessions, check that a session saved by the GUI works with
  `InstaloaderSharp.exe --login`, and the other way round.
* Keep live tests small and do not run several downloads in parallel. Instagram quickly rate-limits or restricts
  accounts that make many requests. Use a secondary account for signed-in tests.

### Testing with a new InstaloaderSharp version

When a new version of InstaloaderSharp is released:

1. Read its changes (release notes and the differences in `Program.cs`, the public API and the README) and look for
   new, changed or removed command line options.
2. Replace `InstaloaderSharp\InstaloaderSharp.exe`, rebuild and run all unit tests.
3. Check that **Settings → About** shows the new version, then follow [Testing with Instagram](#testing-with-instagram).
4. Add any new options to the GUI (see [Project guidelines](#project-guidelines)) and update the version that the
   [README](README.md) says the GUI targets.

## Submitting a pull request

1. Keep the pull request focused on one change; split unrelated changes into separate pull requests.
2. Write clear commit messages: a short summary line in the imperative ("Fix post filter not being saved"),
   followed by a blank line and an explanation of what and why, if needed.
3. Make sure your branch is up to date with `main`, builds without errors and warnings, and passes all unit
   tests.
4. Push the branch to your fork and open a pull request against `main` of
   [InstaloaderSharp/InstaloaderSharpGUI](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/pulls).
5. In the description, explain what the change does, link the related issue (e.g. "Fixes #12"), and describe how
   you tested it. For visual changes, add before and after screenshots.
6. Be ready to answer questions and to update your pull request after review.

## License

InstaloaderSharp GUI is licensed under the [MIT License](LICENSE). By contributing, you agree that your
contributions are licensed under the same license.
