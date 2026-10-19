using System;
using System.Collections.Generic;
using System.Linq;
using InstaloaderSharp;
using InstaloaderSharpGUI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>The options model: validation, mapping to command line arguments and persistence.</summary>
    [TestClass]
    public class JobOptionsTests
    {
        private static JobOptions Ready(params string[] targets)
        {
            var o = new JobOptions { OutputDir = "C:\\Downloads" };
            o.Targets.AddRange(targets.Length > 0 ? targets : new[] { "natgeo" });
            return o;
        }

        private static string Args(JobOptions o)
        {
            return string.Join(" ", o.ToArgs());
        }

        // ------------------------------------------------------------------ defaults

        [TestMethod]
        public void DefaultsMatchTheCommandLineDefaults()
        {
            var o = new JobOptions();
            // Without any option, InstaloaderSharp downloads posts, profile picture, pictures, videos, thumbnails,
            // captions and compressed metadata, sleeps between requests, resumes and uses the iPhone API.
            Assert.IsTrue(o.Posts && o.ProfilePic && o.Pictures && o.Videos && o.VideoThumbnails && o.Captions);
            Assert.IsTrue(o.MetadataJson && o.CompressJson && o.Sleep && o.Resume && o.Iphone);
            Assert.IsFalse(o.Stories || o.Highlights || o.Tagged || o.Reels || o.Igtv || o.Geotags || o.Comments);
            Assert.IsFalse(o.FastUpdate || o.UseLatestStamps || o.Quiet || o.SanitizePaths || o.UseAgedResumeFiles);
            Assert.AreEqual(LoginMode.Anonymous, o.Mode);
            Assert.AreEqual("3", o.MaxConnectionAttempts);
            Assert.AreEqual("300", o.RequestTimeout);
            Assert.AreEqual(0, o.ToArgs().Count, "default options need no arguments");
            Assert.AreEqual("InstaloaderSharp", o.ToCommandLine());
        }

        [TestMethod]
        public void CloneCopiesTheTargetList()
        {
            JobOptions o = Ready("a");
            JobOptions c = o.Clone();
            c.Targets.Add("b");
            c.FastUpdate = true;
            CollectionAssert.AreEqual(new[] { "a" }, o.Targets);
            Assert.IsFalse(o.FastUpdate);
        }

        [TestMethod]
        public void BrowserNames()
        {
            CollectionAssert.AreEqual(new[] { "brave", "chrome", "chromium", "edge", "firefox", "librewolf", "opera", "opera_gx", "safari", "vivaldi" },
                                      JobOptions.Browsers);
            Assert.AreEqual("Opera GX", JobOptions.BrowserDisplayName("opera_gx"));
            Assert.AreEqual("LibreWolf", JobOptions.BrowserDisplayName("librewolf"));
            Assert.AreEqual("Firefox", JobOptions.BrowserDisplayName("firefox"));
        }

        // ------------------------------------------------------------------ validation

        [TestMethod]
        public void ReadyOptionsAreValid()
        {
            JobOptions.ValidationResult v = Ready().Validate();
            Assert.IsTrue(v.Ok, string.Join("; ", v.Errors));
            Assert.AreEqual(0, v.Warnings.Count);
        }

        private static void AssertError(JobOptions o, string expectedPart)
        {
            JobOptions.ValidationResult v = o.Validate();
            Assert.IsFalse(v.Ok, "expected an error containing \"" + expectedPart + "\"");
            Assert.IsTrue(v.Errors.Any(e => e.Contains(expectedPart)), "errors were: " + string.Join("; ", v.Errors));
        }

        private static void AssertWarning(JobOptions o, string expectedPart)
        {
            JobOptions.ValidationResult v = o.Validate();
            Assert.IsTrue(v.Ok, string.Join("; ", v.Errors));
            Assert.IsTrue(v.Warnings.Any(e => e.Contains(expectedPart)), "warnings were: " + string.Join("; ", v.Warnings));
        }

        [TestMethod]
        public void NeedsTargetsAndAFolder()
        {
            var o = new JobOptions();
            AssertError(o, "at least one target");
            AssertError(o, "folder");
            // The Download page checks without targets while the list is still being filled.
            Assert.IsFalse(o.Validate(false).Errors.Any(e => e.Contains("target")));
        }

        [TestMethod]
        public void RejectsRemovedFeedTargets()
        {
            AssertError(Ready(":feed-all"), ":feed-all and :feed-liked were removed");
            AssertError(Ready(":feed-liked"), ":feed-all and :feed-liked were removed");
        }

        [TestMethod]
        public void RejectsContradictionsLikeTheCommandLine()
        {
            JobOptions o = Ready();
            o.Captions = false;
            o.PostMetadataTxt = "{likes}";
            AssertError(o, "contradicts");

            o = Ready();
            o.Captions = false;
            o.StoryitemMetadataTxt = "{caption}";
            AssertError(o, "contradicts");

            o = Ready();
            o.Pictures = false;
            o.FastUpdate = true;
            AssertError(o, "Fast update");
        }

        [TestMethod]
        public void SignInNeedsAUsernameOrBrowser()
        {
            JobOptions o = Ready();
            o.Mode = LoginMode.Login;
            AssertError(o, "username");
            o.Username = "someone";
            Assert.IsTrue(o.Validate().Ok);

            o = Ready();
            o.Mode = LoginMode.BrowserCookies;
            o.Browser = "";
            AssertError(o, "browser");
        }

        [TestMethod]
        public void ValidatesNumbersAndCodes()
        {
            JobOptions o = Ready();
            o.Count = "ten";
            AssertError(o, "whole number");
            o = Ready();
            o.Count = "-1";
            AssertError(o, "whole number");
            o = Ready();
            o.MaxConnectionAttempts = "x";
            AssertError(o, "connection attempts");
            o = Ready();
            o.RequestTimeout = "0";
            AssertError(o, "timeout");
            o = Ready();
            o.AbortOn = "302,abc";
            AssertError(o, "Abort on");
            o = Ready();
            o.AbortOn = "99";
            AssertError(o, "Invalid HTTP status code: 99");
        }

        [TestMethod]
        public void ValidatesFiltersWithTheLibrary()
        {
            JobOptions o = Ready();
            o.PostFilter = "likes > 100 and not is_video";
            o.StoryitemFilter = "date_utc >= datetime(2024, 1, 1)";
            Assert.IsTrue(o.Validate().Ok);

            o.PostFilter = "foo > 1";
            AssertError(o, "Post filter: Invalid filter: foo not a Post attribute.");
            o.PostFilter = "";
            o.StoryitemFilter = "likes >";
            AssertError(o, "Story item filter: Invalid filter: invalid syntax");
        }

        [TestMethod]
        public void ValidateFilterAcceptsEmpty()
        {
            Assert.IsNull(JobOptions.ValidateFilter<Post>(null));
            Assert.IsNull(JobOptions.ValidateFilter<Post>("  "));
            Assert.IsNull(JobOptions.ValidateFilter<StoryItem>("is_video"));
            Assert.IsNotNull(JobOptions.ValidateFilter<StoryItem>("likes > 1"), "StoryItem has no likes");
        }

        [TestMethod]
        public void ValidatesSlideLikeTheLibrary()
        {
            foreach (string ok in new[] { "", "1", "last", "2-4", "3-last", " 2 " })
            {
                Assert.IsNull(JobOptions.ValidateSlide(ok), "\"" + ok + "\"");
                // The library must accept everything the GUI accepts.
                using (Library.CreateLoader(new JobOptions { Slide = ok }))
                {
                }
            }
            Assert.AreEqual("--slide parameter must be greater than 0.", JobOptions.ValidateSlide("0"));
            foreach (string bad in new[] { "abc", "first", "4-2", "0-3", "1-2-3", "last-2", "2-x" })
                Assert.AreEqual("Invalid data for --slide parameter.", JobOptions.ValidateSlide(bad), bad);

            JobOptions o = Ready();
            o.Slide = "abc";
            AssertError(o, "Images of multi-image posts: Invalid data for --slide parameter.");
        }

        [TestMethod]
        public void MissingJsonTargetIsAnError()
        {
            AssertError(Ready("C:\\does\\not\\exist.json.xz"), "JSON target not found");
        }

        [TestMethod]
        public void WarnsAboutLoginOnlyFeaturesWhenAnonymous()
        {
            JobOptions o = Ready();
            o.Stories = true;
            AssertWarning(o, "stories will be skipped");

            o = Ready();
            o.Highlights = true;
            AssertWarning(o, "Highlights and comments require login");
            o = Ready();
            o.Comments = true;
            AssertWarning(o, "Highlights and comments require login");

            foreach (string t in new[] { ":feed", ":stories", ":saved", "@natgeo", "%123" })
                AssertWarning(Ready(t), "require login");

            o = Ready(":feed");
            o.Stories = true;
            o.Mode = LoginMode.BrowserCookies;
            Assert.AreEqual(0, o.Validate().Warnings.Count, "no warnings when signed in");
        }

        // ------------------------------------------------------------------ parsing helpers

        [TestMethod]
        public void ParsesNumbers()
        {
            string err;
            var o = new JobOptions();
            Assert.IsNull(o.ParseCount(out err));
            Assert.IsNull(err);
            o.Count = " 50 ";
            Assert.AreEqual(50L, o.ParseCount(out err));
            o.MaxConnectionAttempts = "";
            Assert.AreEqual(3, o.ParseMaxConnectionAttempts(out err));
            o.MaxConnectionAttempts = "0";
            Assert.AreEqual(0, o.ParseMaxConnectionAttempts(out err), "0 = retry forever");
            o.RequestTimeout = "";
            Assert.AreEqual(300.0, o.ParseRequestTimeout(out err));
            o.RequestTimeout = "1.5";
            Assert.AreEqual(1.5, o.ParseRequestTimeout(out err), "invariant culture");
            o.AbortOn = " 302, 400 ,429";
            CollectionAssert.AreEqual(new[] { 302, 400, 429 }, o.ParseAbortOn(out err));
            o.AbortOn = "";
            Assert.IsNull(o.ParseAbortOn(out err));
            Assert.IsNull(err);
        }

        [TestMethod]
        public void EffectiveFiles()
        {
            var o = new JobOptions();
            Assert.IsNull(o.EffectiveStampsFile, "disabled");
            o.UseLatestStamps = true;
            Assert.AreEqual(Instaloader.GetDefaultStampsFilename(), o.EffectiveStampsFile);
            o.LatestStampsFile = "  D:\\stamps.ini ";
            Assert.AreEqual("D:\\stamps.ini", o.EffectiveStampsFile);

            Assert.IsNull(o.EffectiveSessionFile);
            o.SessionFile = " D:\\session ";
            Assert.AreEqual("D:\\session", o.EffectiveSessionFile);
        }

        [TestMethod]
        public void TemplateLines()
        {
            Assert.IsNull(JobOptions.Lines(null));
            Assert.IsNull(JobOptions.Lines(""));
            Assert.AreEqual("{caption}\n{likes} likes", JobOptions.Lines("{caption}\r\n{likes} likes"));
            Assert.IsNull(JobOptions.NullIfBlank("  "));
            Assert.AreEqual("x", JobOptions.NullIfBlank(" x "));
        }

        // ------------------------------------------------------------------ command line

        [TestMethod]
        public void ProfileContentArguments()
        {
            JobOptions o = new JobOptions { Posts = false };
            Assert.AreEqual("--profile-pic-only", Args(o));
            o.Stories = true;
            Assert.AreEqual("--no-posts --stories", Args(o));
            o = new JobOptions { ProfilePic = false, Highlights = true, Tagged = true, Reels = true, Igtv = true };
            Assert.AreEqual("--no-profile-pic --highlights --tagged --reels --igtv", Args(o));
        }

        [TestMethod]
        public void PostContentArguments()
        {
            var o = new JobOptions
            {
                Slide = "2-4",
                Pictures = false,
                Videos = false,
                VideoThumbnails = false,
                Geotags = true,
                Comments = true,
                MetadataJson = false,
                CompressJson = false
            };
            Assert.AreEqual("--slide=2-4 --no-pictures --no-videos --no-video-thumbnails --geotags --comments --no-metadata-json --no-compress-json",
                            Args(o));
        }

        [TestMethod]
        public void TemplatesBecomeOneArgumentPerLine()
        {
            var o = new JobOptions { PostMetadataTxt = "{caption}\r\n\r\n{likes} likes", StoryitemMetadataTxt = "{date_utc}" };
            CollectionAssert.AreEqual(new[] { "--post-metadata-txt={caption}", "--post-metadata-txt={likes} likes", "--storyitem-metadata-txt={date_utc}" },
                                      o.ToArgs());
            o.Captions = false;
            CollectionAssert.AreEqual(new[] { "--no-captions" }, o.ToArgs(), "templates are not written without captions");
        }

        [TestMethod]
        public void SelectionArguments()
        {
            var o = new JobOptions { FastUpdate = true, UseLatestStamps = true, PostFilter = "likes > 1", StoryitemFilter = "is_video", Count = "5" };
            Assert.AreEqual("--fast-update --latest-stamps --post-filter=likes > 1 --storyitem-filter=is_video --count=5", Args(o));
            o.LatestStampsFile = "D:\\s.ini";
            StringAssert.Contains(Args(o), "--latest-stamps=D:\\s.ini");
        }

        [TestMethod]
        public void LoginArgumentsDependOnTheMode()
        {
            var o = new JobOptions { Username = "me", SessionFile = "D:\\s", Browser = "edge", CookieFile = "D:\\c" };
            Assert.AreEqual("", Args(o), "anonymous ignores the stored login fields");
            o.Mode = LoginMode.Login;
            Assert.AreEqual("--login=me --sessionfile=D:\\s", Args(o));
            o.Mode = LoginMode.BrowserCookies;
            Assert.AreEqual("--load-cookies=edge --cookiefile=D:\\c", Args(o));
        }

        [TestMethod]
        public void HowArgumentsOmitDefaults()
        {
            var o = new JobOptions
            {
                DirnamePattern = "{profile}",
                FilenamePattern = "{shortcode}",
                TitlePattern = "{target}",
                ResumePrefix = "r",
                UseAgedResumeFiles = true,
                SanitizePaths = true,
                UserAgent = "UA",
                Sleep = false,
                MaxConnectionAttempts = "3",
                RequestTimeout = "300",
                AbortOn = "302, 429",
                Iphone = false,
                Quiet = true
            };
            Assert.AreEqual("--dirname-pattern={profile} --filename-pattern={shortcode} --title-pattern={target} --resume-prefix=r " +
                            "--use-aged-resume-files --sanitize-paths --user-agent=UA --no-sleep --abort-on=302,429 --no-iphone --quiet",
                            Args(o));
            o.MaxConnectionAttempts = "0";
            o.RequestTimeout = "60";
            StringAssert.Contains(Args(o), "--max-connection-attempts=0 --request-timeout=60");
            o.Resume = false;
            StringAssert.Contains(Args(o), "--no-resume");
            Assert.IsFalse(Args(o).Contains("--resume-prefix"), "--no-resume and --resume-prefix contradict");
        }

        [TestMethod]
        public void ShortcodeTargetsComeAfterDoubleDash()
        {
            JobOptions o = Ready("natgeo", "-abc", "#tag", "-def");
            CollectionAssert.AreEqual(new[] { "natgeo", "--", "-abc", "#tag", "-def" }, o.ToArgs());
            CollectionAssert.AreEqual(new string[0], o.ToArgs(false));
        }

        [TestMethod]
        public void LatestStampsWithoutFileIsNotFollowedByATarget()
        {
            // "--latest-stamps natgeo" would make natgeo the stamps file.
            JobOptions o = Ready("natgeo", "-abc");
            o.UseLatestStamps = true;
            CollectionAssert.AreEqual(new[] { "--latest-stamps", "--", "natgeo", "-abc" }, o.ToArgs());
            o.Count = "5";
            CollectionAssert.AreEqual(new[] { "--latest-stamps", "--count=5", "natgeo", "--", "-abc" }, o.ToArgs());
            o.Count = "";
            o.LatestStampsFile = "D:\\s.ini";
            CollectionAssert.AreEqual(new[] { "--latest-stamps=D:\\s.ini", "natgeo", "--", "-abc" }, o.ToArgs());
        }

        [TestMethod]
        public void QuotesForTheShell()
        {
            Assert.AreEqual("natgeo", JobOptions.Quote("natgeo"));
            Assert.AreEqual("\"\"", JobOptions.Quote(""));
            Assert.AreEqual("\"#tag\"", JobOptions.Quote("#tag"));
            Assert.AreEqual("\"%123\"", JobOptions.Quote("%123"));
            Assert.AreEqual("\"--post-filter=likes > 1\"", JobOptions.Quote("--post-filter=likes > 1"));
            Assert.AreEqual("\"a\\\"b\"", JobOptions.Quote("a\"b"));
            JobOptions o = Ready("natgeo", "#sunset");
            o.FastUpdate = true;
            Assert.AreEqual("InstaloaderSharp --fast-update natgeo \"#sunset\"", o.ToCommandLine());
        }

        // ------------------------------------------------------------------ persistence

        [TestMethod]
        public void SaveAndLoadRoundTrip()
        {
            JobOptions o = Ready("natgeo", "#sunset", "C:\\a b\\post.json");
            o.Mode = LoginMode.BrowserCookies;
            o.Browser = "opera_gx";
            o.FastUpdate = true;
            o.Pictures = false;
            o.PostMetadataTxt = "{caption}\r\n{likes}";
            o.Count = "12";
            var store = new Dictionary<string, string>();
            o.Save(store);
            Assert.AreEqual("1", store["FastUpdate"]);
            Assert.AreEqual("0", store["Pictures"]);
            Assert.AreEqual("BrowserCookies", store["Mode"]);

            var loaded = new JobOptions();
            loaded.Load(store);
            CollectionAssert.AreEqual(o.ToArgs(), loaded.ToArgs());
            CollectionAssert.AreEqual(o.Targets, loaded.Targets);
            Assert.AreEqual(o.OutputDir, loaded.OutputDir);
            Assert.AreEqual(o.PostMetadataTxt, loaded.PostMetadataTxt);
        }

        [TestMethod]
        public void LoadIgnoresUnknownAndInvalidValues()
        {
            var o = new JobOptions();
            o.Load(new Dictionary<string, string> { { "Mode", "NoSuchMode" }, { "SomethingOld", "1" }, { "Reels", "1" } });
            Assert.AreEqual(LoginMode.Anonymous, o.Mode);
            Assert.IsTrue(o.Reels);
        }

        [TestMethod]
        public void PasswordsAreNeverPersisted()
        {
            var store = new Dictionary<string, string>();
            new JobOptions().Save(store);
            Assert.IsFalse(store.Keys.Any(k => k.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }
}
