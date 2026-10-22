using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using InstaloaderSharp;
using InstaloaderSharp.Util;
using InstaloaderSharpGUI.UI;
using ExitCode = InstaloaderSharp.Program.ExitCode;

namespace InstaloaderSharpGUI.Core
{
    public sealed class JobFinishedEventArgs : EventArgs
    {
        public ExitCode ExitCode;
        public TimeSpan Elapsed;
        public string Message;
    }

    /// <summary>
    /// Runs a download on a background thread by driving the InstaloaderSharp library directly
    /// (Instaloader + Program.MainImpl), the same way InstaloaderSharp.exe does after parsing its arguments.
    /// </summary>
    internal sealed class DownloadEngine
    {
        private readonly SynchronizationContext _ui;
        private Thread _thread;
        private volatile bool _stopRequested;

        public DownloadEngine()
        {
            _ui = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public bool IsRunning { get { return _thread != null && _thread.IsAlive; } }
        public bool StopRequested { get { return _stopRequested; } }

        /// <summary>Raised on the UI thread when the job ends.</summary>
        public event EventHandler<JobFinishedEventArgs> Finished;

        /// <summary>Asks the user for input on the UI thread: (message, isPassword) → answer or null when cancelled.</summary>
        public Func<string, bool, string> Prompt;

        public void Start(JobOptions options, string password)
        {
            if (IsRunning)
                throw new InvalidOperationException("A download is already running.");
            JobOptions o = options.Clone();
            _stopRequested = false;
            _thread = new Thread(() => Run(o, password)) { IsBackground = true, Name = "InstaloaderSharp download" };
            _thread.Start();
        }

        /// <summary>Requests a graceful stop, like pressing Ctrl+C in the console (resume information is saved).</summary>
        public void Stop()
        {
            if (!IsRunning)
                return;
            _stopRequested = true;
            Interrupt.Raise();
        }

        /// <summary>Prompts on the UI thread, blocking the worker until the user answers.</summary>
        public string PromptOnUi(string message, bool password)
        {
            string answer = null;
            Func<string, bool, string> p = Prompt;
            if (p == null)
                return null;
            _ui.Send(_ => answer = p(message, password), null);
            return answer;
        }

        private static void ClearPendingInterrupt()
        {
            try
            {
                Interrupt.Check();
            }
            catch (KeyboardInterruptException)
            {
            }
        }

        private void Run(JobOptions o, string password)
        {
            var sw = Stopwatch.StartNew();
            string previousDir = Environment.CurrentDirectory;
            ExitCode code = ExitCode.UnexpectedError;
            string message = null;
            try
            {
                ClearPendingInterrupt();
                if (o.PreventSleep)
                    Native.PreventSleep(true);
                string outDir = Path.GetFullPath(o.OutputDir.Trim());
                Directory.CreateDirectory(outDir);
                // InstaloaderSharp writes relative to the working directory, exactly like the console program.
                Environment.CurrentDirectory = outDir;
                ConsoleRouter.App(string.Format("▶ Starting download into {0}", outDir));
                ConsoleRouter.App("  Equivalent command: " + o.ToCommandLine());
                code = Execute(o, password, out message);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unexpected error: " + e);
                message = e.Message;
                code = ExitCode.UnexpectedError;
            }
            finally
            {
                try
                {
                    Environment.CurrentDirectory = previousDir;
                }
                catch (Exception)
                {
                }
                if (o.PreventSleep)
                    Native.PreventSleep(false);
                ClearPendingInterrupt();
            }
            sw.Stop();
            var args = new JobFinishedEventArgs { ExitCode = code, Elapsed = sw.Elapsed, Message = message };
            _ui.Post(_ =>
            {
                var h = Finished;
                if (h != null)
                    h(this, args);
            }, null);
        }

        /// <summary>Mirrors InstaloaderSharp.Program.MainWithArgs, with the GUI handling login prompts.</summary>
        private ExitCode Execute(JobOptions o, string password, out string message)
        {
            message = null;
            ExitCode exitCode;
            try
            {
                if (!o.LoggedInMode && o.Stories)
                {
                    Console.Error.WriteLine("Login is required to download stories.");
                    o.Stories = false;
                }
                if (o.Targets.Contains(":feed-all") || o.Targets.Contains(":feed-liked"))
                    throw new InvalidArgumentException(":feed-all and :feed-liked were removed. Use :feed as target and " +
                                                       "eventually --post-filter=viewer_has_liked.");
                if (!o.Captions && (JobOptions.NullIfBlank(o.PostMetadataTxt) != null || JobOptions.NullIfBlank(o.StoryitemMetadataTxt) != null))
                    throw new InvalidArgumentException("--no-captions and --post-metadata-txt or --storyitem-metadata-txt " +
                                                       "given; That contradicts.");
                if (!o.Pictures && o.FastUpdate)
                    throw new InvalidArgumentException("--no-pictures and --fast-update cannot be used together.");

                string err;
                long? count = o.ParseCount(out err);
                if (err != null)
                    throw new InvalidArgumentException(err);
                err = JobOptions.ValidateSlide(o.Slide);
                if (err != null)
                    throw new InvalidArgumentException(err);

                Instaloader loader = Library.CreateLoader(o);
                try
                {
                    Library.SignIn(loader, o, password, PromptOnUi);
                    exitCode = InstaloaderSharp.Program.MainImpl(
                        loader,
                        new List<string>(o.Targets),
                        username: null,
                        password: null,
                        sessionfile: o.Mode == LoginMode.Login ? o.EffectiveSessionFile : null,
                        downloadProfilePic: o.ProfilePic,
                        downloadPosts: o.Posts,
                        downloadStories: o.Stories,
                        downloadHighlights: o.Highlights,
                        downloadTagged: o.Tagged,
                        downloadReels: o.Reels,
                        downloadIgtv: o.Igtv,
                        fastUpdate: o.FastUpdate,
                        latestStampsFile: o.EffectiveStampsFile,
                        maxCount: count,
                        postFilterStr: JobOptions.NullIfBlank(o.PostFilter),
                        storyitemFilterStr: JobOptions.NullIfBlank(o.StoryitemFilter),
                        browser: null,
                        cookiefile: null);
                }
                finally
                {
                    loader.Close();
                }
                if (loader.HasStoredErrors && exitCode == ExitCode.Success)
                    exitCode = ExitCode.NonFatalError;
            }
            catch (KeyboardInterruptException)
            {
                Console.Error.WriteLine("\nInterrupted by user.");
                exitCode = ExitCode.UserAborted;
            }
            catch (InvalidArgumentException err)
            {
                Console.Error.WriteLine(err.Message);
                message = err.Message;
                exitCode = ExitCode.InitFailure;
            }
            catch (LoginException err)
            {
                Console.Error.WriteLine(err.Message);
                message = err.Message;
                exitCode = ExitCode.LoginFailure;
            }
            catch (InstaloaderException err)
            {
                Console.Out.WriteLine("Fatal error: {0}", err.Message);
                message = err.Message;
                exitCode = ExitCode.UnexpectedError;
            }
            return exitCode;
        }
    }

