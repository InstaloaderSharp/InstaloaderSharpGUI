using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InstaloaderSharp;
using InstaloaderSharpGUI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>The GUI's helpers around the InstaloaderSharp library (all offline).</summary>
    [TestClass]
    public class LibraryTests
    {
        private RouterScope _router;

        [TestInitialize]
        public void Init()
        {
            _router = new RouterScope();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _router.Dispose();
        }

        [TestMethod]
        public void VersionsComeFromTheLoadedAssemblies()
        {
            Version lib = typeof(Instaloader).Assembly.GetName().Version;
            Assert.AreEqual(lib.Revision > 0 ? lib.ToString(4) : lib.ToString(3), Library.Version);
            Version gui = typeof(Library).Assembly.GetName().Version;
            Assert.AreEqual(gui.Revision > 0 ? gui.ToString(4) : gui.ToString(3), Library.GuiVersion);
        }

        [TestMethod]
        public void SessionFilesLiveInTheInstaloaderFolder()
        {
            using (var appData = new IsolatedAppData())
            {
                Assert.AreEqual(appData.ConfigDir, Library.ConfigDir);
                Assert.AreEqual(Path.Combine(appData.ConfigDir, "session-someone"), Library.DefaultSessionFile("SomeOne"));
            }
        }

        [TestMethod]
        public void ListsSavedSessionsNewestFirst()
        {
            using (var appData = new IsolatedAppData())
            {
                Assert.AreEqual(0, Library.ListSessions().Count, "no folder yet");
                Directory.CreateDirectory(appData.ConfigDir);
                string older = Path.Combine(appData.ConfigDir, "session-older.user");
                string newer = Path.Combine(appData.ConfigDir, "session-newer_user");
                File.WriteAllText(older, "x");
                File.WriteAllText(newer, "x");
                File.WriteAllText(Path.Combine(appData.ConfigDir, "latest-stamps.ini"), "");
                File.SetLastWriteTime(older, new DateTime(2024, 1, 1));
                File.SetLastWriteTime(newer, new DateTime(2025, 1, 1));

                List<Library.SessionInfo> sessions = Library.ListSessions();
                CollectionAssert.AreEqual(new[] { "newer_user", "older.user" }, sessions.Select(s => s.Username).ToList());
                Assert.AreEqual(newer, sessions[0].Path);
                Assert.AreEqual(new DateTime(2025, 1, 1), sessions[0].Modified);
            }
        }

        [TestMethod]
        public void CreateLoaderMapsEveryOption()
        {
            var o = new JobOptions
            {
                Sleep = false,
                Quiet = true,
                UserAgent = "Agent",
                DirnamePattern = "{profile}",
                FilenamePattern = "{shortcode}",
                TitlePattern = "{target}",
                Pictures = false,
                Videos = false,
                VideoThumbnails = false,
                Geotags = true,
                Comments = true,
                MetadataJson = false,
                CompressJson = false,
                PostMetadataTxt = "{likes}\r\n{caption}",
                StoryitemMetadataTxt = "{date_utc}",
                MaxConnectionAttempts = "7",
                RequestTimeout = "12.5",
                ResumePrefix = "resume",
                UseAgedResumeFiles = true,
                Slide = "2-3",
                AbortOn = "302,429",
                Iphone = false,
                SanitizePaths = true
            };
            using (Instaloader l = Library.CreateLoader(o))
            {
                Assert.IsFalse(l.Context.Sleep);
                Assert.IsTrue(l.Context.Quiet);
                Assert.AreEqual("Agent", l.Context.UserAgent);
                Assert.AreEqual("{profile}", l.DirnamePattern);
                Assert.AreEqual("{shortcode}", l.FilenamePattern);
                Assert.AreEqual("{target}", l.TitlePattern);
                Assert.IsFalse(l.DownloadPictures || l.DownloadVideos || l.DownloadVideoThumbnails || l.SaveMetadata || l.CompressJson);
                Assert.IsTrue(l.DownloadGeotags && l.DownloadComments && l.SanitizePaths);
                Assert.AreEqual("{likes}\n{caption}", l.PostMetadataTxtPattern);
                Assert.AreEqual("{date_utc}", l.StoryitemMetadataTxtPattern);
                Assert.AreEqual(7, l.Context.MaxConnectionAttempts);
                Assert.AreEqual(12.5, l.Context.RequestTimeout);
                Assert.AreEqual("resume", l.ResumePrefix);
                Assert.IsFalse(l.CheckResumeBbd);
                Assert.AreEqual("2-3", l.Slide);
                CollectionAssert.AreEqual(new[] { 302, 429 }, l.Context.FatalStatusCodes);
                Assert.IsFalse(l.Context.IphoneSupport);
            }
        }

        [TestMethod]
        public void CreateLoaderUsesTheLibraryDefaults()
        {
            using (Instaloader l = Library.CreateLoader(new JobOptions()))
            {
                Assert.IsTrue(l.Context.Sleep);
                Assert.AreEqual(InstaloaderContext.DefaultUserAgent(), l.Context.UserAgent);
                Assert.AreEqual(3, l.Context.MaxConnectionAttempts);
                Assert.AreEqual(300.0, l.Context.RequestTimeout);
                Assert.AreEqual("iterator", l.ResumePrefix);
                Assert.IsTrue(l.CheckResumeBbd && l.SaveMetadata && l.CompressJson && l.Context.IphoneSupport);
            }
        }

        [TestMethod]
        public void CreateLoaderMapsResumeAndCaptions()
        {
            using (Instaloader l = Library.CreateLoader(new JobOptions { Resume = false, ResumePrefix = "ignored" }))
                Assert.IsNull(l.ResumePrefix, "no resume files");
            using (Instaloader l = Library.CreateLoader(new JobOptions { Captions = false }))
                Assert.AreEqual("", l.PostMetadataTxtPattern, "no caption files");
            using (Instaloader l = Library.CreateLoader(new JobOptions { MaxConnectionAttempts = "bad", RequestTimeout = "bad" }))
            {
                Assert.AreEqual(3, l.Context.MaxConnectionAttempts, "invalid values fall back to the defaults");
                Assert.AreEqual(300.0, l.Context.RequestTimeout);
            }
        }

        [TestMethod]
        public void InvalidSlideIsRejectedByTheLibrary()
        {
            try
            {
                Library.CreateLoader(new JobOptions { Slide = "3-2" });
                Assert.Fail("expected InvalidArgumentException");
            }
            catch (InvalidArgumentException e)
            {
                StringAssert.Contains(e.Message, "--slide");
            }
        }

        [TestMethod]
        public void SignInAnonymouslyDoesNothing()
        {
            using (Instaloader l = Library.CreateLoader(new JobOptions()))
            {
                Assert.IsNull(Library.SignIn(l, new JobOptions(), null, (m, p) => { Assert.Fail("must not prompt"); return null; }));
                Assert.IsFalse(l.Context.IsLoggedIn);
            }
        }

        [TestMethod]
        public void SignInWithoutSessionAsksForThePassword()
        {
            using (new IsolatedAppData())
            using (Instaloader l = Library.CreateLoader(new JobOptions()))
            {
                var o = new JobOptions { Mode = LoginMode.Login, Username = " @SomeOne " };
                var prompts = new List<string>();
                try
                {
                    // No saved session and no password: the user is asked; cancelling stops before any request.
                    Library.SignIn(l, o, "", (message, password) =>
                    {
                        Assert.IsTrue(password);
                        prompts.Add(message);
                        return null;
                    });
                    Assert.Fail("expected LoginException");
                }
                catch (LoginException e)
                {
                    Assert.AreEqual("Login cancelled.", e.Message);
                }
                Assert.AreEqual(1, prompts.Count);
                StringAssert.Contains(prompts[0], "someone");
                StringAssert.Contains(RouterScope.Text(), "Session file does not exist yet");
            }
        }

        [TestMethod]
        public void PythonNames()
        {
            Assert.AreEqual("date_utc", Library.PythonName("DateUtc"));
            Assert.AreEqual("is_video", Library.PythonName("IsVideo"));
            Assert.AreEqual("likes", Library.PythonName("Likes"));
            Assert.AreEqual("video_view_count", Library.PythonName("VideoViewCount"));
        }

        [TestMethod]
        public void FilterAttributesComeFromTheStructures()
        {
            Dictionary<string, string> post = Library.FilterAttributes(typeof(Post)).ToDictionary(kv => kv.Key, kv => kv.Value);
            Assert.AreEqual("int", post["likes"]);
            Assert.AreEqual("int", post["comments"]);
            Assert.AreEqual("bool", post["is_video"]);
            Assert.AreEqual("datetime", post["date_utc"]);
            Assert.AreEqual("str", post["caption"]);
            Assert.AreEqual("list", post["caption_hashtags"]);
            Assert.AreEqual("float", post["video_duration"]);
            Assert.IsFalse(post.ContainsKey("context") || post.ContainsKey("node"), "internal objects are not filter attributes");
            Assert.IsFalse(post.ContainsKey("owner_profile"), "only simple values are offered");

            // Every offered attribute must be accepted by the library's filter parser.
            foreach (string name in post.Keys)
                Assert.IsNull(JobOptions.ValidateFilter<Post>(name), name);

            Dictionary<string, string> story = Library.FilterAttributes(typeof(StoryItem)).ToDictionary(kv => kv.Key, kv => kv.Value);
            Assert.AreEqual("datetime", story["expiring_utc"]);
            Assert.IsFalse(story.ContainsKey("likes"));
            foreach (string name in story.Keys)
                Assert.IsNull(JobOptions.ValidateFilter<StoryItem>(name), name);
        }

        [TestMethod]
        public void DownloadImageWithoutUrl()
        {
            using (Instaloader l = Library.CreateLoader(new JobOptions()))
            {
                Assert.IsNull(Library.DownloadImage(l, null));
                Assert.IsNull(Library.DownloadImage(l, ""));
            }
        }
    }
}
