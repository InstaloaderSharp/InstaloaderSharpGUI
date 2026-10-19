using System;
using System.IO;
using System.Linq;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.Pages;
using InstaloaderSharpGUI.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExitCode = InstaloaderSharp.Program.ExitCode;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>The pages and their binding to the options (without showing a window or contacting Instagram).</summary>
    [TestClass]
    public class PageTests
    {
        private IsolatedAppData _appData;
        private RouterScope _router;

        [TestInitialize]
        public void Init()
        {
            _router = new RouterScope();
            _appData = new IsolatedAppData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _appData.Dispose();
            _router.Dispose();
        }

        private static AppState NewState()
        {
            var state = new AppState();
            state.Options.OutputDir = "C:\\Downloads";
            return state;
        }

        // ------------------------------------------------------------------ Download page

        [TestMethod]
        public void AddedTargetsAreListedOnce()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new DownloadPage(state);
                int changes = 0;
                state.Changed += (s, e) => changes++;

                // Other pages (Look up) add targets through the shared state.
                state.AddTarget("natgeo");
                state.AddTarget("#sunset");
                state.AddTarget("natgeo");
                page.AddTargets(new[] { ":feed", "#sunset" });

                CollectionAssert.AreEqual(new[] { "natgeo", "#sunset", ":feed" }, state.Options.Targets);
                var list = UiFind.Field<TargetList>(page, "_list");
                Assert.AreEqual(3, list.Items.Count);
                Assert.AreEqual("Your feed", ((TargetEntry)list.Items[2]).Value);
                Assert.IsTrue(changes > 0);
            });
        }

        [TestMethod]
        public void SwitchesAreBoundToTheOptions()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new DownloadPage(state);
                ToggleSwitch fast = UiFind.Switch(page, "Fast update");
                Assert.IsFalse(fast.Checked);
                fast.Checked = true;
                Assert.IsTrue(state.Options.FastUpdate);

                UiFind.Invoke(UiFind.Row(page, "Reels"), "OnClick", EventArgs.Empty);
                Assert.IsTrue(state.Options.Reels, "clicking the row flips the option");

                state.Options.Comments = true;
                state.Options.Posts = false;
                page.RefreshFromOptions();
                Assert.IsTrue(UiFind.Switch(page, "Comments").Checked);
                Assert.IsFalse(UiFind.Switch(page, "Posts").Checked);
            });
        }

        [TestMethod]
        public void EveryProfileAndPostOptionHasASwitch()
        {
            Sta.Run(() =>
            {
                var page = new DownloadPage(NewState());
                foreach (string title in new[] { "Posts", "Profile picture", "Stories", "Highlights", "Tagged posts", "Reels",
                                                 "Pictures", "Videos", "Video thumbnails", "Captions", "Geotags", "Comments",
                                                 "Metadata JSON", "Compress JSON", "Fast update", "Remember latest downloads" })
                    Assert.IsNotNull(UiFind.Switch(page, title), title);
                Assert.IsFalse(UiFind.All(page).OfType<SettingRow>().Any(r => r.Text == "IGTV"), "IGTV is not offered");
            });
        }

        [TestMethod]
        public void CompressJsonDependsOnMetadataJson()
        {
            Sta.Run(() =>
            {
                var page = new DownloadPage(NewState());
                SettingRow compress = UiFind.Row(page, "Compress JSON");
                Assert.IsTrue(compress.Enabled);
                UiFind.Switch(page, "Metadata JSON").Checked = false;
                Assert.IsFalse(compress.Enabled);
            });
        }

        [TestMethod]
        public void StampsFileIsShownOnlyWhenEnabled()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new DownloadPage(state);
                SettingRow stamps = UiFind.Row(page, "Remember latest downloads");
                Assert.IsFalse(stamps.Content.IsSetVisible());
                ((ToggleSwitch)stamps.Accessory).Checked = true;
                Assert.IsTrue(stamps.Content.IsSetVisible());
                Assert.IsTrue(state.Options.UseLatestStamps);
            });
        }

        [TestMethod]
        public void InvalidFilterIsShownUnderTheBox()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new DownloadPage(state);
                state.Options.PostFilter = "foo > 1";
                page.RefreshFromOptions();
                SettingRow row = UiFind.Row(page, "Post filter");
                Assert.AreEqual("Invalid filter: foo not a Post attribute.", row.Description);
                Assert.AreEqual(Theme.Error, row.DescriptionColor);

                state.Options.PostFilter = "likes > 1";
                page.RefreshFromOptions();
                StringAssert.StartsWith(row.Description, "✓ Valid filter.");
            });
        }

        [TestMethod]
        public void WarningsAppearUnderTheTargets()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new DownloadPage(state);
                var validation = UiFind.Field<TextBlock>(page, "_validation");
                Assert.IsFalse(validation.IsSetVisible());

                UiFind.Switch(page, "Stories").Checked = true;
                Assert.IsTrue(validation.IsSetVisible());
                StringAssert.Contains(validation.Text, "stories will be skipped");
                Assert.AreEqual(Theme.Warning, validation.TextColor);

                UiFind.Switch(page, "Pictures").Checked = false;
                UiFind.Switch(page, "Fast update").Checked = true;
                Assert.AreEqual(Theme.Error, validation.TextColor, "errors take precedence");
            });
        }

        // ------------------------------------------------------------------ Settings page

        [TestMethod]
        public void CommandLineFollowsTheOptions()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new SettingsPage(state);
                var command = UiFind.Field<DarkTextBox>(page, "_command");
                Assert.AreEqual("InstaloaderSharp", command.Text);
                state.Options.Targets.Add("natgeo");
                state.Options.FastUpdate = true;
                state.NotifyChanged();
                Assert.AreEqual("InstaloaderSharp --fast-update natgeo", command.Text);
                Assert.IsTrue(command.ReadOnly);
            });
        }

        [TestMethod]
        public void SettingsAreBound()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new SettingsPage(state);
                UiFind.Switch(page, "Sanitize paths").Checked = true;
                UiFind.Switch(page, "Pause between requests").Checked = false;
                UiFind.Switch(page, "Quiet mode").Checked = true;
                Assert.IsTrue(state.Options.SanitizePaths && !state.Options.Sleep && state.Options.Quiet);

                state.Options.Iphone = false;
                page.RefreshFromOptions();
                Assert.IsFalse(UiFind.Switch(page, "Use iPhone API").Checked);
            });
        }

        [TestMethod]
        public void ResumeOptionsDependOnResuming()
        {
            Sta.Run(() =>
            {
                var page = new SettingsPage(NewState());
                SettingRow prefix = UiFind.Row(page, "Resume file prefix");
                SettingRow aged = UiFind.Row(page, "Use aged resume files");
                Assert.IsTrue(prefix.Enabled && aged.Enabled);
                UiFind.Switch(page, "Resume interrupted downloads").Checked = false;
                Assert.IsFalse(prefix.Enabled || aged.Enabled);
            });
        }

        [TestMethod]
        public void AboutShowsTheLoadedVersions()
        {
            Sta.Run(() =>
            {
                var page = new SettingsPage(NewState());
                Assert.IsNotNull(UiFind.Row(page, "InstaloaderSharp GUI " + Library.GuiVersion));
                Assert.AreEqual("C# version of Instaloader.", UiFind.Row(page, "InstaloaderSharp " + Library.Version).Description);
                Assert.IsNotNull(UiFind.Row(page, "Documentation"));
            });
        }

        // ------------------------------------------------------------------ Account page

        [TestMethod]
        public void SavedSessionsAreListed()
        {
            Directory.CreateDirectory(_appData.ConfigDir);
            File.WriteAllText(Path.Combine(_appData.ConfigDir, "session-alice"), "x");
            File.WriteAllText(Path.Combine(_appData.ConfigDir, "session-bob"), "x");
            Sta.Run(() =>
            {
                var page = new AccountPage(NewState());
                var card = UiFind.Field<Card>(page, "_sessionsCard");
                CollectionAssert.AreEquivalent(new[] { "@alice", "@bob" }, card.Controls.OfType<SettingRow>().Select(r => r.Text).ToArray());
            });
        }

        [TestMethod]
        public void NoSessionsMessage()
        {
            Sta.Run(() =>
            {
                var page = new AccountPage(NewState());
                var card = UiFind.Field<Card>(page, "_sessionsCard");
                Assert.AreEqual("No saved sessions", card.Controls.OfType<SettingRow>().Single().Text);
            });
        }

        [TestMethod]
        public void SignInMethodShowsItsFields()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new AccountPage(state);
                SettingRow username = UiFind.Row(page, "Username");
                SettingRow browser = UiFind.Row(page, "Browser");
                SettingRow anonymous = UiFind.Row(page, "Downloading anonymously");
                Assert.IsTrue(anonymous.IsSetVisible());
                Assert.IsFalse(username.IsSetVisible() || browser.IsSetVisible());

                var accountChanges = 0;
                state.AccountChanged += (s, e) => accountChanges++;
                UiFind.Field<SegmentedControl>(page, "_mode").SelectedIndex = (int)LoginMode.Login;
                Assert.AreEqual(LoginMode.Login, state.Options.Mode);
                Assert.IsTrue(username.IsSetVisible());
                Assert.IsFalse(anonymous.IsSetVisible() || browser.IsSetVisible());
                Assert.AreEqual(1, accountChanges);

                state.Options.Mode = LoginMode.BrowserCookies;
                state.Options.Browser = "opera_gx";
                page.RefreshFromOptions();
                Assert.IsTrue(browser.IsSetVisible());
                Assert.AreEqual("Opera GX", ((DarkComboBox)browser.Accessory).SelectedItem);
            });
        }

        [TestMethod]
        public void PasswordStaysInMemory()
        {
            Sta.Run(() =>
            {
                AppState state = NewState();
                var page = new AccountPage(state);
                var box = (DarkTextBox)UiFind.Row(page, "Password").Accessory;
                Assert.IsTrue(box.UseSystemPasswordChar);
                box.Text = "secret";
                Assert.AreEqual("secret", state.Password);
                var store = new System.Collections.Generic.Dictionary<string, string>();
                state.Options.Save(store);
                Assert.IsFalse(store.Values.Contains("secret"));
            });
        }

        // ------------------------------------------------------------------ Activity page

        [TestMethod]
        public void OutcomeTexts()
        {
            var tracker = new ActivityTracker { Errors = 2, Warnings = 1 };
            AssertOutcome(ExitCode.Success, null, tracker, "Download complete", Theme.Success);
            AssertOutcome(ExitCode.NonFatalError, null, tracker, "Completed with 3 problem(s)", Theme.Warning);
            AssertOutcome(ExitCode.UserAborted, null, tracker, "Stopped. Start again to resume", Theme.Warning);
            AssertOutcome(ExitCode.DownloadAborted, "429", tracker, "Download aborted: 429", Theme.Error);
            AssertOutcome(ExitCode.LoginFailure, "Login cancelled.", tracker, "Login failed: Login cancelled.", Theme.Error);
            AssertOutcome(ExitCode.InitFailure, "bad", tracker, "Could not start: bad", Theme.Error);
            AssertOutcome(ExitCode.UnexpectedError, null, tracker, "Failed", Theme.Error);
        }

        private static void AssertOutcome(ExitCode code, string message, ActivityTracker t, string text, System.Drawing.Color color)
        {
            string actual;
            System.Drawing.Color actualColor;
            ActivityPage.Describe(new JobFinishedEventArgs { ExitCode = code, Message = message }, t, out actual, out actualColor);
            StringAssert.StartsWith(actual, text, code.ToString());
            Assert.AreEqual(color, actualColor, code.ToString());
        }

        [TestMethod]
        public void ElapsedTime()
        {
            Assert.AreEqual("0:05", ActivityPage.FormatElapsed(TimeSpan.FromSeconds(5)));
            Assert.AreEqual("12:34", ActivityPage.FormatElapsed(new TimeSpan(0, 12, 34)));
            Assert.AreEqual("2:03:04", ActivityPage.FormatElapsed(new TimeSpan(2, 3, 4)));
        }
    }
}
