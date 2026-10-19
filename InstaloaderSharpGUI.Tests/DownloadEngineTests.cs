using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using InstaloaderSharpGUI.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExitCode = InstaloaderSharp.Program.ExitCode;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>
    /// Running downloads through the library. Only cases that end before any request to Instagram are tested here;
    /// see CONTRIBUTING.md for testing against Instagram.
    /// </summary>
    [TestClass]
    public class DownloadEngineTests
    {
        private RouterScope _router;
        private IsolatedAppData _appData;
        private TempDirectory _output;

        [TestInitialize]
        public void Init()
        {
            _router = new RouterScope();
            _appData = new IsolatedAppData();
            _output = new TempDirectory();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _output.Dispose();
            _appData.Dispose();
            _router.Dispose();
        }

        private JobOptions Options(params string[] targets)
        {
            var o = new JobOptions { OutputDir = Path.Combine(_output.Path, "downloads") };
            o.Targets.AddRange(targets);
            return o;
        }

        private static JobFinishedEventArgs Run(DownloadEngine engine, JobOptions o, string password = null)
        {
            JobFinishedEventArgs result = null;
            using (var done = new ManualResetEvent(false))
            {
                EventHandler<JobFinishedEventArgs> handler = (s, e) =>
                {
                    result = e;
                    done.Set();
                };
                engine.Finished += handler;
                try
                {
                    engine.Start(o, password);
                    Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(60)), "the download did not finish");
                }
                finally
                {
                    engine.Finished -= handler;
                }
            }
            // Finished is raised after the worker's last step; give the thread a moment to end.
            for (int i = 0; i < 100 && engine.IsRunning; i++)
                Thread.Sleep(20);
            Assert.IsFalse(engine.IsRunning);
            return result;
        }

        [TestMethod]
        public void InvalidTargetFailsWithoutStopping()
        {
            string previousDir = Environment.CurrentDirectory;
            JobOptions o = Options("bad target!");
            JobFinishedEventArgs e = Run(new DownloadEngine(), o);

            Assert.AreEqual(ExitCode.NonFatalError, e.ExitCode, "like the console: the error is reported, the run completes");
            Assert.AreEqual(previousDir, Environment.CurrentDirectory, "working directory is restored");
            Assert.IsTrue(Directory.Exists(o.OutputDir), "download folder is created");
            string log = RouterScope.Text();
            StringAssert.Contains(log, "Starting download into " + o.OutputDir);
            StringAssert.Contains(log, "Equivalent command: InstaloaderSharp \"bad target!\"");
            StringAssert.Contains(log, "Invalid username bad target!");
            StringAssert.Contains(log, "Errors or warnings occurred:");
        }

        [TestMethod]
        public void OptionsAreCopiedWhenStarting()
        {
            JobOptions o = Options("bad target!");
            var engine = new DownloadEngine();
            JobFinishedEventArgs result = null;
            var done = new ManualResetEvent(false);
            engine.Finished += (s, e) => { result = e; done.Set(); };
            engine.Start(o, null);
            // Changing the options while a download runs only affects the next download.
            o.Targets.Clear();
            o.Targets.Add(":feed-all");
            Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(60)));
            Assert.AreEqual(ExitCode.NonFatalError, result.ExitCode);
        }

        [TestMethod]
        public void ContradictingOptionsAreAnInitFailure()
        {
            JobOptions o = Options("natgeo");
            o.Pictures = false;
            o.FastUpdate = true;
            JobFinishedEventArgs e = Run(new DownloadEngine(), o);
            Assert.AreEqual(ExitCode.InitFailure, e.ExitCode);
            Assert.AreEqual("--no-pictures and --fast-update cannot be used together.", e.Message);
        }

        [TestMethod]
        public void RemovedTargetsAreAnInitFailure()
        {
            JobFinishedEventArgs e = Run(new DownloadEngine(), Options(":feed-liked"));
            Assert.AreEqual(ExitCode.InitFailure, e.ExitCode);
            StringAssert.Contains(e.Message, ":feed-all and :feed-liked were removed");
        }

        [TestMethod]
        public void InvalidCountIsAnInitFailure()
        {
            JobOptions o = Options("natgeo");
            o.Count = "many";
            Assert.AreEqual(ExitCode.InitFailure, Run(new DownloadEngine(), o).ExitCode);
        }

        [TestMethod]
        public void InvalidSlideIsAnInitFailure()
        {
            JobOptions o = Options("natgeo");
            o.Slide = "first";
            JobFinishedEventArgs e = Run(new DownloadEngine(), o);
            Assert.AreEqual(ExitCode.InitFailure, e.ExitCode);
            StringAssert.Contains(e.Message, "--slide");
        }

        [TestMethod]
        public void StoriesAreSkippedWithoutLogin()
        {
            JobOptions o = Options("bad target!");
            o.Stories = true;
            Run(new DownloadEngine(), o);
            StringAssert.Contains(RouterScope.Text(), "Login is required to download stories.");
        }

        [TestMethod]
        public void PasswordIsAskedOnTheUiAndCancellingIsALoginFailure()
        {
            JobOptions o = Options("natgeo");
            o.Mode = LoginMode.Login;
            o.Username = "someone";
            var prompts = new List<KeyValuePair<string, bool>>();
            var engine = new DownloadEngine
            {
                Prompt = (message, password) =>
                {
                    prompts.Add(new KeyValuePair<string, bool>(message, password));
                    return null;
                }
            };
            JobFinishedEventArgs e = Run(engine, o);
            Assert.AreEqual(ExitCode.LoginFailure, e.ExitCode);
            Assert.AreEqual("Login cancelled.", e.Message);
            Assert.AreEqual(1, prompts.Count);
            Assert.IsTrue(prompts[0].Value, "asked as a password");
            StringAssert.Contains(prompts[0].Key, "someone");
        }

        [TestMethod]
        public void PromptOnUiWithoutHandlerCancels()
        {
            Assert.IsNull(new DownloadEngine().PromptOnUi("Password?", true));
        }

        [TestMethod]
        public void StopWhenIdleDoesNothing()
        {
            var engine = new DownloadEngine();
            engine.Stop();
            Assert.IsFalse(engine.IsRunning);
            Assert.IsFalse(engine.StopRequested);
        }

        [TestMethod]
        public void EngineCanRunAgain()
        {
            var engine = new DownloadEngine();
            Assert.AreEqual(ExitCode.NonFatalError, Run(engine, Options("bad target!")).ExitCode);
            Assert.AreEqual(ExitCode.InitFailure, Run(engine, Options(":feed-all")).ExitCode);
        }
    }
}
