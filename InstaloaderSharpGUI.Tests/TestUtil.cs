using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using InstaloaderSharp;
using InstaloaderSharp.Util;
using InstaloaderSharpGUI.Core;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Tests
{
    /// <summary>A temporary directory that is deleted when disposed.</summary>
    internal sealed class TempDirectory : IDisposable
    {
        public readonly string Path;

        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "InstaloaderSharpGUI.Tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string File(string name)
        {
            string file = System.IO.Path.Combine(Path, name);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file));
            return file;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch (Exception)
            {
            }
        }
    }

    /// <summary>
    /// Points %LOCALAPPDATA% (where InstaloaderSharp keeps session files and latest stamps) to a temporary directory,
    /// so tests never see or change the real sessions.
    /// </summary>
    internal sealed class IsolatedAppData : IDisposable
    {
        private readonly string _previous;
        public readonly TempDirectory Directory = new TempDirectory();

        public IsolatedAppData()
        {
            _previous = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            Environment.SetEnvironmentVariable("LOCALAPPDATA", Directory.Path);
        }

        /// <summary>The InstaloaderSharp config directory inside the temporary %LOCALAPPDATA%.</summary>
        public string ConfigDir { get { return System.IO.Path.Combine(Directory.Path, "Instaloader"); } }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("LOCALAPPDATA", _previous);
            Directory.Dispose();
        }
    }

    /// <summary>Installs the GUI's console router for a test and restores the previous console streams afterwards.</summary>
    internal sealed class RouterScope : IDisposable
    {
        private readonly TextWriter _out = Console.Out;
        private readonly TextWriter _err = Console.Error;
        private readonly TextReader _in = Console.In;

        public RouterScope()
        {
            ConsoleRouter.Install();
            // End any partial line left by an earlier test (it would become part of the next prompt).
            Console.Out.Write("\n");
            ConsoleRouter.Drain(int.MaxValue);
            ConsoleRouter.PromptHandler = null;
            ClearInterrupt();
        }

        /// <summary>Everything written since the last call, as one string.</summary>
        public static string Text()
        {
            return string.Concat(ConsoleRouter.Drain(int.MaxValue).Select(c => c.Key));
        }

        public static void ClearInterrupt()
        {
            try
            {
                Interrupt.Check();
            }
            catch (KeyboardInterruptException)
            {
            }
        }

        public void Dispose()
        {
            ConsoleRouter.PromptHandler = null;
            ConsoleRouter.Drain(int.MaxValue);
            ClearInterrupt();
            Console.SetOut(_out);
            Console.SetError(_err);
            Console.SetIn(_in);
        }
    }

    internal static class Sta
    {
        /// <summary>Runs WinForms code on an STA thread (some controls, e.g. RichTextBox, require it).</summary>
        public static void Run(Action action)
        {
            Exception error = null;
            var t = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    error = e;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (error != null)
                throw new TargetInvocationException("Test code on the STA thread failed: " + error.Message, error);
        }
    }

    internal static class UiFind
    {
        public static IEnumerable<Control> All(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (Control d in All(c))
                    yield return d;
            }
        }

        /// <summary>The setting row with the given title.</summary>
        public static SettingRow Row(Control root, string title)
        {
            SettingRow row = All(root).OfType<SettingRow>().FirstOrDefault(r => r.Text == title);
            if (row == null)
                throw new InvalidOperationException("No setting row titled \"" + title + "\".");
            return row;
        }

        public static ToggleSwitch Switch(Control root, string title)
        {
            return (ToggleSwitch)Row(root, title).Accessory;
        }

        /// <summary>Calls a protected method such as OnClick.</summary>
        public static void Invoke(Control c, string method, params object[] args)
        {
            MethodInfo m = c.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            m.Invoke(c, args);
        }

        public static T Field<T>(object o, string name)
        {
            FieldInfo f = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)f.GetValue(o);
        }
    }
}
