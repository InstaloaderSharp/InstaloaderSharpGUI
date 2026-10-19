using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using InstaloaderSharp;
using InstaloaderSharp.Http;

namespace InstaloaderSharpGUI.Core
{
    /// <summary>Thin helpers around the InstaloaderSharp library used by several pages.</summary>
    internal static class Library
    {
        /// <summary>Version of the referenced InstaloaderSharp library.</summary>
        /// <remarks>
        /// Read from the assembly version of the InstaloaderSharp.exe that is actually loaded, so it is correct after
        /// the library is replaced. (Instaloader.Version is a const and would be compiled into the GUI.)
        /// </remarks>
        public static string Version
        {
            get { return FormatVersion(typeof(Instaloader).Assembly.GetName().Version); }
        }

        /// <summary>Version of this GUI.</summary>
        public static string GuiVersion
        {
            get { return FormatVersion(typeof(Library).Assembly.GetName().Version); }
        }

        /// <summary>"1.0.0" for 1.0.0.0; the revision is only shown when it is not 0.</summary>
        private static string FormatVersion(Version v)
        {
            return v.Revision > 0 ? v.ToString(4) : v.ToString(3);
        }

        /// <summary>Folder where InstaloaderSharp keeps session files and the latest-stamps database.</summary>
        public static string ConfigDir
        {
            get { return Path.GetDirectoryName(Path.GetFullPath(Instaloader.GetDefaultSessionFilename("x"))); }
        }

        public static string DefaultSessionFile(string username)
        {
            return Path.GetFullPath(Instaloader.GetDefaultSessionFilename(username.ToLowerInvariant()));
        }

        public sealed class SessionInfo
        {
            public string Username;
            public string Path;
            public DateTime Modified;
        }

        /// <summary>Session files saved by InstaloaderSharp (and by the GUI) in the default location.</summary>
        public static List<SessionInfo> ListSessions()
        {
            var list = new List<SessionInfo>();
            try
            {
                string dir = ConfigDir;
                if (!Directory.Exists(dir))
                    return list;
                foreach (string f in Directory.GetFiles(dir, "session-*"))
                {
                    list.Add(new SessionInfo
                    {
                        Username = Path.GetFileName(f).Substring("session-".Length),
                        Path = f,
                        Modified = File.GetLastWriteTime(f)
                    });
                }
            }
            catch (Exception)
            {
            }
            return list.OrderByDescending(s => s.Modified).ToList();
        }

        /// <summary>Creates an Instaloader configured from the options (the same mapping the console program uses).</summary>
        public static Instaloader CreateLoader(JobOptions o)
        {
            string err;
            string postTxt = o.Captions ? JobOptions.Lines(o.PostMetadataTxt) : "";
            string storyTxt = o.Captions ? JobOptions.Lines(o.StoryitemMetadataTxt) : "";
            string resumePrefix = o.Resume ? (JobOptions.NullIfBlank(o.ResumePrefix) ?? "iterator") : null;
            int attempts = o.ParseMaxConnectionAttempts(out err);
            double timeout = o.ParseRequestTimeout(out err);
            if (attempts < 0) attempts = 3;
            if (double.IsNaN(timeout)) timeout = 300;
            return new Instaloader(
                sleep: o.Sleep,
                quiet: o.Quiet,
                userAgent: JobOptions.NullIfBlank(o.UserAgent),
                dirnamePattern: JobOptions.NullIfBlank(o.DirnamePattern),
                filenamePattern: JobOptions.NullIfBlank(o.FilenamePattern),
                downloadPictures: o.Pictures,
                downloadVideos: o.Videos,
                downloadVideoThumbnails: o.VideoThumbnails,
                downloadGeotags: o.Geotags,
                downloadComments: o.Comments,
                saveMetadata: o.MetadataJson,
                compressJson: o.CompressJson,
                postMetadataTxtPattern: postTxt,
                storyitemMetadataTxtPattern: storyTxt,
                maxConnectionAttempts: attempts,
                requestTimeout: timeout,
                resumePrefix: resumePrefix,
                checkResumeBbd: !o.UseAgedResumeFiles,
                slide: JobOptions.NullIfBlank(o.Slide),
                fatalStatusCodes: o.ParseAbortOn(out err),
                iphoneSupport: o.Iphone,
                titlePattern: JobOptions.NullIfBlank(o.TitlePattern),
                sanitizePaths: o.SanitizePaths);
        }