    /// <summary>
    /// Interprets InstaloaderSharp's console output to drive progress bars and counters.
    /// </summary>
    internal sealed class ActivityTracker
    {
        private static readonly Regex ProfileRx = new Regex(@"^\[\s*(\d+)/\s*(\d+)\]\s+Downloading profile (\S+)");
        private static readonly Regex CountRx = new Regex(@"^\[\s*(\d+)/\s*(\d+)\]\s");
        private static readonly Regex OpenCountRx = new Regex(@"^\[\s*(\d+)\]\s");
        private static readonly Regex RetrievingRx = new Regex(@"^(Retrieving .+?)(?:\.\.\.|\.)?\s*$");
        // Counted steps rather than items, e.g. "[3/9] Retrieving highlights "Travel" from profile natgeo" and
        // "[1/2] Retrieving stories from profile natgeo." (or "[  1] ..." without a total).
        private static readonly Regex StepRx = new Regex(@"^\[\s*(\d+)(?:/\s*(\d+))?\]\s+(?:Downloading profile |Retrieving )");
        private static readonly Regex CountedRetrievingRx = new Regex(@"^\[\s*(\d+)(?:/\s*(\d+))?\]\s+(Retrieving .+?)(?:\.\.\.|\.)?\s*$");

        private string _line = "";

