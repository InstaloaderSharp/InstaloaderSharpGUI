using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Pages
{
    /// <summary>Live progress, counters, recent files and the full InstaloaderSharp output.</summary>
    internal sealed class ActivityPage : Control
    {
        private readonly AppState _state;
        private readonly PageHeader _header;
        private readonly StatTile _items, _existing, _warnings, _errors, _elapsed;
        private readonly Label _phase;
        private readonly Label _current;
        private readonly ProgressBarEx _profileBar;
        private readonly ProgressBarEx _itemBar;
        private readonly Panel _logFrame;
        private readonly LogView _log;
        private readonly HStack _toolbar;
        private readonly ToggleSwitch _errorsOnly;
        private readonly ToggleSwitch _follow;
        private readonly ListView _recent;
        private readonly ImageList _thumbs;
        private FileSystemWatcher _watcher;
        private readonly Queue<string> _newFiles = new Queue<string>();
        private readonly object _filesLock = new object();
        private DateTime _startedAt;
        private bool _running;

        public ActivityPage(AppState state)
        {
            _state = state;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Window;

            _header = new PageHeader("Activity", "Progress and full output of the current download.");
            Controls.Add(_header);

            _items = new StatTile("Items", Glyphs.Photo);
            _existing = new StatTile("Already saved", Glyphs.Accept);
            _warnings = new StatTile("Warnings", Glyphs.Warning);
            _errors = new StatTile("Errors", Glyphs.Error);
            _elapsed = new StatTile("Elapsed", Glyphs.History);
            Controls.AddRange(new Control[] { _items, _existing, _warnings, _errors, _elapsed });

            _phase = new Label { AutoSize = false, Font = Theme.BodyStrong, ForeColor = Theme.Text, BackColor = Theme.Window, AutoEllipsis = true, Text = "Idle" };
            _current = new Label { AutoSize = false, Font = Theme.Small, ForeColor = Theme.TextSecondary, BackColor = Theme.Window, AutoEllipsis = true, Text = "Nothing is being downloaded." };
            _profileBar = new ProgressBarEx { BarColor = Theme.Info };
            _itemBar = new ProgressBarEx();
            Controls.AddRange(new Control[] { _phase, _current, _profileBar, _itemBar });

            _toolbar = new HStack();
            var label = new Label { Text = "Output", Font = Theme.Section, ForeColor = Theme.Text, AutoSize = true, BackColor = Theme.Window };
            _toolbar.Add(label);
            var errorsOnlyLabel = new Label { Text = "Problems only", ForeColor = Theme.TextSecondary, AutoSize = true, BackColor = Theme.Window, Padding = new Padding(Theme.S(12), Theme.S(6), 0, 0) };
            _toolbar.Add(errorsOnlyLabel);
            _errorsOnly = _toolbar.Add(new ToggleSwitch { AccessibleName = "Show problems only" });
            var followLabel = new Label { Text = "Auto-scroll", ForeColor = Theme.TextSecondary, AutoSize = true, BackColor = Theme.Window, Padding = new Padding(Theme.S(8), Theme.S(6), 0, 0) };
            _toolbar.Add(followLabel);
            _follow = _toolbar.Add(new ToggleSwitch { Checked = true, AccessibleName = "Auto-scroll" });
            var clear = _toolbar.Add(new FlatButton("Clear", ButtonKind.Subtle, Glyphs.Clear), true);
            var save = _toolbar.Add(new FlatButton("Save…", ButtonKind.Subtle, Glyphs.Save), true);
            var copy = _toolbar.Add(new FlatButton("Copy", ButtonKind.Subtle, Glyphs.Copy), true);
            var openFolder = _toolbar.Add(new FlatButton("Open folder", ButtonKind.Subtle, Glyphs.FolderOpen), true);
            Controls.Add(_toolbar);

            _logFrame = new Panel { BackColor = Theme.Input, Padding = new Padding(Theme.S(12), Theme.S(8), Theme.S(4), Theme.S(8)) };
            _log = new LogView { Dock = DockStyle.Fill };
            _logFrame.Controls.Add(_log);
            Controls.Add(_logFrame);

            _thumbs = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(Theme.S(72), Theme.S(72)) };
            _recent = new ListView
            {
                View = View.LargeIcon,
                LargeImageList = _thumbs,
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Surface,
                ForeColor = Theme.TextSecondary,
                MultiSelect = false,
                Font = Theme.Small,
                ShowItemToolTips = true
            };
            Native.UseDarkScrollBars(_recent);
            _recent.ItemActivate += (s, e) =>
            {
                if (_recent.SelectedItems.Count > 0)
                    DownloadPage.Open((string)_recent.SelectedItems[0].Tag);
            };
            Controls.Add(_recent);

            _errorsOnly.CheckedChanged += (s, e) => _log.ErrorsOnly = _errorsOnly.Checked;
            _follow.CheckedChanged += (s, e) => _log.AutoScroll = _follow.Checked;
            clear.Click += (s, e) => _log.ClearLog();
            copy.Click += (s, e) =>
            {
                string t = _log.PlainText;
                if (t.Length > 0)
                    Clipboard.SetText(t);
            };
            save.Click += (s, e) => SaveLog();
            openFolder.Click += (s, e) => DownloadPage.OpenFolder(_state.Options.OutputDir);
        }

        public LogView Log { get { return _log; } }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (_recent == null)
                return; // still constructing
            int pad = Theme.S(32);
            int w = Math.Max(Theme.S(300), ClientSize.Width - pad * 2);
            int y = Theme.S(24);
            int hh = _header.PreferredHeight(w);
            _header.SetBounds(pad, y, w, hh);
            y += hh + Theme.S(8);

            int gap = Theme.S(10);
            var tiles = new[] { _items, _existing, _warnings, _errors, _elapsed };
            int tw = (w - gap * (tiles.Length - 1)) / tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
                tiles[i].SetBounds(pad + i * (tw + gap), y, tw, Theme.S(72));
            y += Theme.S(72) + Theme.S(16);

            _phase.SetBounds(pad, y, w, Theme.S(22));
            y += Theme.S(24);
            _profileBar.SetBounds(pad, y, w, Theme.S(4));
            _profileBar.Visible = _profileBar.Maximum > 1 && _running;
            if (_profileBar.Visible)
                y += Theme.S(10);
            _itemBar.SetBounds(pad, y, w, Theme.S(6));
            y += Theme.S(12);
            _current.SetBounds(pad, y, w, Theme.S(20));
            y += Theme.S(32);

            int recentW = ClientSize.Width >= Theme.S(980) ? Theme.S(220) : 0;
            int logW = w - (recentW > 0 ? recentW + gap : 0);
            int th = _toolbar.PreferredHeight(logW);
            _toolbar.SetBounds(pad, y, logW, th);
            y += th + Theme.S(8);

            int bottom = ClientSize.Height - Theme.S(24);
            _logFrame.SetBounds(pad, y, logW, Math.Max(Theme.S(120), bottom - y));
            _recent.Visible = recentW > 0;
            if (recentW > 0)
                _recent.SetBounds(pad + w - recentW, y, recentW, Math.Max(Theme.S(120), bottom - y));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_recent.Visible)
            {
                // caption above the recent files list
                var r = new Rectangle(_recent.Left, _toolbar.Top + _toolbar.Height - Theme.S(32), _recent.Width, Theme.S(32));
                TextRenderer.DrawText(e.Graphics, "Recent files", Theme.Section, r, Theme.Text,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }

        // -------------------------------------------------------------------------------- job lifecycle

        public void JobStarted(string outputDir)
        {
            _running = true;
            _startedAt = DateTime.Now;
            _phase.Text = "Starting…";
            _phase.ForeColor = Theme.Text;
            _current.Text = "";
            _itemBar.Indeterminate = true;
            _itemBar.BarColor = Theme.Accent;
            _profileBar.Value = 0;
            StartWatching(outputDir);
            PerformLayout();
        }

        public void JobFinished(JobFinishedEventArgs e, ActivityTracker t)
        {
            _running = false;
            _itemBar.Indeterminate = false;
            _itemBar.Maximum = 1;
            _itemBar.Value = 1;
            StopWatching();
            string text;
            Color color;
            Describe(e, t, out text, out color);
            _phase.Text = text;
            _phase.ForeColor = color;
            _itemBar.BarColor = color == Theme.Success ? Theme.Success : color;
            _current.Text = string.Format("Finished in {0}.", FormatElapsed(e.Elapsed));
            PerformLayout();
        }

        public static void Describe(JobFinishedEventArgs e, ActivityTracker t, out string text, out Color color)
        {
            switch (e.ExitCode)
            {
                case InstaloaderSharp.Program.ExitCode.Success:
                    text = "Download complete";
                    color = Theme.Success;
                    break;
                case InstaloaderSharp.Program.ExitCode.NonFatalError:
                    text = string.Format("Completed with {0} problem(s) - see the output below", Math.Max(1, t.Errors + t.Warnings));
                    color = Theme.Warning;
                    break;
                case InstaloaderSharp.Program.ExitCode.UserAborted:
                    text = "Stopped. Start again to resume where it left off.";
                    color = Theme.Warning;
                    break;
                case InstaloaderSharp.Program.ExitCode.DownloadAborted:
                    text = "Download aborted" + (string.IsNullOrEmpty(e.Message) ? "" : ": " + e.Message);
                    color = Theme.Error;
                    break;
                case InstaloaderSharp.Program.ExitCode.LoginFailure:
                    text = "Login failed" + (string.IsNullOrEmpty(e.Message) ? "" : ": " + e.Message);
                    color = Theme.Error;
                    break;
                case InstaloaderSharp.Program.ExitCode.InitFailure:
                    text = "Could not start" + (string.IsNullOrEmpty(e.Message) ? "" : ": " + e.Message);
                    color = Theme.Error;
                    break;
                default:
                    text = "Failed" + (string.IsNullOrEmpty(e.Message) ? "" : ": " + e.Message);
                    color = Theme.Error;
                    break;
            }
        }

        public static string FormatElapsed(TimeSpan t)
        {
            return t.TotalHours >= 1 ? string.Format("{0}:{1:mm\\:ss}", (int)t.TotalHours, t) : t.ToString("m\\:ss");
        }

        /// <summary>Called by the main window's UI timer with fresh tracker state.</summary>
        public void UpdateFrom(ActivityTracker t)
        {
            if (_running)
                _elapsed.Value = FormatElapsed(DateTime.Now - _startedAt);
            if (!t.Changed)
            {
                DrainFiles();
                return;
            }
            _items.Value = t.Processed.ToString("N0");
            _existing.Value = t.Existing.ToString("N0");
            _warnings.Value = t.Warnings.ToString("N0");
            _warnings.Accent = t.Warnings > 0 ? Theme.Warning : (Color?)null;
            _errors.Value = t.Errors.ToString("N0");
            _errors.Accent = t.Errors > 0 ? Theme.Error : (Color?)null;
            if (_running)
            {
                string phase = t.Phase.Length > 0 ? t.Phase : "Working…";
                if (t.ProfileTotal > 1)
                    phase += string.Format("   ·   profile {0} of {1}", t.ProfileIndex, t.ProfileTotal);
                _phase.Text = phase;
                if (t.ItemTotal > 0)
                {
                    _itemBar.Indeterminate = false;
                    _itemBar.Maximum = t.ItemTotal;
                    _itemBar.Value = t.Item;
                    _current.Text = string.Format("{0} of {1}   {2}", t.Item, t.ItemTotal, t.CurrentItem);
                }
                else
                {
                    _itemBar.Indeterminate = true;
                    _current.Text = t.Item > 0 ? string.Format("Item {0}   {1}", t.Item, t.CurrentItem) : t.CurrentItem;
                }
                bool showProfiles = t.ProfileTotal > 1;
                if (showProfiles)
                {
                    _profileBar.Maximum = t.ProfileTotal;
                    _profileBar.Value = Math.Max(0, t.ProfileIndex - 1);
                }
                if (_profileBar.Visible != showProfiles)
                    PerformLayout();
            }
            DrainFiles();
        }

        public void ResetStats()
        {
            foreach (StatTile tile in new[] { _items, _existing, _warnings, _errors, _elapsed })
            {
                tile.Value = "0";
                tile.Accent = null;
            }
            _elapsed.Value = "0:00";
        }

        private void SaveLog()
        {
            using (var dlg = new SaveFileDialog
            {
                Title = "Save output",
                Filter = "Text files (*.txt)|*.txt|All files|*.*",
                FileName = "instaloader-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log.txt"
            })
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    File.WriteAllText(dlg.FileName, _log.PlainText.Replace("\n", Environment.NewLine).Replace("\r\r", "\r"), new UTF8Encoding(false));
            }
        }

        // -------------------------------------------------------------------------------- recent files

        private static readonly string[] MediaExtensions = { ".jpg", ".jpeg", ".png", ".mp4", ".webp", ".heic" };

        private void StartWatching(string dir)
        {
            StopWatching();
            try
            {
                Directory.CreateDirectory(dir);
                _watcher = new FileSystemWatcher(dir) { IncludeSubdirectories = true, NotifyFilter = NotifyFilters.FileName };
                _watcher.Created += (s, e) => OnFile(e.FullPath);
                _watcher.Renamed += (s, e) => OnFile(e.FullPath);
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception)
            {
                _watcher = null;
            }
        }

        private void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnFile(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (!MediaExtensions.Contains(ext) || path.EndsWith(".temp", StringComparison.OrdinalIgnoreCase))
                return;
            lock (_filesLock)
            {
                _newFiles.Enqueue(path);
                while (_newFiles.Count > 50)
                    _newFiles.Dequeue();
            }
        }

        private void DrainFiles()
        {
            List<string> files;
            lock (_filesLock)
            {
                if (_newFiles.Count == 0)
                    return;
                files = _newFiles.ToList();
                _newFiles.Clear();
            }
            _recent.BeginUpdate();
            foreach (string f in files)
            {
                if (_recent.Items.ContainsKey(f))
                    continue;
                int imageIndex = AddThumb(f);
                var item = new ListViewItem(Path.GetFileName(f), imageIndex) { Name = f, Tag = f, ToolTipText = f };
                _recent.Items.Insert(0, item);
            }
            while (_recent.Items.Count > 60)
            {
                _recent.Items.RemoveAt(_recent.Items.Count - 1);
            }
            _recent.EndUpdate();
        }

        private int AddThumb(string path)
        {
            Size size = _thumbs.ImageSize;
            var bmp = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Theme.SurfacePressed);
                Theme.HighQuality(g);
                string ext = Path.GetExtension(path).ToLowerInvariant();
                bool drawn = false;
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(path);
                        using (var ms = new MemoryStream(data))
                        using (Image img = Image.FromStream(ms))
                        {
                            float scale = Math.Max((float)size.Width / img.Width, (float)size.Height / img.Height);
                            float w = img.Width * scale, h = img.Height * scale;
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(img, (size.Width - w) / 2, (size.Height - h) / 2, w, h);
                            drawn = true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!drawn)
                    Theme.DrawGlyph(g, ext == ".mp4" ? Glyphs.Video : Glyphs.Photo, Theme.IconLarge, new Rectangle(Point.Empty, size), Theme.TextSecondary);
            }
            _thumbs.Images.Add(bmp);
            if (_thumbs.Images.Count > 200)
            {
                // Rebuild occasionally to bound memory (indices shift, so clear the list as well).
                _recent.Items.Clear();
                _thumbs.Images.Clear();
                _thumbs.Images.Add(bmp);
            }
            return _thumbs.Images.Count - 1;
        }

        /// <summary>Small statistic tile (label + big number).</summary>
        private sealed class StatTile : Control
        {
            private readonly string _glyph;
            private string _value = "0";
            private Color? _accent;

            public StatTile(string label, string glyph)
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                Text = label;
                _glyph = glyph;
                AccessibleRole = AccessibleRole.StaticText;
            }

            public string Value
            {
                get { return _value; }
                set
                {
                    if (_value == value) return;
                    _value = value;
                    AccessibleName = Text + ": " + value;
                    Invalidate();
                }
            }

            public Color? Accent
            {
                get { return _accent; }
                set { if (_accent != value) { _accent = value; Invalidate(); } }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(Theme.Window);
                Theme.HighQuality(g);
                var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
                Theme.FillRound(g, Theme.Surface, r, Theme.S(8f));
                if (_accent.HasValue)
                    Theme.DrawRound(g, Theme.Blend(Theme.Surface, _accent.Value, 0.5f), r, Theme.S(8f));
                Color c = _accent ?? Theme.TextSecondary;
                int pad = Theme.S(14);
                Theme.DrawGlyph(g, _glyph, Theme.IconSmall, new Rectangle(pad, Theme.S(12), Theme.S(14), Theme.S(18)), c);
                TextRenderer.DrawText(g, Text, Theme.Small, new Rectangle(pad + Theme.S(20), Theme.S(12), Width - pad * 2, Theme.S(18)), Theme.TextSecondary,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, _value, Theme.Display, new Rectangle(pad - Theme.S(1), Theme.S(32), Width - pad * 2, Theme.S(30)),
                                      _accent ?? Theme.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }
    }
}
