using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using InstaloaderSharp;

namespace InstaloaderSharpGUI.Core
{
    public enum LoginMode
    {
        Anonymous,
        Login,
        BrowserCookies
    }

    /// <summary>
    /// Every option that InstaloaderSharp's command line offers, plus GUI-only settings. Public fields are persisted
    /// automatically by <see cref="Save"/>/<see cref="Load"/> (except those marked <see cref="TransientAttribute"/>).
    /// </summary>
    public sealed class JobOptions
    {
        // ---- Targets
        public List<string> Targets = new List<string>();

        // ---- What to download of each profile
        public bool ProfilePic = true;
        public bool Posts = true;
        public bool Stories;
        public bool Highlights;
        public bool Tagged;
        public bool Reels;
        public bool Igtv;

        // ---- What to download of each post
        public bool Pictures = true;
        public bool Videos = true;
        public bool VideoThumbnails = true;
        public bool Geotags;
        public bool Comments;
        public bool Captions = true;
        public bool MetadataJson = true;
        public bool CompressJson = true;
        public string Slide = "";
        public string PostMetadataTxt = "";
        public string StoryitemMetadataTxt = "";

        // ---- Which posts to download
        public bool FastUpdate;
        public bool UseLatestStamps;
        public string LatestStampsFile = "";
        public string PostFilter = "";
        public string StoryitemFilter = "";
        public string Count = "";

        // ---- Login
        public LoginMode Mode = LoginMode.Anonymous;
        public string Username = "";
        public string SessionFile = "";
        public string Browser = "firefox";
        public string CookieFile = "";

        // ---- How to download
        public string DirnamePattern = "";
        public string FilenamePattern = "";
        public string TitlePattern = "";
        public bool Resume = true;
        public string ResumePrefix = "";
        public bool UseAgedResumeFiles;
        public bool SanitizePaths;
        public string UserAgent = "";
        public bool Sleep = true;
        public string MaxConnectionAttempts = "3";
        public string RequestTimeout = "300";
        public string AbortOn = "";
        public bool Iphone = true;
        public bool Quiet;

        // ---- GUI
        public string OutputDir = "";
        public bool PreventSleep = true;
        public bool NotifyWhenDone = true;

        public static readonly string[] Browsers =
            { "brave", "chrome", "chromium", "edge", "firefox", "librewolf", "opera", "opera_gx", "safari", "vivaldi" };

        public static string BrowserDisplayName(string browser)
        {
            switch (browser)
            {
                case "opera_gx": return "Opera GX";
                case "librewolf": return "LibreWolf";
                default: return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(browser);
            }
        }

        public JobOptions Clone()
        {
            var c = (JobOptions)MemberwiseClone();
            c.Targets = new List<string>(Targets);
            return c;
        }

        public bool LoggedInMode { get { return Mode != LoginMode.Anonymous; } }

        /// <summary>Path of the latest-stamps file actually used (null if disabled).</summary>
        public string EffectiveStampsFile
        {
            get
            {
                if (!UseLatestStamps)
                    return null;
                return string.IsNullOrWhiteSpace(LatestStampsFile) ? Instaloader.GetDefaultStampsFilename() : LatestStampsFile.Trim();
            }
        }

        public string EffectiveSessionFile
        {
            get { return string.IsNullOrWhiteSpace(SessionFile) ? null : SessionFile.Trim(); }
        }

