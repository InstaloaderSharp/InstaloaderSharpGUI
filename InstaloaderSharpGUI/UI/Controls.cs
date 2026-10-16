using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    /// <summary>Mutually exclusive options shown side by side (replaces radio buttons). Arrow keys move the selection.</summary>
    public class SegmentedControl : Control
    {
        private readonly List<string> _items = new List<string>();
        private readonly List<string> _glyphs = new List<string>();
        private int _selected;
        private int _hover = -1;

        public event EventHandler SelectedIndexChanged;

        public SegmentedControl(params string[] items)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            Font = Theme.Body;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PageTabList;
            foreach (string s in items)
                AddItem(s, null);
            Height = Theme.S(34);
        }

        public void AddItem(string text, string glyph)
        {
            _items.Add(text);
            _glyphs.Add(glyph);
            Invalidate();
        }

        public void SetGlyphs(params string[] glyphs)
        {
            for (int i = 0; i < glyphs.Length && i < _glyphs.Count; i++)
                _glyphs[i] = glyphs[i];
        }

        public int SelectedIndex
        {
            get { return _selected; }
            set
            {
                value = Math.Max(0, Math.Min(_items.Count - 1, value));
                if (_selected == value)
                    return;
                _selected = value;
                AccessibleName = _items.Count > 0 ? _items[_selected] : "";
                Invalidate();
                var h = SelectedIndexChanged;
                if (h != null)
                    h(this, EventArgs.Empty);
            }
        }

        private int ItemWidth(int i)
        {
            int w = TextRenderer.MeasureText(_items[i], Font, Size.Empty, TextFormatFlags.NoPadding).Width + Theme.S(28);
            if (!string.IsNullOrEmpty(_glyphs[i]))
                w += Theme.S(24);
            return w;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = Theme.S(8);
            for (int i = 0; i < _items.Count; i++)
                w += ItemWidth(i);
            return new Size(w, Theme.S(34));
        }

        private Rectangle ItemRect(int i)
        {
            int x = Theme.S(4);
            for (int j = 0; j < i; j++)
                x += ItemWidth(j);
            return new Rectangle(x, Theme.S(4), ItemWidth(i), Height - Theme.S(8));
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < _items.Count; i++)
                if (ItemRect(i).Contains(p))
                    return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = HitTest(e.Location);
            if (h != _hover) { _hover = h; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int h = HitTest(e.Location);
            if (h >= 0)
                SelectedIndex = h;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Left) SelectedIndex--;
            else if (e.KeyCode == Keys.Right) SelectedIndex++;
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.BackOf(Parent));
            Theme.HighQuality(g);
            int totalW = GetPreferredSize(Size.Empty).Width;
            var outer = new RectangleF(0.5f, 0.5f, Math.Min(Width, totalW) - 1.5f, Height - 1.5f);
            Theme.FillRound(g, Theme.Input, outer, Theme.S(6f));
            Theme.DrawRound(g, Theme.Border, outer, Theme.S(6f));
            for (int i = 0; i < _items.Count; i++)
            {
                Rectangle r = ItemRect(i);
                bool sel = i == _selected;
                if (sel)
                {
                    Theme.FillRound(g, Theme.SurfacePressed, r, Theme.S(4f));
                    using (var b = new SolidBrush(Theme.Accent))
                        g.FillRectangle(b, r.X + r.Width / 2 - Theme.S(8), r.Bottom - Theme.S(3), Theme.S(16), Theme.S(3));
                }
                else if (i == _hover && Enabled)
                {
                    Theme.FillRound(g, Theme.SurfaceHover, r, Theme.S(4f));
                }
                Color c = !Enabled ? Theme.TextDisabled : sel ? Theme.Text : Theme.TextSecondary;
                int x = r.X + Theme.S(14);
                if (!string.IsNullOrEmpty(_glyphs[i]))
                {
                    Theme.DrawGlyph(g, _glyphs[i], Theme.Icon, new Rectangle(x, r.Y, Theme.S(18), r.Height), c);
                    x += Theme.S(24);
                }
                TextRenderer.DrawText(g, _items[i], sel ? Theme.BodyStrong : Font, new Rectangle(x, r.Y, r.Right - x, r.Height), c,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
                if (sel && Focused && ShowFocusCues)
                    Theme.DrawRound(g, Theme.AccentText, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), Theme.S(4f), Theme.S(1.5f));
            }
        }
    }

    /// <summary>Navigation item for <see cref="NavRail"/>.</summary>
    public sealed class NavItem
    {
        public string Text;
        public string Glyph;
        public string Badge;
        public Color BadgeColor = Theme.Accent;
        public Control Page;
    }

    /// <summary>Left navigation rail with labelled icons and an accent selection indicator.</summary>
    public class NavRail : Control
    {
        public readonly List<NavItem> Items = new List<NavItem>();
        private int _selected = -1;
        private int _hover = -1;

        public event EventHandler SelectedIndexChanged;

        public NavRail()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            BackColor = Theme.Sidebar;
            Font = Theme.Body;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PageTabList;
            AccessibleName = "Navigation";
        }

        private int ItemH { get { return Theme.S(40); } }
        private int Top0 { get { return Theme.S(8); } }

        public int SelectedIndex
        {
            get { return _selected; }
            set
            {
                if (value < 0 || value >= Items.Count || value == _selected)
                    return;
                _selected = value;
                Invalidate();
                var h = SelectedIndexChanged;
                if (h != null)
                    h(this, EventArgs.Empty);
            }
        }

        public NavItem SelectedItem { get { return _selected >= 0 ? Items[_selected] : null; } }

        private Rectangle ItemRect(int i)
        {
            return new Rectangle(Theme.S(8), Top0 + i * (ItemH + Theme.S(4)), Width - Theme.S(16), ItemH);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(Width, Top0 + Items.Count * (ItemH + Theme.S(4)) + Theme.S(8));
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < Items.Count; i++)
                if (ItemRect(i).Contains(p))
                    return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = HitTest(e.Location);
            if (h != _hover) { _hover = h; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int h = HitTest(e.Location);
            if (h >= 0)
                SelectedIndex = h;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Up || keyData == Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Up && _selected > 0) SelectedIndex = _selected - 1;
            else if (e.KeyCode == Keys.Down) SelectedIndex = _selected + 1;
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            Theme.HighQuality(g);
            for (int i = 0; i < Items.Count; i++)
            {
                NavItem item = Items[i];
                Rectangle r = ItemRect(i);
                bool sel = i == _selected;
                if (sel)
                    Theme.FillRound(g, Theme.SurfaceHover, r, Theme.S(6f));
                else if (i == _hover)
                    Theme.FillRound(g, Theme.Blend(Theme.Sidebar, Theme.SurfaceHover, 0.6f), r, Theme.S(6f));
                if (sel)
                {
                    var ind = new RectangleF(r.X, r.Y + r.Height / 2f - Theme.S(8f), Theme.S(3f), Theme.S(16f));
                    Theme.FillRound(g, Theme.Accent, ind, Theme.S(1.5f));
                }
                Color c = sel ? Theme.Text : Theme.TextSecondary;
                Theme.DrawGlyph(g, item.Glyph, Theme.Icon, new Rectangle(r.X + Theme.S(12), r.Y, Theme.S(20), r.Height), sel ? Theme.AccentText : c);
                TextRenderer.DrawText(g, item.Text, sel ? Theme.BodyStrong : Font, new Rectangle(r.X + Theme.S(44), r.Y, r.Width - Theme.S(50), r.Height), c,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
                if (!string.IsNullOrEmpty(item.Badge))
                {
                    Size bs = TextRenderer.MeasureText(item.Badge, Theme.SmallStrong, Size.Empty, TextFormatFlags.NoPadding);
                    int bw = Math.Max(bs.Width + Theme.S(12), Theme.S(20));
                    var br = new Rectangle(r.Right - bw - Theme.S(8), r.Y + (r.Height - Theme.S(20)) / 2, bw, Theme.S(20));
                    Theme.FillRound(g, item.BadgeColor, br, Theme.S(10f));
                    TextRenderer.DrawText(g, item.Badge, Theme.SmallStrong, br, Color.White,
                                          TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
                if (sel && Focused && ShowFocusCues)
                    Theme.DrawRound(g, Theme.AccentText, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), Theme.S(6f), Theme.S(1.5f));
            }
        }
    }

    /// <summary>Thin rounded progress bar with determinate and indeterminate (animated) modes.</summary>
    public class ProgressBarEx : Control
    {
        private long _value;
        private long _maximum = 100;
        private bool _indeterminate;
        private float _phase;
        private readonly Timer _timer;
        private Color _color = Theme.Accent;

        public ProgressBarEx()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = Theme.S(6);
            AccessibleRole = AccessibleRole.ProgressBar;
            _timer = new Timer { Interval = 16 };
            _timer.Tick += (s, e) => { _phase = (_phase + 0.012f) % 1.6f; Invalidate(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _timer.Dispose();
            base.Dispose(disposing);
        }

        public long Value
        {
            get { return _value; }
            set { _value = Math.Max(0, Math.Min(value, _maximum)); AccessibleDescription = Percent + "%"; Invalidate(); }
        }

        public long Maximum
        {
            get { return _maximum; }
            set { _maximum = Math.Max(1, value); Invalidate(); }
        }

        public int Percent { get { return (int)(_value * 100 / Math.Max(1, _maximum)); } }

        public Color BarColor
        {
            get { return _color; }
            set { _color = value; Invalidate(); }
        }

        public bool Indeterminate
        {
            get { return _indeterminate; }
            set
            {
                _indeterminate = value;
                if (value) _timer.Start(); else _timer.Stop();
                Invalidate();
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(proposedSize.Width, Theme.S(6));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.BackOf(Parent));
            Theme.HighQuality(g);
            var track = new RectangleF(0, 0, Width - 0.5f, Height - 0.5f);
            float radius = Height / 2f;
            Theme.FillRound(g, Theme.SurfacePressed, track, radius);
            RectangleF bar;
            if (_indeterminate)
            {
                float w = Width * 0.3f;
                float x = (_phase - 0.3f) * Width;
                bar = new RectangleF(x, 0, w, track.Height);
                bar.Intersect(track);
            }
            else
            {
                bar = new RectangleF(0, 0, track.Width * _value / Math.Max(1, _maximum), track.Height);
            }
            if (bar.Width >= 1)
            {
                using (var path = Theme.RoundRect(bar, radius))
                using (var brush = new LinearGradientBrush(new RectangleF(0, 0, Math.Max(2, Width), Height),
                                                           _color, _color == Theme.Accent ? Theme.AccentAlt : _color,
                                                           LinearGradientMode.Horizontal))
                    g.FillPath(brush, path);
            }
        }
    }

    /// <summary>Circular avatar showing a picture, or initials on an accent gradient.</summary>
    public class Avatar : Control
    {
        private Image _image;

        public Avatar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(Theme.S(36), Theme.S(36));
            BackColor = Color.Transparent;
        }

        public Image Image
        {
            get { return _image; }
            set { _image = value; Invalidate(); }
        }

        /// <summary>Glyph shown when there is no text (e.g. anonymous user).</summary>
        public string Glyph { get; set; }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return Size;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.BackOf(Parent));
            Theme.HighQuality(g);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(r);
                if (_image != null)
                {
                    g.SetClip(path);
                    g.DrawImage(_image, r);
                    g.ResetClip();
                }
                else
                {
                    using (var brush = new LinearGradientBrush(r, Theme.Accent, Theme.AccentAlt, 45f))
                        g.FillPath(brush, path);
                    string t = !string.IsNullOrEmpty(Text) ? Text.Substring(0, 1).ToUpperInvariant() : null;
                    if (t != null)
                    {
                        using (var f = new Font("Segoe UI Semibold", Math.Max(8f, Height / 2.6f / Theme.ScaleFactor)))
                            Theme.DrawGlyph(g, t, f, ClientRectangle, Color.White);
                    }
                    else if (!string.IsNullOrEmpty(Glyph))
                    {
                        Theme.DrawGlyph(g, Glyph, Height > Theme.S(48) ? Theme.IconLarge : Theme.Icon, ClientRectangle, Color.White);
                    }
                }
            }
        }
    }

    /// <summary>Dark tooltip (owner drawn).</summary>
    public class DarkToolTip : ToolTip
    {
        public DarkToolTip()
        {
            OwnerDraw = true;
            AutoPopDelay = 15000;
            InitialDelay = 500;
            ReshowDelay = 100;
            Popup += OnPopup;
            Draw += OnDraw;
        }

        private static void OnPopup(object sender, PopupEventArgs e)
        {
            var tip = (ToolTip)sender;
            string text = tip.GetToolTip(e.AssociatedControl);
            Size s = TextRenderer.MeasureText(text, Theme.Small, new Size(Theme.S(360), 0), TextFormatFlags.WordBreak);
            e.ToolTipSize = new Size(s.Width + Theme.S(16), s.Height + Theme.S(12));
        }

        private static void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            using (var b = new SolidBrush(Theme.SurfacePressed))
                e.Graphics.FillRectangle(b, e.Bounds);
            using (var p = new Pen(Theme.BorderStrong))
                e.Graphics.DrawRectangle(p, 0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1);
            TextRenderer.DrawText(e.Graphics, e.ToolTipText, Theme.Small,
                                  new Rectangle(Theme.S(8), Theme.S(6), e.Bounds.Width - Theme.S(16), e.Bounds.Height - Theme.S(12)),
                                  Theme.Text, TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }
    }

    /// <summary>Dark renderer for context menus and drop-down menus.</summary>
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColors())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? Theme.Text : Theme.TextDisabled;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Theme.TextSecondary;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle r = e.ImageRectangle;
            Theme.DrawGlyph(e.Graphics, Glyphs.Accept, Theme.IconSmall, r, Theme.AccentText);
        }

        private sealed class DarkColors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Theme.SurfaceHover; } }
            public override Color ImageMarginGradientBegin { get { return Theme.SurfaceHover; } }
            public override Color ImageMarginGradientMiddle { get { return Theme.SurfaceHover; } }
            public override Color ImageMarginGradientEnd { get { return Theme.SurfaceHover; } }
            public override Color MenuBorder { get { return Theme.BorderStrong; } }
            public override Color MenuItemBorder { get { return Theme.SurfacePressed; } }
            public override Color MenuItemSelected { get { return Theme.SurfacePressed; } }
            public override Color MenuItemSelectedGradientBegin { get { return Theme.SurfacePressed; } }
            public override Color MenuItemSelectedGradientEnd { get { return Theme.SurfacePressed; } }
            public override Color MenuItemPressedGradientBegin { get { return Theme.SurfacePressed; } }
            public override Color MenuItemPressedGradientEnd { get { return Theme.SurfacePressed; } }
            public override Color SeparatorDark { get { return Theme.Border; } }
            public override Color SeparatorLight { get { return Theme.Border; } }
            public override Color CheckBackground { get { return Theme.AccentSubtle; } }
            public override Color CheckSelectedBackground { get { return Theme.AccentSubtle; } }
            public override Color CheckPressedBackground { get { return Theme.AccentSubtle; } }
        }
    }

    internal static class Menus
    {
        public static ContextMenuStrip Create()
        {
            var m = new ContextMenuStrip
            {
                Renderer = new DarkMenuRenderer(),
                Font = Theme.Body,
                ShowImageMargin = false,
                ForeColor = Theme.Text,
                BackColor = Theme.SurfaceHover
            };
            return m;
        }

        public static ToolStripMenuItem Item(this ContextMenuStrip menu, string text, EventHandler onClick, string shortcut = null)
        {
            var item = new ToolStripMenuItem(text, null, onClick) { ForeColor = Theme.Text, Padding = new Padding(0, Theme.S(3), 0, Theme.S(3)) };
            if (shortcut != null)
                item.ShortcutKeyDisplayString = shortcut;
            menu.Items.Add(item);
            return item;
        }

        public static void Separator(this ContextMenuStrip menu)
        {
            menu.Items.Add(new ToolStripSeparator());
        }

        /// <summary>Shows a menu below a button (used for drop-down buttons).</summary>
        public static void ShowBelow(this ContextMenuStrip menu, Control anchor)
        {
            menu.Show(anchor, new Point(0, anchor.Height + Theme.S(2)));
        }
    }
}
