using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    public enum ButtonKind
    {
        /// <summary>Main call to action - accent gradient fill.</summary>
        Primary,
        /// <summary>Standard button with a subtle fill and border.</summary>
        Secondary,
        /// <summary>No fill until hovered; for toolbars and low-emphasis actions.</summary>
        Subtle,
        /// <summary>Destructive action.</summary>
        Danger
    }

    /// <summary>
    /// A flat, rounded button with an optional icon glyph. Derives from <see cref="Button"/> so that keyboard
    /// activation, AcceptButton/CancelButton and accessibility keep working.
    /// </summary>
    public class FlatButton : Button
    {
        private bool _hover;
        private bool _pressed;
        private ButtonKind _kind = ButtonKind.Secondary;
        private string _glyph;

        public FlatButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = Theme.Body;
            ForeColor = Theme.Text;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Height = Theme.S(32);
            UseVisualStyleBackColor = false;
        }

        public FlatButton(string text, ButtonKind kind = ButtonKind.Secondary, string glyph = null) : this()
        {
            Text = text;
            _kind = kind;
            _glyph = glyph;
            AccessibleName = text;
            AutoSizeToContent();
        }

        public ButtonKind Kind
        {
            get { return _kind; }
            set { _kind = value; Invalidate(); }
        }

        public string Glyph
        {
            get { return _glyph; }
            set { _glyph = value; Invalidate(); }
        }

        /// <summary>When set, the button is square and shows only its glyph (Text becomes the tooltip/accessible name).</summary>
        public bool IconOnly { get; set; }

        public void AutoSizeToContent()
        {
            Size = GetPreferredSize(Size.Empty);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int h = Theme.S(32);
            if (IconOnly)
                return new Size(h, h);
            int w = TextRenderer.MeasureText(Text ?? "", Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            w += Theme.S(_kind == ButtonKind.Subtle ? 20 : 28);
            if (!string.IsNullOrEmpty(_glyph))
                w += Theme.S(string.IsNullOrEmpty(Text) ? 16 : 24);
            return new Size(Math.Max(w, h), h);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left) { _pressed = true; Invalidate(); }
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Paint the parent's background so that the rounded corners blend in.
            Color parentBack = Theme.BackOf(Parent);
            using (var b = new SolidBrush(parentBack))
                pevent.Graphics.FillRectangle(b, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnPaintBackground(e);
            Graphics g = e.Graphics;
            Theme.HighQuality(g);
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = Theme.S(5f);
            Color text = Theme.Text;
            bool enabled = Enabled;

            switch (_kind)
            {
                case ButtonKind.Primary:
                    if (!enabled)
                    {
                        Theme.FillRound(g, Theme.SurfaceHover, r, radius);
                        text = Theme.TextDisabled;
                    }
                    else
                    {
                        Color c1 = _pressed ? Theme.AccentPressed : _hover ? Theme.AccentHover : Theme.Accent;
                        Color c2 = _pressed ? Theme.Blend(Theme.AccentAlt, Color.Black, 0.15f)
                                            : _hover ? Theme.Blend(Theme.AccentAlt, Color.White, 0.1f) : Theme.AccentAlt;
                        using (var path = Theme.RoundRect(r, radius))
                        using (var brush = new LinearGradientBrush(new RectangleF(0, 0, Width, Height), c1, c2, LinearGradientMode.Horizontal))
                            g.FillPath(brush, path);
                        text = Color.White;
                    }
                    break;
                case ButtonKind.Danger:
                    {
                        Color fill = !enabled ? Theme.Surface : _pressed ? Color.FromArgb(120, 36, 36) : _hover ? Color.FromArgb(150, 44, 44) : Color.FromArgb(70, 32, 34);
                        Theme.FillRound(g, fill, r, radius);
                        Theme.DrawRound(g, enabled ? Color.FromArgb(150, 60, 60) : Theme.Border, r, radius);
                        text = enabled ? Color.FromArgb(255, 220, 218) : Theme.TextDisabled;
                    }
                    break;
                case ButtonKind.Subtle:
                    if (enabled && (_hover || _pressed))
                        Theme.FillRound(g, _pressed ? Theme.SurfacePressed : Theme.SurfaceHover, r, radius);
                    text = enabled ? Theme.Text : Theme.TextDisabled;
                    break;
                default:
                    {
                        Color fill = !enabled ? Theme.Surface : _pressed ? Theme.SurfacePressed : _hover ? Theme.SurfaceHover : Theme.Surface;
                        Theme.FillRound(g, fill, r, radius);
                        Theme.DrawRound(g, enabled && _hover ? Theme.BorderStrong : Theme.Border, r, radius);
                        text = enabled ? Theme.Text : Theme.TextDisabled;
                    }
                    break;
            }

            // Visible keyboard focus indicator
            if (Focused && ShowFocusCues && enabled)
                Theme.DrawRound(g, Theme.AccentText, new RectangleF(1.5f, 1.5f, Width - 3.5f, Height - 3.5f), radius, Theme.S(1.5f));

            bool hasGlyph = !string.IsNullOrEmpty(_glyph);
            bool hasText = !IconOnly && !string.IsNullOrEmpty(Text);
            if (hasGlyph && !hasText)
            {
                Theme.DrawGlyph(g, _glyph, Theme.Icon, ClientRectangle, text);
                return;
            }
            int textW = TextRenderer.MeasureText(Text ?? "", Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            int glyphW = hasGlyph ? Theme.S(16) : 0;
            int gap = hasGlyph ? Theme.S(8) : 0;
            int total = glyphW + gap + textW;
            int x = Math.Max(Theme.S(6), (Width - total) / 2);
            if (hasGlyph)
                Theme.DrawGlyph(g, _glyph, Theme.Icon, new Rectangle(x, 0, glyphW, Height), text);
            TextRenderer.DrawText(g, Text, Font, new Rectangle(x + glyphW + gap, 0, Width - (x + glyphW + gap) - Theme.S(4), Height), text,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                                  TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