        internal static string NullIfBlank(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        internal static string Lines(string s)
        {
            // Each line of the multi-line template boxes corresponds to one --post-metadata-txt occurrence.
            if (string.IsNullOrEmpty(s))
                return null;
            string joined = string.Join("\n", s.Replace("\r\n", "\n").Split('\n'));
            return joined.Length == 0 ? null : joined;
        }

        // ------------------------------------------------------------------------------------------ validation

        /// <summary>Problems that prevent starting (errors) or that the user should know about (warnings).</summary>
        public sealed class ValidationResult
        {
            public readonly List<string> Errors = new List<string>();
            public readonly List<string> Warnings = new List<string>();
            public bool Ok { get { return Errors.Count == 0; } }
        }

        /// <summary>Validates the options the same way InstaloaderSharp's command line does.</summary>
        public ValidationResult Validate(bool requireTargets = true)
        {
            var r = new ValidationResult();
            if (requireTargets && Targets.Count == 0)
                r.Errors.Add("Add at least one target (a profile, #hashtag, post, location...) to download.");
            if (Targets.Contains(":feed-all") || Targets.Contains(":feed-liked"))
                r.Errors.Add(":feed-all and :feed-liked were removed. Use :feed as target and eventually the post filter viewer_has_liked.");
            if (string.IsNullOrWhiteSpace(OutputDir))
                r.Errors.Add("Choose a folder to save the downloads to.");
            if (Mode == LoginMode.Login && string.IsNullOrWhiteSpace(Username))
                r.Errors.Add("Enter your Instagram username on the Account page, or switch to anonymous downloading.");
            if (Mode == LoginMode.BrowserCookies && string.IsNullOrWhiteSpace(Browser))
                r.Errors.Add("Choose the browser to import cookies from.");
            if (!Captions && (NullIfBlank(PostMetadataTxt) != null || NullIfBlank(StoryitemMetadataTxt) != null))
                r.Errors.Add("Captions (txt files) are turned off but a post or story item text template is set; that contradicts.");
            if (!Pictures && FastUpdate)
                r.Errors.Add("\"Pictures\" off and \"Fast update\" cannot be used together.");
            if (!LoggedInMode && Stories)
                r.Warnings.Add("Login is required to download stories - stories will be skipped.");
            if (!LoggedInMode && (Highlights || Comments))
                r.Warnings.Add("Highlights and comments require login and will probably fail when downloading anonymously.");
            if (!LoggedInMode && Targets.Any(t => t == ":feed" || t == ":stories" || t == ":saved" || t.StartsWith("@") || t.StartsWith("%")))
                r.Warnings.Add("Feed, stories, saved posts, followees and locations require login.");
            string err;
            if (ParseCount(out err) == null && err != null) r.Errors.Add(err);
            if (ParseMaxConnectionAttempts(out err) < 0) r.Errors.Add(err);
            if (double.IsNaN(ParseRequestTimeout(out err))) r.Errors.Add(err);
            if (ParseAbortOn(out err) == null && err != null) r.Errors.Add(err);
            err = ValidateSlide(Slide);
            if (err != null) r.Errors.Add("Images of multi-image posts: " + err);
            err = ValidateFilter<Post>(PostFilter);
            if (err != null) r.Errors.Add("Post filter: " + err);
            err = ValidateFilter<StoryItem>(StoryitemFilter);
            if (err != null) r.Errors.Add("Story item filter: " + err);
            foreach (string t in Targets)
            {
                if ((t.EndsWith(".json") || t.EndsWith(".json.xz")) && !File.Exists(t))
                    r.Errors.Add("JSON target not found: " + t);
            }
            return r;
        }

        /// <summary>
        /// Checks a --slide value the way Instaloader parses it ("2", "last", "2-4", "3-last"). Returns an error
        /// message or null. (For text that is not a number, the library itself fails with a Python ValueError.)
        /// </summary>
        public static string ValidateSlide(string slide)
        {
            if (string.IsNullOrWhiteSpace(slide))
                return null;
            string[] parts = slide.Trim().Split('-');
            int first, second;
            Func<string, int?> number = s =>
            {
                int n;
                return int.TryParse(s.Replace("_", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : (int?)null;
            };
            if (parts.Length == 1)
            {
                if (parts[0] == "last")
                    return null;
                if (number(parts[0]) == null)
                    return "Invalid data for --slide parameter.";
                return number(parts[0]) > 0 ? null : "--slide parameter must be greater than 0.";
            }
            if (parts.Length == 2 && number(parts[0]) != null)
            {
                first = number(parts[0]).Value;
                if (parts[1] == "last")
                    return null;
                if (number(parts[1]) != null)
                {
                    second = number(parts[1]).Value;
                    if (0 < first && first < second)
                        return null;
                }
            }
            return "Invalid data for --slide parameter.";
        }

        /// <summary>Compiles a filter expression with InstaloaderSharp's own parser. Returns an error message or null.</summary>
        public static string ValidateFilter<T>(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return null;
            try
            {
                InstaloaderSharp.Program.FilterstrToFilterfunc<T>(filter.Trim());
                return null;
            }
            catch (InstaloaderException e)
            {
                return e.Message;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public long? ParseCount(out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(Count))
                return null;
            long n;
            if (long.TryParse(Count.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) && n >= 0)
                return n;
            error = "Maximum count must be a whole number.";
            return null;
        }

        public int ParseMaxConnectionAttempts(out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(MaxConnectionAttempts))
                return 3;
            int n;
            if (int.TryParse(MaxConnectionAttempts.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) && n >= 0)
                return n;
            error = "Maximum connection attempts must be a whole number (0 = retry forever).";
            return -1;
        }

        public double ParseRequestTimeout(out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(RequestTimeout))
                return 300;
            double d;
            if (double.TryParse(RequestTimeout.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out d) && d > 0)
                return d;
            error = "Request timeout must be a positive number of seconds.";
            return double.NaN;
        }

        public List<int> ParseAbortOn(out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(AbortOn))
                return null;
            try
            {
                return InstaloaderSharp.Program.HttpStatusCodeList(AbortOn.Replace(" ", ""));
            }
            catch (Exception e)
            {
                error = "Abort on: " + (e is FormatException || e is InstaloaderSharp.Python.PyValueError
                    ? "use a comma-separated list of HTTP status codes, e.g. 302,400,429" : e.Message);
                return null;
            }
        }

        // ------------------------------------------------------------------------------------------ CLI mapping

        /// <summary>The equivalent InstaloaderSharp command line arguments.</summary>
        public List<string> ToArgs(bool includeTargets = true)
        {
            var a = new List<string>();
            Action<bool, string> flag = (cond, name) => { if (cond) a.Add(name); };
            Action<string, string> opt = (name, value) => { if (!string.IsNullOrWhiteSpace(value)) a.Add(name + "=" + value.Trim()); };

            if (!Posts && ProfilePic && !Stories && !Highlights && !Tagged && !Reels && !Igtv)
                a.Add("--profile-pic-only");
            else
            {
                flag(!Posts, "--no-posts");
                flag(!ProfilePic, "--no-profile-pic");
            }
            flag(Stories, "--stories");
            flag(Highlights, "--highlights");
            flag(Tagged, "--tagged");
            flag(Reels, "--reels");
            flag(Igtv, "--igtv");

            opt("--slide", Slide);
            flag(!Pictures, "--no-pictures");
            flag(!Videos, "--no-videos");
            flag(!VideoThumbnails, "--no-video-thumbnails");
            flag(Geotags, "--geotags");
            flag(Comments, "--comments");
            flag(!Captions, "--no-captions");
            if (Captions)
            {
                foreach (string line in (PostMetadataTxt ?? "").Replace("\r\n", "\n").Split('\n').Where(l => l.Length > 0))
                    a.Add("--post-metadata-txt=" + line);
                foreach (string line in (StoryitemMetadataTxt ?? "").Replace("\r\n", "\n").Split('\n').Where(l => l.Length > 0))
                    a.Add("--storyitem-metadata-txt=" + line);
            }
            flag(!MetadataJson, "--no-metadata-json");
            flag(!CompressJson, "--no-compress-json");

            flag(FastUpdate, "--fast-update");
            if (UseLatestStamps)
                a.Add(string.IsNullOrWhiteSpace(LatestStampsFile) ? "--latest-stamps" : "--latest-stamps=" + LatestStampsFile.Trim());
            opt("--post-filter", PostFilter);
            opt("--storyitem-filter", StoryitemFilter);
            opt("--count", Count);

            if (Mode == LoginMode.Login)
            {
                opt("--login", Username);
                opt("--sessionfile", SessionFile);
            }
            else if (Mode == LoginMode.BrowserCookies)
            {
                opt("--load-cookies", Browser);
                opt("--cookiefile", CookieFile);
            }

            opt("--dirname-pattern", DirnamePattern);
            opt("--filename-pattern", FilenamePattern);
            opt("--title-pattern", TitlePattern);
            if (!Resume)
                a.Add("--no-resume");
            else
                opt("--resume-prefix", ResumePrefix);
            flag(UseAgedResumeFiles, "--use-aged-resume-files");
            flag(SanitizePaths, "--sanitize-paths");
            opt("--user-agent", UserAgent);
            flag(!Sleep, "--no-sleep");
            if (!string.IsNullOrWhiteSpace(MaxConnectionAttempts) && MaxConnectionAttempts.Trim() != "3")
                opt("--max-connection-attempts", MaxConnectionAttempts);
            if (!string.IsNullOrWhiteSpace(RequestTimeout) && RequestTimeout.Trim() != "300")
                opt("--request-timeout", RequestTimeout);
            opt("--abort-on", AbortOn == null ? null : AbortOn.Replace(" ", ""));
            flag(!Iphone, "--no-iphone");
            flag(Quiet, "--quiet");

            if (includeTargets && Targets.Count > 0)
            {
                // "--latest-stamps" has an optional value: a target right after it would be read as the stamps file.
                bool dashDash = a.Count > 0 && a[a.Count - 1] == "--latest-stamps";
                if (dashDash)
                    a.Add("--");
                foreach (string t in Targets)
                {
                    if (t.StartsWith("-") && !dashDash)
                    {
                        a.Add("--");
                        dashDash = true;
                    }
                    a.Add(t);
                }
            }
            return a;
        }

        public static string Quote(string arg)
        {
            if (arg.Length > 0 && arg.IndexOfAny(new[] { ' ', '\t', '"', '#', '%', '&', '|', '<', '>', '^', '{', '}' }) < 0)
                return arg;
            return "\"" + arg.Replace("\\\"", "\\\\\"").Replace("\"", "\\\"") + "\"";
        }

        public string ToCommandLine()
        {
            var sb = new StringBuilder("InstaloaderSharp");
            foreach (string arg in ToArgs())
                sb.Append(' ').Append(Quote(arg));
            return sb.ToString();
        }

        /// <summary>
        /// Applies an argument dictionary produced by InstaloaderSharp's own argument parser
        /// (see <see cref="CommandLineImport"/>), mapping it exactly like the console program does.
        /// </summary>
        public void ApplyParsedArgs(Dictionary<string, object> args)
        {
            Func<string, bool> flag = name => { object v; return args.TryGetValue(name, out v) && v is bool && (bool)v; };
            Func<string, string> str = name => { object v; return args.TryGetValue(name, out v) && v != null ? Convert.ToString(v, CultureInfo.InvariantCulture) : null; };
            Func<string, List<string>> list = name =>
            {
                object v;
                if (!args.TryGetValue(name, out v) || v == null) return null;
                var l = v as System.Collections.IEnumerable;
                return l == null || v is string ? null : l.Cast<object>().Select(o => Convert.ToString(o, CultureInfo.InvariantCulture)).ToList();
            };

            List<string> targets = list("profile");
            if (targets != null && targets.Count > 0)
                Targets = targets.Select(t => (t.EndsWith(".json") || t.EndsWith(".json.xz")) ? Path.GetFullPath(t) : t).ToList();

            ProfilePic = !flag("no_profile_pic") || flag("profile_pic_only");
            Posts = !(flag("no_posts") || flag("stories_only") || flag("profile_pic_only"));
            Stories = flag("stories") || flag("stories_only");
            Highlights = flag("highlights");
            Tagged = flag("tagged");
            Reels = flag("reels");
            Igtv = flag("igtv");

            Slide = str("slide") ?? "";
            Pictures = !flag("no_pictures");
            Videos = !flag("no_videos");
            VideoThumbnails = !flag("no_video_thumbnails");
            Geotags = flag("geotags");
            Comments = flag("comments");
            Captions = !flag("no_captions");
            List<string> ptxt = list("post_metadata_txt");
            PostMetadataTxt = ptxt != null ? string.Join(Environment.NewLine, ptxt) : "";
            List<string> stxt = list("storyitem_metadata_txt");
            StoryitemMetadataTxt = stxt != null ? string.Join(Environment.NewLine, stxt) : "";
            MetadataJson = !flag("no_metadata_json");
            CompressJson = !flag("no_compress_json");

            FastUpdate = flag("fast_update");
            string stamps = str("latest_stamps");
            UseLatestStamps = stamps != null;
            LatestStampsFile = stamps != null && stamps != Instaloader.GetDefaultStampsFilename() ? stamps : "";
            PostFilter = str("post_filter") ?? "";
            StoryitemFilter = str("storyitem_filter") ?? "";
            Count = str("count") ?? "";

            if (str("login") != null)
            {
                Mode = LoginMode.Login;
                Username = str("login");
            }
            else if (str("load_cookies") != null)
            {
                Mode = LoginMode.BrowserCookies;
                Browser = str("load_cookies").ToLowerInvariant();
            }
            else
            {
                Mode = LoginMode.Anonymous;
            }
            SessionFile = str("sessionfile") ?? "";
            CookieFile = str("cookiefile") ?? "";

            DirnamePattern = str("dirname_pattern") ?? "";
            FilenamePattern = str("filename_pattern") ?? "";
            TitlePattern = str("title_pattern") ?? "";
            Resume = !flag("no_resume");
            ResumePrefix = str("resume_prefix") ?? "";
            UseAgedResumeFiles = flag("use_aged_resume_files");
            SanitizePaths = flag("sanitize_paths");
            UserAgent = str("user_agent") ?? "";
            Sleep = !flag("no_sleep");
            MaxConnectionAttempts = str("max_connection_attempts") ?? "3";
            RequestTimeout = str("request_timeout") ?? "300";
            List<string> abort = list("abort_on");
            AbortOn = abort != null ? string.Join(",", abort) : "";
            Iphone = !flag("no_iphone");
            Quiet = flag("quiet");
        }

        // ------------------------------------------------------------------------------------------ persistence

        public void Save(IDictionary<string, string> store)
        {
            foreach (FieldInfo f in typeof(JobOptions).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object v = f.GetValue(this);
                string s;
                if (v == null) continue;
                if (v is List<string>) s = string.Join("\n", (List<string>)v);
                else if (v is bool) s = (bool)v ? "1" : "0";
                else s = Convert.ToString(v, CultureInfo.InvariantCulture);
                store[f.Name] = s;
            }
        }

        public void Load(IDictionary<string, string> store)
        {
            foreach (FieldInfo f in typeof(JobOptions).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string s;
                if (!store.TryGetValue(f.Name, out s))
                    continue;
                try
                {
                    if (f.FieldType == typeof(List<string>))
                        f.SetValue(this, s.Split('\n').Where(x => x.Length > 0).ToList());
                    else if (f.FieldType == typeof(bool))
                        f.SetValue(this, s == "1");
                    else if (f.FieldType == typeof(string))
                        f.SetValue(this, s);
                    else if (f.FieldType.IsEnum)
                        f.SetValue(this, Enum.Parse(f.FieldType, s));
                }
                catch (Exception)
                {
                    // ignore malformed values
                }
            }
        }
    }
}
