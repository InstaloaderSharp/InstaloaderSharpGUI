using System.Collections.Generic;
using System.IO;
using System.Text;
using InstaloaderSharpGUI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>settings.ini reading and writing (always in a temporary folder, never the real settings).</summary>
    [TestClass]
    public class SettingsStoreTests
    {
        [TestMethod]
        public void RoundTripsAwkwardValues()
        {
            using (var dir = new TempDirectory())
            {
                string file = dir.File("sub\\settings.ini");
                var values = new Dictionary<string, string>
                {
                    { "Path", "C:\\Users\\me\\Pictures\\a=b" },
                    { "Template", "{caption}\r\n{likes} likes\nlast" },
                    { "Targets", "natgeo\n#sunset\n-abc" },
                    { "Unicode", "Grüße ✓ ：feed" },
                    { "Empty", "" },
                    { "Backslash n", "\\n is not a newline" }
                };
                SettingsStore.Save(values, file);
                Dictionary<string, string> loaded = SettingsStore.Load(file);

                Assert.AreEqual(values.Count, loaded.Count);
                Assert.AreEqual("C:\\Users\\me\\Pictures\\a=b", loaded["Path"], "only the first '=' separates key and value");
                Assert.AreEqual("{caption}\n{likes} likes\nlast", loaded["Template"], "line breaks are stored as \\n");
                Assert.AreEqual("natgeo\n#sunset\n-abc", loaded["Targets"]);
                Assert.AreEqual("Grüße ✓ ：feed", loaded["Unicode"]);
                Assert.AreEqual("", loaded["Empty"]);
                Assert.AreEqual("\\n is not a newline", loaded["Backslash n"]);
            }
        }

        [TestMethod]
        public void WritesOneSortedLinePerValue()
        {
            using (var dir = new TempDirectory())
            {
                string file = dir.File("settings.ini");
                SettingsStore.Save(new Dictionary<string, string> { { "B", "2" }, { "A", "x\ny" } }, file);
                string[] lines = File.ReadAllLines(file, Encoding.UTF8);
                CollectionAssert.AreEqual(new[] { "; InstaloaderSharp GUI settings", "A=x\\ny", "B=2" }, lines);
                byte[] bytes = File.ReadAllBytes(file);
                Assert.AreNotEqual(0xEF, bytes[0], "UTF-8 without byte order mark");
            }
        }

        [TestMethod]
        public void IgnoresCommentsAndBrokenLines()
        {
            using (var dir = new TempDirectory())
            {
                string file = dir.File("settings.ini");
                File.WriteAllText(file, "; comment=1\r\nno equals sign\r\n=no key\r\nKey=Value\r\n");
                Dictionary<string, string> loaded = SettingsStore.Load(file);
                Assert.AreEqual(1, loaded.Count);
                Assert.AreEqual("Value", loaded["Key"]);
            }
        }

        [TestMethod]
        public void MissingFileGivesNoSettings()
        {
            using (var dir = new TempDirectory())
                Assert.AreEqual(0, SettingsStore.Load(dir.File("none.ini")).Count);
        }

        [TestMethod]
        public void OptionsSurviveTheFile()
        {
            using (var dir = new TempDirectory())
            {
                string file = dir.File("settings.ini");
                var o = new JobOptions { OutputDir = "D:\\Insta", Mode = LoginMode.Login, Username = "me", PostMetadataTxt = "{caption}\n{likes}" };
                o.Targets.AddRange(new[] { "natgeo", "#sunset" });
                var store = new Dictionary<string, string>();
                o.Save(store);
                SettingsStore.Save(store, file);

                var loaded = new JobOptions();
                loaded.Load(SettingsStore.Load(file));
                CollectionAssert.AreEqual(o.ToArgs(), loaded.ToArgs());
                Assert.AreEqual("D:\\Insta", loaded.OutputDir);
            }
        }

        [TestMethod]
        public void DefaultLocation()
        {
            StringAssert.EndsWith(SettingsStore.Folder, "\\InstaloaderSharpGUI");
            Assert.AreEqual(Path.Combine(SettingsStore.Folder, "settings.ini"), SettingsStore.FilePath);
        }
    }
}
