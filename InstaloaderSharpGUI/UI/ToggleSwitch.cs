using System;
using System.Drawing;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    /// <summary>
    /// An on/off switch (Fluent style). Derives from <see cref="CheckBox"/> to keep keyboard (Space) and
    /// accessibility support. Shows an explicit "On"/"Off" state label so state never relies on color alone.
    /// </summary>
    public class ToggleSwitch : CheckBox
    {
        private bool _hover;
        private float _knob; // 0 = off, 1 = on (animated)
        private readonly Timer _anim;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            Size = new Size(Theme.S(84), Theme.S(28));
            Cursor = Cursors.Hand;
            Font = Theme.Body;
            _anim = new Timer { Interval = 15 };
            _anim.Tick += (s, e) =>
            {
                float target = Checked ? 1f : 0f;
                float step = 0.2f;
                if (Math.Abs(_knob - target) <= step) { _knob = target; _anim.Stop(); }
                else _knob += _knob < target ? step : -step;
                Invalidate();
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _anim.Dispose();
            base.Dispose(disposing);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(Theme.S(84), Theme.S(28));
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            if (IsHandleCreated && Visible)
                _anim.Start();
            else
                _knob = Checked ? 1f : 0f;
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _knob = Checked ? 1f : 0f;
        }

        protected override void OnMouseEnter(EventArgs eventargs) { _hover = true; Invalidate(); base.OnMouseEnter(eventargs); }
        protected override void OnMouseLeave(EventArgs eventargs) { _hover = false; Invalidate(); base.OnMouseLeave(eventargs); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color parentBack = Theme.BackOf(Parent);
            g.Clear(parentBack);
            Theme.HighQuality(g);

            int trackW = Theme.S(40), trackH = Theme.S(20);
            var track = new RectangleF(Width - trackW - Theme.S(2) + 0.5f, (Height - trackH) / 2f + 0.5f, trackW - 1, trackH - 1);
            bool enabled = Enabled;

            Color onColor = enabled ? (_hover ? Theme.AccentHover : Theme.Accent) : Theme.SurfacePressed;
            Color offFill = _hover && enabled ? Theme.SurfaceHover : Theme.Input;
            Color fill = Theme.Blend(offFill, onColor, _knob);
            Theme.FillRound(g, fill, track, trackH / 2f);
            if (_knob < 0.99f)
                Theme.DrawRound(g, Theme.Blend(enabled ? Theme.TextSecondary : Theme.TextDisabled, onColor, _knob), track, trackH / 2f);

            float knobD = Theme.S(12f) + Theme.S(2f) * _knob;
            float left = track.X + Theme.S(4f);
            float right = track.Right - Theme.S(4f) - knobD;
            float kx = left + (right - left) * _knob;
            float ky = track.Y + (track.Height - knobD) / 2f;
            Color knobColor = enabled ? Theme.Blend(Theme.TextSecondary, Color.White, _knob) : Theme.TextDisabled;
            using (var b = new SolidBrush(knobColor))
                g.FillEllipse(b, kx, ky, knobD, knobD);

            string state = Checked ? "On" : "Off";
            var textRect = new Rectangle(0, 0, (int)track.X - Theme.S(8), Height);
            TextRenderer.DrawText(g, state, Font, textRect, enabled ? Theme.TextSecondary : Theme.TextDisabled,
                                  TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            if (Focused && ShowFocusCues)
                Theme.DrawRound(g, Theme.AccentText, new RectangleF(track.X - 2, track.Y - 2, track.Width + 4, track.Height + 4),
                                trackH / 2f + 2, Theme.S(1.5f));
        }
    }
}
