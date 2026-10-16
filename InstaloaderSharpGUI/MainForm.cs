using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.Pages;
using InstaloaderSharpGUI.UI;
using ExitCode = InstaloaderSharp.Program.ExitCode;

namespace InstaloaderSharpGUI
{
    /// <summary>Main window: navigation rail, page host and a persistent status bar with the primary action.</summary>
    internal sealed class MainForm : ThemedForm
    {
        private readonly AppState _state = new AppState();
        private readonly NavRail _nav;
        private readonly Panel _host;
        private readonly AccountChip _chip;
        private readonly StatusBar _status;
        private readonly DownloadPage _downloadPage;
        private readonly ActivityPage _activityPage;
        private readonly LookupPage _lookupPage;
        private readonly AccountPage _accountPage;
        private readonly SettingsPage _settingsPage;
        private readonly NavItem _activityItem;
        private readonly ActivityTracker _tracker = new ActivityTracker();
        private readonly Timer _pump;
        private readonly DownloadEngine _engine;
        private bool _closeAfterStop;
        private Dictionary<string, string> _settings;

        public MainForm()
        {
            Text = "InstaloaderSharp GUI";
            MinimumSize = new Size(Theme.S(900), Theme.S(620));
            Size = new Size(Theme.S(1260), Theme.S(840));
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            _settings = SettingsStore.Load();
            _state.Options.Load(_settings);
            if (string.IsNullOrWhiteSpace(_state.Options.OutputDir))
                _state.Options.OutputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "InstaloaderSharp");

            _engine = new DownloadEngine();
            _engine.Prompt = (msg, pw) => MessageDialog.Prompt(this, pw ? "Password required" : "Verification required", msg, pw,
                                                                okText: pw ? "Sign in" : "Verify");
            _engine.Finished += OnFinished;
            _state.Engine = _engine;
            _state.ToolTip = new DarkToolTip();
            _state.MainWindow = this;
            ConsoleRouter.PromptHandler = AskFromLibrary;

            SuspendLayout();

            // ---------------------------------------------------------------- page host and pages
            _host = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Window };
            _downloadPage = new DownloadPage(_state);
            _activityPage = new ActivityPage(_state);
            _lookupPage = new LookupPage(_state);
            _accountPage = new AccountPage(_state);
            _settingsPage = new SettingsPage(_state);
            _settingsPage.RefreshAll = RefreshAllPages;

            // ---------------------------------------------------------------- status bar
            _status = new StatusBar { Dock = DockStyle.Bottom };
            _status.Start.Click += (s, e) => StartDownload();
            _status.Stop.Click += (s, e) => StopDownload();
            _state.ToolTip.SetToolTip(_status.Start, "Start downloading all targets (Ctrl+Enter or F5)");
            _state.ToolTip.SetToolTip(_status.Stop, "Stop after the current item. The next start resumes where it stopped (Shift+F5)");

            // ---------------------------------------------------------------- sidebar
            var sidebar = new Panel { Dock = DockStyle.Left, Width = Theme.S(236), BackColor = Theme.Sidebar };
            var brand = new Brand { Dock = DockStyle.Top, Height = Theme.S(72) };
            _nav = new NavRail { Dock = DockStyle.Fill };
            _chip = new AccountChip { Dock = DockStyle.Bottom, Height = Theme.S(68) };
            _chip.Click += (s, e) => Navigate(typeof(AccountPage));
            _state.ToolTip.SetToolTip(_chip, "Account and sign-in (Ctrl+4)");
            sidebar.Controls.Add(_nav);
            sidebar.Controls.Add(_chip);
            sidebar.Controls.Add(brand);

            _nav.Items.Add(new NavItem { Text = "Download", Glyph = Glyphs.Download, Page = _downloadPage });
            _activityItem = new NavItem { Text = "Activity", Glyph = Glyphs.Activity, Page = _activityPage };
            _nav.Items.Add(_activityItem);
            _nav.Items.Add(new NavItem { Text = "Look up", Glyph = Glyphs.Search, Page = _lookupPage });
            _nav.Items.Add(new NavItem { Text = "Account", Glyph = Glyphs.Contact, Page = _accountPage });
            _nav.Items.Add(new NavItem { Text = "Settings", Glyph = Glyphs.Settings, Page = _settingsPage });
            foreach (NavItem item in _nav.Items)
            {
                item.Page.Dock = DockStyle.Fill;
                item.Page.Visible = false;
                _host.Controls.Add(item.Page);
            }
            _nav.SelectedIndexChanged += (s, e) => ShowPage(_nav.SelectedItem);