        /// <summary>
        /// Logs <paramref name="loader"/> in according to the options, exactly like the console's --login /
        /// --load-cookies handling, but asking for passwords and 2FA codes through <paramref name="prompt"/>
        /// (message, isPassword) instead of the console. Returns the logged in username (null if anonymous).
        /// </summary>
        public static string SignIn(Instaloader loader, JobOptions o, string password, Func<string, bool, string> prompt)
        {
            if (o.Mode == LoginMode.BrowserCookies)
            {
                InstaloaderSharp.Program.ImportSession(o.Browser.ToLowerInvariant(), loader, JobOptions.NullIfBlank(o.CookieFile));
                return loader.Context.Username;
            }
            if (o.Mode != LoginMode.Login)
                return null;

            string username = o.Username.Trim().TrimStart('@').ToLowerInvariant();
            string sessionfile = o.EffectiveSessionFile;
            if (!Regex.IsMatch(username, @"^[A-Za-z0-9._]+$"))
                loader.Context.Error(string.Format("Warning: Parameter \"{0}\" for --login is not a valid username.", username));
            try
            {
                loader.LoadSessionFromFile(username, sessionfile);
            }
            catch (FileNotFoundException err)
            {
                if (sessionfile != null)
                    Console.Error.WriteLine(err.Message);
                loader.Context.Log("Session file does not exist yet - Logging in.");
            }
            if (!loader.Context.IsLoggedIn || username != loader.TestLogin())
            {
                string pw = password;
                bool asked = false;
                while (true)
                {
                    if (string.IsNullOrEmpty(pw))
                    {
                        pw = prompt(string.Format(asked ? "That password was not accepted. Enter the Instagram password for {0}:"
                                                        : "Enter the Instagram password for {0}:", username), true);
                        asked = true;
                        if (pw == null)
                            throw new LoginException("Login cancelled.");
                    }
                    try
                    {
                        loader.Login(username, pw);
                        break;
                    }
                    catch (TwoFactorAuthRequiredException)
                    {
                        // https://github.com/instaloader/instaloader/issues/1217
                        loader.Context.Error("Warning: There have been reports of 2FA currently not working. " +
                                             "Consider importing session cookies from your browser.");
                        bool retry = false;
                        while (true)
                        {
                            string code = prompt(retry ? "That code was not accepted. Enter the 2FA verification code:"
                                                       : "Enter the 2FA verification code from your authenticator app or SMS:", false);
                            if (code == null)
                                throw new LoginException("Two-factor authentication cancelled.");
                            try
                            {
                                loader.TwoFactorLogin(code.Trim());
                                break;
                            }
                            catch (BadCredentialsException err)
                            {
                                Console.Error.WriteLine(err.Message);
                                retry = true;
                            }
                        }
                        break;
                    }
                    catch (BadCredentialsException err)
                    {
                        Console.Error.WriteLine(err.Message);
                        pw = null;
                    }
                }
            }
            loader.Context.Log(string.Format("Logged in as {0}.", username));
            return username;
        }

        /// <summary>
        /// Creates a loader for look-ups (profile info, search). Reuses the saved session when signed in, but never
        /// prompts; falls back to anonymous access.
        /// </summary>
        public static Instaloader CreateLookupLoader(JobOptions o)
        {
            JobOptions lookup = o.Clone();
            lookup.Quiet = false;
            Instaloader loader = CreateLoader(lookup);
            try
            {
                if (o.Mode == LoginMode.Login && !string.IsNullOrWhiteSpace(o.Username))
                    loader.LoadSessionFromFile(o.Username.Trim().ToLowerInvariant(), o.EffectiveSessionFile);
                else if (o.Mode == LoginMode.BrowserCookies)
                    InstaloaderSharp.Program.ImportSession(o.Browser.ToLowerInvariant(), loader, JobOptions.NullIfBlank(o.CookieFile));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Warning: Looking up anonymously - could not load your session: " + e.Message);
            }
            return loader;
        }

