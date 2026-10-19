using System.IO;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>Converting what the user types or pastes into InstaloaderSharp targets.</summary>
    [TestClass]
    public class TargetsTests
    {
        private static string Parse(string input, TargetType type = TargetType.Auto)
        {
            string error;
            return Targets.Parse(input, type, out error);
        }

        private static string Error(string input, TargetType type = TargetType.Auto)
        {
            string error;
            Assert.IsNull(Targets.Parse(input, type, out error), "\"" + input + "\" should not be accepted");
            Assert.IsFalse(string.IsNullOrEmpty(error));
            return error;
        }

        [TestMethod]
        public void AutoDetectsConsoleSyntax()
        {
            Assert.AreEqual("natgeo", Parse("natgeo"));
            Assert.AreEqual("nat.geo_1", Parse("nat.geo_1"));
            Assert.AreEqual("#sunset", Parse("#sunset"));
            Assert.AreEqual("#ñandú", Parse("#ñandú"));
            Assert.AreEqual("%213385402", Parse("%213385402"));
            Assert.AreEqual("-DdwalmrkTSK", Parse("-DdwalmrkTSK"));
            Assert.AreEqual("-Ab_c-9", Parse("-Ab_c-9"));
        }

        [TestMethod]
        public void AtMeansProfileInAutoDetect()
        {
            // As on Instagram; the console's followee syntax needs TargetType.Followees.
            Assert.AreEqual("natgeo", Parse("@natgeo"));
        }

        [TestMethod]
        public void SpecialTargetsAreCaseInsensitive()
        {
            Assert.AreEqual(":feed", Parse(":feed"));
            Assert.AreEqual(":stories", Parse(":Stories"));
            Assert.AreEqual(":saved", Parse(":SAVED"));
            StringAssert.Contains(Error(":feed-all"), ":feed");
        }

        [TestMethod]
        public void TrimsWhitespaceAndQuotes()
        {
            Assert.AreEqual("natgeo", Parse("  natgeo \t"));
            Assert.AreEqual("#sunset", Parse("\"#sunset\""));
            Assert.AreEqual("natgeo", Parse("'natgeo'"));
            Assert.AreEqual("-abc", Parse("<https://www.instagram.com/p/abc/>"));
        }

        [TestMethod]
        public void UnderstandsInstagramLinks()
        {
            Assert.AreEqual("natgeo", Parse("https://www.instagram.com/natgeo/"));
            Assert.AreEqual("natgeo", Parse("instagram.com/natgeo"));
            Assert.AreEqual("natgeo", Parse("http://instagram.com/natgeo?hl=en"));
            Assert.AreEqual("-CxYz123", Parse("https://www.instagram.com/p/CxYz123/?igsh=abc123"));
            Assert.AreEqual("-AbC-9", Parse("https://instagram.com/reel/AbC-9/"));
            Assert.AreEqual("-AbC-9", Parse("https://www.instagram.com/reels/AbC-9/"));
            Assert.AreEqual("-Tv_1", Parse("https://www.instagram.com/tv/Tv_1/"));
            Assert.AreEqual("-Code", Parse("https://www.instagram.com/natgeo/p/Code/"));
            Assert.AreEqual("-Code", Parse("https://www.instagram.com/natgeo/reel/Code/"));
            Assert.AreEqual("#sunset", Parse("https://www.instagram.com/explore/tags/sunset/"));
            Assert.AreEqual("#café", Parse("https://www.instagram.com/explore/tags/caf%C3%A9/"));
            Assert.AreEqual("%213385402", Parse("https://www.instagram.com/explore/locations/213385402/paris-france/"));
            Assert.AreEqual("natgeo", Parse("https://www.instagram.com/stories/natgeo/3456789/"));
            Assert.AreEqual("natgeo", Parse("https://www.instagram.com/natgeo/#fragment"));
        }

        [TestMethod]
        public void RejectsLinksThatAreNotTargets()
        {
            Error("https://www.instagram.com/");
            Error("https://www.instagram.com/accounts/login/");
            Error("https://www.instagram.com/direct/inbox/");
            Error("https://www.instagram.com/explore/");
        }

        [TestMethod]
        public void RejectsInvalidInput()
        {
            Error("");
            Error("   ");
            Error("bad target!");
            Error("name with space");
            Error("#bad-tag");
            Error("%12ab");
            Error(":unknown");
        }

        [TestMethod]
        public void ExplicitTypesAddThePrefix()
        {
            Assert.AreEqual("natgeo", Parse("@natgeo", TargetType.Profile));
            Assert.AreEqual("#sunset", Parse("sunset", TargetType.Hashtag));
            Assert.AreEqual("#sunset", Parse("#sunset", TargetType.Hashtag));
            Assert.AreEqual("%123", Parse("123", TargetType.Location));
            Assert.AreEqual("%123", Parse("%123", TargetType.Location));
            Assert.AreEqual("-abc", Parse("abc", TargetType.Post));
            Assert.AreEqual("-abc", Parse("-abc", TargetType.Post));
            Assert.AreEqual("@natgeo", Parse("natgeo", TargetType.Followees));
            Assert.AreEqual("@natgeo", Parse("@natgeo", TargetType.Followees));
        }

        [TestMethod]
        public void ExplicitTypesValidate()
        {
            Error("not a name", TargetType.Profile);
            Error("bad-tag", TargetType.Hashtag);
            StringAssert.Contains(Error("paris", TargetType.Location), "number");
            Error("a b", TargetType.Post);
            Error("not a name", TargetType.Followees);
        }

        [TestMethod]
        public void FolloweesFromProfileLink()
        {
            Assert.AreEqual("@natgeo", Parse("https://www.instagram.com/natgeo/", TargetType.Followees));
        }

        [TestMethod]
        public void JsonTargetsBecomeAbsolutePaths()
        {
            using (var dir = new TempDirectory())
            {
                string file = dir.File("profile\\2024-01-01_00-00-00_UTC.json.xz");
                File.WriteAllText(file, "");
                Assert.AreEqual(file, Parse(file));
                Assert.AreEqual(file, Parse(file, TargetType.Json));

                string previous = Directory.GetCurrentDirectory();
                try
                {
                    Directory.SetCurrentDirectory(dir.Path);
                    // Downloads change the working directory, so relative paths must be resolved immediately.
                    Assert.AreEqual(file, Parse("profile\\2024-01-01_00-00-00_UTC.json.xz"));
                }
                finally
                {
                    Directory.SetCurrentDirectory(previous);
                }
                StringAssert.Contains(Error(dir.File("missing.json")), "not found");
                StringAssert.Contains(Error(dir.File("missing.txt"), TargetType.Json), "not found");
            }
        }

        [TestMethod]
        public void DescribesEveryKind()
        {
            AssertKind("natgeo", "Profile", Glyphs.Contact, "natgeo");
            AssertKind("@natgeo", "Followees", Glyphs.People, "@natgeo");
            AssertKind("#sunset", "Hashtag", Glyphs.Tag, "#sunset");
            AssertKind("%123", "Location", Glyphs.MapPin, "%123");
            AssertKind("-abc", "Post", Glyphs.Photo, "-abc");
            AssertKind(":feed", "Feed", Glyphs.Home, "Your feed");
            AssertKind(":stories", "Stories", Glyphs.History, "Stories of accounts you follow");
            AssertKind(":saved", "Saved", Glyphs.Bookmark, "Your saved posts");
            AssertKind("C:\\data\\natgeo\\post.json.xz", "JSON file", Glyphs.Document, "post.json.xz");
        }

        private static void AssertKind(string target, string kind, string glyph, string shown)
        {
            TargetEntry e = Targets.Describe(target);
            Assert.AreEqual(kind, e.Kind, target);
            Assert.AreEqual(glyph, e.Glyph, target);
            Assert.AreEqual(shown, e.Value, target);
            // The raw target is kept for downloading, whatever is displayed.
            Assert.AreEqual(target, Targets.RawOf(e), target);
        }

        [TestMethod]
        public void DetailsMentionLoginRequirements()
        {
            StringAssert.Contains(Targets.Describe("@natgeo").Detail, "requires login");
            StringAssert.Contains(Targets.Describe("%123").Detail, "requires login");
            StringAssert.Contains(Targets.Describe(":saved").Detail, "requires login");
            Assert.AreEqual("instagram.com/p/abc", Targets.Describe("-abc").Detail);
            Assert.AreEqual("C:\\data\\natgeo", Targets.Describe("C:\\data\\natgeo\\post.json").Detail);
            Assert.IsNull(Targets.Describe("natgeo").Detail);
        }

        [TestMethod]
        public void TypeNamesMatchTheEnum()
        {
            Assert.AreEqual(7, Targets.TypeNames.Length);
            Assert.AreEqual("Auto-detect", Targets.TypeNames[(int)TargetType.Auto]);
            Assert.AreEqual("Followees of", Targets.TypeNames[(int)TargetType.Followees]);
            Assert.AreEqual("JSON file", Targets.TypeNames[(int)TargetType.Json]);
        }
    }
}
