using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InstaloaderSharp;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Pages
{
    /// <summary>Main page: what to download (targets), what to include, which posts and where to save.</summary>
    internal sealed class DownloadPage : PageBase
    {
        private readonly DarkComboBox _typeCombo;
        private readonly DarkTextBox _input;
        private readonly TextBlock _hint;
        private readonly TargetList _list;
        private readonly TargetListHost _listHost;
        private readonly TextBlock _validation;
        private ToggleSwitch _compressJson;
        private ToggleSwitch _metadataJson;
        private readonly Timer _hintTimer;

        public DownloadPage(AppState state) : base(state)
        {
            Add(new PageHeader("Download",
                               "Add profiles, hashtags, posts or locations, choose what to save, then press Start download (Ctrl+Enter)."));

            // ------------------------------------------------------------------ Targets
            Add(new SectionTitle("Targets"), Theme.S(8));
            var targets = Add(new Card());

            var inputRow = new HStack();
            _typeCombo = new DarkComboBox { Width = Theme.S(150) };
            _typeCombo.Items.AddRange(Targets.TypeNames);
            _typeCombo.SelectedIndex = 0;
            _typeCombo.AccessibleName = "Target type";
            inputRow.Add(_typeCombo);
            _input = new DarkTextBox { Placeholder = "Username, #hashtag, Instagram link, %location ID or -shortcode" };
            _input.InnerTextBox.AccessibleName = "Target to add";
            inputRow.Fill = _input;
            inputRow.Add(_input);
            var addButton = inputRow.Add(new FlatButton("Add", ButtonKind.Primary, Glyphs.Add));
            targets.Add(Padded(inputRow, 16, 14));
            Tip(_typeCombo, "Auto-detect understands usernames, #hashtags, %location IDs, -shortcodes, :feed/:stories/:saved, " +
                            "JSON files and Instagram links. Choose a type to be explicit (\"Followees of\" downloads every account a profile follows).");

            _hint = new TextBlock("Tip: paste several names or links at once - one per line, or separated by spaces or commas.");
            targets.Add(Padded(_hint, 16, 0));

            var chips = new HStack();
            var feed = chips.Add(new FlatButton("Your feed", ButtonKind.Secondary, Glyphs.Home));
            var stories = chips.Add(new FlatButton("Followed stories", ButtonKind.Secondary, Glyphs.History));
            var saved = chips.Add(new FlatButton("Saved posts", ButtonKind.Secondary, Glyphs.Bookmark));
            var json = chips.Add(new FlatButton("JSON file…", ButtonKind.Secondary, Glyphs.Document));
            var importList = chips.Add(new FlatButton("Import list…", ButtonKind.Subtle, Glyphs.Import));
            var clear = chips.Add(new FlatButton("Clear all", ButtonKind.Subtle, Glyphs.Delete), true);
            Tip(feed, "Download pictures from your feed (:feed). Requires login.");
            Tip(stories, "Download the stories of the accounts you follow (:stories). Requires login.");
            Tip(saved, "Download the posts you marked as saved (:saved). Requires login.");
            Tip(json, "Re-download posts or story items from previously saved .json / .json.xz metadata files.");
            Tip(importList, "Add targets from a text file with one target per line (console syntax, e.g. @profile, #hashtag, %123, -shortcode).");
            targets.Add(Padded(chips, 16, 12));

            _list = new TargetList { EmptyText = "No targets yet. Type a username or paste an Instagram link above, then press Enter." };
            _listHost = new TargetListHost(_list);
            targets.Add(_listHost);

            _validation = Add(new TextBlock("", Theme.Warning, Glyphs.Warning));
            _validation.Visible = false;

            // ------------------------------------------------------------------ What to download
            Card what = AddSection("What to download of each profile");
            Toggle(what, "Posts", "Regular posts of each profile.", o => o.Posts, (o, v) => o.Posts = v, Glyphs.Photo);
            Toggle(what, "Profile picture", "Saved when it changed since the last download.", o => o.ProfilePic, (o, v) => o.ProfilePic = v, Glyphs.Contact);
            Toggle(what, "Stories", "Current stories of each profile.", o => o.Stories, (o, v) => o.Stories = v, Glyphs.History, "Requires login");
            Toggle(what, "Highlights", "Story highlights of each profile.", o => o.Highlights, (o, v) => o.Highlights = v, Glyphs.Star, "Requires login");
            Toggle(what, "Tagged posts", "Posts in which the profile is tagged.", o => o.Tagged, (o, v) => o.Tagged = v, Glyphs.Tag);
            Toggle(what, "Reels", "Reels videos of each profile.", o => o.Reels, (o, v) => o.Reels = v, Glyphs.Video);

            // ------------------------------------------------------------------ Post content
            Card content = AddSection("What to save of each post");
            Toggle(content, "Pictures", "Turning this off also skips video thumbnails. Cannot be combined with Fast update.",
                   o => o.Pictures, (o, v) => o.Pictures = v, Glyphs.Photo);
            Toggle(content, "Videos", null, o => o.Videos, (o, v) => o.Videos = v, Glyphs.Video);
            Toggle(content, "Video thumbnails", null, o => o.VideoThumbnails, (o, v) => o.VideoThumbnails = v, Glyphs.Photo);
            Toggle(content, "Captions", "A .txt file with the caption of each post.", o => o.Captions, (o, v) => o.Captions = v, Glyphs.Document);
            Toggle(content, "Geotags", "Location name and a Google Maps link as a text file. May need one extra request per post.",
                   o => o.Geotags, (o, v) => o.Geotags = v, Glyphs.MapPin);
            Toggle(content, "Comments", "Download and update the comments of each post. Needs one extra request per post.",
                   o => o.Comments, (o, v) => o.Comments = v, Glyphs.People, "Requires login");
            _metadataJson = Toggle(content, "Metadata JSON", "A JSON file with all metadata of each post. Needed to re-download posts later or to use them as targets.",
                                   o => o.MetadataJson, (o, v) => o.MetadataJson = v, Glyphs.Code);
            _compressJson = Toggle(content, "Compress JSON", "Store metadata as xz-compressed .json.xz instead of pretty-printed .json.",
                                   o => o.CompressJson, (o, v) => o.CompressJson = v, Glyphs.Code);
            TextRow(content, "Images of multi-image posts",
                    "Which images of sidecar posts to download, e.g. 1, last, 2-4 or 3-last. Leave empty for all.",
                    o => o.Slide, (o, v) => o.Slide = v, "All", Theme.S(140), Glyphs.Photo);

            // ------------------------------------------------------------------ Which posts
            Card which = AddSection("Which posts to download");
            Toggle(which, "Fast update", "For each target, stop at the first already-downloaded item. Recommended for keeping an archive up to date.",
                   o => o.FastUpdate, (o, v) => o.FastUpdate = v, Glyphs.Refresh);
            TextRow(which, "Maximum number of posts", "Applies to #hashtags, %locations, your feed and saved posts. Leave empty for no limit.",
                    o => o.Count, (o, v) => o.Count = v, "No limit", Theme.S(140), Glyphs.Filter);

            var stampsRow = new SettingRow("Remember latest downloads",
                                           "Store the time of the newest item downloaded per profile, so updates work even if you delete the folders.",
                                           Glyphs.History);
            var stampsSwitch = new ToggleSwitch { Checked = O.UseLatestStamps };
            stampsRow.Accessory = stampsSwitch;
            which.Add(stampsRow);
            var stampsFile = PathRow(which, "Stamps file", null, o => o.LatestStampsFile, (o, v) => o.LatestStampsFile = v, false,
                                     Instaloader.GetDefaultStampsFilename(), "INI files|*.ini|All files|*.*", true, null, stampsRow);
            Action syncStamps = () => stampsRow.Content.Visible = stampsSwitch.Checked;
            stampsSwitch.CheckedChanged += (s, e) =>
            {
                syncStamps();
                if (Refreshing) return;
                O.UseLatestStamps = stampsSwitch.Checked;
                Changed();
            };
            OnRefresh(() => { stampsSwitch.Checked = O.UseLatestStamps; syncStamps(); });
            syncStamps();
            Tip(stampsFile, "Leave empty to use the default file: " + Instaloader.GetDefaultStampsFilename());

            FilterRow<Post>(which, "Post filter",
                            "Only download posts for which this expression is true. Uses Post attributes.",
                            o => o.PostFilter, (o, v) => o.PostFilter = v,
                            new[] { "likes > 1000", "not is_video", "date_utc >= datetime(2024, 1, 1)",
                                    "datetime(2023, 1, 1) <= date_utc < datetime(2024, 1, 1)", "'travel' in caption_hashtags",
                                    "viewer_has_liked", "typename == 'GraphSidecar'", "comments > 10 and likes > 100" });
            FilterRow<StoryItem>(which, "Story item filter",
                                 "Only download story items for which this expression is true. Uses StoryItem attributes.",
                                 o => o.StoryitemFilter, (o, v) => o.StoryitemFilter = v,
                                 new[] { "not is_video", "is_video", "date_utc >= datetime(2024, 1, 1)" });

            // ------------------------------------------------------------------ Output
            Card output = AddSection("Save to");
            var outRow = new SettingRow("Download folder",
                                        "Each target gets its own sub folder (change the naming under Settings → File naming).",
                                        Glyphs.Folder);
            PathRow(output, "Download folder", null, o => o.OutputDir, (o, v) => o.OutputDir = v, true,
                    "Choose a folder", existing: outRow);
            var open = new FlatButton("Open", ButtonKind.Subtle, Glyphs.OpenInNew);
            ((HStack)outRow.Content).Add(open);
            open.Click += (s, e) => OpenFolder(O.OutputDir);
            output.Add(outRow);

            // ------------------------------------------------------------------ behaviour
            _hintTimer = new Timer { Interval = 250 };
            _hintTimer.Tick += (s, e) => { _hintTimer.Stop(); UpdateHint(); };
            _input.TextChanged += (s, e) => { _hintTimer.Stop(); _hintTimer.Start(); };
            _typeCombo.SelectedIndexChanged += (s, e) => UpdateHint();
            _input.InnerTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Control)
                {
                    e.SuppressKeyPress = true;
                    AddFromInput();
                }
            };
            addButton.Click += (s, e) => AddFromInput();
            feed.Click += (s, e) => AddTargets(new[] { ":feed" });
            stories.Click += (s, e) => AddTargets(new[] { ":stories" });
            saved.Click += (s, e) => AddTargets(new[] { ":saved" });
            json.Click += (s, e) => BrowseJson();
            importList.Click += (s, e) => ImportList();
            clear.Click += (s, e) =>
            {
                if (O.Targets.Count == 0)
                    return;
                if (MessageDialog.Show(this, string.Format("Remove all {0} targets from the list?", O.Targets.Count), "Clear targets",
                                       DialogIcon.Question, MessageBoxButtons.YesNo, yesText: "Clear all", noText: "Keep") == DialogResult.Yes)
                {
                    O.Targets.Clear();
                    SyncList();
                }
            };
            _list.RemoveRequested += (s, entry) => RemoveTargets(new[] { entry });
            _list.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                    RemoveTargets(_list.SelectedItems.Cast<TargetEntry>().ToList());
                else if (e.Control && e.KeyCode == Keys.V)
                    PasteTargets();
                else if (e.Control && e.KeyCode == Keys.C)
                    CopySelected();
                else if (e.Control && e.KeyCode == Keys.A)
                    for (int i = 0; i < _list.Items.Count; i++) _list.SetSelected(i, true);
            };
            _list.DoubleClick += (s, e) => LookUpSelected();

            ContextMenuStrip menu = Menus.Create();
            menu.Item("Look up profile", (s, e) => LookUpSelected());
            menu.Item("Open on Instagram", (s, e) => OpenSelectedOnInstagram());
            menu.Separator();
            menu.Item("Copy", (s, e) => CopySelected(), "Ctrl+C");
            menu.Item("Paste targets", (s, e) => PasteTargets(), "Ctrl+V");
            menu.Separator();
            menu.Item("Remove", (s, e) => RemoveTargets(_list.SelectedItems.Cast<TargetEntry>().ToList()), "Del");
            _list.ContextMenuStrip = menu;

            _metadataJson.CheckedChanged += (s, e) => SyncDependencies();
            OnRefresh(SyncList);
            OnRefresh(SyncDependencies);
            SyncList();
            SyncDependencies();
            state.Changed += (s, e) => UpdateValidation();
            state.AddTarget = t => AddTargets(new[] { t });
        }

        private void SyncDependencies()
        {
            _compressJson.Parent.Enabled = _metadataJson.Checked;
        }

        private void FilterRow<T>(Card card, string title, string description, Func<JobOptions, string> get, Action<JobOptions, string> set, string[] examples)
        {
            var row = new SettingRow(title, description, Glyphs.Filter);
            var stack = new HStack();
            var box = new DarkTextBox { Placeholder = "e.g. " + examples[0], Font = Theme.Mono };
            stack.Fill = box;
            stack.Add(box);
            var attrs = stack.Add(new FlatButton("Attributes", ButtonKind.Secondary, Glyphs.ChevronDown));
            var ex = stack.Add(new FlatButton("Examples", ButtonKind.Subtle, Glyphs.ChevronDown));
            row.Content = stack;
            card.Add(row);

            ContextMenuStrip attrMenu = Menus.Create();
            foreach (var kv in Library.FilterAttributes(typeof(T)))
            {
                string name = kv.Key;
                attrMenu.Item(name, (s, e) => box.InsertAtCaret(name)).ShortcutKeyDisplayString = kv.Value;
            }
            ContextMenuStrip exMenu = Menus.Create();
            foreach (string example in examples)
            {
                string x = example;
                exMenu.Item(x, (s, e) => { box.Text = x; box.Focus(); });
            }
            exMenu.Separator();
            exMenu.Item("Clear filter", (s, e) => box.Text = "");
            attrs.Click += (s, e) => attrMenu.ShowBelow(attrs);
            ex.Click += (s, e) => exMenu.ShowBelow(ex);
            Tip(attrs, "Insert a " + typeof(T).Name + " attribute. Combine with and / or / not, comparisons (== != < <= > >=), " +
                       "'in' and datetime(year, month, day).");

            var validate = new Timer { Interval = 400 };
            Action check = () =>
            {
                validate.Stop();
                string err = JobOptions.ValidateFilter<T>(box.Text);
                box.HasError = err != null;
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    row.Description = description;
                    row.DescriptionColor = null;
                }
                else if (err != null)
                {
                    row.Description = err;
                    row.DescriptionColor = Theme.Error;
                }
                else
                {
                    row.Description = "✓ Valid filter. " + description;
                    row.DescriptionColor = Theme.Success;
                }
            };
            validate.Tick += (s, e) => check();
            box.Text = get(O) ?? "";
            box.TextChanged += (s, e) =>
            {
                validate.Stop();
                validate.Start();
                if (Refreshing) return;
                set(O, box.Text);
                Changed();
            };
            OnRefresh(() => { box.Text = get(O) ?? ""; check(); });
            check();
        }

        // ------------------------------------------------------------------------------- targets

        private static IEnumerable<string> SplitInput(string text, TargetType type)
        {
            // JSON paths and links may contain spaces/commas only in rare cases; split on new lines first.
            foreach (string line in text.Replace("\r", "").Split('\n'))
            {
                string l = line.Trim();
                if (l.Length == 0)
                    continue;
                if (type == TargetType.Json || l.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || l.EndsWith(".json.xz", StringComparison.OrdinalIgnoreCase))
                {
                    yield return l;
                    continue;
                }
                foreach (string part in l.Split(new[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    yield return part;
            }
        }

        private TargetType SelectedType { get { return (TargetType)Math.Max(0, _typeCombo.SelectedIndex); } }

        private void UpdateHint()
        {
            string text = _input.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                _input.HasError = false;
                _hint.TextColor = Theme.TextSecondary;
                _hint.Glyph = null;
                _hint.Text = "Tip: paste several names or links at once - one per line, or separated by spaces or commas.";
                return;
            }
            var parts = SplitInput(text, SelectedType).ToList();
            var ok = new List<string>();
            string firstError = null;
            foreach (string p in parts)
            {
                string err;
                string t = Targets.Parse(p, SelectedType, out err);
                if (t != null) ok.Add(t);
                else if (firstError == null) firstError = p + ": " + err;
            }
            if (firstError != null)
            {
                _input.HasError = true;
                _hint.TextColor = Theme.Error;
                _hint.Glyph = Glyphs.Error;
                _hint.Text = firstError;
            }
            else
            {
                _input.HasError = false;
                _hint.TextColor = Theme.Success;
                _hint.Glyph = Glyphs.Accept;
                if (ok.Count == 1)
                {
                    TargetEntry d = Targets.Describe(ok[0]);
                    _hint.Text = string.Format("Press Enter to add {0}: {1}", d.Kind.ToLowerInvariant(), ok[0]);
                }
                else
                {
                    _hint.Text = string.Format("Press Enter to add {0} targets.", ok.Count);
                }
            }
        }

        private void AddFromInput()
        {
            string text = _input.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                _input.Focus();
                return;
            }
            var added = new List<string>();
            var failed = new List<string>();
            foreach (string p in SplitInput(text, SelectedType))
            {
                string err;
                string t = Targets.Parse(p, SelectedType, out err);
                if (t != null) added.Add(t);
                else failed.Add(p);
            }
            AddTargets(added);
            // Keep what could not be understood so the user can fix it.
            _input.Text = string.Join(" ", failed);
            _input.Focus();
            UpdateHint();
        }

        public void AddTargets(IEnumerable<string> targets)
        {
            int before = O.Targets.Count;
            foreach (string t in targets)
                if (!O.Targets.Contains(t))
                    O.Targets.Add(t);
            if (O.Targets.Count != before)
            {
                SyncList();
                _list.ClearSelected();
                _list.TopIndex = Math.Max(0, _list.Items.Count - 1);
            }
        }

        private void RemoveTargets(IList<TargetEntry> entries)
        {
            if (State.IsBusy && entries.Count > 0)
            {
                // Removing from the list does not affect a running download; that is fine but say so.
                ConsoleRouter.App("Note: changes to the target list apply to the next download.");
            }
            foreach (TargetEntry e in entries)
                O.Targets.Remove(Targets.RawOf(e));
            SyncList();
        }

        private void SyncList()
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (string t in O.Targets)
                _list.Items.Add(Targets.Describe(t));
            _list.EndUpdate();
            _listHost.UpdateHeight();
            PerformLayout();
            Changed();
        }

        private void PasteTargets()
        {
            string text = Clipboard.ContainsText() ? Clipboard.GetText() : "";
            if (text.Length == 0)
                return;
            _input.Text = text;
            AddFromInput();
        }

        private void CopySelected()
        {
            var items = _list.SelectedItems.Cast<TargetEntry>().Select(Targets.RawOf).ToList();
            if (items.Count > 0)
                Clipboard.SetText(string.Join(Environment.NewLine, items));
        }

        private string SelectedProfile()
        {
            var e = _list.SelectedItem as TargetEntry;
            if (e == null)
                return null;
            string raw = Targets.RawOf(e);
            if (raw.StartsWith("@")) return raw.Substring(1);
            if (raw.Length > 0 && char.IsLetterOrDigit(raw[0]) && !raw.Contains(Path.DirectorySeparatorChar)) return raw;
            return null;
        }

        private void LookUpSelected()
        {
            string p = SelectedProfile();
            if (p != null)
                LookupPage.Request(State, p);
        }

        private void OpenSelectedOnInstagram()
        {
            var e = _list.SelectedItem as TargetEntry;
            if (e == null)
                return;
            string raw = Targets.RawOf(e);
            string url;
            if (raw.StartsWith("#")) url = "https://www.instagram.com/explore/tags/" + Uri.EscapeDataString(raw.Substring(1)) + "/";
            else if (raw.StartsWith("%")) url = "https://www.instagram.com/explore/locations/" + raw.Substring(1) + "/";
            else if (raw.StartsWith("-")) url = "https://www.instagram.com/p/" + raw.Substring(1) + "/";
            else if (raw.StartsWith("@")) url = "https://www.instagram.com/" + raw.Substring(1) + "/";
            else if (raw == ":feed" || raw == ":stories") url = "https://www.instagram.com/";
            else if (raw == ":saved") url = "https://www.instagram.com/";
            else if (raw.EndsWith(".json") || raw.EndsWith(".json.xz")) { OpenFolder(Path.GetDirectoryName(raw)); return; }
            else url = "https://www.instagram.com/" + raw + "/";
            Open(url);
        }

        private void BrowseJson()
        {
            using (var dlg = new OpenFileDialog
            {
                Title = "Choose metadata files to re-download",
                Filter = "Instaloader metadata (*.json;*.json.xz)|*.json;*.json.xz|All files|*.*",
                Multiselect = true
            })
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    AddTargets(dlg.FileNames);
            }
        }

        private void ImportList()
        {
            using (var dlg = new OpenFileDialog
            {
                Title = "Import targets",
                Filter = "Text files (*.txt)|*.txt|All files|*.*"
            })
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK)
                    return;
                var targets = new List<string>();
                var bad = new List<string>();
                foreach (string line in File.ReadAllLines(dlg.FileName))
                {
                    string l = line.Trim();
                    if (l.Length == 0)
                        continue;
                    if (l.StartsWith("--"))
                    {
                        bad.Add(l);
                        continue;
                    }
                    // Console syntax: @profile means followees here.
                    string err;
                    string t = l.StartsWith("@") ? Targets.Parse(l, TargetType.Followees, out err) : Targets.Parse(l, TargetType.Auto, out err);
                    if (t != null) targets.Add(t);
                    else bad.Add(l);
                }
                AddTargets(targets);
                if (bad.Count > 0)
                    MessageDialog.Show(this, string.Format("{0} target(s) added. {1} line(s) were skipped:\n\n{2}\n\n" +
                                                           "To import a full command line with options, use Settings → Import command line.",
                                                           targets.Count, bad.Count, string.Join("\n", bad.Take(8))),
                                       "Some lines were skipped", DialogIcon.Warning);
            }
        }

        private void UpdateValidation()
        {
            JobOptions.ValidationResult v = O.Validate(false);
            var messages = v.Errors.Concat(v.Warnings).Where(m => !m.StartsWith("Choose a folder")).ToList();
            bool show = messages.Count > 0;
            string text = string.Join("\n", messages);
            if (_validation.Visible != show || _validation.Text != text)
            {
                _validation.Text = text;
                _validation.TextColor = v.Errors.Count > 0 ? Theme.Error : Theme.Warning;
                _validation.Glyph = v.Errors.Count > 0 ? Glyphs.Error : Glyphs.Warning;
                _validation.Visible = show;
                PerformLayout();
            }
        }

        public void FocusInput()
        {
            _input.Focus();
        }

        internal static void OpenFolder(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return;
                string full = Path.GetFullPath(path.Trim());
                Directory.CreateDirectory(full);
                Process.Start("explorer.exe", "\"" + full + "\"");
            }
            catch (Exception)
            {
            }
        }

        internal static void Open(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>Hosts the target list, growing with its content up to a maximum height.</summary>
        private sealed class TargetListHost : Control
        {
            private readonly TargetList _list;

            public TargetListHost(TargetList list)
            {
                _list = list;
                BackColor = Theme.Surface;
                Controls.Add(list);
            }

            public void UpdateHeight()
            {
                if (Parent != null)
                    Parent.PerformLayout();
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                int rows = Math.Max(2, Math.Min(_list.Items.Count, 8));
                return new Size(proposedSize.Width, rows * _list.ItemHeight + Theme.S(12));
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                _list.SetBounds(Theme.S(8), Theme.S(2), Width - Theme.S(16), Height - Theme.S(10));
            }
        }
    }
}
