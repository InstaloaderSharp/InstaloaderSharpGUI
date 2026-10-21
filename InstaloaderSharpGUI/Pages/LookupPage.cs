using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InstaloaderSharp;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Pages
{
    /// <summary>Preview profiles and hashtags and search Instagram, using the library's structures directly.</summary>
    internal sealed class LookupPage : PageBase
    {
        private static LookupPage _instance;

        private readonly DarkTextBox _query;
        private readonly FlatButton _lookup;
        private readonly FlatButton _search;
        private readonly TextBlock _status;
        private readonly InfoCard _info;
        private readonly SectionTitle _resultsTitle;
        private readonly Card _results;
        private readonly TextBlock _empty;
        private bool _busy;

        public LookupPage(AppState state) : base(state)
        {
            _instance = this;
            Add(new PageHeader("Look up",
                               "Preview a profile or hashtag before downloading it, or search Instagram for accounts, hashtags and places."));

            var card = Add(new Card(), Theme.S(8));
            var row = new HStack();
            _query = new DarkTextBox { Placeholder = "Username, #hashtag, Instagram link or search words" };
            _query.InnerTextBox.AccessibleName = "Look up or search";
            row.Fill = _query;
            row.Add(_query);
            _lookup = row.Add(new FlatButton("Look up", ButtonKind.Primary, Glyphs.Contact));
            _search = row.Add(new FlatButton("Search", ButtonKind.Secondary, Glyphs.Search));
            card.Add(Padded(row, 16, 14));
            Tip(_lookup, "Show details of a profile (username) or hashtag (#tag).");
            Tip(_search, "Search Instagram for matching profiles, hashtags and locations. Requires sign-in.");

            _status = Add(new TextBlock(""));
            _status.Visible = false;

            _empty = Add(new TextBlock("Try:  natgeo   ·   #sunset   ·   https://www.instagram.com/natgeo/   ·   or several words to search.\n" +
                                       "Search requires that you are signed in (Account page). Some profiles and details are only shown when you are signed in.",
                                       Theme.TextSecondary, Glyphs.Info), Theme.S(4));

            _info = Add(new InfoCard(), Theme.S(8));
            _info.Visible = false;
            _info.AddButton.Click += (s, e) => { if (_info.Target != null) AddTarget(_info.Target); };
            _info.FolloweesButton.Click += (s, e) => { if (_info.Username != null) AddTarget("@" + _info.Username); };
            _info.OpenButton.Click += (s, e) => { if (_info.Url != null) DownloadPage.Open(_info.Url); };

            _resultsTitle = Add(new SectionTitle("Search results"), Theme.S(14));
            _resultsTitle.Visible = false;
            _results = Add(new Card());
            _results.Visible = false;

            _query.InnerTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    string q = _query.Text.Trim();
                    if (q.Contains(" ") && !q.StartsWith("http"))
                        Search();
                    else
                        LookUp();
                }
            };
            _lookup.Click += (s, e) => LookUp();
            _search.Click += (s, e) => Search();
        }

        /// <summary>Opens the page and looks up the given profile.</summary>
        public static void Request(AppState state, string username)
        {
            if (_instance == null)
                return;
            if (state.Navigate != null)
                state.Navigate(typeof(LookupPage));
            _instance._query.Text = username;
            _instance.LookUp();
        }

        public void FocusQuery()
        {
            _query.Focus();
        }

        private void AddTarget(string target)
        {
            if (State.AddTarget != null)
                State.AddTarget(target);
            SetStatus(string.Format("Added {0} to the download list.", target), Theme.Success, Glyphs.Accept);
        }

        private void SetStatus(string text, Color color, string glyph)
        {
            _status.Text = text ?? "";
            _status.TextColor = color;
            _status.Glyph = glyph;
            _status.Visible = !string.IsNullOrEmpty(text);
            PerformLayout();
        }

        private void SetBusy(bool busy, string message)
        {
            _busy = busy;
            _lookup.Enabled = _search.Enabled = !busy;
            Cursor = busy ? Cursors.AppStarting : Cursors.Default;
            if (busy)
                SetStatus(message, Theme.TextSecondary, Glyphs.Refresh);
        }

        private sealed class InfoData
        {
            public string Title, Subtitle, Body, Target, Username, Url, External;
            public List<KeyValuePair<string, string>> Stats = new List<KeyValuePair<string, string>>();
            public List<string> Badges = new List<string>();
            public Image Picture;
            public bool IsProfile;
        }

        private static string N(long? v)
        {
            if (v == null) return "–";
            long n = v.Value;
            if (n >= 10000000) return (n / 1000000.0).ToString("0.#") + "M";
            if (n >= 10000) return (n / 1000.0).ToString("0.#") + "K";
            return n.ToString("N0");
        }

        private void LookUp()
        {
            if (_busy)
                return;
            string err;
            string q = _query.Text.Trim();
            if (q.Length == 0)
            {
                _query.Focus();
                return;
            }
            string target = Targets.Parse(q, TargetType.Auto, out err);
            if (target == null || !(target.StartsWith("#") || char.IsLetterOrDigit(target[0]) || target[0] == '_' || target[0] == '.'))
            {
                if (target != null && target.StartsWith("-"))
                    err = "Single posts cannot be previewed here - add them to the download list directly.";
                SetStatus(err ?? "Enter a username or #hashtag.", Theme.Error, Glyphs.Error);
                return;
            }
            JobOptions o = O.Clone();
            SetBusy(true, "Looking up " + target + "…");
            RunBackground(() =>
            {
                using (Instaloader loader = Library.CreateLookupLoader(o))
                {
                    var d = new InfoData { Target = target };
                    if (target.StartsWith("#"))
                    {
                        Hashtag tag = Hashtag.FromName(loader.Context, target.Substring(1));
                        d.Title = "#" + tag.Name;
                        d.Subtitle = "Hashtag";
                        d.Body = tag.Description;
                        d.Stats.Add(new KeyValuePair<string, string>("Posts", N(tag.Mediacount)));
                        d.Url = "https://www.instagram.com/explore/tags/" + Uri.EscapeDataString(tag.Name) + "/";
                        d.Picture = Library.DownloadImage(loader, tag.ProfilePicUrl);
                    }
                    else
                    {
                        Profile p = Profile.FromUsername(loader.Context, target);
                        d.IsProfile = true;
                        d.Username = p.Username;
                        d.Target = p.Username;
                        d.Title = string.IsNullOrEmpty(p.FullName) ? "@" + p.Username : p.FullName;
                        d.Subtitle = "@" + p.Username + "  ·  ID " + p.Userid;
                        d.Body = p.Biography;
                        d.External = p.ExternalUrl;
                        d.Stats.Add(new KeyValuePair<string, string>("Posts", N(p.Mediacount)));
                        d.Stats.Add(new KeyValuePair<string, string>("Followers", N(p.Followers)));
                        d.Stats.Add(new KeyValuePair<string, string>("Following", N(p.Followees)));
                        if (p.IsVerified) d.Badges.Add("Verified");
                        if (p.IsPrivate) d.Badges.Add(p.FollowedByViewer ? "Private · you follow" : "Private");
                        if (p.IsBusinessAccount) d.Badges.Add(string.IsNullOrEmpty(p.BusinessCategoryName) ? "Business" : p.BusinessCategoryName);
                        Profile profile = p;
                        string story = StoryBadge(loader.Context.IsLoggedIn,
                                                  () => loader.GetStories(new List<long> { profile.Userid }).Sum(s => s.Itemcount),
                                                  () => profile.HasPublicStory);
                        if (story != null) d.Badges.Add(story);
                        if (loader.Context.IsLoggedIn && p.FollowsViewer) d.Badges.Add("Follows you");
                        d.Url = "https://www.instagram.com/" + p.Username + "/";
                        d.Picture = Library.DownloadImage(loader, p.ProfilePicUrl);
                    }
                    return d;
                }
            }, d =>
            {
                SetBusy(false, null);
                SetStatus(null, Theme.TextSecondary, null);
                _info.Show(d.Title, d.Subtitle, d.Body, d.External, d.Stats, d.Badges, d.Picture, d.Target, d.Username, d.Url, d.IsProfile);
                _info.Visible = true;
                _empty.Visible = false;
                PerformLayout();
            }, e =>
            {
                SetBusy(false, null);
                string msg = e.Message;
                if (e is ProfileNotExistsException)
                    msg = "Profile not found: " + msg;
                else if (e is LoginRequiredException)
                    msg = "Instagram requires login for this. Sign in on the Account page. (" + msg + ")";
                SetStatus(msg, Theme.Error, Glyphs.Error);
            });
        }

        /// <summary>
        /// Badge text for a profile's current story, or null when there is none (or it cannot be determined).
        /// Signed in, the story is fetched the same way a download does, so the badge only appears when there is
        /// something to download. Profile.HasViewableStory is not used: it is true for every followed account.
        /// Anonymously, Instagram only says whether there is a public story; downloading it requires login.
        /// </summary>
        internal static string StoryBadge(bool loggedIn, Func<int> currentStoryItems, Func<bool> hasPublicStory)
        {
            try
            {
                if (loggedIn)
                {
                    int items = currentStoryItems();
                    if (items <= 0)
                        return null;
                    return items == 1 ? "Story · 1 item" : string.Format("Story · {0} items", items);
                }
                return hasPublicStory() ? "Has story · sign in to download" : null;
            }
            catch (Exception)
            {
                // e.g. rate limited: rather show no badge than a wrong one
                return null;
            }
        }

        private sealed class SearchItem
        {
            public string Title, Detail, Target, Glyph;
            public bool IsProfile;
        }

        private void Search()
        {
            if (_busy)
                return;
            string q = _query.Text.Trim();
            if (q.Length == 0)
            {
                _query.Focus();
                return;
            }
            JobOptions o = O.Clone();
            SetBusy(true, "Searching Instagram for \"" + q + "\"…");
            RunBackground(() =>
            {
                using (Instaloader loader = Library.CreateLookupLoader(o))
                {
                    var results = new TopSearchResults(loader.Context, q);
                    var items = new List<SearchItem>();
                    foreach (Profile p in results.GetProfiles().Take(25))
                        items.Add(new SearchItem
                        {
                            Title = "@" + p.Username,
                            Detail = (p.FullName ?? "") + (p.IsVerified ? "  ·  verified" : "") + (p.IsPrivate ? "  ·  private" : ""),
                            Target = p.Username,
                            Glyph = Glyphs.Contact,
                            IsProfile = true
                        });
                    foreach (Hashtag h in results.GetHashtags().Take(15))
                        items.Add(new SearchItem
                        {
                            Title = "#" + h.Name,
                            Detail = h.Mediacount.HasValue ? N(h.Mediacount) + " posts" : "Hashtag",
                            Target = "#" + h.Name,
                            Glyph = Glyphs.Tag
                        });
                    foreach (PostLocation l in results.GetLocations().Take(15))
                        items.Add(new SearchItem
                        {
                            Title = l.Name,
                            Detail = "Location %" + l.Id + (string.IsNullOrEmpty(l.Slug) ? "" : "  ·  " + l.Slug),
                            Target = "%" + l.Id,
                            Glyph = Glyphs.MapPin
                        });
                    return items;
                }
            }, items =>
            {
                SetBusy(false, null);
                ShowResults(items);
                SetStatus(items.Count == 0 ? "No results." : null, Theme.TextSecondary, Glyphs.Info);
            }, e =>
            {
                SetBusy(false, null);
                SetStatus("Search failed: " + e.Message + (e is LoginRequiredException ? " - sign in on the Account page." : ""), Theme.Error, Glyphs.Error);
            });
        }

        private void ShowResults(List<SearchItem> items)
        {
            _results.SuspendLayout();
            foreach (Control c in _results.Controls)
                c.Dispose();
            _results.Controls.Clear();
            foreach (SearchItem item in items)
            {
                SearchItem it = item;
                var row = new SettingRow(it.Title, it.Detail, it.Glyph);
                var buttons = new HStack();
                if (it.IsProfile || it.Target.StartsWith("#"))
                {
                    var details = buttons.Add(new FlatButton("Details", ButtonKind.Subtle, Glyphs.Info));
                    details.Click += (s, e) => { _query.Text = it.Target; LookUp(); };
                }
                var add = buttons.Add(new FlatButton("Add", ButtonKind.Secondary, Glyphs.Add));
                add.Click += (s, e) => AddTarget(it.Target);
                int w = 0;
                foreach (Control b in buttons.Controls) w += b.Width + buttons.Spacing;
                buttons.Width = w;
                row.Accessory = buttons;
                row.AccessoryWidth = w;
                _results.Add(row);
            }
            _results.ResumeLayout();
            _results.Visible = _resultsTitle.Visible = items.Count > 0;
            if (items.Count > 0)
                _empty.Visible = false;
            _results.PerformLayout();
            PerformLayout();
        }

        /// <summary>Card showing a profile or hashtag.</summary>
        internal sealed class InfoCard : Control
        {
            public readonly FlatButton AddButton;
            public readonly FlatButton FolloweesButton;
            public readonly FlatButton OpenButton;
            private readonly Avatar _avatar;
            private readonly HStack _buttons;
            private string _title = "", _subtitle = "", _body = "", _external;
            private List<KeyValuePair<string, string>> _stats = new List<KeyValuePair<string, string>>();
            private List<string> _badges = new List<string>();
            public string Target, Username, Url;

            public InfoCard()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Theme.Surface;
                _avatar = new Avatar { Size = new Size(Theme.S(96), Theme.S(96)), Glyph = Glyphs.Contact };
                Controls.Add(_avatar);
                _buttons = new HStack();
                AddButton = _buttons.Add(new FlatButton("Add to download list", ButtonKind.Primary, Glyphs.Add));
                FolloweesButton = _buttons.Add(new FlatButton("Add accounts they follow", ButtonKind.Secondary, Glyphs.People));
                OpenButton = _buttons.Add(new FlatButton("Open on Instagram", ButtonKind.Subtle, Glyphs.OpenInNew));
                Controls.Add(_buttons);
            }

            public void Show(string title, string subtitle, string body, string external, List<KeyValuePair<string, string>> stats,
                             List<string> badges, Image picture, string target, string username, string url, bool isProfile)
            {
                _title = title ?? "";
                _subtitle = subtitle ?? "";
                _body = body ?? "";
                _external = external;
                _stats = stats;
                _badges = badges;
                Target = target;
                Username = username;
                Url = url;
                _avatar.Image = picture;
                _avatar.Text = picture == null ? (title ?? "").TrimStart('@', '#') : "";
                FolloweesButton.Visible = isProfile;
                AccessibleName = _title + ". " + _subtitle;
                if (Parent != null) Parent.PerformLayout();
                PerformLayout();
                Invalidate();
            }

            private int TextX { get { return Theme.S(24) + _avatar.Width + Theme.S(20); } }

            // Heights of the rows above the biography. Used both to measure and to paint the card, so that the
            // reserved space always matches what is drawn.
            private static int TopMargin { get { return Theme.S(20); } }
            private static int TitleRowH { get { return Theme.S(34); } }
            private static int SubtitleRowH { get { return Theme.S(26); } }
            private static int BadgeRowH { get { return Theme.S(32); } }
            private static int StatsRowH { get { return Theme.S(56); } }
            private static int BodyGap { get { return Theme.S(16); } }

            private const TextFormatFlags BodyFlags = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;

            private string BodyText
            {
                get { return _body + (string.IsNullOrEmpty(_external) ? "" : "\n" + _external); }
            }

            private int BodyTop
            {
                get { return TopMargin + TitleRowH + SubtitleRowH + (_badges.Count > 0 ? BadgeRowH : 0) + StatsRowH; }
            }

            private int TextWidth(int width)
            {
                return Math.Max(Theme.S(100), width - TextX - Theme.S(24));
            }

            private int BodyHeight(int width)
            {
                string text = BodyText;
                if (text.Trim().Length == 0) return 0;
                return TextRenderer.MeasureText(text, Theme.Body, new Size(TextWidth(width), 0), BodyFlags).Height;
            }

            /// <summary>Where the biography is drawn, for the given card width.</summary>
            internal Rectangle BodyBounds(int width)
            {
                return new Rectangle(TextX, BodyTop, TextWidth(width), BodyHeight(width));
            }

            internal Rectangle ButtonBounds { get { return _buttons.Bounds; } }

            public override Size GetPreferredSize(Size proposedSize)
            {
                int w = proposedSize.Width > 0 ? proposedSize.Width : Width;
                int y = BodyTop;
                int bh = BodyHeight(w);
                if (bh > 0) y += bh + BodyGap;
                y += _buttons.PreferredHeight(TextWidth(w)) + Theme.S(20);
                // The avatar column must fit too.
                return new Size(w, Math.Max(y, Theme.S(24) + _avatar.Height + Theme.S(24)));
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                using (var path = Theme.RoundRect(new RectangleF(0, 0, Width, Height), Theme.S(8f)))
                    Region = new Region(path);
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                if (_buttons == null)
                    return; // still constructing
                _avatar.Location = new Point(Theme.S(24), Theme.S(24));
                int bw = TextWidth(Width);
                int bh = _buttons.PreferredHeight(bw);
                _buttons.SetBounds(TextX, Height - bh - Theme.S(20), bw, bh);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(Theme.Surface);
                Theme.HighQuality(g);
                int x = TextX;
                int w = TextWidth(Width);
                int y = TopMargin;
                TextRenderer.DrawText(g, _title, Theme.Title, new Rectangle(x - Theme.S(2), y, w, TitleRowH), Theme.Text,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                y += TitleRowH;
                TextRenderer.DrawText(g, _subtitle, Theme.Body, new Rectangle(x, y, w, Theme.S(20)), Theme.TextSecondary,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                y += SubtitleRowH;
                int bx = x;
                foreach (string b in _badges)
                {
                    Size s = TextRenderer.MeasureText(b, Theme.SmallStrong, Size.Empty, TextFormatFlags.NoPadding);
                    var r = new Rectangle(bx, y, s.Width + Theme.S(16), Theme.S(22));
                    Color c = b == "Verified" ? Theme.Info : b.StartsWith("Private") ? Theme.Warning : Theme.AccentText;
                    Theme.FillRound(g, Theme.Blend(Theme.Surface, c, 0.18f), r, Theme.S(11f));
                    TextRenderer.DrawText(g, b, Theme.SmallStrong, r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                    bx += r.Width + Theme.S(6);
                }
                if (_badges.Count > 0)
                    y += BadgeRowH;
                int sx = x;
                foreach (var kv in _stats)
                {
                    TextRenderer.DrawText(g, kv.Value, Theme.Display, new Point(sx - Theme.S(2), y), Theme.Text, TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, kv.Key, Theme.Small, new Point(sx, y + Theme.S(26)), Theme.TextSecondary, TextFormatFlags.NoPrefix);
                    int vw = Math.Max(TextRenderer.MeasureText(kv.Value, Theme.Display).Width, TextRenderer.MeasureText(kv.Key, Theme.Small).Width);
                    sx += vw + Theme.S(28);
                }
                Rectangle body = BodyBounds(Width);
                if (body.Height > 0)
                    TextRenderer.DrawText(g, BodyText, Theme.Body, body, Theme.Text, BodyFlags);
            }
        }
    }
}
