using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    internal static class ControlExtensions
    {
        private static readonly MethodInfo GetStateMethod =
            typeof(Control).GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);

        /// <summary>
        /// Whether the control's own Visible flag is set, regardless of whether its parents are visible
        /// (Control.Visible returns false for every child of a hidden parent).
        /// </summary>
        public static bool IsSetVisible(this Control c)
        {
            if (GetStateMethod == null)
                return c.Visible;
            return (bool)GetStateMethod.Invoke(c, new object[] { 0x2 });
        }

        public static int PreferredHeight(this Control c, int width)
        {
            return c.GetPreferredSize(new Size(width, 0)).Height;
        }
    }

    /// <summary>
    /// Scrollable page that stacks its children vertically at full width (capped at a readable maximum width).
    /// Children are laid out in the order they were added.
    /// </summary>
    public class StackPage : Panel
    {
        public StackPage()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            AutoScroll = true;
            BackColor = Theme.Window;
            Padding = new Padding(Theme.S(32), Theme.S(24), Theme.S(32), Theme.S(32));
            Spacing = Theme.S(8);
            MaxContentWidth = Theme.S(1040);
            Native.UseDarkScrollBars(this);
        }

        public int Spacing { get; set; }
        public int MaxContentWidth { get; set; }

        /// <summary>Extra space inserted above a child (e.g. before section titles).</summary>
        public Dictionary<Control, int> TopGap = new Dictionary<Control, int>();

        public T Add<T>(T control, int topGap = 0) where T : Control
        {
            if (topGap > 0)
                TopGap[control] = topGap;
            Controls.Add(control);
            return control;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int width = ClientSize.Width - Padding.Horizontal;
            if (MaxContentWidth > 0)
                width = Math.Min(width, MaxContentWidth);
            width = Math.Max(width, Theme.S(200));
            int y = Padding.Top + AutoScrollPosition.Y;
            int x = Padding.Left + AutoScrollPosition.X;
            bool first = true;
            foreach (Control c in Controls)
            {
                if (!c.IsSetVisible())
                    continue;
                int gap;
                if (!first)
                    y += Spacing;
                if (TopGap.TryGetValue(c, out gap) && !first)
                    y += gap;
                int h = c.PreferredHeight(width);
                c.SetBounds(x, y, width, h);
                y += h;
                first = false;
            }
            // ScrollableControl deflates the scroll area by Padding, so add the bottom padding on top of it.
            int total = y - AutoScrollPosition.Y + Padding.Bottom * 2;
            if (AutoScrollMinSize.Height != total)
                AutoScrollMinSize = new Size(0, total);
            // Lets ScrollableControl update the scroll bars for the new content size.
            base.OnLayout(levent);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }
    }

    /// <summary>Page heading: large title with a secondary description line.</summary>
    public class PageHeader : Control
    {
        public string Description { get; set; }

        public PageHeader(string title, string description)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Text = title;
            Description = description;
            BackColor = Theme.Window;
            AccessibleRole = AccessibleRole.StaticText;
            AccessibleName = title;
            AccessibleDescription = description;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = proposedSize.Width > 0 ? proposedSize.Width : Width;
            int h = TextRenderer.MeasureText(Text, Theme.Title).Height;
            if (!string.IsNullOrEmpty(Description))
                h += Theme.S(4) + TextRenderer.MeasureText(Description, Theme.Body, new Size(w, 0), TextFormatFlags.WordBreak).Height;
            return new Size(w, h + Theme.S(8));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            int th = TextRenderer.MeasureText(Text, Theme.Title).Height;
            TextRenderer.DrawText(e.Graphics, Text, Theme.Title, new Rectangle(-Theme.S(2), 0, Width, th), Theme.Text,
                                  TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
            if (!string.IsNullOrEmpty(Description))
                TextRenderer.DrawText(e.Graphics, Description, Theme.Body, new Rectangle(0, th + Theme.S(4), Width, Height - th),
                                      Theme.TextSecondary, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        }
    }

    /// <summary>Section title placed above a card.</summary>
    public class SectionTitle : Control
    {
        public SectionTitle(string text)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Text = text;
            BackColor = Theme.Window;
            AccessibleRole = AccessibleRole.StaticText;
            AccessibleName = text;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(proposedSize.Width, TextRenderer.MeasureText(Text, Theme.Section).Height + Theme.S(10));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            TextRenderer.DrawText(e.Graphics, Text, Theme.Section, new Rectangle(0, Theme.S(6), Width, Height), Theme.Text,
                                  TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
        }
    }

    /// <summary>Wrapped paragraph of secondary (or colored) text.</summary>
    public class TextBlock : Control
    {
        private Color _color = Theme.TextSecondary;
        private string _glyph;

        public TextBlock(string text, Color? color = null, string glyph = null)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Text = text;
            if (color.HasValue)
                _color = color.Value;
            _glyph = glyph;
            Font = Theme.Body;
            BackColor = Color.Transparent;
            AccessibleRole = AccessibleRole.StaticText;
        }

        public Color TextColor
        {
            get { return _color; }
            set { _color = value; Invalidate(); }
        }

        public string Glyph
        {
            get { return _glyph; }
            set { _glyph = value; Invalidate(); }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            AccessibleName = Text;
            // The height may change when the text wraps differently; re-flow every container up to the page.
            for (Control p = Parent; p != null && !(p is Form); p = p.Parent)
                p.PerformLayout();
            Invalidate();
        }

        private int GlyphWidth { get { return string.IsNullOrEmpty(_glyph) ? 0 : Theme.S(24); } }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = proposedSize.Width > 0 ? proposedSize.Width : Width;
            if (string.IsNullOrEmpty(Text))
                return new Size(w, 0);
            int h = TextRenderer.MeasureText(Text, Font, new Size(Math.Max(1, w - GlyphWidth), 0),
                                             TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
            return new Size(w, h);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.BackOf(this));
            if (!string.IsNullOrEmpty(_glyph))
                Theme.DrawGlyph(e.Graphics, _glyph, Theme.Icon, new Rectangle(0, 0, Theme.S(18), Theme.S(20)), _color);
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(GlyphWidth, 0, Width - GlyphWidth, Height), _color,
                                  TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        }
    }

    /// <summary>
    /// Rounded surface that stacks rows vertically with hairline dividers between them (Fluent "settings card").
    /// </summary>
    public class Card : Control
    {
        private readonly List<Control> _dividerAfter = new List<Control>();

        public Card()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.ContainerControl, true);
            BackColor = Theme.Surface;
        }

        /// <summary>Inner padding used when a card hosts free-form content instead of rows.</summary>
        public Padding ContentPadding { get; set; }

        public T Add<T>(T row) where T : Control
        {
            Controls.Add(row);
            return row;
        }

        private IEnumerable<Control> VisibleChildren()
        {
            foreach (Control c in Controls)
                if (c.IsSetVisible())
                    yield return c;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = proposedSize.Width > 0 ? proposedSize.Width : Width;
            int inner = w - ContentPadding.Horizontal;
            int h = ContentPadding.Vertical;
            int n = 0;
            foreach (Control c in VisibleChildren())
            {
                if (n++ > 0)
                    h += 1;
                h += c.PreferredHeight(inner);
            }
            return new Size(w, h);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int inner = Width - ContentPadding.Horizontal;
            int y = ContentPadding.Top;
            int n = 0;
            _dividerAfter.Clear();
            Control prev = null;
            foreach (Control c in VisibleChildren())
            {
                if (n++ > 0)
                {
                    _dividerAfter.Add(prev);
                    y += 1;
                }
                int h = c.PreferredHeight(inner);
                c.SetBounds(ContentPadding.Left, y, inner, h);
                y += h;
                prev = c;
            }
            Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.VisibleChanged += ChildVisibleChanged;
        }

        private void ChildVisibleChanged(object sender, EventArgs e)
        {
            PerformLayout();
            if (Parent != null)
                Parent.PerformLayout();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color back = Theme.BackOf(Parent);
            e.Graphics.Clear(back);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            // Clip children to the rounded shape so rows do not cover the corners.
            if (Width > 0 && Height > 0)
            {
                using (var path = Theme.RoundRect(new RectangleF(0, 0, Width, Height), Theme.S(8f)))
                {
                    Region old = Region;
                    Region = new Region(path);
                    if (old != null)
                        old.Dispose();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Surface);
            using (var p = new Pen(Theme.Divider))
            {
                foreach (Control c in _dividerAfter)
                    g.DrawLine(p, 0, c.Bottom, Width, c.Bottom);
            }
        }
    }

    /// <summary>
    /// A row inside a <see cref="Card"/>: icon, title, wrapped description and an accessory control on the right
    /// (toggle, button...) and/or a full-width content control underneath.
    /// </summary>
    public class SettingRow : Control
    {
        private Control _accessory;
        private Control _content;
        private bool _hover;
        private string _description;

        public SettingRow(string title, string description = null, string glyph = null)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.ContainerControl, true);
            Text = title;
            _description = description;
            Glyph = glyph;
            BackColor = Theme.Surface;
            AccessibleRole = AccessibleRole.Grouping;
            AccessibleName = title;
            AccessibleDescription = description;
        }

        public string Glyph { get; set; }

        /// <summary>Small pill shown next to the title, e.g. "Requires login".</summary>
        public string Badge { get; set; }

        public Color BadgeColor = Theme.AccentText;

        /// <summary>Fixed width for the accessory; 0 = its own width. Negative = fraction (in percent) of the row width.</summary>
        public int AccessoryWidth { get; set; }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value)
                    return;
                _description = value;
                AccessibleDescription = value;
                if (Parent != null)
                {
                    Parent.PerformLayout();
                    if (Parent.Parent != null)
                        Parent.Parent.PerformLayout();
                }
                Invalidate();
            }
        }

        /// <summary>Description color override (e.g. for validation messages).</summary>
        public Color? DescriptionColor { get; set; }

        public Control Accessory
        {
            get { return _accessory; }
            set
            {
                if (_accessory != null)
                    Controls.Remove(_accessory);
                _accessory = value;
                if (value != null)
                {
                    Controls.Add(value);
                    if (value is ToggleSwitch)
                        value.AccessibleName = Text;
                }
                PerformLayout();
            }
        }

        public Control Content
        {
            get { return _content; }
            set
            {
                if (_content != null)
                    Controls.Remove(_content);
                _content = value;
                if (value != null)
                    Controls.Add(value);
                PerformLayout();
            }
        }

        private int Pad { get { return Theme.S(16); } }
        private int GlyphW { get { return string.IsNullOrEmpty(Glyph) ? 0 : Theme.S(36); } }

        private int AccessoryW(int width)
        {
            if (_accessory == null || !_accessory.IsSetVisible())
                return 0;
            if (AccessoryWidth < 0)
                return width * -AccessoryWidth / 100;
            if (AccessoryWidth > 0)
                return AccessoryWidth;
            return _accessory.GetPreferredSize(Size.Empty).Width;
        }

        private int TextWidth(int width)
        {
            int aw = AccessoryW(width);
            return Math.Max(Theme.S(80), width - Pad * 2 - GlyphW - (aw > 0 ? aw + Theme.S(16) : 0));
        }

        private int TitleHeight { get { return TextRenderer.MeasureText("Ag", Theme.Body).Height; } }

        private int DescHeight(int width)
        {
            if (string.IsNullOrEmpty(_description))
                return 0;
            return TextRenderer.MeasureText(_description, Theme.Small, new Size(TextWidth(width), 0),
                                            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
        }

        private int HeaderHeight(int width)
        {
            int textH = TitleHeight + (string.IsNullOrEmpty(_description) ? 0 : Theme.S(2) + DescHeight(width));
            int accH = _accessory != null && _accessory.IsSetVisible() ? _accessory.GetPreferredSize(Size.Empty).Height : 0;
            return Math.Max(textH, accH);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = proposedSize.Width > 0 ? proposedSize.Width : Width;
            int vpad = Theme.S(12);
            int h = vpad + HeaderHeight(w) + vpad;
            if (_content != null && _content.IsSetVisible())
                h += _content.PreferredHeight(w - Pad * 2 - GlyphW) + Theme.S(2) + vpad - Theme.S(4);
            return new Size(w, Math.Max(h, Theme.S(56)));
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vpad = Theme.S(12);
            int headerH = Math.Max(HeaderHeight(Width), Theme.S(56) - vpad * 2);
            if (_accessory != null)
            {
                int aw = AccessoryW(Width);
                int ah = _accessory.GetPreferredSize(Size.Empty).Height;
                _accessory.SetBounds(Width - Pad - aw, vpad + (headerH - ah) / 2, aw, ah);
            }
            if (_content != null)
            {
                int x = Pad + GlyphW;
                int cw = Width - x - Pad;
                _content.SetBounds(x, vpad + headerH + Theme.S(10), cw, _content.PreferredHeight(cw));
            }
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { SetHover(true); base.OnMouseEnter(e); }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Moving onto a child control also raises MouseLeave; keep the highlight in that case.
            SetHover(ClientRectangle.Contains(PointToClient(MousePosition)));
            base.OnMouseLeave(e);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.MouseLeave += (s, a) => SetHover(ClientRectangle.Contains(PointToClient(MousePosition)));
        }

        private void SetHover(bool hover)
        {
            if (_hover == hover)
                return;
            _hover = hover;
            bool clickable = _accessory is ToggleSwitch && Enabled;
            Color back = clickable && _hover ? Theme.Blend(Theme.Surface, Theme.SurfaceHover, 0.6f) : Theme.Surface;
            if (BackColor != back)
            {
                BackColor = back; // children paint their rounded corners against this
                Invalidate(true);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            // Large click target: clicking anywhere on the row flips its switch.
            var toggle = _accessory as ToggleSwitch;
            if (toggle != null && toggle.Enabled && Enabled)
            {
                toggle.Checked = !toggle.Checked;
                toggle.Focus();
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color back = BackColor;
            g.Clear(back);
            int vpad = Theme.S(12);
            int headerH = Math.Max(HeaderHeight(Width), Theme.S(56) - vpad * 2);
            int textH = TitleHeight + (string.IsNullOrEmpty(_description) ? 0 : Theme.S(2) + DescHeight(Width));
            int y = vpad + Math.Max(0, (headerH - textH) / 2);
            if (_content != null && _content.IsSetVisible())
                y = vpad + Math.Max(0, (headerH - textH) / 2);
            if (!string.IsNullOrEmpty(Glyph))
                Theme.DrawGlyph(g, Glyph, Theme.Icon, new Rectangle(Pad, vpad, Theme.S(20), headerH), Enabled ? Theme.Text : Theme.TextDisabled);
            int x = Pad + GlyphW;
            int tw = TextWidth(Width);
            Color titleColor = Enabled ? Theme.Text : Theme.TextDisabled;
            TextRenderer.DrawText(g, Text, Theme.Body, new Rectangle(x, y, tw, TitleHeight), titleColor,
                                  TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            if (!string.IsNullOrEmpty(Badge))
            {
                int titleW = TextRenderer.MeasureText(Text, Theme.Body, Size.Empty, TextFormatFlags.NoPadding).Width;
                Size bs = TextRenderer.MeasureText(Badge, Theme.Small, Size.Empty, TextFormatFlags.NoPadding);
                var br = new Rectangle(x + titleW + Theme.S(10), y + (TitleHeight - bs.Height - Theme.S(4)) / 2, bs.Width + Theme.S(12), bs.Height + Theme.S(4));
                Theme.HighQuality(g);
                Theme.FillRound(g, Theme.Blend(back, BadgeColor, 0.16f), br, br.Height / 2f);
                TextRenderer.DrawText(g, Badge, Theme.Small, br, Enabled ? BadgeColor : Theme.TextDisabled,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            if (!string.IsNullOrEmpty(_description))
            {
                Color dc = !Enabled ? Theme.TextDisabled : DescriptionColor ?? Theme.TextSecondary;
                TextRenderer.DrawText(g, _description, Theme.Small, new Rectangle(x, y + TitleHeight + Theme.S(2), tw, DescHeight(Width) + 2), dc,
                                      TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            }
        }
    }

    /// <summary>A row of controls laid out left-to-right (wrapping when out of space).</summary>
    public class HStack : Control
    {
        public HStack()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.ContainerControl | ControlStyles.SupportsTransparentBackColor, true);
            Spacing = Theme.S(8);
            BackColor = Color.Transparent;
        }

        public int Spacing { get; set; }

        /// <summary>Child that stretches to fill the remaining width of its line (e.g. a text box).</summary>
        public Control Fill { get; set; }

        /// <summary>Controls pushed to the right edge.</summary>
        public HashSet<Control> RightAligned = new HashSet<Control>();

        public T Add<T>(T c, bool rightAligned = false) where T : Control
        {
            Controls.Add(c);
            if (rightAligned)
                RightAligned.Add(c);
            return c;
        }

        private List<List<Control>> Lines(int width, out List<int> heights)
        {
            var lines = new List<List<Control>>();
            heights = new List<int>();
            var line = new List<Control>();
            int x = 0, lh = 0;
            foreach (Control c in Controls)
            {
                if (!c.IsSetVisible())
                    continue;
                Size ps = c == Fill ? new Size(Theme.S(160), c.GetPreferredSize(Size.Empty).Height) : c.GetPreferredSize(Size.Empty);
                if (line.Count > 0 && x + ps.Width > width)
                {
                    lines.Add(line);
                    heights.Add(lh);
                    line = new List<Control>();
                    x = 0;
                    lh = 0;
                }
                line.Add(c);
                x += ps.Width + Spacing;
                lh = Math.Max(lh, ps.Height);
            }
            if (line.Count > 0)
            {
                lines.Add(line);
                heights.Add(lh);
            }
            return lines;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = proposedSize.Width > 0 ? proposedSize.Width : Math.Max(Width, 1);
            List<int> heights;
            Lines(w, out heights);
            int h = 0;
            for (int i = 0; i < heights.Count; i++)
                h += heights[i] + (i > 0 ? Spacing : 0);
            return new Size(w, h);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            List<int> heights;
            List<List<Control>> lines = Lines(Width, out heights);
            int y = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                int lh = heights[i];
                int used = 0;
                foreach (Control c in lines[i])
                    if (c != Fill)
                        used += c.GetPreferredSize(Size.Empty).Width + Spacing;
                int x = 0;
                int right = Width;
                // right aligned from the end
                for (int j = lines[i].Count - 1; j >= 0; j--)
                {
                    Control c = lines[i][j];
                    if (!RightAligned.Contains(c))
                        continue;
                    Size ps = c.GetPreferredSize(Size.Empty);
                    right -= ps.Width;
                    c.SetBounds(right, y + (lh - ps.Height) / 2, ps.Width, ps.Height);
                    right -= Spacing;
                }
                foreach (Control c in lines[i])
                {
                    if (RightAligned.Contains(c))
                        continue;
                    Size ps = c.GetPreferredSize(Size.Empty);
                    int w = c == Fill ? Math.Max(Theme.S(80), Width - used - (lines[i].Count == 1 ? 0 : 0)) : ps.Width;
                    if (c == Fill)
                        w = Math.Max(Theme.S(80), Width - used);
                    c.SetBounds(x, y + (lh - ps.Height) / 2, w, ps.Height);
                    x += w + Spacing;
                }
                y += lh + Spacing;
            }
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
