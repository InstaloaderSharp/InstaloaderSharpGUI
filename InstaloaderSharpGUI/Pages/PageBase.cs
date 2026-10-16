using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Pages
{
    /// <summary>State shared by all pages.</summary>
    internal sealed class AppState
    {
        public JobOptions Options = new JobOptions();

        /// <summary>Password typed on the Account page. Kept in memory only, never saved.</summary>
        public string Password = "";

        /// <summary>Username of the session verified with "Sign in" / "Check session" (null if unknown).</summary>
        public string VerifiedUser;
        public Image Avatar;

        public DownloadEngine Engine;
        public DarkToolTip ToolTip;
        public Form MainWindow;

        /// <summary>Raised whenever any option changes.</summary>
        public event EventHandler Changed;

        /// <summary>Raised when the sign-in state or method changes.</summary>
        public event EventHandler AccountChanged;

        /// <summary>Asks the main window to navigate to a page by type.</summary>
        public Action<Type> Navigate;

        /// <summary>Adds a target to the download list (from other pages).</summary>
        public Action<string> AddTarget;

        public void NotifyChanged()
        {
            var h = Changed;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        public void NotifyAccountChanged()
        {
            var h = AccountChanged;
            if (h != null)
                h(this, EventArgs.Empty);
            NotifyChanged();
        }

        public bool IsBusy { get { return Engine != null && Engine.IsRunning; } }
    }

    /// <summary>
    /// Base class for pages: a scrolling stack with helpers that create setting rows bound to <see cref="JobOptions"/>.
    /// </summary>
    internal class PageBase : StackPage
    {
        protected readonly AppState State;
        private readonly List<Action> _refreshers = new List<Action>();
        private bool _refreshing;

        public PageBase(AppState state)
        {
            State = state;
        }

        protected JobOptions O { get { return State.Options; } }

        /// <summary>Reloads every bound control from the options (e.g. after importing a command line).</summary>
        public virtual void RefreshFromOptions()
        {
            _refreshing = true;
            try
            {
                foreach (Action a in _refreshers)
                    a();
            }
            finally
            {
                _refreshing = false;
            }
        }

        protected bool Refreshing { get { return _refreshing; } }

        protected void OnRefresh(Action a)
        {
            _refreshers.Add(a);
        }

        protected void Changed()
        {
            if (!_refreshing)
                State.NotifyChanged();
        }

        protected Card AddSection(string title, string description = null)
        {
            Add(new SectionTitle(title), Theme.S(14));
            if (description != null)
                Add(new TextBlock(description));
            return Add(new Card());
        }

        protected void Tip(Control c, string text)
        {
            if (State.ToolTip != null && !string.IsNullOrEmpty(text))
                State.ToolTip.SetToolTip(c, text);
        }

        protected ToggleSwitch Toggle(Card card, string title, string description, Func<JobOptions, bool> get, Action<JobOptions, bool> set,
                                      string glyph = null, string badge = null)
        {
            var row = new SettingRow(title, description, glyph) { Badge = badge };
            var sw = new ToggleSwitch();
            row.Accessory = sw;
            sw.Checked = get(O);
            sw.CheckedChanged += (s, e) =>
            {
                if (_refreshing) return;
                set(O, sw.Checked);
                Changed();
            };
            OnRefresh(() => sw.Checked = get(O));
            card.Add(row);
            return sw;
        }

        protected DarkTextBox TextRow(Card card, string title, string description, Func<JobOptions, string> get, Action<JobOptions, string> set,
                                      string placeholder = null, int width = 0, string glyph = null, bool mono = false, SettingRow existing = null)
        {
            SettingRow row = existing ?? new SettingRow(title, description, glyph);
            var box = new DarkTextBox { Placeholder = placeholder ?? "" };
            if (mono)
                box.Font = Theme.Mono;
            box.Text = get(O) ?? "";
            box.TextChanged += (s, e) =>
            {
                if (_refreshing) return;
                set(O, box.Text);
                Changed();
            };
            OnRefresh(() => box.Text = get(O) ?? "");
            if (width > 0)
            {
                row.Accessory = box;
                row.AccessoryWidth = width;
            }
            else
            {
                row.Content = box;
            }
            if (existing == null)
                card.Add(row);
            return box;
        }

        /// <summary>A path text box with Browse (and optional Open) buttons, shown under the row's title.</summary>
        protected DarkTextBox PathRow(Card card, string title, string description, Func<JobOptions, string> get, Action<JobOptions, string> set,
                                      bool folder, string placeholder = null, string filter = "All files|*.*", bool saveDialog = false,
                                      string glyph = null, SettingRow existing = null)
        {
            SettingRow row = existing ?? new SettingRow(title, description, glyph);
            var stack = new HStack();
            var box = new DarkTextBox { Placeholder = placeholder ?? "" };
            stack.Fill = box;
            stack.Add(box);
            var browse = stack.Add(new FlatButton("Browse…", ButtonKind.Secondary, Glyphs.FolderOpen));
            box.Text = get(O) ?? "";
            box.TextChanged += (s, e) =>
            {
                if (_refreshing) return;
                set(O, box.Text);
                Changed();
            };
            OnRefresh(() => box.Text = get(O) ?? "");
            browse.Click += (s, e) =>
            {
                string current = box.Text.Trim();
                if (folder)
                {
                    using (var dlg = new FolderBrowserDialog { Description = title, ShowNewFolderButton = true })
                    {
                        if (Directory.Exists(current))
                            dlg.SelectedPath = current;
                        if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                            box.Text = dlg.SelectedPath;
                    }
                }
                else
                {
                    FileDialog dlg = saveDialog ? (FileDialog)new SaveFileDialog { OverwritePrompt = false } : new OpenFileDialog { CheckFileExists = false };
                    using (dlg)
                    {
                        dlg.Filter = filter;
                        dlg.Title = title;
                        try
                        {
                            if (current.Length > 0)
                            {
                                dlg.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(current));
                                dlg.FileName = Path.GetFileName(current);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                            box.Text = dlg.FileName;
                    }
                }
            };
            row.Content = stack;
            if (existing == null)
                card.Add(row);
            return box;
        }

        protected static Control Padded(Control child, int horizontal = 16, int vertical = 12)
        {
            return new PaddedHost(child, new Padding(Theme.S(horizontal), Theme.S(vertical), Theme.S(horizontal), Theme.S(vertical)));
        }

        protected void ShowError(string message, string heading = null)
        {
            MessageDialog.Show(this, message, heading, DialogIcon.Error);
        }

        /// <summary>Runs library work off the UI thread and reports back on the UI thread.</summary>
        protected void RunBackground<T>(Func<T> work, Action<T> done, Action<Exception> failed)
        {
            var t = new System.Threading.Thread(() =>
            {
                T result = default(T);
                Exception error = null;
                try
                {
                    result = work();
                }
                catch (Exception e)
                {
                    error = e;
                }
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (error != null) failed(error);
                        else done(result);
                    }));
                }
                catch (Exception)
                {
                    // window closed
                }
            }) { IsBackground = true, Name = "InstaloaderSharp GUI worker" };
            t.Start();
        }

        /// <summary>Prompt usable from worker threads: blocks until the user answers on the UI thread.</summary>
        protected string PromptFromWorker(string message, bool password)
        {
            Func<string> ask = () => MessageDialog.Prompt(this, password ? "Password required" : "Verification required", message, password,
                                                          okText: password ? "Sign in" : "Verify");
            if (InvokeRequired)
                return (string)Invoke(ask);
            return ask();
        }
    }

    /// <summary>Hosts one child with padding, reporting the child's preferred height.</summary>
    internal sealed class PaddedHost : Control
    {
        private readonly Control _child;
        private readonly Padding _pad;

        public PaddedHost(Control child, Padding pad)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.ContainerControl | ControlStyles.SupportsTransparentBackColor, true);
            _child = child;
            _pad = pad;
            BackColor = Color.Transparent;
            Controls.Add(child);
            child.VisibleChanged += (s, e) => { if (Parent != null) Parent.PerformLayout(); };
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = proposedSize.Width > 0 ? proposedSize.Width : Width;
            return new Size(w, _child.PreferredHeight(w - _pad.Horizontal) + _pad.Vertical);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int w = Width - _pad.Horizontal;
            _child.SetBounds(_pad.Left, _pad.Top, w, _child.PreferredHeight(w));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.BackOf(this));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }
    }
}
