using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    public enum LogKind
    {
        /// <summary>Normal library output (stdout).</summary>
        Output,
        /// <summary>Library error output (stderr).</summary>
        Error,
        /// <summary>Warnings (stderr lines starting with "Warning").</summary>
        Warning,
        /// <summary>Messages from the GUI itself.</summary>
        App,
        Success
    }

    /// <summary>Colored, append-only console view. Keeps memory bounded by trimming old text.</summary>
    public class LogView : RichTextBox
    {
        private const int MaxChars = 2000000;
        private readonly StringBuilder _plain = new StringBuilder();
        private readonly List<KeyValuePair<string, LogKind>> _history = new List<KeyValuePair<string, LogKind>>();
        private bool _errorsOnly;

        public LogView()
        {
            BorderStyle = BorderStyle.None;
            BackColor = Theme.Input;
            ForeColor = Theme.Text;
            Font = Theme.Mono;
            ReadOnly = true;
            DetectUrls = false;
            WordWrap = true;
            HideSelection = false;
            ScrollBars = RichTextBoxScrollBars.Vertical;
            AutoScroll = true;
            ShortcutsEnabled = true;
            Native.UseDarkScrollBars(this);
        }

        /// <summary>Scroll to the end whenever text is appended.</summary>
        public bool AutoScroll { get; set; }

        public bool ErrorsOnly
        {
            get { return _errorsOnly; }
            set
            {
                if (_errorsOnly == value)
                    return;
                _errorsOnly = value;
                Rebuild();
            }
        }

        public string PlainText { get { return _plain.ToString(); } }

        public static Color ColorOf(LogKind kind)
        {
            switch (kind)
            {
                case LogKind.Error: return Theme.Error;
                case LogKind.Warning: return Theme.Warning;
                case LogKind.App: return Theme.Info;
                case LogKind.Success: return Theme.Success;
                default: return Theme.Text;
            }
        }

        public void Append(IList<KeyValuePair<string, LogKind>> chunks)
        {
            if (chunks.Count == 0)
                return;
            foreach (var c in chunks)
            {
                _history.Add(c);
                _plain.Append(c.Key);
            }
            if (_plain.Length > MaxChars)
            {
                Trim();
                return;
            }
            Native.SuspendDrawing(this);
            try
            {
                foreach (var c in chunks)
                    if (!_errorsOnly || c.Value == LogKind.Error || c.Value == LogKind.Warning)
                        AppendColored(c.Key, c.Value);
            }
            finally
            {
                Native.ResumeDrawing(this);
            }
            if (AutoScroll)
            {
                SelectionStart = TextLength;
                ScrollToCaret();
            }
        }

        private void AppendColored(string text, LogKind kind)
        {
            SelectionStart = TextLength;
            SelectionLength = 0;
            SelectionColor = ColorOf(kind);
            SelectedText = text;
        }

        private void Trim()
        {
            // Drop the oldest half of the history.
            int drop = _history.Count / 2;
            _history.RemoveRange(0, drop);
            _plain.Clear();
            foreach (var c in _history)
                _plain.Append(c.Key);
            Rebuild();
        }

        private void Rebuild()
        {
            Native.SuspendDrawing(this);
            try
            {
                Clear();
                foreach (var c in _history)
                    if (!_errorsOnly || c.Value == LogKind.Error || c.Value == LogKind.Warning)
                        AppendColored(c.Key, c.Value);
            }
            finally
            {
                Native.ResumeDrawing(this);
            }
            SelectionStart = TextLength;
            ScrollToCaret();
        }

        public void ClearLog()
        {
            _history.Clear();
            _plain.Clear();
            Clear();
        }
    }

    /// <summary>A target entry in the download list.</summary>
    public sealed class TargetEntry
    {
        public string Value;
        public string Raw;
        public string Kind;
        public Color KindColor;
        public string Glyph;
        public string Detail;

        public override string ToString()
        {
            return Value;
        }
    }

    /// <summary>Owner-drawn list of download targets with a type badge and a remove button on each row.</summary>
    public class TargetList : ListBox
    {
        private int _hover = -1;
        private bool _hoverRemove;

        public event EventHandler<TargetEntry> RemoveRequested;

        public TargetList()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = Theme.S(40);
            BorderStyle = BorderStyle.None;
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            IntegralHeight = false;
            SelectionMode = SelectionMode.MultiExtended;
            Native.UseDarkScrollBars(this);
        }

        /// <summary>Text shown when the list is empty.</summary>
        public string EmptyText { get; set; }

        private Rectangle RemoveRect(Rectangle bounds)
        {
            int s = Theme.S(28);
            return new Rectangle(bounds.Right - s - Theme.S(8), bounds.Y + (bounds.Height - s) / 2, s, s);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = IndexFromPoint(e.Location);
            bool overRemove = idx >= 0 && RemoveRect(GetItemRectangle(idx)).Contains(e.Location);
            if (idx != _hover || overRemove != _hoverRemove)
            {
                _hover = idx;
                _hoverRemove = overRemove;
                Cursor = overRemove ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int idx = IndexFromPoint(e.Location);
            if (e.Button == MouseButtons.Left && idx >= 0 && RemoveRect(GetItemRectangle(idx)).Contains(e.Location))
            {
                var h = RemoveRequested;
                if (h != null)
                    h(this, (TargetEntry)Items[idx]);
                return;
            }
            if (e.Button == MouseButtons.Right && idx >= 0 && !GetSelected(idx))
            {
                ClearSelected();
                SetSelected(idx, true);
            }
            base.OnMouseDown(e);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var b = new SolidBrush(BackColor))
                g.FillRectangle(b, e.Bounds);
            if (e.Index < 0 || e.Index >= Items.Count)
                return;
            var item = (TargetEntry)Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0;
            Theme.HighQuality(g);
            var rowRect = new Rectangle(e.Bounds.X + Theme.S(4), e.Bounds.Y + Theme.S(2), e.Bounds.Width - Theme.S(8), e.Bounds.Height - Theme.S(4));
            if (selected)
                Theme.FillRound(g, Theme.SurfacePressed, rowRect, Theme.S(5f));
            else if (e.Index == _hover)
                Theme.FillRound(g, Theme.SurfaceHover, rowRect, Theme.S(5f));

            int x = rowRect.X + Theme.S(10);
            // Type badge
            Theme.DrawGlyph(g, item.Glyph, Theme.Icon, new Rectangle(x, rowRect.Y, Theme.S(20), rowRect.Height), item.KindColor);
            x += Theme.S(30);
            Size ks = TextRenderer.MeasureText(item.Kind, Theme.SmallStrong, Size.Empty, TextFormatFlags.NoPadding);
            int badgeW = Math.Max(ks.Width + Theme.S(14), Theme.S(74));
            var badge = new Rectangle(x, rowRect.Y + (rowRect.Height - Theme.S(20)) / 2, badgeW, Theme.S(20));
            Theme.FillRound(g, Theme.Blend(Theme.Surface, item.KindColor, 0.18f), badge, Theme.S(10f));
            TextRenderer.DrawText(g, item.Kind, Theme.SmallStrong, badge, item.KindColor,
                                  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            x += badgeW + Theme.S(12);
            Rectangle rr = RemoveRect(e.Bounds);
            int textW = rr.X - x - Theme.S(8);
            int valueW = Math.Min(textW, TextRenderer.MeasureText(item.Value, Theme.BodyStrong, Size.Empty, TextFormatFlags.NoPadding).Width + Theme.S(4));
            TextRenderer.DrawText(g, item.Value, Theme.BodyStrong, new Rectangle(x, rowRect.Y, valueW, rowRect.Height), Theme.Text,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (!string.IsNullOrEmpty(item.Detail) && textW - valueW > Theme.S(40))
                TextRenderer.DrawText(g, item.Detail, Theme.Small, new Rectangle(x + valueW + Theme.S(8), rowRect.Y, textW - valueW - Theme.S(8), rowRect.Height),
                                      Theme.TextSecondary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (e.Index == _hover || selected)
            {
                if (_hoverRemove && e.Index == _hover)
                    Theme.FillRound(g, Theme.SurfacePressed, rr, Theme.S(4f));
                Theme.DrawGlyph(g, Glyphs.Close, Theme.IconSmall, rr, _hoverRemove && e.Index == _hover ? Theme.Error : Theme.TextSecondary);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == Native.WM_PAINT && Items.Count == 0 && !string.IsNullOrEmpty(EmptyText))
            {
                using (Graphics g = CreateGraphics())
                {
                    g.Clear(BackColor);
                    var r = new Rectangle(Theme.S(16), 0, Width - Theme.S(32), Height);
                    TextRenderer.DrawText(g, EmptyText, Theme.Body, r, Theme.TextSecondary,
                                          TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                }
            }
        }
    }
}
