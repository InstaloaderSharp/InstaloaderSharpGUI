using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    /// <summary>
    /// Dark color palette, typography and drawing helpers shared by every control. Colors are chosen so that
    /// body text reaches WCAG AA contrast (at least 4.5:1) against the surfaces it is drawn on.
    /// </summary>
    internal static class Theme
    {
        // Surfaces (from back to front)
        public static readonly Color Window = Color.FromArgb(24, 24, 27);
        public static readonly Color Sidebar = Color.FromArgb(30, 30, 34);
        public static readonly Color Surface = Color.FromArgb(36, 36, 41);
        public static readonly Color SurfaceHover = Color.FromArgb(44, 44, 50);
        public static readonly Color SurfacePressed = Color.FromArgb(52, 52, 59);
        public static readonly Color Input = Color.FromArgb(29, 29, 33);
        public static readonly Color InputHover = Color.FromArgb(34, 34, 39);
        public static readonly Color Border = Color.FromArgb(56, 56, 64);
        public static readonly Color BorderStrong = Color.FromArgb(78, 78, 88);
        public static readonly Color Divider = Color.FromArgb(46, 46, 53);

        // Text
        public static readonly Color Text = Color.FromArgb(242, 242, 245);
        public static readonly Color TextSecondary = Color.FromArgb(172, 172, 184);
        public static readonly Color TextDisabled = Color.FromArgb(105, 105, 116);

        // Accent - Instagram inspired magenta with a purple gradient partner
        public static readonly Color Accent = Color.FromArgb(214, 41, 118);
        public static readonly Color AccentHover = Color.FromArgb(232, 62, 138);
        public static readonly Color AccentPressed = Color.FromArgb(186, 30, 100);
        public static readonly Color AccentAlt = Color.FromArgb(129, 52, 175);
        public static readonly Color AccentText = Color.FromArgb(255, 128, 182);
        public static readonly Color AccentSubtle = Color.FromArgb(58, 30, 47);

        // Status
        public static readonly Color Success = Color.FromArgb(63, 185, 80);
        public static readonly Color Warning = Color.FromArgb(227, 179, 65);
        public static readonly Color Error = Color.FromArgb(248, 97, 89);
        public static readonly Color Info = Color.FromArgb(88, 166, 255);

        public static readonly Font Body;
        public static readonly Font BodyStrong;
        public static readonly Font Small;
        public static readonly Font SmallStrong;
        public static readonly Font Section;
        public static readonly Font Title;
        public static readonly Font Display;
        public static readonly Font Mono;
        public static readonly Font Icon;
        public static readonly Font IconSmall;
        public static readonly Font IconLarge;
        public static readonly float ScaleFactor;

        static Theme()
        {
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
                ScaleFactor = g.DpiX / 96f;

            Body = new Font("Segoe UI", 9.75f);
            BodyStrong = new Font("Segoe UI Semibold", 9.75f);
            Small = new Font("Segoe UI", 8.25f);
            SmallStrong = new Font("Segoe UI Semibold", 8.25f);
            Section = new Font("Segoe UI Semibold", 11f);
            Title = new Font("Segoe UI Semibold", 18f);
            Display = new Font("Segoe UI Semibold", 13f);
            Mono = new Font(FontExists("Cascadia Mono") ? "Cascadia Mono" : "Consolas", 9.25f);
            string iconFamily = FontExists("Segoe Fluent Icons") ? "Segoe Fluent Icons" : "Segoe MDL2 Assets";
            Icon = new Font(iconFamily, 11.25f);
            IconSmall = new Font(iconFamily, 8.25f);
            IconLarge = new Font(iconFamily, 18f);
        }

        private static bool FontExists(string name)
        {
            using (var fonts = new InstalledFontCollection())
            {
                foreach (FontFamily f in fonts.Families)
                    if (string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                        return true;
            }
            return false;
        }

        /// <summary>Scales a 96-DPI pixel value to the current display.</summary>
        public static int S(int px)
        {
            return (int)Math.Round(px * ScaleFactor);
        }

        public static float S(float px)
        {
            return px * ScaleFactor;
        }

        public static GraphicsPath RoundRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0.5f)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRound(Graphics g, Color color, RectangleF r, float radius)
        {
            using (var path = RoundRect(r, radius))
            using (var b = new SolidBrush(color))
                g.FillPath(b, path);
        }

        public static void DrawRound(Graphics g, Color color, RectangleF r, float radius, float width = 1f)
        {
            using (var path = RoundRect(r, radius))
            using (var p = new Pen(color, width))
                g.DrawPath(p, path);
        }

        public static void HighQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        public static Color Blend(Color a, Color b, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        public static void DrawText(Graphics g, string text, Font font, Rectangle bounds, Color color,
                                    TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                                                            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix)
        {
            TextRenderer.DrawText(g, text, font, bounds, color, flags);
        }

        public static void DrawGlyph(Graphics g, string glyph, Font font, Rectangle bounds, Color color)
        {
            TextRenderer.DrawText(g, glyph, font, bounds, color,
                                  TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                                  TextFormatFlags.NoPrefix);
        }

        /// <summary>The first opaque background color found walking up from <paramref name="c"/>.</summary>
        public static Color BackOf(Control c)
        {
            while (c != null)
            {
                if (c.BackColor.A == 255)
                    return c.BackColor;
                c = c.Parent;
            }
            return Window;
        }

        /// <summary>Applies the dark palette to a tree of standard controls that do not paint themselves.</summary>
        public static void Apply(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is ListView || c is TreeView || c is ListBox || c is RichTextBox)
                {
                    c.BackColor = c.BackColor == SystemColors.Window ? Input : c.BackColor;
                    c.ForeColor = Text;
                    Native.UseDarkScrollBars(c);
                }
                Apply(c);
            }
        }
    }

    /// <summary>Segoe MDL2 / Fluent icon code points.</summary>
    internal static class Glyphs
    {
        public const string Download = "";
        public const string Contact = "";
        public const string Search = "";
        public const string Settings = "";
        public const string Activity = "";
        public const string Play = "";
        public const string Stop = "";
        public const string Folder = "";
        public const string FolderOpen = "";
        public const string Add = "";
        public const string Delete = "";
        public const string Clear = "";
        public const string Close = "";
        public const string Paste = "";
        public const string Copy = "";
        public const string Save = "";
        public const string Info = "";
        public const string Warning = "";
        public const string Error = "";
        public const string Accept = "";
        public const string ChevronDown = "";
        public const string ChevronUp = "";
        public const string ChevronRight = "";
        public const string Lock = "";
        public const string Unlock = "";
        public const string Globe = "";
        public const string Tag = "";
        public const string MapPin = "";
        public const string Photo = "";
        public const string Video = "";
        public const string People = "";
        public const string Home = "";
        public const string Bookmark = "";
        public const string History = "";
        public const string Document = "";
        public const string Link = "";
        public const string Refresh = "";
        public const string Code = "";
        public const string Filter = "";
        public const string Import = "";
        public const string Export = "";
        public const string SignOut = "";
        public const string Key = "";
        public const string Shield = "";
        public const string Contact2 = "";
        public const string Star = "";
        public const string More = "";
        public const string OpenInNew = "";
        public const string Help = "";
    }
}
