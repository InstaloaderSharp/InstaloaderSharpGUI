using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InstaloaderSharpGUI.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>The custom dark-theme controls: behaviour and layout (not their painting).</summary>
    [TestClass]
    public class ControlTests
    {
        // ------------------------------------------------------------------ theme

        [TestMethod]
        public void ScalesForTheDisplay()
        {
            Assert.AreEqual((int)Math.Round(10 * Theme.ScaleFactor), Theme.S(10));
            Assert.AreEqual(10 * Theme.ScaleFactor, Theme.S(10f), 0.0001);
            Assert.IsTrue(Theme.ScaleFactor >= 1f);
        }

        [TestMethod]
        public void BlendsColors()
        {
            Assert.AreEqual(Color.Black.ToArgb(), Theme.Blend(Color.Black, Color.White, 0).ToArgb());
            Assert.AreEqual(Color.White.ToArgb(), Theme.Blend(Color.Black, Color.White, 1).ToArgb());
            Assert.AreEqual(127, Theme.Blend(Color.Black, Color.White, 0.5f).R);
            Assert.AreEqual(Color.White.ToArgb(), Theme.Blend(Color.Black, Color.White, 3).ToArgb(), "clamped");
        }

        [TestMethod]
        public void BackgroundComesFromTheFirstOpaqueParent()
        {
            Sta.Run(() =>
            {
                var panel = new Panel { BackColor = Color.Red };
                var stack = new HStack();
                var button = new FlatButton("x");
                stack.Add(button);
                panel.Controls.Add(stack);
                Assert.AreEqual(Color.Red.ToArgb(), Theme.BackOf(button.Parent).ToArgb());
                Assert.AreEqual(Theme.Window.ToArgb(), Theme.BackOf(null).ToArgb());
            });
        }

        [TestMethod]
        public void TextContrastIsAccessible()
        {
            // WCAG AA: at least 4.5:1 for body text on the surfaces it is drawn on.
            foreach (Color back in new[] { Theme.Window, Theme.Sidebar, Theme.Surface, Theme.Input })
            {
                Assert.IsTrue(Contrast(Theme.Text, back) >= 4.5, "Text on " + back);
                Assert.IsTrue(Contrast(Theme.TextSecondary, back) >= 4.5, "TextSecondary on " + back);
            }
            Assert.IsTrue(Contrast(Color.White, Theme.Accent) >= 4.5, "white on the accent (primary buttons)");
        }

        private static double Luminance(Color c)
        {
            Func<int, double> ch = v =>
            {
                double s = v / 255.0;
                return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            };
            return 0.2126 * ch(c.R) + 0.7152 * ch(c.G) + 0.0722 * ch(c.B);
        }

        private static double Contrast(Color a, Color b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        // ------------------------------------------------------------------ inputs

        [TestMethod]
        public void ButtonSizeFollowsContent()
        {
            Sta.Run(() =>
            {
                var plain = new FlatButton("Download");
                var withGlyph = new FlatButton("Download", ButtonKind.Primary, Glyphs.Download);
                Assert.IsTrue(withGlyph.Width > plain.Width, "the icon takes space");
                Assert.AreEqual(Theme.S(32), plain.Height);
                var icon = new FlatButton("Settings", ButtonKind.Subtle, Glyphs.Settings) { IconOnly = true };
                Size s = icon.GetPreferredSize(Size.Empty);
                Assert.AreEqual(s.Width, s.Height, "icon-only buttons are square");
                Assert.AreEqual("Download", withGlyph.AccessibleName);
            });
        }

        [TestMethod]
        public void ToggleSwitchIsACheckBox()
        {
            Sta.Run(() =>
            {
                var t = new ToggleSwitch();
                Assert.IsInstanceOfType(t, typeof(CheckBox), "keeps keyboard and accessibility support");
                Assert.AreEqual(new Size(Theme.S(84), Theme.S(28)), t.GetPreferredSize(Size.Empty));
                int changes = 0;
                t.CheckedChanged += (s, e) => changes++;
                t.Checked = true;
                t.Checked = true;
                Assert.AreEqual(1, changes);
            });
        }

        [TestMethod]
        public void TextBoxForwardsToTheInnerBox()
        {
            Sta.Run(() =>
            {
                var box = new DarkTextBox();
                int changes = 0;
                box.TextChanged += (s, e) => changes++;
                box.Text = "natgeo";
                Assert.AreEqual("natgeo", box.InnerTextBox.Text);
                box.InnerTextBox.Text = "typed";
                Assert.AreEqual("typed", box.Text);
                Assert.AreEqual(2, changes, "TextChanged of the inner box is raised by the control");
                box.Text = null;
                Assert.AreEqual("", box.Text);

                box.UseSystemPasswordChar = true;
                Assert.IsTrue(box.InnerTextBox.UseSystemPasswordChar);
                box.ReadOnly = true;
                Assert.IsTrue(box.InnerTextBox.ReadOnly);
            });
        }

        [TestMethod]
        public void TextBoxInsertsAtTheCaret()
        {
            Sta.Run(() =>
            {
                var box = new DarkTextBox { Text = "{date_utc}_UTC" };
                box.InnerTextBox.SelectionStart = 10;
                box.InsertAtCaret("_{shortcode}");
                Assert.AreEqual("{date_utc}_{shortcode}_UTC", box.Text);
                Assert.AreEqual(22, box.InnerTextBox.SelectionStart, "caret after the insertion");
            });
        }

        [TestMethod]
        public void TextBoxHeights()
        {
            Sta.Run(() =>
            {
                var single = new DarkTextBox();
                Assert.AreEqual(Theme.S(32), single.GetPreferredSize(Size.Empty).Height);
                var multi = new DarkTextBox { Multiline = true, MultilineHeight = Theme.S(76) };
                Assert.IsTrue(multi.InnerTextBox.Multiline && multi.InnerTextBox.AcceptsReturn);
                Assert.AreEqual(Theme.S(76), multi.GetPreferredSize(Size.Empty).Height);
            });
        }

        [TestMethod]
        public void ComboBoxIsADropDownList()
        {
            Sta.Run(() =>
            {
                var c = new DarkComboBox { Width = 123 };
                Assert.AreEqual(ComboBoxStyle.DropDownList, c.DropDownStyle);
                Assert.AreEqual(DrawMode.OwnerDrawFixed, c.DrawMode);
                Assert.AreEqual(123, c.GetPreferredSize(Size.Empty).Width);
            });
        }

        [TestMethod]
        public void SegmentedControlSelection()
        {
            Sta.Run(() =>
            {
                var seg = new SegmentedControl("Anonymous", "Instagram login", "Browser cookies");
                var raised = new List<int>();
                seg.SelectedIndexChanged += (s, e) => raised.Add(seg.SelectedIndex);
                seg.SelectedIndex = 2;
                seg.SelectedIndex = 2;
                seg.SelectedIndex = 9;
                seg.SelectedIndex = -4;
                CollectionAssert.AreEqual(new[] { 2, 0 }, raised, "clamped, and only raised on changes");
                Assert.AreEqual("Anonymous", seg.AccessibleName);
            });
        }

        [TestMethod]
        public void NavRailSelection()
        {
            Sta.Run(() =>
            {
                var nav = new NavRail();
                nav.Items.Add(new NavItem { Text = "Download" });
                nav.Items.Add(new NavItem { Text = "Activity" });
                int changes = 0;
                nav.SelectedIndexChanged += (s, e) => changes++;
                Assert.IsNull(nav.SelectedItem);
                nav.SelectedIndex = 1;
                nav.SelectedIndex = 5;
                nav.SelectedIndex = -1;
                Assert.AreEqual(1, nav.SelectedIndex, "out of range is ignored");
                Assert.AreEqual("Activity", nav.SelectedItem.Text);
                Assert.AreEqual(1, changes);
            });
        }

        [TestMethod]
        public void ProgressBarClamps()
        {
            Sta.Run(() =>
            {
                var bar = new ProgressBarEx { Maximum = 200 };
                bar.Value = 50;
                Assert.AreEqual(25, bar.Percent);
                bar.Value = 500;
                Assert.AreEqual(200, bar.Value);
                bar.Value = -3;
                Assert.AreEqual(0, bar.Value);
                bar.Maximum = 0;
                Assert.AreEqual(1, bar.Maximum, "never divides by zero");
            });
        }

        // ------------------------------------------------------------------ layout

        [TestMethod]
        public void SettingRowClickFlipsItsSwitch()
        {
            Sta.Run(() =>
            {
                var row = new SettingRow("Fast update", "Stop at the first already-downloaded item.");
                var sw = new ToggleSwitch();
                row.Accessory = sw;
                Assert.AreEqual("Fast update", sw.AccessibleName, "the switch is named after its row");
                UiFind.Invoke(row, "OnClick", EventArgs.Empty);
                Assert.IsTrue(sw.Checked, "the whole row is a click target");
                row.Enabled = false;
                UiFind.Invoke(row, "OnClick", EventArgs.Empty);
                Assert.IsTrue(sw.Checked, "disabled rows do not react");
            });
        }

        [TestMethod]
        public void SettingRowHeightGrowsWithContent()
        {
            Sta.Run(() =>
            {
                var row = new SettingRow("Title");
                int minimum = row.PreferredHeight(600);
                Assert.AreEqual(Theme.S(56), minimum);
                row.Description = "A description that is long enough to wrap onto more than one line when the row is narrow. " +
                                  "It keeps going for a while so that it certainly needs several lines.";
                Assert.IsTrue(row.PreferredHeight(300) > row.PreferredHeight(1200), "wrapped description");
                row.Content = new DarkTextBox();
                int withContent = row.PreferredHeight(1200);
                row.Content = null;
                Assert.IsTrue(withContent > row.PreferredHeight(1200) + Theme.S(30), "content goes under the title");
            });
        }

        [TestMethod]
        public void CardStacksVisibleRowsWithDividers()
        {
            Sta.Run(() =>
            {
                var card = new Card();
                var a = card.Add(new SettingRow("A"));
                var b = card.Add(new SettingRow("B"));
                var c = card.Add(new SettingRow("C"));
                int row = a.PreferredHeight(500);
                Assert.AreEqual(row * 3 + 2, card.PreferredHeight(500), "rows plus 1px dividers");
                b.Visible = false;
                Assert.AreEqual(row * 2 + 1, card.PreferredHeight(500), "hidden rows take no space");
                card.SetBounds(0, 0, 500, card.PreferredHeight(500));
                card.PerformLayout();
                Assert.AreEqual(0, a.Top);
                Assert.AreEqual(a.Bottom + 1, c.Top);
                Assert.AreEqual(500, c.Width);
            });
        }

        [TestMethod]
        public void StackPageStacksInAddOrderEvenWhenHidden()
        {
            Sta.Run(() =>
            {
                var page = new StackPage { Size = new Size(1000, 300), Visible = false };
                var first = page.Add(new TextBlock("first"));
                var hidden = page.Add(new TextBlock("hidden"));
                var gapped = page.Add(new TextBlock("third"), 40);
                hidden.Visible = false;
                page.PerformLayout();
                Assert.AreEqual(page.Padding.Top, first.Top);
                Assert.AreEqual(first.Bottom + page.Spacing + 40, gapped.Top, "spacing plus the extra gap; hidden child skipped");
                Assert.AreEqual(page.Padding.Left, first.Left);
                Assert.AreEqual(1000 - page.Padding.Horizontal, first.Width);
            });
        }

        [TestMethod]
        public void StackPageCapsTheContentWidth()
        {
            Sta.Run(() =>
            {
                var page = new StackPage { Size = new Size(4000, 300) };
                var block = page.Add(new TextBlock("text"));
                page.PerformLayout();
                Assert.AreEqual(page.MaxContentWidth, block.Width, "line length stays readable on wide windows");
            });
        }

        [TestMethod]
        public void StackPageScrollsWhenContentIsTaller()
        {
            Sta.Run(() =>
            {
                using (var form = new Form { ClientSize = new Size(800, 300) })
                {
                    var page = new StackPage { Dock = DockStyle.Fill };
                    form.Controls.Add(page);
                    for (int i = 0; i < 20; i++)
                        page.Add(new SettingRow("Row " + i));
                    form.PerformLayout();
                    IntPtr handle = page.Handle;
                    page.PerformLayout();
                    Assert.IsTrue(page.AutoScrollMinSize.Height > 20 * Theme.S(56));
                    // Regression: the scroll bar was never updated because the base layout did not run.
                    Assert.IsTrue(page.VerticalScroll.Visible, "vertical scroll bar shown");
                    Assert.IsTrue(page.VerticalScroll.Maximum >= page.AutoScrollMinSize.Height - 1);
                }
            });
        }

        [TestMethod]
        public void HStackWrapsAndFills()
        {
            Sta.Run(() =>
            {
                var stack = new HStack();
                var box = new DarkTextBox();
                stack.Fill = box;
                stack.Add(box);
                var add = stack.Add(new FlatButton("Add"));
                var clear = stack.Add(new FlatButton("Clear all"), true);

                stack.SetBounds(0, 0, 800, stack.PreferredHeight(800));
                stack.PerformLayout();
                Assert.AreEqual(800, clear.Right, "right aligned");
                Assert.AreEqual(box.Right + stack.Spacing, add.Left);
                Assert.IsTrue(box.Width > 400, "the fill control takes the remaining width");

                int oneLine = stack.PreferredHeight(800);
                Assert.IsTrue(stack.PreferredHeight(Theme.S(200)) > oneLine, "wraps when too narrow");
            });
        }

        [TestMethod]
        public void TextBlockHeightDependsOnWrapping()
        {
            Sta.Run(() =>
            {
                var t = new TextBlock(string.Join(" ", System.Linq.Enumerable.Repeat("word", 60)));
                Assert.IsTrue(t.PreferredHeight(100) > t.PreferredHeight(5000));
                t.Text = "";
                Assert.AreEqual(0, t.PreferredHeight(500), "empty text takes no space");
            });
        }

        // ------------------------------------------------------------------ log

        [TestMethod]
        public void LogViewKeepsAllTextAndFiltersProblems()
        {
            Sta.Run(() =>
            {
                var log = new LogView();
                log.Append(new List<KeyValuePair<string, LogKind>>
                {
                    new KeyValuePair<string, LogKind>("normal\n", LogKind.Output),
                    new KeyValuePair<string, LogKind>("Warning: w\n", LogKind.Warning),
                    new KeyValuePair<string, LogKind>("failed\n", LogKind.Error),
                    new KeyValuePair<string, LogKind>("gui\n", LogKind.App)
                });
                Assert.AreEqual("normal\nWarning: w\nfailed\ngui\n", log.PlainText);
                StringAssert.Contains(log.Text, "normal");

                log.ErrorsOnly = true;
                Assert.IsFalse(log.Text.Contains("normal"));
                Assert.IsFalse(log.Text.Contains("gui"));
                StringAssert.Contains(log.Text, "Warning: w");
                StringAssert.Contains(log.Text, "failed");
                Assert.AreEqual("normal\nWarning: w\nfailed\ngui\n", log.PlainText, "copy/save still get everything");

                log.ErrorsOnly = false;
                StringAssert.Contains(log.Text, "normal");
                log.ClearLog();
                Assert.AreEqual("", log.PlainText);
                Assert.AreEqual("", log.Text);
            });
        }

        [TestMethod]
        public void LogColors()
        {
            Assert.AreEqual(Theme.Error, LogView.ColorOf(LogKind.Error));
            Assert.AreEqual(Theme.Warning, LogView.ColorOf(LogKind.Warning));
            Assert.AreEqual(Theme.Info, LogView.ColorOf(LogKind.App));
            Assert.AreEqual(Theme.Success, LogView.ColorOf(LogKind.Success));
            Assert.AreEqual(Theme.Text, LogView.ColorOf(LogKind.Output));
        }
    }
}
