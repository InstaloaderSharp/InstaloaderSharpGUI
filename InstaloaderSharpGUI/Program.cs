using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace InstaloaderSharpGUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // InstaloaderSharp.exe is referenced as a library and must sit next to InstaloaderSharpGUI.exe.
            string library = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InstaloaderSharp.exe");
            if (!File.Exists(library))
            {
                MessageBox.Show("InstaloaderSharp.exe was not found.\n\nPlace InstaloaderSharpGUI.exe in the same folder as InstaloaderSharp.exe:\n" +
                                AppDomain.CurrentDomain.BaseDirectory,
                                "InstaloaderSharp GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowCrash(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowCrash(e.ExceptionObject as Exception);
            Run();
        }

        // Kept separate so that a missing InstaloaderSharp.exe is reported above instead of failing to JIT Main.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Run()
        {
            Core.ConsoleRouter.Install();
            Application.Run(new MainForm());
        }

        private static int _crashShown;

        private static void ShowCrash(Exception e)
        {
            if (e == null || Interlocked.Exchange(ref _crashShown, 1) == 1)
                return;
            try
            {
                Console.Error.WriteLine("Unexpected error: " + e);
                UI.MessageDialog.Show(null, e.Message + "\n\n" + e.GetType().FullName + "\n" + e.StackTrace,
                                      "Something went wrong", UI.DialogIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(e.ToString(), "InstaloaderSharp GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Interlocked.Exchange(ref _crashShown, 0);
            }
        }
    }
}
