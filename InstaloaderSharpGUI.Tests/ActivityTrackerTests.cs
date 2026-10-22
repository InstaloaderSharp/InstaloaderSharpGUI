using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>Reading progress from InstaloaderSharp's console output (formats as printed by the library).</summary>
    [TestClass]
    public class ActivityTrackerTests
    {
        private static ActivityTracker Feed(params string[] output)
        {
            var t = new ActivityTracker();
            foreach (string s in output)
                t.Feed(s, LogKind.Output);
            return t;
        }

        [TestMethod]
        public void ProfileProgress()
        {
            ActivityTracker t = Feed("[2/5] Downloading profile natgeo\n");
            Assert.AreEqual(2, t.ProfileIndex);
            Assert.AreEqual(5, t.ProfileTotal);
            Assert.AreEqual("Downloading profile natgeo", t.Phase);
            Assert.AreEqual(0, t.Processed, "a profile line is not an item");
        }

        [TestMethod]
        public void ItemCounterIsReadBeforeTheLineEnds()
        {
            // The library prints "[  1/120] " first and the rest of the line when the item is done.
            var t = new ActivityTracker();
            t.Feed("[  1/120] ", LogKind.Output);
            Assert.AreEqual(1, t.Item);
            Assert.AreEqual(120, t.ItemTotal);
            Assert.AreEqual(1, t.Processed);
            Assert.IsTrue(t.Changed);

            t.Feed("natgeo\\2024-01-01_00-00-00_UTC.jpg ", LogKind.Output);
            t.Feed("json \n", LogKind.Output);
            Assert.AreEqual(1, t.Processed, "the same item is counted once");
            Assert.AreEqual("natgeo\\2024-01-01_00-00-00_UTC.jpg json", t.CurrentItem.Trim());
        }

        [TestMethod]
        public void ItemsWithoutTotal()
        {
            // Hashtags, the feed etc. have no known total: "[  3] ".
            ActivityTracker t = Feed("[  1] a.jpg\n", "[  2] b.jpg\n", "[  3] ");
            Assert.AreEqual(3, t.Item);
            Assert.AreEqual(0, t.ItemTotal);
            Assert.AreEqual(3, t.Processed);
        }

        [TestMethod]
        public void CountsAlreadyDownloadedFiles()
        {
            ActivityTracker t = Feed("[1/3] natgeo\\a.jpg exists natgeo\\a.json.xz exists\n",
                                     "[2/3] natgeo\\b.jpg json\n",
                                     "[3/3] natgeo\\c.mp4 exists\n");
            Assert.AreEqual(3, t.Processed);
            Assert.AreEqual(2, t.Existing, "counted per item, not per file");
        }

        [TestMethod]
        public void PhaseFromRetrievingMessages()
        {
            Assert.AreEqual("Retrieving posts from profile natgeo", Feed("Retrieving posts from profile natgeo.\n").Phase);
            Assert.AreEqual("Retrieving pictures with hashtag #sunset", Feed("Retrieving pictures with hashtag #sunset...\n").Phase);
            Assert.AreEqual("Logged in as someone", Feed("Logged in as someone.\n").Phase);
            Assert.AreEqual("Downloading stories", Feed("Downloading stories\n").Phase);
        }

        [TestMethod]
        public void CountedHighlightsAreStepsNotItems()
        {
            // InstaloaderSharp 1.0.6+ counts highlights: "[3/9] Retrieving highlights "..." from profile ...".
            ActivityTracker t = Feed("[1/1] Downloading profile natgeo\n",
                                     "[1/3] Retrieving highlights \"Travel\" from profile natgeo\n",
                                     "[ 1/12] ", "natgeo\\a.jpg json\n",
                                     "[ 2/12] ", "natgeo\\b.jpg exists\n",
                                     "[2/3] Retrieving highlights \"Food\" from profile natgeo\n");
            Assert.AreEqual(2, t.Processed, "only the story items are items");
            Assert.AreEqual(1, t.Existing);
            Assert.AreEqual("Retrieving highlights \"Food\" from profile natgeo (2 of 3)", t.Phase);
            Assert.AreEqual(0, t.Item, "a new highlight starts a new list of items");
            Assert.AreEqual(0, t.ItemTotal);
            Assert.AreEqual(1, t.ProfileTotal, "profile progress is kept");

            // The item counter of the new highlight follows.
            t.Feed("[ 1/4] ", LogKind.Output);
            Assert.AreEqual(1, t.Item);
            Assert.AreEqual(4, t.ItemTotal);
            Assert.AreEqual(3, t.Processed);
        }

        [TestMethod]
        public void CountedStoriesAreStepsNotItems()
        {
            // Stories of several profiles: "[1/2] Retrieving stories from profile ...", and for :stories "[  1] ...".
            ActivityTracker t = Feed("Downloading stories\n",
                                     "[1/2] Retrieving stories from profile natgeo.\n",
                                     "[1/3] ", "natgeo\\s1.jpg\n");
            Assert.AreEqual(1, t.Processed);
            Assert.AreEqual("Retrieving stories from profile natgeo (1 of 2)", t.Phase);

            t = Feed("Retrieving all visible stories...\n", "[  1] Retrieving stories from profile natgeo.\n", "[1/5] ");
            Assert.AreEqual(1, t.Processed, "the story's first item, not the profile line");
            Assert.AreEqual("Retrieving stories from profile natgeo", t.Phase);
            Assert.AreEqual(5, t.ItemTotal);
        }

        [TestMethod]
        public void NewPhaseResetsTheItemCounter()
        {
            ActivityTracker t = Feed("[5/9] x.jpg\n", "Retrieving reels videos for profile natgeo.\n");
            Assert.AreEqual(0, t.Item);
            Assert.AreEqual(0, t.ItemTotal);
            Assert.AreEqual(1, t.Processed, "totals are kept");
        }

        [TestMethod]
        public void LongItemTextIsShortened()
        {
            ActivityTracker t = Feed("[1/1] " + new string('x', 300) + "\n");
            Assert.AreEqual(161, t.CurrentItem.Length);
            StringAssert.EndsWith(t.CurrentItem, "…");
        }

        [TestMethod]
        public void CountsErrorsAndWarningsPerLine()
        {
            var t = new ActivityTracker();
            t.Feed("natgeo: Invalid username\nsecond error\n", LogKind.Error);
            t.Feed("Warning: something\n", LogKind.Warning);
            // The summary at the end repeats earlier errors under a header; the header itself is not an error.
            t.Feed("\nErrors or warnings occurred:\n", LogKind.Error);
            Assert.AreEqual(2, t.Errors);
            Assert.AreEqual(1, t.Warnings);
        }

        [TestMethod]
        public void IgnoresMessagesFromTheGui()
        {
            var t = new ActivityTracker();
            t.Feed("[1/2] not from the library\n", LogKind.App);
            Assert.AreEqual(0, t.Processed);
            Assert.IsFalse(t.Changed);
        }

        [TestMethod]
        public void ResetClearsEverything()
        {
            ActivityTracker t = Feed("[1/2] Downloading profile a\n", "[1/3] a.jpg exists\n");
            t.Feed("oops\n", LogKind.Error);
            t.Reset();
            Assert.AreEqual(0, t.ProfileIndex + t.ProfileTotal + t.Item + t.ItemTotal + t.Processed + t.Existing + t.Errors + t.Warnings);
            Assert.AreEqual("", t.Phase);
            Assert.AreEqual("", t.CurrentItem);
            // A partial line from before the reset must not be continued.
            t.Feed("[ 4/4] ", LogKind.Output);
            Assert.AreEqual(1, t.Processed);
        }
    }
}