        public long Item;
        public long ItemTotal;          // 0 = unknown
        public long ProfileIndex;
        public long ProfileTotal;
        public long Processed;          // posts / story items started
        public long Existing;           // files that were already downloaded
        public long Errors;
        public long Warnings;
        public string Phase = "";
        public string CurrentItem = "";
        public bool Changed;

        public void Reset()
        {
            _line = "";
            Item = ItemTotal = ProfileIndex = ProfileTotal = Processed = Existing = Errors = Warnings = 0;
            Phase = CurrentItem = "";
            Changed = true;
        }

        public void Feed(string text, LogKind kind)
        {
            if (kind == LogKind.Error || kind == LogKind.Warning)
            {
                foreach (string l in text.Split('\n'))
                {
                    string t = l.Trim();
                    if (t.Length == 0 || t == "Errors or warnings occurred:")
                        continue;
                    if (kind == LogKind.Warning) Warnings++; else Errors++;
                }
                Changed = true;
                return;
            }
            if (kind != LogKind.Output)
                return;
            int start = 0;
            while (start <= text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string piece = nl >= 0 ? text.Substring(start, nl - start) : text.Substring(start);
                bool fresh = _line.Length == 0;
                _line += piece;
                if (fresh || piece.Length > 0)
                    InspectPartial(fresh);
                if (nl < 0)
                    break;
                InspectComplete(_line.TrimEnd('\r'));
                _line = "";
                start = nl + 1;
            }
        }

        private void InspectPartial(bool fresh)
        {
            // Item counters are printed before the item finishes; react to them as soon as they appear.
            // Counted steps (profiles, highlights, stories) are not items.
            if (fresh && StepRx.IsMatch(_line))
                return;
            Match m = CountRx.Match(_line);
            if (fresh && m.Success)
            {
                Item = long.Parse(m.Groups[1].Value);
                ItemTotal = long.Parse(m.Groups[2].Value);
                Processed++;
                Changed = true;
                return;
            }
            m = OpenCountRx.Match(_line);
            if (fresh && m.Success)
            {
                Item = long.Parse(m.Groups[1].Value);
                ItemTotal = 0;
                Processed++;
                Changed = true;
            }
        }

        private void InspectComplete(string line)
        {
            Match m = ProfileRx.Match(line);
            if (m.Success)
            {
                ProfileIndex = long.Parse(m.Groups[1].Value);
                ProfileTotal = long.Parse(m.Groups[2].Value);
                Phase = "Downloading profile " + m.Groups[3].Value;
                Item = 0;
                ItemTotal = 0;
                Changed = true;
                return;
            }
            m = CountedRetrievingRx.Match(line);
            if (m.Success)
            {
                Phase = m.Groups[3].Value + (m.Groups[2].Success
                    ? string.Format(" ({0} of {1})", m.Groups[1].Value, m.Groups[2].Value)
                    : "");
                Item = 0;
                ItemTotal = 0;
                Changed = true;
                return;
            }
            if (CountRx.IsMatch(line) || OpenCountRx.IsMatch(line))
            {
                string rest = Regex.Replace(line, @"^\[[^\]]*\]\s*", "");
                if (rest.Contains(" exists"))
                    Existing++;
                CurrentItem = rest.Length > 160 ? rest.Substring(0, 160) + "…" : rest;
                Changed = true;
                return;
            }
            m = RetrievingRx.Match(line);
            if (m.Success)
            {
                Phase = m.Groups[1].Value;
                Item = 0;
                ItemTotal = 0;
                Changed = true;
                return;
            }
            if (line.StartsWith("Logged in as ") || line.StartsWith("Downloading stories") || line.StartsWith("Loaded session"))
            {
                Phase = line.TrimEnd('.');
                Changed = true;
            }
        }
    }
}
