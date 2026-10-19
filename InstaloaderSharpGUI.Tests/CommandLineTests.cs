using System;
using System.Collections.Generic;
using System.IO;
using InstaloaderSharp;
using InstaloaderSharpGUI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>
    /// Importing command lines and argument files. Parsing itself is InstaloaderSharp's (tested there); these tests
    /// cover how the GUI reaches the parser and maps its result onto the options.
    /// </summary>
    [TestClass]
    public class CommandLineTests
    {
        private RouterScope _router;

        [TestInitialize]
        public void Init()
        {
            // The parser prints its errors to the console; the router captures them.
            _router = new RouterScope();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _router.Dispose();
        }

        private static JobOptions Import(string commandLine)
        {
            string error;
            Dictionary<string, object> parsed = Library.ParseCommandLine(Library.SplitCommandLine(commandLine), out error);
            Assert.IsNotNull(parsed, error);
            var o = new JobOptions();
            o.ApplyParsedArgs(parsed);
            return o;
        }

        private static string ImportError(string commandLine)
        {
            string error;
            Assert.IsNull(Library.ParseCommandLine(Library.SplitCommandLine(commandLine), out error));
            Assert.IsFalse(string.IsNullOrEmpty(error));
            return error;
        }

        [TestMethod]
        public void SplitsLikeWindows()
        {
            CollectionAssert.AreEqual(new[] { "--login=me", "#tag", "--", "-abc" }, Library.SplitCommandLine("--login=me \"#tag\" -- -abc"));
            CollectionAssert.AreEqual(new[] { "--post-filter=likes > 1", "a\"b", "c d" },
                                      Library.SplitCommandLine("\"--post-filter=likes > 1\" a\\\"b \"c d\""));
            CollectionAssert.AreEqual(new[] { "a", "b" }, Library.SplitCommandLine("a\r\nb"), "pasted line breaks separate arguments");
            Assert.AreEqual(0, Library.SplitCommandLine("").Length);
            Assert.AreEqual(0, Library.SplitCommandLine("   ").Length);
        }

        [TestMethod]
        public void RemovesTheProgramName()
        {
            foreach (string program in new[] { "InstaloaderSharp", "InstaloaderSharp.exe", "instaloader", "\"C:\\Tools\\InstaloaderSharp.exe\"" })
                CollectionAssert.AreEqual(new List<string> { "natgeo" }, Import(program + " natgeo").Targets, program);
        }

        [TestMethod]
        public void ReportsParserErrors()
        {
            StringAssert.Contains(ImportError("--bogus-option natgeo"), "unrecognized arguments: --bogus-option");
            StringAssert.Contains(ImportError("natgeo --count"), "--count");
            // The parser's usage text is not shown, only its error line.
            Assert.IsFalse(ImportError("--bogus").Contains("usage:"));
            Assert.AreEqual("", RouterScope.Text(), "parser output must not reach the activity log");
        }

        [TestMethod]
        public void RoundTripsEveryOption()
        {
            var o = new JobOptions();
            o.Targets.AddRange(new[] { "natgeo", "@nasa", "#sunset", "%123", ":feed", ":stories", ":saved", "-CxYz123" });
            o.ProfilePic = false;
            o.Stories = o.Highlights = o.Tagged = o.Reels = o.Igtv = true;
            o.Pictures = true;
            o.Videos = o.VideoThumbnails = false;
            o.Geotags = o.Comments = true;
            o.MetadataJson = true;
            o.CompressJson = false;
            o.Slide = "1-3";
            o.PostMetadataTxt = "{caption}" + Environment.NewLine + "{likes} likes";
            o.StoryitemMetadataTxt = "{date_utc}";
            o.FastUpdate = o.UseLatestStamps = true;
            o.LatestStampsFile = "D:\\stamps.ini";
            o.PostFilter = "likes > 100 and not is_video";
            o.StoryitemFilter = "is_video";
            o.Count = "50";
            o.Mode = LoginMode.Login;
            o.Username = "someone";
            o.SessionFile = "D:\\session-someone";
            o.DirnamePattern = "{profile}";
            o.FilenamePattern = "{date_utc}_{shortcode}";
            o.TitlePattern = "{target}_{typename}";
            o.ResumePrefix = "resume";
            o.UseAgedResumeFiles = o.SanitizePaths = o.Quiet = true;
            o.UserAgent = "Test Agent/1.0";
            o.Sleep = o.Iphone = false;
            o.MaxConnectionAttempts = "5";
            o.RequestTimeout = "60";
            o.AbortOn = "302,429";

            string error;
            Dictionary<string, object> parsed = Library.ParseCommandLine(o.ToArgs().ToArray(), out error);
            Assert.IsNotNull(parsed, error);
            var back = new JobOptions();
            back.ApplyParsedArgs(parsed);
            CollectionAssert.AreEqual(o.ToArgs(), back.ToArgs());
            Assert.AreEqual(o.PostMetadataTxt, back.PostMetadataTxt);
            Assert.AreEqual(o.SessionFile, back.SessionFile);
        }

        [TestMethod]
        public void ShorthandOptionsMapToSwitches()
        {
            JobOptions o = Import("--profile-pic-only natgeo");
            Assert.IsTrue(o.ProfilePic);
            Assert.IsFalse(o.Posts);

            o = Import("--stories-only natgeo");
            Assert.IsTrue(o.Stories);
            Assert.IsFalse(o.Posts);

            o = Import("-F -s -V natgeo");
            Assert.IsTrue(o.FastUpdate && o.Stories);
            Assert.IsFalse(o.Videos);

            o = Import("--fast --no-pro natgeo");
            Assert.IsTrue(o.FastUpdate, "abbreviated options are accepted");
            Assert.IsFalse(o.ProfilePic);

            o = Import("--only-if=viewer_has_liked :feed");
            Assert.AreEqual("viewer_has_liked", o.PostFilter);
        }

        [TestMethod]
        public void SignInModes()
        {
            JobOptions o = Import("--login=Someone natgeo");
            Assert.AreEqual(LoginMode.Login, o.Mode);
            Assert.AreEqual("Someone", o.Username);

            o = Import("-b Chrome -B D:\\Cookies natgeo");
            Assert.AreEqual(LoginMode.BrowserCookies, o.Mode);
            Assert.AreEqual("chrome", o.Browser);
            Assert.AreEqual("D:\\Cookies", o.CookieFile);

            Assert.AreEqual(LoginMode.Anonymous, Import("natgeo").Mode);
        }

        [TestMethod]
        public void LatestStampsDefaultFileIsLeftEmpty()
        {
            JobOptions o = Import("--latest-stamps -- natgeo");
            Assert.IsTrue(o.UseLatestStamps);
            CollectionAssert.AreEqual(new[] { "natgeo" }, o.Targets);
            Assert.AreEqual("", o.LatestStampsFile);
            Assert.AreEqual(Instaloader.GetDefaultStampsFilename(), o.EffectiveStampsFile);

            o = Import("--latest-stamps=D:\\s.ini natgeo");
            Assert.AreEqual("D:\\s.ini", o.LatestStampsFile);

            // Without "--", the console reads the target as the stamps file - the GUI's command must avoid that.
            o = Import("--latest-stamps natgeo");
            Assert.AreEqual("natgeo", o.LatestStampsFile);
            Assert.AreEqual(0, o.Targets.Count);
        }

        [TestMethod]
        public void LatestStampsWithoutFileRoundTripsWithTargets()
        {
            var o = new JobOptions { UseLatestStamps = true };
            o.Targets.AddRange(new[] { "natgeo", "-abc" });
            string error;
            Dictionary<string, object> parsed = Library.ParseCommandLine(o.ToArgs().ToArray(), out error);
            Assert.IsNotNull(parsed, error);
            var back = new JobOptions();
            back.ApplyParsedArgs(parsed);
            CollectionAssert.AreEqual(new[] { "natgeo", "-abc" }, back.Targets);
            Assert.AreEqual("", back.LatestStampsFile);
        }

        [TestMethod]
        public void RepeatedTemplatesBecomeLines()
        {
            JobOptions o = Import("\"--post-metadata-txt={caption}\" \"--post-metadata-txt={likes} likes\" natgeo");
            Assert.AreEqual("{caption}" + Environment.NewLine + "{likes} likes", o.PostMetadataTxt);
        }

        [TestMethod]
        public void UnmentionedOptionsGetDefaults()
        {
            JobOptions o = Import("natgeo");
            CollectionAssert.AreEqual(new string[0], o.ToArgs(false));
            Assert.AreEqual("3", o.MaxConnectionAttempts);
            Assert.AreEqual("300", o.RequestTimeout);
            Assert.AreEqual("", o.AbortOn);
            Assert.IsTrue(o.Resume);
        }

        [TestMethod]
        public void ImportWithoutTargetsKeepsNoTargets()
        {
            Assert.AreEqual(0, Import("--fast-update").Targets.Count);
        }

        [TestMethod]
        public void JsonTargetsAreResolvedAgainstTheCurrentDirectory()
        {
            using (var dir = new TempDirectory())
            {
                string previous = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = dir.Path;
                    JobOptions o = Import("natgeo\\post.json.xz");
                    Assert.AreEqual(Path.Combine(dir.Path, "natgeo\\post.json.xz"), o.Targets[0]);
                }
                finally
                {
                    Environment.CurrentDirectory = previous;
                }
            }
        }

        [TestMethod]
        public void ReadsArgumentFiles()
        {
            using (var dir = new TempDirectory())
            {
                File.WriteAllLines(dir.File("args.txt"), new[] { "--login=someone", "--fast-update", "+more.txt", "natgeo" });
                File.WriteAllLines(dir.File("more.txt"), new[] { "--stories", "#sunset" });
                string previous = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = dir.Path;
                    string error;
                    Dictionary<string, object> parsed = Library.ParseCommandLine(new[] { "+args.txt" }, out error);
                    Assert.IsNotNull(parsed, error);
                    var o = new JobOptions();
                    o.ApplyParsedArgs(parsed);
                    Assert.AreEqual("someone", o.Username);
                    Assert.IsTrue(o.FastUpdate && o.Stories);
                    CollectionAssert.AreEquivalent(new[] { "natgeo", "#sunset" }, o.Targets);
                }
                finally
                {
                    Environment.CurrentDirectory = previous;
                }
            }
        }

        [TestMethod]
        public void SavedArgumentFileCanBeReadBack()
        {
            // "Save args file…" writes one argument per line, as read by the "+file" syntax.
            var o = new JobOptions { FastUpdate = true, PostFilter = "likes > 10" };
            o.Targets.AddRange(new[] { "natgeo", "-abc" });
            using (var dir = new TempDirectory())
            {
                File.WriteAllLines(dir.File("args.txt"), o.ToArgs());
                string previous = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = dir.Path;
                    string error;
                    Dictionary<string, object> parsed = Library.ParseCommandLine(new[] { "+args.txt" }, out error);
                    Assert.IsNotNull(parsed, error);
                    var back = new JobOptions();
                    back.ApplyParsedArgs(parsed);
                    CollectionAssert.AreEqual(o.ToArgs(), back.ToArgs());
                }
                finally
                {
                    Environment.CurrentDirectory = previous;
                }
            }
        }
    }
}
