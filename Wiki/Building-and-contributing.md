This page is for people who want to build InstaloaderSharp GUI themselves, run its tests, report a bug or contribute. The full guidelines are in [CONTRIBUTING.md](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/blob/main/CONTRIBUTING.md).

## Contents

- [Building from source](#building-from-source)
- [Running the unit tests](#running-the-unit-tests)
- [Repository layout](#repository-layout)
- [Reporting bugs](#reporting-bugs)
- [Reporting security issues](#reporting-security-issues)
- [Suggesting features and asking questions](#suggesting-features-and-asking-questions)
- [Contributing code](#contributing-code)
- [Testing with Instagram](#testing-with-instagram)
- [Testing with a new InstaloaderSharp version](#testing-with-a-new-instaloadersharp-version)
- [Editing this wiki](#editing-this-wiki)

## Building from source

You need:

- Visual Studio 2015 or later, or the MSBuild 14 that comes with it,
- the .NET Framework 4.8.1 developer pack (reference assemblies),
- a compiled `InstaloaderSharp.exe`. It is not part of the repository: download it from the [InstaloaderSharp releases](https://github.com/InstaloaderSharp/InstaloaderSharp/releases) (the version named in the README) or [build it](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Building-and-contributing).

1. Clone the repository:

   ```
   git clone https://github.com/InstaloaderSharp/InstaloaderSharpGUI.git
   ```

2. Put `InstaloaderSharp.exe` into the `InstaloaderSharp` folder next to the solution:

   ```
   InstaloaderSharpGUI.sln
   InstaloaderSharp\InstaloaderSharp.exe      ← referenced as a library
   InstaloaderSharpGUI\                      ← the GUI project
   InstaloaderSharpGUI.Tests\                ← the unit tests
   ```

3. Open `InstaloaderSharpGUI.sln` in Visual Studio and build, or run:

   ```
   "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" InstaloaderSharpGUI.sln /p:Configuration=Release
   ```

   Without Visual Studio's test framework, build only the program with `InstaloaderSharpGUI\InstaloaderSharpGUI.csproj` instead of the solution.

The result is in `InstaloaderSharpGUI\bin\Release\`: `InstaloaderSharpGUI.exe` and a copy of `InstaloaderSharp.exe`. No NuGet packages or other dependencies are used. The build must finish without errors and warnings.

## Running the unit tests

The solution contains a unit test project, `InstaloaderSharpGUI.Tests` (MSTest). The tests run offline: they make no requests to Instagram, need no account, and never touch your real settings or sessions. They test the GUI's own code and how it uses InstaloaderSharp, not what [InstaloaderSharp's tests](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Building-and-contributing#running-the-unit-tests) already cover:

| Tests | Cover |
|---|---|
| `TargetsTests` | Understanding typed and pasted targets, including Instagram links |
| `JobOptionsTests`, `SettingsStoreTests` | Validating options, mapping every option to command-line arguments, saving options |
| `CommandLineTests` | Importing command lines and argument files through InstaloaderSharp's parser, round trips of every option |
| `LibraryTests` | Creating an `Instaloader` from the options, sessions, signing in without a session, filter attributes |
| `ConsoleRouterTests`, `ActivityTrackerTests` | Capturing the console output, answering console input with dialogs, reading progress from the output |
| `DownloadEngineTests` | Downloads that end before any request to Instagram |
| `ControlTests`, `PageTests` | The custom controls, layout, colour contrast, and how the pages are bound to the options |

To run them, build the solution in Visual Studio, open **Test Explorer** from the **Test** menu and choose **Run All**. On the command line:

```
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" InstaloaderSharpGUI.sln /p:Configuration=Debug
"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" InstaloaderSharpGUI.Tests\bin\Debug\InstaloaderSharpGUI.Tests.dll
```

Use the `vstest.console.exe` of your Visual Studio version (for newer versions, it is under `Common7\IDE\Extensions\TestPlatform`). The test runner does not load tests from a network drive (*Operation is not supported*, `0x80131515`); clone to a local drive, or copy `InstaloaderSharpGUI.Tests\bin\Debug` to one and run the tests there.

## Repository layout

| Path | Contents |
|---|---|
| `InstaloaderSharpGUI/Program.cs`, `MainForm.cs` | Start-up, the main window: sidebar, status bar, shortcuts, starting and stopping downloads |
| `InstaloaderSharpGUI/Core/` | Options and their mapping to the command line, the download engine, the console capture, target parsing, helpers around the library, `settings.ini` |
| `InstaloaderSharpGUI/Pages/` | The Download, Activity, Look up, Account and Settings pages |
| `InstaloaderSharpGUI/UI/` | The dark theme and the custom controls |
| `InstaloaderSharpGUI.Tests/` | Unit tests |
| `Wiki/` | This manual |

See [How it works](How-it-works) for how these parts work together.

## Reporting bugs

Report bugs in the [issue tracker](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues). Many problems shown in the GUI come from InstaloaderSharp or from Instagram, so first check [Is it a GUI problem?](Troubleshooting-and-FAQ#is-it-a-gui-problem) and report the problem to the right project.

Please include:

- both versions (*Settings → About*) and your Windows version;
- what you did, step by step;
- the equivalent command (*Settings → Command line → Copy*);
- the complete output from the Activity page (**Copy** or **Save…**) and the result shown in the status bar;
- for display problems, a screenshot and your display scaling;
- for a crash, the full text of the **Something went wrong** dialog;
- whether `InstaloaderSharp.exe` with the same command shows the problem too.

> Never post passwords, session files, cookie values or two-factor codes. Before posting output, a command or settings, remove anything private, such as usernames of private accounts.

## Reporting security issues

Do not open a public issue for a security problem, for example in how passwords, sessions or cookies are handled. Report it privately through the repository's [Security tab](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/security) ("Report a vulnerability").

## Suggesting features and asking questions

Feature requests and questions are welcome as [issues](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/issues/new). Features of the user interface belong here. New download features or options must exist in InstaloaderSharp first; request them in the [InstaloaderSharp repository](https://github.com/InstaloaderSharp/InstaloaderSharp/issues). Features that act on Instagram (liking, following, posting) are out of scope.

## Contributing code

Fork the repository, create a branch from `main`, make your change, and open a pull request. For larger changes, open an issue first. The main rules from [CONTRIBUTING.md](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/blob/main/CONTRIBUTING.md#project-guidelines):

- C# 6 and .NET Framework 4.8.1, compiled with Visual Studio 2015 / MSBuild 14; no newer language features.
- No NuGet packages or other dependencies.
- Use the InstaloaderSharp library, do not duplicate it, and never start `InstaloaderSharp.exe` as a separate process.
- Keep every command-line option available in the GUI, and the GUI's downloads identical to the equivalent command.
- Never store passwords, and do not contact Instagram unless the user starts something.
- Keep the window responsive, and follow the design: the colours of `Theme`, sizes scaled with `Theme.S(...)`, the existing controls, keyboard access and descriptions for every option.
- Add new files to the project files, and add tests where possible.

## Testing with Instagram

Unit tests cannot detect changes on Instagram's side or how the window looks, so also test by hand: try the affected pages with the mouse and the keyboard, at another display scaling, and download something small (a single post, a profile with **Maximum number of posts**, or with **Posts** off). Compare with `InstaloaderSharp.exe` using the copied command. Keep live tests small, and use a secondary account for signed-in tests.

## Testing with a new InstaloaderSharp version

When a new InstaloaderSharp version is released:

1. Read its changes: the release notes and the differences in `Program.cs`, the public API and the README. Look for new, changed or removed command-line options, and for changed methods that the GUI uses.
2. Run the existing GUI build with the new `InstaloaderSharp.exe`.
3. Replace `InstaloaderSharp\InstaloaderSharp.exe`, rebuild, and run all unit tests in Debug and Release.
4. Check that *Settings → About* shows the new version, and test with Instagram.
5. Add any new options to the GUI, and update the version that the README says the GUI has been tested with.

## Editing this wiki

The pages of this wiki are kept in the repository's [`Wiki` folder](https://github.com/InstaloaderSharp/InstaloaderSharpGUI/tree/main/Wiki), one Markdown file per page, with `_Sidebar.md` as the navigation. Improvements are welcome as pull requests like any other change. Links between pages use the page name without `.md`, for example `[Signing in](Signing-in)`. Topics that work the same as in InstaloaderSharp, such as file formats, the filter language and name placeholders, link to the [InstaloaderSharp manual](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki) instead of repeating it.
