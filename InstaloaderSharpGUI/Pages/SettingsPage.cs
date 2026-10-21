using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using InstaloaderSharp;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Pages
{
    /// <summary>Advanced options: naming, text templates, resuming, networking, command line import/export and about.</summary>
    internal sealed class SettingsPage : PageBase
    {
        private readonly DarkTextBox _command;
        private readonly DarkTextBox _resumePrefix;

        /// <summary>Placeholders offered by "Insert" for the name patterns: {target}, {profile}, {filename} or Post properties.</summary>
        internal static readonly string[][] PatternTokens =
        {
            new[] { "{target}", "The target you specified: profile name, #hashtag, :feed…" },
            new[] { "{profile}", "Owner's profile name" },
            new[] { "{owner_username}", "Owner's username" },
            new[] { "{owner_id}", "Owner's unique ID" },
            new[] { "{shortcode}", "Post shortcode" },
            new[] { "{mediaid}", "Media ID" },
            new[] { "{typename}", "Type, e.g. GraphImage, GraphSidecar" },
            new[] { "{date_utc}", "Date (UTC), e.g. 2024-01-31_18-05-00" },
            new[] { "{date_utc:%Y-%m-%d}", "Date (UTC) with custom format" },
            new[] { "{date_local}", "Date in local time" },
            new[] { "{date_utc:%Y}", "Year (UTC)" },
            new[] { "{filename}", "Original file name on Instagram's server" },
        };

        /// <summary>Placeholders offered by "Insert" for the caption templates: Post properties.</summary>
        internal static readonly string[][] TxtTokens =
        {
            new[] { "{caption}", "Post caption" },
            new[] { "{likes}", "Number of likes" },
            new[] { "{comments}", "Number of comments" },
            new[] { "{date_utc}", "Date (UTC)" },
            new[] { "{owner_username}", "Owner's username" },
            new[] { "{shortcode}", "Shortcode" },
            new[] { "{location}", "Location" },
            new[] { "{caption_hashtags}", "Hashtags in the caption" },
            new[] { "{caption_mentions}", "Mentions in the caption" },
            new[] { "{tagged_users}", "Tagged users" },
            new[] { "{video_view_count}", "Video views" },
        };

        public SettingsPage(AppState state) : base(state)
        {
            Add(new PageHeader("Settings", "Advanced options. The defaults work well for most people - every option maps to an InstaloaderSharp command line option."));

            // ------------------------------------------------------------------ Naming
            Card naming = AddSection("File naming");
            PatternRow(naming, "Folder name pattern", "Name of the folder posts are stored in. Default: {target}.  (--dirname-pattern)",
                       o => o.DirnamePattern, (o, v) => o.DirnamePattern = v, "{target}");
            PatternRow(naming, "File name pattern", "Prefix of post and story file names, relative to the folder. Default: {date_utc}_UTC.  (--filename-pattern)",
                       o => o.FilenamePattern, (o, v) => o.FilenamePattern = v, "{date_utc}_UTC");
            PatternRow(naming, "Title pattern", "Prefix of profile pictures, hashtag pictures and highlight covers. Default: {date_utc}_UTC_{typename}.  (--title-pattern)",
                       o => o.TitlePattern, (o, v) => o.TitlePattern = v, "{date_utc}_UTC_{typename}");
            Toggle(naming, "Sanitize paths", "Make file and folder names valid on both Windows and Unix.  (--sanitize-paths)",
                   o => o.SanitizePaths, (o, v) => o.SanitizePaths = v, Glyphs.Shield);

            // ------------------------------------------------------------------ Text files
            Card txt = AddSection("Caption text files");
            TemplateRow(txt, "Post text template",
                        "What to write into the .txt file of each post. Default: {caption}. Each line becomes one line in the file.  (--post-metadata-txt)",
                        o => o.PostMetadataTxt, (o, v) => o.PostMetadataTxt = v);
            TemplateRow(txt, "Story item text template",
                        "What to write into the .txt file of each story item. Empty: no text file.  (--storyitem-metadata-txt)",
                        o => o.StoryitemMetadataTxt, (o, v) => o.StoryitemMetadataTxt = v);

            // ------------------------------------------------------------------ Resume
            Card resume = AddSection("Resuming interrupted downloads");
            var resumeSwitch = Toggle(resume, "Resume interrupted downloads",
                                      "When a download is stopped, save where it was and continue from there next time.  (--no-resume when off)",
                                      o => o.Resume, (o, v) => o.Resume = v, Glyphs.Play);
            _resumePrefix = TextRow(resume, "Resume file prefix", "Prefix of the files that store resume information.  (--resume-prefix)",
                                    o => o.ResumePrefix, (o, v) => o.ResumePrefix = v, "iterator", Theme.S(200), Glyphs.Document);
            var aged = Toggle(resume, "Use aged resume files", "Also resume from resume files that are older than the last successful download.  (--use-aged-resume-files)",
                              o => o.UseAgedResumeFiles, (o, v) => o.UseAgedResumeFiles = v, Glyphs.History);
            Action syncResume = () => { _resumePrefix.Parent.Enabled = resumeSwitch.Checked; aged.Parent.Enabled = resumeSwitch.Checked; };
            resumeSwitch.CheckedChanged += (s, e) => syncResume();
            OnRefresh(syncResume);
            syncResume();

            // ------------------------------------------------------------------ Network
            Card net = AddSection("Connection");
            Toggle(net, "Pause between requests",
                   "Wait between requests to respect Instagram's rate limits. Turning this off greatly increases the risk of being blocked.  (--no-sleep when off)",
                   o => o.Sleep, (o, v) => o.Sleep = v, Glyphs.History);
            Toggle(net, "Use iPhone API", "Download higher quality versions of pictures and videos through the iPhone API when available.  (--no-iphone when off)",
                   o => o.Iphone, (o, v) => o.Iphone = v, Glyphs.Photo);
            TextRow(net, "Maximum connection attempts", "Retries before a request is given up. 0 = retry forever.  (--max-connection-attempts)",
                    o => o.MaxConnectionAttempts, (o, v) => o.MaxConnectionAttempts = v, "3", Theme.S(120), Glyphs.Refresh);
            TextRow(net, "Request timeout", "Seconds to wait before a request times out.  (--request-timeout)",
                    o => o.RequestTimeout, (o, v) => o.RequestTimeout = v, "300", Theme.S(120), Glyphs.History);
            TextRow(net, "Abort on HTTP status codes", "Comma-separated status codes that stop the download immediately, e.g. 302,400,429.  (--abort-on)",
                    o => o.AbortOn, (o, v) => o.AbortOn = v, "None", Theme.S(200), Glyphs.Stop);
            TextRow(net, "User agent", "User agent for HTTP requests. Leave empty for the default.  (--user-agent)",
                    o => o.UserAgent, (o, v) => o.UserAgent = v, InstaloaderContext.DefaultUserAgent(), 0, Glyphs.Globe);

            // ------------------------------------------------------------------ Behaviour
            Card behaviour = AddSection("Behaviour");
            Toggle(behaviour, "Quiet mode", "Only show errors in the output, no progress messages.  (--quiet)",
                   o => o.Quiet, (o, v) => o.Quiet = v, Glyphs.Info);
            Toggle(behaviour, "Keep the computer awake", "Prevent sleep while a download is running.",
                   o => o.PreventSleep, (o, v) => o.PreventSleep = v, Glyphs.Refresh);
            Toggle(behaviour, "Notify when finished", "Flash the taskbar button and play a sound when a download ends while the window is in the background.",
                   o => o.NotifyWhenDone, (o, v) => o.NotifyWhenDone = v, Glyphs.Info);

            // ------------------------------------------------------------------ Command line
            Card cli = AddSection("Command line");
            var cmdRow = new SettingRow("Equivalent InstaloaderSharp command",
                                        "Run the same download from a script or scheduled task, or import a command line you already use.", Glyphs.Code);
            _command = new DarkTextBox { Multiline = true, ReadOnly = true, Font = Theme.Mono, MultilineHeight = Theme.S(76) };
            var cmdStack = new CommandBox(_command);
            var copy = cmdStack.Buttons.Add(new FlatButton("Copy", ButtonKind.Secondary, Glyphs.Copy));
            var export = cmdStack.Buttons.Add(new FlatButton("Save args file…", ButtonKind.Secondary, Glyphs.Save));
            var import = cmdStack.Buttons.Add(new FlatButton("Import command line…", ButtonKind.Secondary, Glyphs.Paste));
            var importFile = cmdStack.Buttons.Add(new FlatButton("Import args file…", ButtonKind.Secondary, Glyphs.Import));
            var reset = cmdStack.Buttons.Add(new FlatButton("Reset to defaults", ButtonKind.Subtle, Glyphs.Refresh), true);
            cmdRow.Content = cmdStack;
            cli.Add(cmdRow);
            copy.Click += (s, e) => Clipboard.SetText(O.ToCommandLine());
            export.Click += (s, e) => ExportArgs();
            import.Click += (s, e) => ImportCommandLine();
            importFile.Click += (s, e) => ImportArgsFile();
            reset.Click += (s, e) => ResetDefaults();
            Tip(export, "Saves the options as an InstaloaderSharp argument file. Run it with: InstaloaderSharp +args.txt");

            // ------------------------------------------------------------------ About
            Card about = AddSection("About");
            var gui = Assembly.GetExecutingAssembly().GetName().Version;
            var guiRow = new SettingRow("InstaloaderSharp GUI " + gui.ToString(3),
                                        "Graphical front end that drives the InstaloaderSharp library directly.", Glyphs.Info);
            var guiGh = new FlatButton("GitHub", ButtonKind.Subtle, Glyphs.OpenInNew);
            guiGh.Click += (s, e) => DownloadPage.Open("https://github.com/InstaloaderSharp/InstaloaderSharpGUI");
            guiRow.Accessory = guiGh;
            about.Add(guiRow);
            var libRow = new SettingRow("InstaloaderSharp " + Library.Version,
                                        "C# version of Instaloader.", Glyphs.Code);
            var gh = new FlatButton("GitHub", ButtonKind.Subtle, Glyphs.OpenInNew);
            gh.Click += (s, e) => DownloadPage.Open("https://github.com/InstaloaderSharp/InstaloaderSharp");
            libRow.Accessory = gh;
            about.Add(libRow);
            var dataRow = new SettingRow("Data folders", "Sessions and stamps: " + Library.ConfigDir + "\nGUI settings: " + SettingsStore.Folder, Glyphs.Folder);
            var openData = new FlatButton("Open", ButtonKind.Subtle, Glyphs.FolderOpen);
            openData.Click += (s, e) => DownloadPage.OpenFolder(Library.ConfigDir);
            dataRow.Accessory = openData;
            about.Add(dataRow);
            var helpRow = new SettingRow("Documentation", "InstaloaderSharp GUI documentation.", Glyphs.Help);
            var docs = new FlatButton("Open docs", ButtonKind.Subtle, Glyphs.OpenInNew);
            docs.Click += (s, e) => DownloadPage.Open("https://github.com/InstaloaderSharp/InstaloaderSharpGUI/blob/main/README.md");
            helpRow.Accessory = docs;
            about.Add(helpRow);

            state.Changed += (s, e) => UpdateCommand();
            UpdateCommand();
        }

        private void UpdateCommand()
        {
            string cmd = O.ToCommandLine();
            if (_command.Text != cmd)
                _command.Text = cmd;
        }

        private void PatternRow(Card card, string title, string description, Func<JobOptions, string> get, Action<JobOptions, string> set, string placeholder)
        {
            var row = new SettingRow(title, description, Glyphs.Folder);
            var stack = new HStack();
            var box = new DarkTextBox { Placeholder = placeholder, Font = Theme.Mono };
            stack.Fill = box;
            stack.Add(box);
            var tokens = stack.Add(new FlatButton("Insert", ButtonKind.Secondary, Glyphs.ChevronDown));
            ContextMenuStrip menu = TokenMenu(PatternTokens, box);
            tokens.Click += (s, e) => menu.ShowBelow(tokens);
            box.Text = get(O) ?? "";
            box.TextChanged += (s, e) =>
            {
                if (Refreshing) return;
                set(O, box.Text);
                Changed();
            };
            OnRefresh(() => box.Text = get(O) ?? "");
            row.Content = stack;
            card.Add(row);
        }

        private void TemplateRow(Card card, string title, string description, Func<JobOptions, string> get, Action<JobOptions, string> set)
        {
            var row = new SettingRow(title, description, Glyphs.Document);
            var box = new DarkTextBox { Multiline = true, Font = Theme.Mono, MultilineHeight = Theme.S(64), Placeholder = "Default" };
            var stack = new HStack();
            stack.Fill = box;
            stack.Add(box);
            var tokens = stack.Add(new FlatButton("Insert", ButtonKind.Secondary, Glyphs.ChevronDown));
            ContextMenuStrip menu = TokenMenu(TxtTokens, box);
            tokens.Click += (s, e) => menu.ShowBelow(tokens);
            box.Text = (get(O) ?? "").Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            box.TextChanged += (s, e) =>
            {
                if (Refreshing) return;
                set(O, box.Text);
                Changed();
            };
            OnRefresh(() => box.Text = (get(O) ?? "").Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
            row.Content = stack;
            card.Add(row);
        }

        private static ContextMenuStrip TokenMenu(string[][] tokens, DarkTextBox box)
        {
            ContextMenuStrip menu = Menus.Create();
            foreach (string[] t in tokens)
            {
                string token = t[0];
                menu.Item(token + "   " + t[1], (s, e) => box.InsertAtCaret(token));
            }
            return menu;
        }

        // -------------------------------------------------------------------------- import / export

        private void ExportArgs()
        {
            using (var dlg = new SaveFileDialog
            {
                Title = "Save InstaloaderSharp argument file",
                Filter = "Argument files (*.txt)|*.txt|All files|*.*",
                FileName = "args.txt"
            })
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK)
                    return;
                // One argument per line, as read by InstaloaderSharp's "+file" syntax.
                File.WriteAllLines(dlg.FileName, O.ToArgs(), new UTF8Encoding(false));
                MessageDialog.Show(this, "Run it with:\n\nInstaloaderSharp +\"" + dlg.FileName + "\"\n\nRelative paths are resolved from the folder you run it in.",
                                   "Argument file saved", DialogIcon.Success);
            }
        }

        private void ImportCommandLine()
        {
            string cmd = MessageDialog.Prompt(this, "Import command line",
                                              "Paste an InstaloaderSharp (or Instaloader) command line. All options and targets will be applied; " +
                                              "options that are not mentioned are reset to their defaults.",
                                              false, "", "Import");
            if (cmd == null || cmd.Trim().Length == 0)
                return;
            Apply(Library.SplitCommandLine(cmd));
        }

        private void ImportArgsFile()
        {
            using (var dlg = new OpenFileDialog
            {
                Title = "Import InstaloaderSharp argument file",
                Filter = "Argument files (*.txt)|*.txt|All files|*.*"
            })
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK)
                    return;
                string previous = Environment.CurrentDirectory;
                try
                {
                    // The library resolves "+file" itself (one argument per line, nested files allowed).
                    Environment.CurrentDirectory = Path.GetDirectoryName(dlg.FileName);
                    Apply(new[] { "+" + Path.GetFileName(dlg.FileName) });
                }
                finally
                {
                    Environment.CurrentDirectory = previous;
                }
            }
        }

        private void Apply(string[] args)
        {
            if (State.IsBusy)
            {
                ShowError("Wait until the current download has finished.");
                return;
            }
            string error;
            Dictionary<string, object> parsed = Library.ParseCommandLine(args, out error);
            if (parsed == null)
            {
                ShowError(error, "The command line could not be imported");
                return;
            }
            var fresh = new JobOptions { OutputDir = O.OutputDir, PreventSleep = O.PreventSleep, NotifyWhenDone = O.NotifyWhenDone };
            fresh.ApplyParsedArgs(parsed);
            CopyInto(fresh, O);
            State.VerifiedUser = null;
            State.Avatar = null;
            if (RefreshAll != null)
                RefreshAll();
            State.NotifyAccountChanged();
            MessageDialog.Show(this, string.Format("{0} target(s) and all options were applied.", O.Targets.Count),
                               "Command line imported", DialogIcon.Success);
        }

        private void ResetDefaults()
        {
            if (MessageDialog.Show(this, "Reset every option to its default? Your targets, download folder and sign-in method are kept.",
                                   "Reset to defaults", DialogIcon.Question, MessageBoxButtons.YesNo, yesText: "Reset", noText: "Cancel") != DialogResult.Yes)
                return;
            var fresh = new JobOptions
            {
                Targets = O.Targets,
                OutputDir = O.OutputDir,
                Mode = O.Mode,
                Username = O.Username,
                SessionFile = O.SessionFile,
                Browser = O.Browser,
                CookieFile = O.CookieFile
            };
            CopyInto(fresh, O);
            if (RefreshAll != null)
                RefreshAll();
            State.NotifyChanged();
        }

        private static void CopyInto(JobOptions from, JobOptions to)
        {
            var d = new Dictionary<string, string>();
            from.Save(d);
            to.Targets = new List<string>(from.Targets);
            to.Load(d);
            to.Targets = new List<string>(from.Targets);
        }

        /// <summary>Set by the main window: refreshes every page from the options.</summary>
        public Action RefreshAll;

        /// <summary>Read-only command box with a row of buttons underneath.</summary>
        private sealed class CommandBox : Control
        {
            private readonly DarkTextBox _box;
            public readonly HStack Buttons = new HStack();

            public CommandBox(DarkTextBox box)
            {
                _box = box;
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                BackColor = System.Drawing.Color.Transparent;
                Controls.Add(box);
                Controls.Add(Buttons);
            }

            public override System.Drawing.Size GetPreferredSize(System.Drawing.Size proposedSize)
            {
                int w = proposedSize.Width > 0 ? proposedSize.Width : Width;
                return new System.Drawing.Size(w, _box.MultilineHeight + Theme.S(10) + Buttons.PreferredHeight(w));
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                _box.SetBounds(0, 0, Width, _box.MultilineHeight);
                Buttons.SetBounds(0, _box.MultilineHeight + Theme.S(10), Width, Buttons.PreferredHeight(Width));
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                e.Graphics.Clear(Theme.BackOf(this));
            }
        }
    }
}
