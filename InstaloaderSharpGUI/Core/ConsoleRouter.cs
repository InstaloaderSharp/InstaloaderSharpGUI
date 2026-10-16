using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InstaloaderSharp.Util;
using InstaloaderSharpGUI.UI;

namespace InstaloaderSharpGUI.Core
{
    /// <summary>
    /// InstaloaderSharp reports progress through Console.Out / Console.Error and asks questions through Console.In.
    /// The router replaces those streams once at start-up: output is queued for the GUI (drained by a UI timer),
    /// and reads from Console.In are turned into GUI prompts.
    /// </summary>
    internal static class ConsoleRouter
    {
        private static readonly ConcurrentQueue<KeyValuePair<string, LogKind>> Queue = new ConcurrentQueue<KeyValuePair<string, LogKind>>();
        private static readonly object Lock = new object();
        private static readonly StringBuilder LastOutLine = new StringBuilder();

        [ThreadStatic]
        private static StringBuilder _capture;

        /// <summary>Called (on the reading thread) when the library wants a line of input. Return null to cancel.</summary>
        public static Func<string, string> PromptHandler;

        public static void Install()
        {
            Console.SetOut(new RouterWriter(false));
            Console.SetError(new RouterWriter(true));
            Console.SetIn(new PromptReader());
        }

        /// <summary>Messages written by the GUI itself.</summary>
        public static void App(string message, LogKind kind = LogKind.App)
        {
            Enqueue(message + Environment.NewLine, kind);
        }

        /// <summary>Takes everything written since the last call.</summary>
        public static List<KeyValuePair<string, LogKind>> Drain(int max = 5000)
        {
            var list = new List<KeyValuePair<string, LogKind>>();
            KeyValuePair<string, LogKind> item;
            while (list.Count < max && Queue.TryDequeue(out item))
            {
                // Merge consecutive chunks of the same kind to keep the RichTextBox fast.
                if (list.Count > 0 && list[list.Count - 1].Value == item.Value)
                    list[list.Count - 1] = new KeyValuePair<string, LogKind>(list[list.Count - 1].Key + item.Key, item.Value);
                else
                    list.Add(item);
            }
            return list;
        }

        /// <summary>Runs <paramref name="action"/> and returns what it wrote to the console instead of logging it.</summary>
        public static string Capture(Action action)
        {
            StringBuilder previous = _capture;
            _capture = new StringBuilder();
            try
            {
                action();
                return _capture.ToString();
            }
            finally
            {
                string text = _capture.ToString();
                _capture = previous;
                if (previous != null)
                    previous.Append(text);
            }
        }

        private static void Enqueue(string text, LogKind kind)
        {
            if (string.IsNullOrEmpty(text))
                return;
            if (_capture != null)
            {
                _capture.Append(text);
                return;
            }
            if (kind == LogKind.Output)
            {
                lock (Lock)
                {
                    int nl = text.LastIndexOf('\n');
                    if (nl >= 0)
                    {
                        LastOutLine.Clear();
                        LastOutLine.Append(text, nl + 1, text.Length - nl - 1);
                    }
                    else
                    {
                        LastOutLine.Append(text);
                    }
                }
            }
            Queue.Enqueue(new KeyValuePair<string, LogKind>(text, kind));
        }

        private static string TakePrompt()
        {
            lock (Lock)
            {
                string p = LastOutLine.ToString();
                LastOutLine.Clear();
                return p;
            }
        }

        private sealed class RouterWriter : TextWriter
        {
            private readonly bool _isError;
            private bool _atLineStart = true;
            private LogKind _lineKind = LogKind.Output;

            public RouterWriter(bool isError)
            {
                _isError = isError;
            }

            public override Encoding Encoding { get { return Encoding.UTF8; } }

            public override void Write(char value)
            {
                Write(value.ToString());
            }

            public override void Write(char[] buffer, int index, int count)
            {
                Write(new string(buffer, index, count));
            }

            public override void Write(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return;
                LogKind kind = LogKind.Output;
                if (_isError)
                {
                    lock (this)
                    {
                        if (_atLineStart)
                        {
                            string t = value.TrimStart('\r', '\n');
                            _lineKind = t.StartsWith("Warning", StringComparison.OrdinalIgnoreCase) ? LogKind.Warning : LogKind.Error;
                        }
                        kind = _lineKind;
                        _atLineStart = value.EndsWith("\n");
                    }
                }
                Enqueue(value, kind);
            }

            public override void WriteLine(string value)
            {
                Write((value ?? "") + Environment.NewLine);
            }
        }

        /// <summary>Console.In replacement: each ReadLine shows a dialog with the last line printed as prompt.</summary>
        private sealed class PromptReader : TextReader
        {
            public override string ReadLine()
            {
                string prompt = TakePrompt().Trim();
                Func<string, string> handler = PromptHandler;
                string answer = handler != null ? handler(prompt.Length > 0 ? prompt : "InstaloaderSharp is asking for input:") : null;
                if (answer == null)
                {
                    // Behave like ^C at the console: ConsoleInput checks for a pending interrupt after reading.
                    Interrupt.Raise();
                    return "";
                }
                return answer;
            }

            public override int Read()
            {
                return -1;
            }

            public override int Peek()
            {
                return -1;
            }
        }
    }
}
