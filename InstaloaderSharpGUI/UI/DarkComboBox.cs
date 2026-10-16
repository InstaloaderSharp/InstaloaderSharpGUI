using System;
using System.Drawing;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    /// <summary>A drop-down list painted in the dark theme, matching <see cref="DarkTextBox"/>.</summary>
    public class DarkComboBox : ComboBox
    {
        private bool _hover;

        public DarkComboBox()
        {
            // The closed box is painted by OnPaint; the drop-down list is owner drawn (OnDrawItem).
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            FlatStyle = FlatStyle.Flat;
            BackColor = Theme.Input;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            ItemHeight = Theme.S(24);
            Width = Theme.S(200);
            IntegralHeight = false;
            MaxDropDownItems = 12;
            Cursor = Cursors.Hand;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return Size;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Native.UseDarkScrollBars(this);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnSelectedIndexChanged(EventArgs e) { base.OnSelectedIndexChanged(e); Invalidate(); }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            bool edit = (e.State & DrawItemState.ComboBoxEdit) != 0;
            bool selected = (e.State & DrawItemState.Selected) != 0 && !edit;
            using (var b = new SolidBrush(selected ? Theme.AccentSubtle : Theme.Input))
                e.Graphics.FillRectangle(b, e.Bounds);
            if (selected)
            {
                using (var b = new SolidBrush(Theme.Accent))
                    e.Graphics.FillRectangle(b, e.Bounds.X, e.Bounds.Y + Theme.S(5), Theme.S(3), e.Bounds.Height - Theme.S(10));
            }
            var r = new Rectangle(e.Bounds.X + Theme.S(10), e.Bounds.Y, e.Bounds.Width - Theme.S(12), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, r, Theme.Text,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            PaintClosed(e.Graphics);
        }

        private void PaintClosed(Graphics g)
        {
            Color parentBack = Theme.BackOf(Parent);
            using (var b = new SolidBrush(parentBack))
                g.FillRectangle(b, ClientRectangle);
            Theme.HighQuality(g);
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = Theme.S(5f);
            Theme.FillRound(g, Enabled ? (_hover ? Theme.InputHover : Theme.Input) : Theme.Surface, r, radius);
            Theme.DrawRound(g, Focused || _hover ? Theme.BorderStrong : Theme.Border, r, radius);
            if (Focused)
            {
                using (var path = Theme.RoundRect(r, radius))
                {
                    g.SetClip(new RectangleF(0, Height - Theme.S(2f) - 0.5f, Width, Theme.S(2f) + 1));
                    using (var b = new SolidBrush(Theme.Accent))
                        g.FillPath(b, path);
                    g.ResetClip();
                }
            }
            int arrowW = Theme.S(28);
            string text = SelectedIndex >= 0 ? GetItemText(SelectedItem) : "";
            TextRenderer.DrawText(g, text, Font, new Rectangle(Theme.S(10), 0, Width - arrowW - Theme.S(10), Height),
                                  Enabled ? Theme.Text : Theme.TextDisabled,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            Theme.DrawGlyph(g, Glyphs.ChevronDown, Theme.IconSmall, new Rectangle(Width - arrowW, 0, arrowW - Theme.S(4), Height),
                            Enabled ? Theme.TextSecondary : Theme.TextDisabled);
        }
    }
}
