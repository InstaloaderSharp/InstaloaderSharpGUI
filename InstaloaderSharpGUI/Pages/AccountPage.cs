using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using InstaloaderSharp;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Pages
{
    /// <summary>Sign-in method (anonymous, Instagram login, browser cookies), session check and saved sessions.</summary>
    internal sealed class AccountPage : PageBase
    {
        private readonly AccountStatus _status;
        private readonly SegmentedControl _mode;
        private readonly List<Control> _loginRows = new List<Control>();
        private readonly List<Control> _cookieRows = new List<Control>();
        private readonly List<Control> _anonRows = new List<Control>();
        private readonly Card _sessionsCard;
        private readonly FlatButton _signIn;
        private readonly FlatButton _import;
        private readonly DarkTextBox _password;
        private readonly DarkComboBox _browser;
        private bool _busy;

        public AccountPage(AppState state) : base(state)
        {
            Add(new PageHeader("Account",
                               "Sign in to download private profiles you follow, stories, highlights, your feed and saved posts. " +
                               "Your password is never stored - only the session cookie, just like InstaloaderSharp does."));

            _status = Add(new AccountStatus(), Theme.S(8));
            _status.Check.Click += (s, e) => CheckSession();
            _status.SignOut.Click += (s, e) => SignOut();

            // ------------------------------------------------------------------ Method
            Card method = AddSection("Sign-in method");
            var modeRow = new SettingRow("How should InstaloaderSharp access Instagram?", null, Glyphs.Key);
            _mode = new SegmentedControl("Anonymous", "Instagram login", "Browser cookies");
            _mode.SetGlyphs(Glyphs.Globe, Glyphs.Lock, Glyphs.Import);
            _mode.SelectedIndex = (int)O.Mode;
            modeRow.Content = _mode;
            method.Add(modeRow);

            // Anonymous
            var anon = new SettingRow("Downloading anonymously",
                                      "Public profiles, hashtags and single posts work without signing in, but Instagram limits anonymous " +
                                      "access heavily. Sign in if you see errors such as 401, 403 or 429, or to get higher quality pictures.",
                                      Glyphs.Info);
            method.Add(anon);
            _anonRows.Add(anon);

            // Login
            var userRow = new SettingRow("Username", "Your Instagram username (not your e-mail address).", Glyphs.Contact);
            TextRow(method, null, null, o => o.Username, (o, v) => { o.Username = v; State.VerifiedUser = null; State.Avatar = null; },
                    "your.username", Theme.S(260), existing: userRow);
            method.Add(userRow);
            _loginRows.Add(userRow);

            var pwRow = new SettingRow("Password",
                                       "Only needed when there is no valid saved session. Kept in memory only - never written to disk. " +
                                       "If you leave it empty you will be asked when needed.", Glyphs.Lock);
            _password = new DarkTextBox { UseSystemPasswordChar = true, Placeholder = "Optional" };
            _password.TextChanged += (s, e) => State.Password = _password.Text;
            pwRow.Accessory = _password;
            pwRow.AccessoryWidth = Theme.S(260);
            method.Add(pwRow);
            _loginRows.Add(pwRow);

            var sessRow = new SettingRow("Session file", "Optional. Where the session cookie is stored. Leave empty for the default location.", Glyphs.Document);
            PathRow(method, null, null, o => o.SessionFile, (o, v) => o.SessionFile = v, false, "Default: " + Library.ConfigDir + "\\session-<username>",
                    "All files|*.*", true, existing: sessRow);
            method.Add(sessRow);
            _loginRows.Add(sessRow);

            var signRow = new SettingRow("Sign in now",
                                         "Signs in (asking for a 2FA code if needed) and saves the session, so downloads start without asking again.",
                                         Glyphs.Unlock);
            _signIn = new FlatButton("Sign in", ButtonKind.Primary, Glyphs.Unlock);
            signRow.Accessory = _signIn;
            method.Add(signRow);
            _loginRows.Add(signRow);
            _signIn.Click += (s, e) => SignInNow();

            // Browser cookies
            var browserRow = new SettingRow("Browser",
                                            "A browser in which you are signed in to instagram.com. Close the browser first if importing fails.",
                                            Glyphs.Globe);
            _browser = new DarkComboBox { Width = Theme.S(260) };
            foreach (string b in JobOptions.Browsers)
                _browser.Items.Add(JobOptions.BrowserDisplayName(b));
            SyncBrowser();
            _browser.SelectedIndexChanged += (s, e) =>
            {
                if (Refreshing || _browser.SelectedIndex < 0) return;
                O.Browser = JobOptions.Browsers[_browser.SelectedIndex];
                State.VerifiedUser = null;
                Changed();
                State.NotifyAccountChanged();
            };
            OnRefresh(SyncBrowser);
            browserRow.Accessory = _browser;
            method.Add(browserRow);
            _cookieRows.Add(browserRow);

            var cookieRow = new SettingRow("Cookie file", "Optional. Only needed for a non-default browser profile, e.g. a Firefox cookies.sqlite file.", Glyphs.Document);
            PathRow(method, null, null, o => o.CookieFile, (o, v) => o.CookieFile = v, false, "Use the browser's default profile",
                    "Cookie databases|cookies.sqlite;Cookies;*.sqlite;*.binarycookies|All files|*.*", false, existing: cookieRow);
            method.Add(cookieRow);
            _cookieRows.Add(cookieRow);

            var importRow = new SettingRow("Import now",
                                           "Reads the cookies, verifies them and saves them as a session. Afterwards you can switch to " +
                                           "\"Instagram login\" to reuse that session without reading the browser again.",
                                           Glyphs.Import);
            _import = new FlatButton("Import cookies", ButtonKind.Primary, Glyphs.Import);
            importRow.Accessory = _import;
            method.Add(importRow);
            _cookieRows.Add(importRow);
            _import.Click += (s, e) => ImportCookies();

            _mode.SelectedIndexChanged += (s, e) =>
            {
                SyncMode();
                if (Refreshing) return;
                O.Mode = (LoginMode)_mode.SelectedIndex;
                State.VerifiedUser = null;
                State.Avatar = null;
                Changed();
                State.NotifyAccountChanged();
            };
            OnRefresh(() => { _mode.SelectedIndex = (int)O.Mode; SyncMode(); });

            // ------------------------------------------------------------------ Sessions
            Add(new SectionTitle("Saved sessions"), Theme.S(14));
            var sessionTools = new HStack();
            var refresh = sessionTools.Add(new FlatButton("Refresh", ButtonKind.Subtle, Glyphs.Refresh));
            var openDir = sessionTools.Add(new FlatButton("Open folder", ButtonKind.Subtle, Glyphs.FolderOpen));
            Add(sessionTools);
            refresh.Click += (s, e) => LoadSessions();
            openDir.Click += (s, e) => DownloadPage.OpenFolder(Library.ConfigDir);
            _sessionsCard = Add(new Card());

            Add(new TextBlock("Session files contain a login cookie that gives access to your account. Do not share them. " +
                              "Instagram may temporarily block accounts that download a lot - consider using a secondary account.",
                              Theme.TextSecondary, Glyphs.Shield), Theme.S(8));

            SyncMode();
            LoadSessions();
            state.AccountChanged += (s, e) => UpdateStatus();
            state.Changed += (s, e) => UpdateStatus();
            UpdateStatus();
        }

        private void SyncBrowser()
        {
            int idx = Array.IndexOf(JobOptions.Browsers, (O.Browser ?? "").ToLowerInvariant());
            _browser.SelectedIndex = idx >= 0 ? idx : Array.IndexOf(JobOptions.Browsers, "firefox");
        }

        private void SyncMode()
        {
            var mode = (LoginMode)_mode.SelectedIndex;
            SuspendLayout();
            foreach (Control c in _anonRows) c.Visible = mode == LoginMode.Anonymous;
            foreach (Control c in _loginRows) c.Visible = mode == LoginMode.Login;
            foreach (Control c in _cookieRows) c.Visible = mode == LoginMode.BrowserCookies;
            ResumeLayout();
            PerformLayout();
        }

        public override void RefreshFromOptions()
        {
            base.RefreshFromOptions();
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            _status.Check.Enabled = O.Mode != LoginMode.Anonymous && !_busy;
            _signIn.Enabled = _import.Enabled = _status.SignOut.Enabled = !_busy;
            if (_busy)
                return;
            string user = State.VerifiedUser;
            switch (O.Mode)
            {
                case LoginMode.Anonymous:
                    _status.SetState("Not signed in", "Downloads are anonymous. Choose a sign-in method below to access more.", null, null, false);
                    break;
                case LoginMode.Login:
                    string name = string.IsNullOrWhiteSpace(O.Username) ? null : O.Username.Trim().ToLowerInvariant();
                    if (name == null)
                        _status.SetState("Instagram login", "Enter your username below.", null, null, false);
                    else
                    {
                        bool hasSession = File.Exists(O.EffectiveSessionFile ?? Library.DefaultSessionFile(name));
                        _status.SetState("@" + name,
                                         user == name ? "Signed in - session verified." :
                                         hasSession ? "Saved session found. Use Check session to verify it is still valid." :
                                         "No saved session yet - press Sign in, or you will be asked for your password when downloading.",
                                         name, State.Avatar, hasSession);
                    }
                    break;
                default:
                    _status.SetState(user != null ? "@" + user : "Browser cookies",
                                     "Session is imported from " + JobOptions.BrowserDisplayName(O.Browser) + " each time a download starts.",
                                     user, State.Avatar, false);
                    break;
            }
        }

        private void SetBusy(bool busy, string what = null)
        {
            _busy = busy;
            Cursor = busy ? Cursors.AppStarting : Cursors.Default;
            UpdateStatus();
            if (busy)
                _status.SetState(what, "Please wait…", _status.User, State.Avatar, false, true);
        }

        private sealed class SignInResult
        {
            public string User;
            public Image Avatar;
            public string SessionFile;
        }

        private SignInResult DoSignIn(JobOptions o, string password)
        {
            using (Instaloader loader = Library.CreateLoader(o))
            {
                string user = Library.SignIn(loader, o, password, PromptFromWorker);
                if (!loader.Context.IsLoggedIn || string.IsNullOrEmpty(user))
                    throw new LoginException("Not logged in.");
                string sessionFile = o.Mode == LoginMode.Login ? o.EffectiveSessionFile : null;
                loader.SaveSessionToFile(sessionFile);
                Image avatar = null;
                try
                {
                    Profile me = Profile.FromUsername(loader.Context, user);
                    avatar = Library.DownloadImage(loader, me.ProfilePicUrl);
                }
                catch (Exception)
                {
                }
                return new SignInResult { User = user, Avatar = avatar, SessionFile = sessionFile ?? Library.DefaultSessionFile(user) };
            }
        }

        private void SignInNow()
        {
            if (string.IsNullOrWhiteSpace(O.Username))
            {
                ShowError("Enter your Instagram username first.", "Username missing");
                return;
            }
            JobOptions o = O.Clone();
            string pw = State.Password;
            SetBusy(true, "Signing in as @" + o.Username.Trim().ToLowerInvariant() + "…");
            RunBackground(() => DoSignIn(o, pw), r =>
            {
                SetBusy(false);
                State.VerifiedUser = r.User;
                State.Avatar = r.Avatar;
                _password.Text = "";
                LoadSessions();
                State.NotifyAccountChanged();
                MessageDialog.Show(this, "Session saved to " + r.SessionFile + ".\n\nDownloads will reuse it until it expires.",
                                   "Signed in as @" + r.User, DialogIcon.Success);
            }, e =>
            {
                SetBusy(false);
                ShowError(e.Message, "Sign in failed");
            });
        }

        private void ImportCookies()
        {
            JobOptions o = O.Clone();
            o.Mode = LoginMode.BrowserCookies;
            SetBusy(true, "Importing cookies from " + JobOptions.BrowserDisplayName(o.Browser) + "…");
            RunBackground(() => DoSignIn(o, null), r =>
            {
                SetBusy(false);
                State.VerifiedUser = r.User;
                State.Avatar = r.Avatar;
                LoadSessions();
                State.NotifyAccountChanged();
                if (MessageDialog.Show(this,
                                       "The browser session was saved to " + r.SessionFile + ".\n\nSwitch to \"Instagram login\" as @" + r.User +
                                       " to reuse this session without reading the browser's cookies every time? (Recommended)",
                                       "Signed in as @" + r.User, DialogIcon.Success, MessageBoxButtons.YesNo,
                                       yesText: "Use saved session", noText: "Keep browser cookies") == DialogResult.Yes)
                {
                    O.Mode = LoginMode.Login;
                    O.Username = r.User;
                    O.SessionFile = "";
                    RefreshFromOptions();
                    State.VerifiedUser = r.User;
                    State.NotifyAccountChanged();
                }
            }, e =>
            {
                SetBusy(false);
                ShowError(e.Message, "Importing cookies failed");
            });
        }

        private void CheckSession()
        {
            JobOptions o = O.Clone();
            SetBusy(true, "Checking session…");
            RunBackground(() =>
            {
                using (Instaloader loader = Library.CreateLookupLoader(o))
                {
                    string user = loader.Context.IsLoggedIn ? loader.TestLogin() : null;
                    Image avatar = null;
                    if (user != null)
                    {
                        try
                        {
                            avatar = Library.DownloadImage(loader, Profile.FromUsername(loader.Context, user).ProfilePicUrl);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    return new SignInResult { User = user, Avatar = avatar };
                }
            }, r =>
            {
                SetBusy(false);
                State.VerifiedUser = r.User;
                State.Avatar = r.Avatar;
                State.NotifyAccountChanged();
                if (r.User != null)
                    MessageDialog.Show(this, "The session is valid.", "Signed in as @" + r.User, DialogIcon.Success);
                else
                    MessageDialog.Show(this, o.Mode == LoginMode.Login
                                             ? "There is no valid session for this account. Press Sign in to create one."
                                             : "No valid Instagram session was found in the browser. Sign in to instagram.com in that browser first.",
                                       "Not signed in", DialogIcon.Warning);
            }, e =>
            {
                SetBusy(false);
                ShowError(e.Message, "Checking the session failed");
            });
        }

        private void SignOut()
        {
            if (O.Mode == LoginMode.Login && !string.IsNullOrWhiteSpace(O.Username))
            {
                string file = O.EffectiveSessionFile ?? Library.DefaultSessionFile(O.Username.Trim());
                if (File.Exists(file) &&
                    MessageDialog.Show(this, "Delete the saved session " + file + "?\n\nYou will need your password (and possibly a 2FA code) to sign in again.",
                                       "Sign out", DialogIcon.Question, MessageBoxButtons.YesNo, yesText: "Sign out", noText: "Cancel") != DialogResult.Yes)
                    return;
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch (Exception e)
                {
                    ShowError(e.Message, "Could not delete the session");
                    return;
                }
            }
            O.Mode = LoginMode.Anonymous;
            State.VerifiedUser = null;
            State.Avatar = null;
            State.Password = "";
            _password.Text = "";
            RefreshFromOptions();
            LoadSessions();
            Changed();
            State.NotifyAccountChanged();
        }

        private void LoadSessions()
        {
            _sessionsCard.SuspendLayout();
            foreach (Control c in _sessionsCard.Controls)
                c.Dispose();
            _sessionsCard.Controls.Clear();
            List<Library.SessionInfo> sessions = Library.ListSessions();
            if (sessions.Count == 0)
            {
                _sessionsCard.Add(new SettingRow("No saved sessions", "Sign in or import browser cookies to create one.", Glyphs.Info));
            }
            foreach (Library.SessionInfo info in sessions)
            {
                Library.SessionInfo si = info;
                var row = new SettingRow("@" + si.Username, "Saved " + si.Modified.ToString("g") + "  ·  " + si.Path, Glyphs.Contact);
                var buttons = new HStack();
                var use = buttons.Add(new FlatButton("Use", ButtonKind.Secondary, Glyphs.Accept));
                var del = buttons.Add(new FlatButton("Delete", ButtonKind.Subtle, Glyphs.Delete));
                buttons.Width = use.Width + del.Width + buttons.Spacing;
                row.Accessory = buttons;
                row.AccessoryWidth = buttons.Width;
                use.Click += (s, e) =>
                {
                    O.Mode = LoginMode.Login;
                    O.Username = si.Username;
                    O.SessionFile = "";
                    State.VerifiedUser = null;
                    State.Avatar = null;
                    RefreshFromOptions();
                    Changed();
                    State.NotifyAccountChanged();
                };
                del.Click += (s, e) =>
                {
                    if (MessageDialog.Show(this, "Delete the saved session for @" + si.Username + "?", "Delete session",
                                           DialogIcon.Question, MessageBoxButtons.YesNo, yesText: "Delete", noText: "Cancel") != DialogResult.Yes)
                        return;
                    try
                    {
                        File.Delete(si.Path);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                    if (State.VerifiedUser == si.Username)
                        State.VerifiedUser = null;
                    LoadSessions();
                    State.NotifyAccountChanged();
                };
                _sessionsCard.Add(row);
            }
            _sessionsCard.ResumeLayout();
            _sessionsCard.PerformLayout();
            PerformLayout();
        }

        /// <summary>Large account card: avatar, name, state and actions.</summary>
        private sealed class AccountStatus : Control
        {
            public readonly FlatButton Check;
            public readonly FlatButton SignOut;
            private readonly Avatar _avatar;
            private string _title = "";
            private string _detail = "";
            public string User;

            public AccountStatus()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Theme.Surface;
                _avatar = new Avatar { Size = new Size(Theme.S(56), Theme.S(56)), Glyph = Glyphs.Contact };
                Check = new FlatButton("Check session", ButtonKind.Secondary, Glyphs.Refresh);
                SignOut = new FlatButton("Sign out", ButtonKind.Subtle, Glyphs.SignOut);
                Controls.Add(_avatar);
                Controls.Add(Check);
                Controls.Add(SignOut);
            }

            public void SetState(string title, string detail, string user, Image avatar, bool canSignOut, bool busy = false)
            {
                _title = title ?? "";
                _detail = detail ?? "";
                User = user;
                _avatar.Text = user ?? "";
                _avatar.Image = avatar;
                SignOut.Visible = user != null || canSignOut;
                AccessibleName = _title + ". " + _detail;
                PerformLayout();
                Invalidate();
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                return new Size(proposedSize.Width, Theme.S(88));
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                using (var path = Theme.RoundRect(new RectangleF(0, 0, Width, Height), Theme.S(8f)))
                    Region = new Region(path);
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                if (SignOut == null)
                    return; // still constructing
                int pad = Theme.S(16);
                _avatar.Location = new Point(pad, (Height - _avatar.Height) / 2);
                int x = Width - pad;
                foreach (FlatButton b in new[] { SignOut, Check })
                {
                    if (!b.Visible) continue;
                    x -= b.Width;
                    b.Location = new Point(x, (Height - b.Height) / 2);
                    x -= Theme.S(8);
                }
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(Theme.Surface);
                int x = _avatar.Right + Theme.S(16);
                int right = Check.Visible ? Check.Left : Width;
                right -= Theme.S(12);
                TextRenderer.DrawText(g, _title, Theme.Display, new Rectangle(x, Theme.S(18), right - x, Theme.S(28)), Theme.Text,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, _detail, Theme.Small, new Rectangle(x, Theme.S(48), right - x, Theme.S(32)), Theme.TextSecondary,
                                      TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }
    }
}
