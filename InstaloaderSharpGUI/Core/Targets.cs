using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Core
{
    public enum TargetType
    {
        Auto,
        Profile,
        Hashtag,
        Location,
        Post,
        Followees,
        Json
    }

    /// <summary>Converts user input (names, #tags, Instagram links...) to InstaloaderSharp target strings and back.</summary>
    internal static class Targets
    {
        public static readonly string[] TypeNames =
            { "Auto-detect", "Profile", "Hashtag", "Location ID", "Single post", "Followees of", "JSON file" };

        private static readonly Regex UsernameRx = new Regex(@"^[A-Za-z0-9._]+$");
        private static readonly Regex HashtagRx = new Regex(@"^[\p{L}\p{N}_]+$");
        private static readonly Regex ShortcodeRx = new Regex(@"^[A-Za-z0-9_-]+$");
        private static readonly Regex LocationRx = new Regex(@"^[0-9]+$");

        private static readonly Color ProfileColor = Theme.Info;
        private static readonly Color HashtagColor = Theme.AccentText;
        private static readonly Color LocationColor = Theme.Success;
        private static readonly Color PostColor = Theme.Warning;
        private static readonly Color FolloweesColor = Color.FromArgb(190, 150, 255);
        private static readonly Color SpecialColor = Color.FromArgb(86, 208, 200);
        private static readonly Color FileColor = Theme.TextSecondary;

        /// <summary>
        /// Converts one piece of user input into a target. Returns null and an error message if it cannot be understood.
        /// Note: in auto-detect mode "@name" means the profile "name" (as on Instagram); the console's followee
        /// syntax is available through <see cref="TargetType.Followees"/>.
        /// </summary>
        public static string Parse(string input, TargetType type, out string error)
        {
            error = null;
            string s = (input ?? "").Trim().Trim('"', '\'', '<', '>').Trim();
            if (s.Length == 0)
            {
                error = "Nothing entered.";
                return null;
            }

            // Instagram links
            if (s.IndexOf("instagram.com", StringComparison.OrdinalIgnoreCase) >= 0 || s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                string t = FromUrl(s, out error);
                if (t == null)
                    return null;
                if (type == TargetType.Followees && UsernameRx.IsMatch(t))
                    return "@" + t;
                return t;
            }

            switch (type)
            {
                case TargetType.Profile:
                    s = s.TrimStart('@');
                    if (UsernameRx.IsMatch(s)) return s;
                    error = "A username may only contain letters, numbers, periods and underscores.";
                    return null;
                case TargetType.Hashtag:
                    s = s.TrimStart('#');
                    if (HashtagRx.IsMatch(s)) return "#" + s;
                    error = "A hashtag may only contain letters, numbers and underscores.";
                    return null;
                case TargetType.Location:
                    s = s.TrimStart('%');
                    if (LocationRx.IsMatch(s)) return "%" + s;
                    error = "A location ID is a number, e.g. 213385402 (see the location's Instagram link).";
                    return null;
                case TargetType.Post:
                    s = s.TrimStart('-');
                    if (ShortcodeRx.IsMatch(s)) return "-" + s;
                    error = "Enter the post's link or its shortcode (the part after /p/ in the link).";
                    return null;
                case TargetType.Followees:
                    s = s.TrimStart('@');
                    if (UsernameRx.IsMatch(s)) return "@" + s;
                    error = "A username may only contain letters, numbers, periods and underscores.";
                    return null;
                case TargetType.Json:
                    return ParseJson(s, out error);
            }

            // Auto-detect
            if (s.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".json.xz", StringComparison.OrdinalIgnoreCase))
                return ParseJson(s, out error);
            switch (s[0])
            {
                case '#':
                    return Parse(s, TargetType.Hashtag, out error);
                case '%':
                    return Parse(s, TargetType.Location, out error);
                case '-':
                    return Parse(s, TargetType.Post, out error);
                case '@':
                    return Parse(s, TargetType.Profile, out error);
                case ':':
                    string special = s.ToLowerInvariant();
                    if (special == ":feed" || special == ":stories" || special == ":saved")
                        return special;
                    error = "Special targets are :feed, :stories and :saved.";
                    return null;
            }
            if (UsernameRx.IsMatch(s))
                return s;
            error = "Not recognised. Enter a username, #hashtag, Instagram link, %location_id or -shortcode.";
            return null;
        }

        private static string ParseJson(string s, out string error)
        {
            error = null;
            string full;
            try
            {
                full = Path.GetFullPath(s);
            }
            catch (Exception)
            {
                error = "Invalid file path.";
                return null;
            }
            if (!File.Exists(full))
            {
                error = "File not found: " + full;
                return null;
            }
            return full;
        }

        private static string FromUrl(string url, out string error)
        {
            error = null;
            string u = url;
            int scheme = u.IndexOf("://", StringComparison.Ordinal);
            if (scheme >= 0)
                u = u.Substring(scheme + 3);
            int q = u.IndexOfAny(new[] { '?', '#' });
            if (q >= 0)
                u = u.Substring(0, q);
            string[] parts = u.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            // parts[0] is the host
            if (parts.Length < 2)
            {
                error = "This link does not point to a profile, post, hashtag or location.";
                return null;
            }
            string p1 = parts[1].ToLowerInvariant();
            if ((p1 == "p" || p1 == "reel" || p1 == "reels" || p1 == "tv") && parts.Length >= 3)
                return "-" + parts[2];
            if (p1 == "explore" && parts.Length >= 4 && parts[2].ToLowerInvariant() == "tags")
                return "#" + Uri.UnescapeDataString(parts[3]);
            if (p1 == "explore" && parts.Length >= 4 && parts[2].ToLowerInvariant() == "locations")
                return "%" + parts[3];
            if (p1 == "stories" && parts.Length >= 3)
                return parts[2];
            if (parts.Length >= 3 && (parts[2].ToLowerInvariant() == "p" || parts[2].ToLowerInvariant() == "reel") && parts.Length >= 4)
                return "-" + parts[3];
            if (UsernameRx.IsMatch(parts[1]) && p1 != "explore" && p1 != "accounts" && p1 != "direct")
                return parts[1];
            error = "This link does not point to a profile, post, hashtag or location.";
            return null;
        }

        /// <summary>Describes a target string for display in the list.</summary>
        public static TargetEntry Describe(string target)
        {
            var e = new TargetEntry { Value = target, Raw = target };
            if (target.EndsWith(".json") || target.EndsWith(".json.xz"))
            {
                e.Kind = "JSON file";
                e.Glyph = Glyphs.Document;
                e.KindColor = FileColor;
                e.Value = Path.GetFileName(target);
                e.Detail = Path.GetDirectoryName(target);
            }
            else if (target.StartsWith("@"))
            {
                e.Kind = "Followees";
                e.Glyph = Glyphs.People;
                e.KindColor = FolloweesColor;
                e.Detail = "every account " + target.Substring(1) + " follows · requires login";
            }
            else if (target.StartsWith("#"))
            {
                e.Kind = "Hashtag";
                e.Glyph = Glyphs.Tag;
                e.KindColor = HashtagColor;
            }
            else if (target.StartsWith("%"))
            {
                e.Kind = "Location";
                e.Glyph = Glyphs.MapPin;
                e.KindColor = LocationColor;
                e.Detail = "requires login";
            }
            else if (target.StartsWith("-"))
            {
                e.Kind = "Post";
                e.Glyph = Glyphs.Photo;
                e.KindColor = PostColor;
                e.Detail = "instagram.com/p/" + target.Substring(1);
            }
            else if (target == ":feed")
            {
                e.Kind = "Feed";
                e.Glyph = Glyphs.Home;
                e.KindColor = SpecialColor;
                e.Value = "Your feed";
                e.Detail = ":feed · requires login";
            }
            else if (target == ":stories")
            {
                e.Kind = "Stories";
                e.Glyph = Glyphs.History;
                e.KindColor = SpecialColor;
                e.Value = "Stories of accounts you follow";
                e.Detail = ":stories · requires login";
            }
            else if (target == ":saved")
            {
                e.Kind = "Saved";
                e.Glyph = Glyphs.Bookmark;
                e.KindColor = SpecialColor;
                e.Value = "Your saved posts";
                e.Detail = ":saved · requires login";
            }
            else
            {
                e.Kind = "Profile";
                e.Glyph = Glyphs.Contact;
                e.KindColor = ProfileColor;
            }
            e.Value = e.Value ?? target;
            return e;
        }

        /// <summary>Raw target string of an entry (entries for special targets show friendly text).</summary>
        public static string RawOf(TargetEntry e)
        {
            return e.Raw ?? e.Value;
        }
    }
}
