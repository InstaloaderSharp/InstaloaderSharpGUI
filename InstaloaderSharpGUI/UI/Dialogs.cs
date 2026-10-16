using System;
using System.Drawing;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    /// <summary>Form base class: dark background, dark title bar, application icon.</summary>
    public class ThemedForm : Form
    {
        private static Icon _appIcon;

        public ThemedForm()
        {
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Theme.Window;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            try
            {
                if (_appIcon == null)
                    _appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                Icon = _appIcon;
            }
            catch (Exception)
            {
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Native.UseDarkTitleBar(Handle);
        }
    }

    public enum DialogIcon
    {
        None,
        Info,
        Warning,
        Error,
        Question,
        Success
    }

    /// <summary>Dark replacement for MessageBox, plus a text prompt dialog.</summary>
    public class MessageDialog : ThemedForm
    {
        private readonly TextBlock _message;
        private readonly HStack _buttons;
        private readonly DarkTextBox _input;
        private readonly string _glyph;
        private readonly Color _glyphColor;
        private readonly string _heading;

        private MessageDialog(string title, string heading, string message, DialogIcon icon, bool withInput, bool password)
        {
            Text = title;
            _heading = heading;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;

            switch (icon)
            {
                case DialogIcon.Info: _glyph = Glyphs.Info; _glyphColor = Theme.Info; break;
                case DialogIcon.Warning: _glyph = Glyphs.Warning; _glyphColor = Theme.Warning; break;
                case DialogIcon.Error: _glyph = Glyphs.Error; _glyphColor = Theme.Error; break;
                case DialogIcon.Question: _glyph = Glyphs.Help; _glyphColor = Theme.AccentText; break;
                case DialogIcon.Success: _glyph = Glyphs.Accept; _glyphColor = Theme.Success; break;
            }

            _message = new TextBlock(message, Theme.TextSecondary) { BackColor = Theme.Window };
            Controls.Add(_message);
            if (withInput)
            {
                _input = new DarkTextBox { UseSystemPasswordChar = password };
                _input.InnerTextBox.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter && AcceptButton != null)
                    {
                        e.SuppressKeyPress = true;
                        AcceptButton.PerformClick();
                    }
                };
                Controls.Add(_input);
            }
            _buttons = new HStack { BackColor = Theme.Sidebar };
            Controls.Add(_buttons);
        }

        private void AddButton(string text, DialogResult result, ButtonKind kind, bool accept, bool cancel)
        {
            var b = new FlatButton(text, kind) { DialogResult = result };
            b.Width = Math.Max(b.Width, Theme.S(96));
            _buttons.Add(b, true);
            if (accept)
                AcceptButton = b;
            if (cancel)
                CancelButton = b;
        }

        private void DoLayout()
        {
            int w = Theme.S(460);
            int pad = Theme.S(24);
            int glyphW = string.IsNullOrEmpty(_glyph) ? 0 : Theme.S(44);
            int textX = pad + glyphW;
            int textW = w - textX - pad;
            int headingH = string.IsNullOrEmpty(_heading) ? 0 : TextRenderer.MeasureText(_heading, Theme.Display, new Size(textW, 0), TextFormatFlags.WordBreak).Height + Theme.S(8);
            int y = pad + headingH;
            int mh = _message.PreferredHeight(textW);
            _message.SetBounds(textX, y, textW, mh);
            y += mh + Theme.S(16);
            if (_input != null)
            {
                _input.SetBounds(textX, y, textW, Theme.S(32));
                y += Theme.S(32) + Theme.S(16);
            }
            int footerTop = y + Theme.S(4);
            _buttons.SetBounds(pad, footerTop + Theme.S(16), w - pad * 2, Theme.S(32));
            ClientSize = new Size(w, footerTop + Theme.S(16) + Theme.S(32) + Theme.S(18));
            Tag = footerTop;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            int pad = Theme.S(24);
            if (!string.IsNullOrEmpty(_glyph))
                Theme.DrawGlyph(g, _glyph, Theme.IconLarge, new Rectangle(pad - Theme.S(4), pad - Theme.S(4), Theme.S(36), Theme.S(36)), _glyphColor);
            if (!string.IsNullOrEmpty(_heading))
            {
                int textX = _message.Left;
                TextRenderer.DrawText(g, _heading, Theme.Display, new Rectangle(textX, pad - Theme.S(2), _message.Width, _message.Top - pad),
                                      Theme.Text, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            }
            int footerTop = Tag is int ? (int)Tag : ClientSize.Height - Theme.S(66);
            using (var b = new SolidBrush(Theme.Sidebar))
                g.FillRectangle(b, 0, footerTop, ClientSize.Width, ClientSize.Height - footerTop);
            using (var p = new Pen(Theme.Divider))
                g.DrawLine(p, 0, footerTop, ClientSize.Width, footerTop);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_input != null)
                _input.Focus();
        }

        private static Form OwnerOf(IWin32Window owner)
        {
            var c = owner as Control;
            return c != null ? c.FindForm() : null;
        }

        public static DialogResult Show(IWin32Window owner, string message, string heading = null, DialogIcon icon = DialogIcon.Info,
                                        MessageBoxButtons buttons = MessageBoxButtons.OK, string title = "InstaloaderSharp GUI",
                                        string yesText = null, string noText = null)
        {
            using (var d = new MessageDialog(title, heading, message, icon, false, false))
            {
                switch (buttons)
                {
                    case MessageBoxButtons.OKCancel:
                        d.AddButton("Cancel", DialogResult.Cancel, ButtonKind.Secondary, false, true);
                        d.AddButton(yesText ?? "OK", DialogResult.OK, ButtonKind.Primary, true, false);
                        break;
                    case MessageBoxButtons.YesNo:
                        d.AddButton(noText ?? "No", DialogResult.No, ButtonKind.Secondary, false, true);
                        d.AddButton(yesText ?? "Yes", DialogResult.Yes, ButtonKind.Primary, true, false);
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        d.AddButton("Cancel", DialogResult.Cancel, ButtonKind.Secondary, false, true);
                        d.AddButton(noText ?? "No", DialogResult.No, ButtonKind.Secondary, false, false);
                        d.AddButton(yesText ?? "Yes", DialogResult.Yes, ButtonKind.Primary, true, false);
                        break;
                    default:
                        d.AddButton("OK", DialogResult.OK, ButtonKind.Primary, true, true);
                        break;
                }
                d.DoLayout();
                Form f = OwnerOf(owner);
                if (f == null)
                    d.StartPosition = FormStartPosition.CenterScreen;
                return d.ShowDialog(f);
            }
        }

        /// <summary>Asks for a line of text. Returns null when cancelled.</summary>
        public static string Prompt(IWin32Window owner, string heading, string message, bool password = false,
                                    string initial = "", string okText = "OK", string title = "InstaloaderSharp GUI")
        {
            using (var d = new MessageDialog(title, heading, message, password ? DialogIcon.None : DialogIcon.None, true, password))
            {
                d._input.Text = initial ?? "";
                d.AddButton("Cancel", DialogResult.Cancel, ButtonKind.Secondary, false, true);
                d.AddButton(okText, DialogResult.OK, ButtonKind.Primary, true, false);
                d.DoLayout();
                Form f = OwnerOf(owner);
                if (f == null)
                    d.StartPosition = FormStartPosition.CenterScreen;
                return d.ShowDialog(f) == DialogResult.OK ? d._input.Text : null;
            }
        }
    }
}
