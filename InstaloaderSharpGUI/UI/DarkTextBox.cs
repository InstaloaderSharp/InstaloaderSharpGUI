using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    /// <summary>
    /// A text input with a rounded border, placeholder text, an accent underline while focused and an error state.
    /// Wraps a borderless <see cref="TextBox"/>.
    /// </summary>
    public class DarkTextBox : Control
    {
        private readonly CueTextBox _box;
        private bool _hover;
        private bool _hasError;
        private string _placeholder = "";
        private int _multilineHeight;

        public DarkTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Theme.Input;
            _box = new CueTextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                Font = Theme.Body
            };
            _box.TextChanged += (s, e) => OnTextChanged(e);
            _box.GotFocus += (s, e) => { Invalidate(); _box.Invalidate(); };
            _box.LostFocus += (s, e) => { Invalidate(); _box.Invalidate(); };
            _box.MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            _box.MouseLeave += (s, e) => { _hover = ClientRectangle.Contains(PointToClient(MousePosition)); Invalidate(); };
            _box.KeyDown += (s, e) => OnKeyDown(e);
            _box.KeyPress += (s, e) => OnKeyPress(e);
            _box.HandleCreated += (s, e) => Native.SetCueBanner(_box, _placeholder);
            Native.UseDarkScrollBars(_box);
            Controls.Add(_box);
            Height = Theme.S(32);
            Width = Theme.S(240);
            Cursor = Cursors.IBeam;
        }

        [Browsable(false)]
        public TextBox InnerTextBox { get { return _box; } }

        public override string Text
        {
            get { return _box.Text; }
            set { _box.Text = value ?? ""; }
        }

        public override Font Font
        {
            get { return _box.Font; }
            set { _box.Font = value; LayoutBox(); }
        }

        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value ?? "";
                if (!_box.Multiline)
                    Native.SetCueBanner(_box, _placeholder);
                Invalidate();
            }
        }

        public bool UseSystemPasswordChar
        {
            get { return _box.UseSystemPasswordChar; }
            set { _box.UseSystemPasswordChar = value; }
        }

        public bool ReadOnly
        {
            get { return _box.ReadOnly; }
            set { _box.ReadOnly = value; }
        }

        public bool Multiline
        {
            get { return _box.Multiline; }
            set
            {
                _box.Multiline = value;
                _box.AcceptsReturn = value;
                _box.ScrollBars = value ? ScrollBars.Vertical : ScrollBars.None;
                _box.WordWrap = value;
                if (value && _multilineHeight == 0)
                    _multilineHeight = Theme.S(80);
                LayoutBox();
            }
        }

        public int MultilineHeight
        {
            get { return _multilineHeight; }
            set { _multilineHeight = value; if (Multiline) Height = value; }
        }

        public bool HasError
        {
            get { return _hasError; }
            set { if (_hasError != value) { _hasError = value; Invalidate(); } }
        }

        public void SelectAll()
        {
            _box.SelectAll();
        }

        public void InsertAtCaret(string text)
        {
            int pos = _box.SelectionStart;
            _box.SelectedText = text;
            _box.SelectionStart = pos + text.Length;
            _box.Focus();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(Width, Multiline ? Math.Max(_multilineHeight, Theme.S(48)) : Theme.S(32));
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _box.Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _box.Focus();
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            _box.ForeColor = Enabled ? Theme.Text : Theme.TextDisabled;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Multiline && Height > Theme.S(40))
                _multilineHeight = Height;
            LayoutBox();
        }

        private void LayoutBox()
        {
            int padX = Theme.S(10);
            if (Multiline)
            {
                int padY = Theme.S(7);
                _box.Bounds = new Rectangle(padX, padY, Math.Max(10, Width - padX - Theme.S(4)), Math.Max(10, Height - padY * 2));
            }
            else
            {
                int h = _box.PreferredHeight;
                _box.Bounds = new Rectangle(padX, (Height - h) / 2, Math.Max(10, Width - padX * 2), h);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color parentBack = Theme.BackOf(Parent);
            g.Clear(parentBack);
            Theme.HighQuality(g);
            bool focused = _box.Focused;
            Color fill = !Enabled ? Theme.Surface : (_hover && !focused ? Theme.InputHover : Theme.Input);
            if (_box.BackColor != fill)
                _box.BackColor = fill;
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = Theme.S(5f);
            Theme.FillRound(g, fill, r, radius);
            Color border = _hasError ? Theme.Error : focused ? Theme.BorderStrong : _hover ? Theme.BorderStrong : Theme.Border;
            Theme.DrawRound(g, border, r, radius);
            if (focused || _hasError)
            {
                // Fluent-style accent underline indicates the focused field.
                using (var path = Theme.RoundRect(r, radius))
                {
                    g.SetClip(new RectangleF(0, Height - Theme.S(2f) - 0.5f, Width, Theme.S(2f) + 1));
                    using (var b = new SolidBrush(_hasError ? Theme.Error : Theme.Accent))
                        g.FillPath(b, path);
                    g.ResetClip();
                }
            }
            _box.Cue = Multiline ? _placeholder : null;
        }

        /// <summary>TextBox that paints a placeholder when multiline (EM_SETCUEBANNER only supports single-line).</summary>
        private sealed class CueTextBox : TextBox
        {
            public string Cue;

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == Native.WM_PAINT && Multiline && TextLength == 0 && !Focused && !string.IsNullOrEmpty(Cue))
                {
                    using (Graphics g = CreateGraphics())
                        TextRenderer.DrawText(g, Cue, Font, new Rectangle(1, 0, Width - 2, Height), Theme.TextDisabled,
                                              TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                }
            }
        }
    }
}