            var content = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Window };
            content.Controls.Add(_host);
            content.Controls.Add(_status);
            Controls.Add(content);
            Controls.Add(sidebar);
            ResumeLayout();

            _state.Navigate = Navigate;
            _state.Changed += (s, e) => UpdateStatusIdle();
            _state.AccountChanged += (s, e) => UpdateChip();

            _pump = new Timer { Interval = 100 };
            _pump.Tick += (s, e) => Pump();
            _pump.Start();

            RestoreWindow();
            _nav.SelectedIndex = 0;
            UpdateChip();
            UpdateStatusIdle();
            ConsoleRouter.App(string.Format("InstaloaderSharp GUI ready - using the InstaloaderSharp {0} library.", Library.Version));
        }

        // ------------------------------------------------------------------------------ navigation

        private void ShowPage(NavItem item)
        {
            if (item == null)
                return;
            foreach (NavItem i in _nav.Items)
                if (i != item)
                    i.Page.Visible = false;
            item.Page.Visible = true;
            item.Page.BringToFront();
            if (item == _activityItem && !_engine.IsRunning)
            {
                _activityItem.Badge = null;
                _nav.Invalidate();
            }
        }

        private void Navigate(Type pageType)
        {
            for (int i = 0; i < _nav.Items.Count; i++)
                if (_nav.Items[i].Page.GetType() == pageType)
                    _nav.SelectedIndex = i;
        }

        private void RefreshAllPages()
        {
            foreach (NavItem i in _nav.Items)
            {
                var page = i.Page as PageBase;
                if (page != null)
                    page.RefreshFromOptions();
            }
            UpdateChip();
            UpdateStatusIdle();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Enter:
                case Keys.F5:
                    StartDownload();
                    return true;
                case Keys.Shift | Keys.F5:
                    StopDownload();
                    return true;
                case Keys.Control | Keys.L:
                    Navigate(typeof(DownloadPage));
                    _downloadPage.FocusInput();
                    return true;
                case Keys.Control | Keys.F:
                    Navigate(typeof(LookupPage));
                    _lookupPage.FocusQuery();
                    return true;
                case Keys.Control | Keys.D1: _nav.SelectedIndex = 0; return true;
                case Keys.Control | Keys.D2: _nav.SelectedIndex = 1; return true;
                case Keys.Control | Keys.D3: _nav.SelectedIndex = 2; return true;
                case Keys.Control | Keys.D4: _nav.SelectedIndex = 3; return true;
                case Keys.Control | Keys.D5: _nav.SelectedIndex = 4; return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ------------------------------------------------------------------------------ running

        private string AskFromLibrary(string prompt)
        {
            Func<string> ask = () => MessageDialog.Prompt(this, "InstaloaderSharp needs input", prompt, prompt.ToLowerInvariant().Contains("password"));
            if (InvokeRequired)
                return (string)Invoke(ask);
            return ask();
        }

        private void StartDownload()
        {
            if (_engine.IsRunning)
            {
                Navigate(typeof(ActivityPage));
                return;
            }
            JobOptions o = _state.Options;
            JobOptions.ValidationResult v = o.Validate();
            if (!v.Ok)
            {
                MessageDialog.Show(this, "• " + string.Join("\n• ", v.Errors), "Almost there", DialogIcon.Warning);
                Navigate(v.Errors.Any(e => e.Contains("username") || e.Contains("browser")) ? typeof(AccountPage) : typeof(DownloadPage));
                return;
            }
            if (v.Warnings.Count > 0 &&
                MessageDialog.Show(this, "• " + string.Join("\n• ", v.Warnings) + "\n\nStart anyway?", "Before you start",
                                   DialogIcon.Warning, MessageBoxButtons.YesNo, yesText: "Start download", noText: "Go back") != DialogResult.Yes)
                return;

            SaveSettings();
            _tracker.Reset();
            _activityPage.ResetStats();
            _activityPage.JobStarted(Path.GetFullPath(o.OutputDir.Trim()));
            _activityItem.Badge = "Live";
            _activityItem.BadgeColor = Theme.Accent;
            _nav.Invalidate();
            _engine.Start(o, _state.Password);
            _status.SetRunning(true);
            _status.SetText("Starting download…", string.Format("{0} target(s)", o.Targets.Count), Theme.Text, Glyphs.Download);
            _status.Progress.Indeterminate = true;
            TaskbarProgress.SetState(this, TaskbarState.Indeterminate);
            Navigate(typeof(ActivityPage));
        }

        private void StopDownload()
        {
            if (!_engine.IsRunning)
                return;
            _engine.Stop();
            _status.SetText("Stopping…", "Finishing the current request and saving resume information.", Theme.Warning, Glyphs.Stop);
            _status.Stop.Enabled = false;
            TaskbarProgress.SetState(this, TaskbarState.Paused);
        }

        private void Pump()
        {
            List<KeyValuePair<string, LogKind>> chunks = ConsoleRouter.Drain();
            if (chunks.Count > 0)
            {
                foreach (var c in chunks)
                    _tracker.Feed(c.Key, c.Value);
                _activityPage.Log.Append(chunks);
            }
            _activityPage.UpdateFrom(_tracker);
            if (_engine.IsRunning && _tracker.Changed && !_engine.StopRequested)
            {
                string primary = _tracker.Phase.Length > 0 ? _tracker.Phase : "Downloading…";
                string secondary = _tracker.ItemTotal > 0
                    ? string.Format("{0} of {1}", _tracker.Item, _tracker.ItemTotal)
                    : _tracker.Item > 0 ? string.Format("Item {0}", _tracker.Item) : "";
                if (_tracker.ProfileTotal > 1)
                    secondary = string.Format("Profile {0} of {1}", _tracker.ProfileIndex, _tracker.ProfileTotal) + (secondary.Length > 0 ? "  ·  " + secondary : "");
                if (_tracker.Errors > 0)
                    secondary += string.Format("  ·  {0} error(s)", _tracker.Errors);
                _status.SetText(primary, secondary, Theme.Text, Glyphs.Download);
                if (_tracker.ItemTotal > 0)
                {
                    _status.Progress.Indeterminate = false;
                    _status.Progress.Maximum = _tracker.ItemTotal;
                    _status.Progress.Value = _tracker.Item;
                    TaskbarProgress.SetState(this, TaskbarState.Normal);
                    TaskbarProgress.SetValue(this, _tracker.Item, _tracker.ItemTotal);
                }
                else
                {
                    _status.Progress.Indeterminate = true;
                    TaskbarProgress.SetState(this, TaskbarState.Indeterminate);
                }
            }
            _tracker.Changed = false;
        }

        private void OnFinished(object sender, JobFinishedEventArgs e)
        {
            Pump();
            _activityPage.JobFinished(e, _tracker);
            _status.SetRunning(false);
            _status.Progress.Indeterminate = false;
            string text;
            Color color;
            ActivityPage.Describe(e, _tracker, out text, out color);
            _status.SetText(text, string.Format("{0} item(s) in {1}  ·  {2}", _tracker.Processed, ActivityPage.FormatElapsed(e.Elapsed),
                                                _state.Options.OutputDir), color,
                            color == Theme.Success ? Glyphs.Accept : color == Theme.Error ? Glyphs.Error : Glyphs.Warning);
            _status.Progress.Maximum = 1;
            _status.Progress.Value = 1;
            _status.Progress.BarColor = color;
            bool problem = e.ExitCode != ExitCode.Success && e.ExitCode != ExitCode.UserAborted;
            TaskbarProgress.SetState(this, problem ? TaskbarState.Error : TaskbarState.None);

            if (e.ExitCode == ExitCode.Success || e.ExitCode == ExitCode.NonFatalError)
            {
                if (_state.Options.Mode == LoginMode.Login)
                    _state.VerifiedUser = _state.Options.Username.Trim().ToLowerInvariant();
            }
            if (_nav.SelectedItem != _activityItem)
            {
                long problems = _tracker.Errors + _tracker.Warnings;
                _activityItem.Badge = problems > 0 ? problems.ToString(CultureInfo.InvariantCulture) : "Done";
                _activityItem.BadgeColor = _tracker.Errors > 0 ? Theme.Error : problems > 0 ? Color.FromArgb(160, 120, 30) : Color.FromArgb(40, 130, 60);
            }
            else
            {
                _activityItem.Badge = null;
            }
            _nav.Invalidate();
            UpdateChip();

            if (_closeAfterStop)
            {
                Close();
                return;
            }
            if (_state.Options.NotifyWhenDone && Form.ActiveForm != this)
            {
                Native.FlashUntilFocused(this);
                try
                {
                    (problem ? SystemSounds.Exclamation : SystemSounds.Asterisk).Play();
                }
                catch (Exception)
                {
                }
            }
            if (e.ExitCode == ExitCode.LoginFailure)
                MessageDialog.Show(this, (e.Message ?? "Login failed.") + "\n\nCheck your sign-in settings on the Account page.",
                                   "Could not sign in", DialogIcon.Error);
            else if (e.ExitCode == ExitCode.InitFailure && !string.IsNullOrEmpty(e.Message))
                MessageDialog.Show(this, e.Message, "Could not start", DialogIcon.Error);
        }

        private void UpdateStatusIdle()
        {
            if (_engine.IsRunning || _status == null)
                return;
            JobOptions o = _state.Options;
            string mode = o.Mode == LoginMode.Anonymous ? "anonymous"
                        : o.Mode == LoginMode.Login ? (string.IsNullOrWhiteSpace(o.Username) ? "login" : "@" + o.Username.Trim().ToLowerInvariant())
                        : "cookies from " + JobOptions.BrowserDisplayName(o.Browser);
            string detail = string.Format("{0} target(s)  ·  {1}  ·  {2}", o.Targets.Count, mode,
                                          string.IsNullOrWhiteSpace(o.OutputDir) ? "no folder chosen" : o.OutputDir.Trim());
            JobOptions.ValidationResult v = o.Validate();
            if (o.Targets.Count == 0)
                _status.SetIdle("Add something to download", detail, Theme.TextSecondary, Glyphs.Info);
            else if (!v.Ok)
                _status.SetIdle(v.Errors[0], detail, Theme.Warning, Glyphs.Warning);
            else
                _status.SetIdle("Ready to download", detail, Theme.Text, Glyphs.Accept);
            _status.Start.Enabled = true;
        }

        private void UpdateChip()
        {
            JobOptions o = _state.Options;
            switch (o.Mode)
            {
                case LoginMode.Login:
                    string u = string.IsNullOrWhiteSpace(o.Username) ? "Instagram login" : "@" + o.Username.Trim().ToLowerInvariant();
                    _chip.Set(u, _state.VerifiedUser != null ? "Signed in" : "Instagram login", o.Username, _state.Avatar, _state.VerifiedUser != null);
                    break;
                case LoginMode.BrowserCookies:
                    _chip.Set(_state.VerifiedUser != null ? "@" + _state.VerifiedUser : "Browser cookies",
                              "Cookies from " + JobOptions.BrowserDisplayName(o.Browser), _state.VerifiedUser, _state.Avatar, _state.VerifiedUser != null);
                    break;
                default:
                    _chip.Set("Anonymous", "Not signed in", null, null, false);
                    break;
            }
        }

        // ------------------------------------------------------------------------------ persistence

        private void SaveSettings()
        {
            _state.Options.Save(_settings);
            if (WindowState == FormWindowState.Normal)
                _settings["Window"] = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", Left, Top, Width, Height);
            _settings["WindowMaximized"] = WindowState == FormWindowState.Maximized ? "1" : "0";
            SettingsStore.Save(_settings);
        }

        private void RestoreWindow()
        {
            string w;
            if (_settings.TryGetValue("Window", out w))
            {
                int[] p = w.Split(',').Select(x => { int n; int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out n); return n; }).ToArray();
                if (p.Length == 4 && p[2] > 200 && p[3] > 200)
                {
                    var r = new Rectangle(p[0], p[1], p[2], p[3]);
                    if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(r)))
                    {
                        StartPosition = FormStartPosition.Manual;
                        Bounds = r;
                    }
                }
            }
            string max;
            if (_settings.TryGetValue("WindowMaximized", out max) && max == "1")
                WindowState = FormWindowState.Maximized;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_engine.IsRunning)
            {
                if (_closeAfterStop)
                {
                    e.Cancel = true;
                    return;
                }
                if (MessageDialog.Show(this, "A download is still running. Stop it and exit? It can be resumed the next time you start it.",
                                       "Exit InstaloaderSharp GUI?", DialogIcon.Question, MessageBoxButtons.YesNo,
                                       yesText: "Stop and exit", noText: "Keep running") != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                _closeAfterStop = true;
                e.Cancel = true;
                StopDownload();
                return;
            }
            SaveSettings();
            base.OnFormClosing(e);
        }

        // ------------------------------------------------------------------------------ chrome

        /// <summary>App name and logo at the top of the sidebar.</summary>
        private sealed class Brand : Control
        {
            private Bitmap _logo; // the embedded logo scaled to the on-screen size

            public Brand()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Theme.Sidebar;
                AccessibleRole = AccessibleRole.StaticText;
                AccessibleName = "InstaloaderSharp GUI";
            }

            private Bitmap Logo(int size)
            {
                if (_logo != null && _logo.Width == size)
                    return _logo;
                using (Stream stream = typeof(Brand).Assembly.GetManifestResourceStream("InstaloaderSharpGUI.Logo.png"))
                {
                    if (stream == null)
                        return null;
                    using (var source = new Bitmap(stream))
                    {
                        var scaled = new Bitmap(size, size);
                        using (Graphics g = Graphics.FromImage(scaled))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.DrawImage(source, 0, 0, size, size);
                        }
                        if (_logo != null)
                            _logo.Dispose();
                        _logo = scaled;
                    }
                }
                return _logo;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && _logo != null)
                    _logo.Dispose();
                base.Dispose(disposing);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(BackColor);
                Theme.HighQuality(g);
                int s = Theme.S(36);
                var logo = new Rectangle(Theme.S(18), (Height - s) / 2, s, s);
                Bitmap image = Logo(s);
                if (image != null)
                {
                    // A texture brush fills the rounded shape with anti-aliased edges (clipping would be jagged).
                    using (var path = Theme.RoundRect(logo, Theme.S(10f)))
                    using (var brush = new TextureBrush(image, WrapMode.Clamp))
                    {
                        brush.TranslateTransform(logo.X, logo.Y);
                        g.FillPath(brush, path);
                    }
                }
                int x = logo.Right + Theme.S(12);
                TextRenderer.DrawText(g, "InstaloaderSharp", Theme.BodyStrong, new Rectangle(x, logo.Y - Theme.S(1), Width - x, Theme.S(20)), Theme.Text,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, "GUI  ·  v" + Library.GuiVersion, Theme.Small, new Rectangle(x, logo.Y + Theme.S(18), Width - x, Theme.S(18)), Theme.TextSecondary,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }

        /// <summary>Account summary at the bottom of the sidebar.</summary>
        private sealed class AccountChip : Control
        {
            private readonly Avatar _avatar;
            private string _title = "", _detail = "";
            private bool _ok;
            private bool _hover;

            public AccountChip()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.Selectable, true);
                BackColor = Theme.Sidebar;
                Cursor = Cursors.Hand;
                TabStop = true;
                AccessibleRole = AccessibleRole.PushButton;
                _avatar = new Avatar { Size = new Size(Theme.S(36), Theme.S(36)), Glyph = Glyphs.Contact, Enabled = false };
                Controls.Add(_avatar);
            }

            public void Set(string title, string detail, string user, Image avatar, bool ok)
            {
                _title = title;
                _detail = detail;
                _ok = ok;
                _avatar.Text = string.IsNullOrWhiteSpace(user) ? "" : user.Trim();
                _avatar.Image = avatar;
                AccessibleName = "Account: " + title + ", " + detail;
                Invalidate();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                if (_avatar == null)
                    return;
                _avatar.Location = new Point(Theme.S(20), (Height - _avatar.Height) / 2);
            }

            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                    OnClick(EventArgs.Empty);
            }

            protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
            protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(BackColor);
                Theme.HighQuality(g);
                using (var p = new Pen(Theme.Divider))
                    g.DrawLine(p, Theme.S(12), 0, Width - Theme.S(12), 0);
                var r = new Rectangle(Theme.S(8), Theme.S(8), Width - Theme.S(16), Height - Theme.S(16));
                if (_hover)
                    Theme.FillRound(g, Theme.Blend(Theme.Sidebar, Theme.SurfaceHover, 0.7f), r, Theme.S(6f));
                if (Focused && ShowFocusCues)
                    Theme.DrawRound(g, Theme.AccentText, r, Theme.S(6f), Theme.S(1.5f));
                int x = _avatar.Right + Theme.S(12);
                TextRenderer.DrawText(g, _title, Theme.BodyStrong, new Rectangle(x, Theme.S(14), Width - x - Theme.S(12), Theme.S(20)), Theme.Text,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                int dot = Theme.S(8);
                using (var b = new SolidBrush(_ok ? Theme.Success : Theme.TextDisabled))
                    g.FillEllipse(b, x, Theme.S(40), dot, dot);
                TextRenderer.DrawText(g, _detail, Theme.Small, new Rectangle(x + dot + Theme.S(6), Theme.S(34), Width - x - Theme.S(26), Theme.S(18)), Theme.TextSecondary,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        /// <summary>Always-visible footer: state, progress and the primary Start/Stop action.</summary>
        private sealed class StatusBar : Control
        {
            public readonly FlatButton Start;
            public readonly FlatButton Stop;
            public readonly ProgressBarEx Progress;
            private string _primary = "", _secondary = "", _glyph = Glyphs.Info;
            private Color _color = Theme.Text;
            private bool _running;

            public StatusBar()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Theme.Sidebar;
                Height = Theme.S(68);
                Start = new FlatButton("Start download", ButtonKind.Primary, Glyphs.Play);
                Start.Width = Math.Max(Start.Width, Theme.S(170));
                Start.Height = Theme.S(36);
                Stop = new FlatButton("Stop", ButtonKind.Danger, Glyphs.Stop) { Visible = false };
                Stop.Width = Math.Max(Stop.Width, Theme.S(110));
                Stop.Height = Theme.S(36);
                Progress = new ProgressBarEx { Visible = false };
                Controls.Add(Start);
                Controls.Add(Stop);
                Controls.Add(Progress);
                AccessibleRole = AccessibleRole.StatusBar;
            }

            public void SetRunning(bool running)
            {
                _running = running;
                Start.Visible = !running;
                Stop.Visible = running;
                Stop.Enabled = true;
                Progress.Visible = true;
                Progress.BarColor = Theme.Accent;
                PerformLayout();
            }

            public void SetIdle(string primary, string secondary, Color color, string glyph)
            {
                if (!_running && Progress.Visible && Progress.Indeterminate == false && Progress.Value == Progress.Maximum && Progress.Maximum == 1)
                {
                    // keep the finished bar until the user changes something
                    Progress.Visible = false;
                }
                SetText(primary, secondary, color, glyph);
            }

            public void SetText(string primary, string secondary, Color color, string glyph)
            {
                _primary = primary ?? "";
                _secondary = secondary ?? "";
                _color = color;
                _glyph = glyph;
                AccessibleName = _primary + ". " + _secondary;
                Invalidate();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                if (Progress == null)
                    return; // still constructing
                int pad = Theme.S(24);
                FlatButton action = _running ? Stop : Start;
                Start.Location = new Point(Width - pad - Start.Width, (Height - Start.Height) / 2);
                Stop.Location = new Point(Width - pad - Stop.Width, (Height - Stop.Height) / 2);
                int pw = Math.Min(Theme.S(260), Math.Max(Theme.S(80), Width / 4));
                Progress.SetBounds(action.Left - Theme.S(24) - pw, (Height - Theme.S(6)) / 2, pw, Theme.S(6));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(BackColor);
                using (var p = new Pen(Theme.Divider))
                    g.DrawLine(p, 0, 0, Width, 0);
                int pad = Theme.S(24);
                Theme.DrawGlyph(g, _glyph, Theme.Icon, new Rectangle(pad, 0, Theme.S(20), Height), _color);
                int x = pad + Theme.S(32);
                int right = (Progress.Visible ? Progress.Left : (_running ? Stop.Left : Start.Left)) - Theme.S(16);
                TextRenderer.DrawText(g, _primary, Theme.BodyStrong, new Rectangle(x, Theme.S(13), right - x, Theme.S(22)), _color,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, _secondary, Theme.Small, new Rectangle(x, Theme.S(35), right - x, Theme.S(20)), Theme.TextSecondary,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
                                      TextFormatFlags.PathEllipsis);
            }
        }
    }
}