        /// <summary>Downloads an image (e.g. a profile picture) through the library's HTTP session.</summary>
        public static Image DownloadImage(Instaloader loader, string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            try
            {
                using (Response resp = loader.Context.GetRaw(url))
                {
                    byte[] data = resp.Content;
                    using (var ms = new MemoryStream(data))
                    using (Image img = Image.FromStream(ms))
                        return new Bitmap(img);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ---------------------------------------------------------------------------- command line import

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CommandLineToArgvW(string lpCmdLine, out int pNumArgs);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        /// <summary>Splits a command line using the Windows rules (the same ones InstaloaderSharp.exe receives).</summary>
        public static string[] SplitCommandLine(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return new string[0];
            int argc;
            // CommandLineToArgvW treats the first token specially; prefix a dummy program name.
            IntPtr argv = CommandLineToArgvW("x " + commandLine.Replace("\r", " ").Replace("\n", " "), out argc);
            if (argv == IntPtr.Zero)
                return new string[0];
            try
            {
                var args = new string[argc - 1];
                for (int i = 1; i < argc; i++)
                    args[i - 1] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size));
                return args;
            }
            finally
            {
                LocalFree(argv);
            }
        }

        /// <summary>
        /// Parses an InstaloaderSharp command line (or the contents of a +args.txt file) with the library's own
        /// argument parser, so every option is understood exactly as the console program understands it.
        /// </summary>
        public static Dictionary<string, object> ParseCommandLine(string[] args, out string error)
        {
            error = null;
            var list = args.ToList();
            if (list.Count > 0)
            {
                string first = Path.GetFileNameWithoutExtension(list[0].Trim('"')).ToLowerInvariant();
                if (first == "instaloadersharp" || first == "instaloader")
                    list.RemoveAt(0);
            }
            MethodInfo create = typeof(InstaloaderSharp.Program).GetMethod("CreateParser", BindingFlags.NonPublic | BindingFlags.Static);
            if (create == null)
            {
                error = "This version of InstaloaderSharp does not expose its argument parser.";
                return null;
            }
            Dictionary<string, object> parsed = null;
            Exception failure = null;
            string output = ConsoleRouter.Capture(() =>
            {
                try
                {
                    var parser = (InstaloaderSharp.Cli.ArgParser)create.Invoke(null, null);
                    parsed = parser.Parse(list);
                }
                catch (TargetInvocationException e)
                {
                    failure = e.InnerException;
                }
                catch (Exception e)
                {
                    failure = e;
                }
            });
            if (failure != null || parsed == null)
            {
                string msg = output.Trim();
                if (msg.Length == 0 && failure != null)
                    msg = failure.Message;
                // Keep only the "error:" line of argparse's usage output
                string[] lines = msg.Split('\n');
                string errLine = lines.LastOrDefault(l => l.Contains("error:"));
                error = (errLine ?? msg).Trim();
                if (error.Length == 0)
                    error = "The command line could not be parsed.";
                return null;
            }
            return parsed;
        }

        public static string PythonName(string pascal)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c) && i > 0)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        /// <summary>Attributes usable in filter expressions for the given structure type (Post / StoryItem).</summary>
        public static List<KeyValuePair<string, string>> FilterAttributes(Type type)
        {
            var result = new List<KeyValuePair<string, string>>();
            foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name))
            {
                Type t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                if (p.Name == "Context" || p.Name == "Node")
                    continue;
                if (t == typeof(bool) || t == typeof(string) || t == typeof(int) || t == typeof(long) || t == typeof(double) ||
                    t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(List<string>))
                {
                    string tn = t == typeof(bool) ? "bool" : t == typeof(string) ? "str" : t == typeof(List<string>) ? "list" :
                                t == typeof(DateTime) || t == typeof(DateTimeOffset) ? "datetime" : t == typeof(double) ? "float" : "int";
                    result.Add(new KeyValuePair<string, string>(PythonName(p.Name), tn));
                }
            }
            return result;
        }
    }

    /// <summary>Tiny INI-style key/value store for GUI settings (%LOCALAPPDATA%\InstaloaderSharpGUI\settings.ini).</summary>
    internal static class SettingsStore
    {
        public static string Folder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InstaloaderSharpGUI");
            }
        }

        public static string FilePath { get { return Path.Combine(Folder, "settings.ini"); } }

        public static Dictionary<string, string> Load()
        {
            return Load(FilePath);
        }

        internal static Dictionary<string, string> Load(string path)
        {
            var d = new Dictionary<string, string>();
            try
            {
                if (!File.Exists(path))
                    return d;
                foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0 || line.StartsWith(";"))
                        continue;
                    d[line.Substring(0, eq)] = Unescape(line.Substring(eq + 1));
                }
            }
            catch (Exception)
            {
            }
            return d;
        }

        public static void Save(Dictionary<string, string> d)
        {
            Save(d, FilePath);
        }

        internal static void Save(Dictionary<string, string> d, string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var sb = new StringBuilder();
                sb.AppendLine("; InstaloaderSharp GUI settings");
                foreach (var kv in d.OrderBy(k => k.Key))
                    sb.Append(kv.Key).Append('=').AppendLine(Escape(kv.Value));
                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            }
            catch (Exception)
            {
            }
        }

        private static string Escape(string s)
        {
            return (s ?? "").Replace("\\", "\\\\").Replace("\r", "").Replace("\n", "\\n");
        }

        private static string Unescape(string s)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char n = s[++i];
                    sb.Append(n == 'n' ? '\n' : n);
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }
    }
}
