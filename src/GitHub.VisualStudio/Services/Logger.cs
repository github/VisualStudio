using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace GitHub.VisualStudio
{
    public class VSTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            VsOutputLogger.Write(message);
        }

        public override void WriteLine(string message)
        {
            VsOutputLogger.WriteLine(message);
        }
    }

    public static class VsOutputLogger
    {
        static Lazy<Action<string>> logger = new Lazy<Action<string>>(() => DefaultLogger);

        static Action<string> Logger
        {
            get { return logger.Value; }
        }

        static VsOutputLogger()
        {
            Debug.Listeners.Add(new VSTraceListener());
        }

        public static void SetLogger(Action<string> log)
        {
            logger = new Lazy<Action<string>>(() => log);
        }

        public static void Write(string format, params object[] args)
        {
            var message = string.Format(CultureInfo.CurrentCulture, format, args);
            Write(message);
        }

        public static void Write(string message)
        {
            Logger(message);
        }

        public static void WriteLine(string message)
        {
            Logger(message + Environment.NewLine);
        }

        const string defaultLogPath = @"%AppData%\GitHubVisualStudio\vs.log";
        static object filelock = new object();
        static void DefaultLogger(string log)
        {
            var path = Environment.ExpandEnvironmentVariables(defaultLogPath);
            var dir = Path.GetDirectoryName(path);
            lock (filelock)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.AppendAllText(path, log, Encoding.UTF8);
            }
        }
    }
}
