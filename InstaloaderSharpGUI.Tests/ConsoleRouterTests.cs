using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InstaloaderSharp;
using InstaloaderSharp.Util;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>Capturing the library's console output and answering its console input with GUI prompts.</summary>
    [TestClass]
    public class ConsoleRouterTests
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
        public void OutputIsQueuedAndMerged()
        {
            Console.Out.Write("[ 1/10] ");
            Console.Out.Write("file.jpg");
            Console.Out.WriteLine();
            List<KeyValuePair<string, LogKind>> chunks = ConsoleRouter.Drain();
            Assert.AreEqual(1, chunks.Count, "consecutive output of the same kind is merged");
            Assert.AreEqual("[ 1/10] file.jpg" + Environment.NewLine, chunks[0].Key);
            Assert.AreEqual(LogKind.Output, chunks[0].Value);
            Assert.AreEqual(0, ConsoleRouter.Drain().Count, "drained");
        }

        [TestMethod]
        public void ErrorsAndWarningsAreClassified()
        {
            Console.Out.WriteLine("normal");
            Console.Error.WriteLine("Warning: be careful");
            Console.Error.WriteLine("something failed");
            Console.Error.Write("\nInterrupted by user.\n");
            List<KeyValuePair<string, LogKind>> chunks = ConsoleRouter.Drain();
            CollectionAssert.AreEqual(new[] { LogKind.Output, LogKind.Warning, LogKind.Error }, chunks.Select(c => c.Value).ToArray());
            StringAssert.StartsWith(chunks[1].Key, "Warning: be careful");
            StringAssert.Contains(chunks[2].Key, "something failed");
            StringAssert.Contains(chunks[2].Key, "Interrupted by user.");
        }

        [TestMethod]
        public void AppMessagesAreMarked()
        {
            ConsoleRouter.App("hello");
            ConsoleRouter.App("done", LogKind.Success);
            List<KeyValuePair<string, LogKind>> chunks = ConsoleRouter.Drain();
            Assert.AreEqual("hello" + Environment.NewLine, chunks[0].Key);
            Assert.AreEqual(LogKind.App, chunks[0].Value);
            Assert.AreEqual(LogKind.Success, chunks[1].Value);
        }

        [TestMethod]
        public void DrainRespectsTheLimit()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.Out.Write("o");
                Console.Error.WriteLine("e");
            }
            Assert.AreEqual(3, ConsoleRouter.Drain(3).Count);
            Assert.AreEqual(7, ConsoleRouter.Drain().Count);
        }

        [TestMethod]
        public void CaptureKeepsOutputOutOfTheLog()
        {
            string captured = ConsoleRouter.Capture(() =>
            {
                Console.Out.Write("usage: ...");
                Console.Error.WriteLine("error: bad");
            });
            Assert.AreEqual("usage: ...error: bad" + Environment.NewLine, captured);
            Assert.AreEqual(0, ConsoleRouter.Drain().Count);
        }

        [TestMethod]
        public void NestedCaptureAlsoReachesTheOuterCapture()
        {
            string inner = null;
            string outer = ConsoleRouter.Capture(() =>
            {
                Console.Out.Write("a");
                inner = ConsoleRouter.Capture(() => Console.Out.Write("b"));
                Console.Out.Write("c");
            });
            Assert.AreEqual("b", inner);
            Assert.AreEqual("abc", outer);
        }

        [TestMethod]
        public void CaptureIsPerThread()
        {
            ConsoleRouter.Capture(() =>
            {
                var t = new Thread(() => Console.Out.Write("other thread"));
                t.Start();
                t.Join();
            });
            Assert.AreEqual("other thread", RouterScope.Text(), "a download running meanwhile must still be logged");
        }

        [TestMethod]
        public void ConsoleInputBecomesAPrompt()
        {
            string asked = null;
            ConsoleRouter.PromptHandler = prompt =>
            {
                asked = prompt;
                return "123456";
            };
            // This is what InstaloaderSharp's ConsoleInput.Input does.
            Assert.AreEqual("123456", ConsoleInput.Input("Enter 2FA verification code: "));
            Assert.AreEqual("Enter 2FA verification code:", asked, "the text printed before reading is the prompt");
        }

        [TestMethod]
        public void PromptWithoutPrintedTextHasADefault()
        {
            string asked = null;
            ConsoleRouter.PromptHandler = prompt => { asked = prompt; return "x"; };
            Console.Out.WriteLine("previous line");
            Assert.AreEqual("x", Console.In.ReadLine());
            Assert.AreEqual("InstaloaderSharp is asking for input:", asked);
        }

        [TestMethod]
        public void CancellingAPromptActsLikeCtrlC()
        {
            ConsoleRouter.PromptHandler = prompt => null;
            try
            {
                ConsoleInput.Input("Enter code: ");
                Assert.Fail("expected KeyboardInterruptException");
            }
            catch (KeyboardInterruptException)
            {
            }
            Assert.IsFalse(Interrupt.IsPending, "the interrupt was consumed by the library");
        }

        [TestMethod]
        public void NoHandlerCancels()
        {
            ConsoleRouter.PromptHandler = null;
            Assert.AreEqual("", Console.In.ReadLine());
            Assert.IsTrue(Interrupt.IsPending);
        }

        [TestMethod]
        public void LibraryLogGoesThroughTheRouter()
        {
            using (Instaloader l = new Instaloader())
            {
                l.Context.Log("from the library");
                l.Context.Error("Warning: from the library");
            }
            List<KeyValuePair<string, LogKind>> chunks = ConsoleRouter.Drain();
            Assert.IsTrue(chunks.Any(c => c.Value == LogKind.Output && c.Key.Contains("from the library")));
            Assert.IsTrue(chunks.Any(c => c.Value == LogKind.Warning && c.Key.Contains("Warning: from the library")));
        }
    }
}
