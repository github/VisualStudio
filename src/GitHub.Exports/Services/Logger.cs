using GitHub.Info;
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
        static readonly string defaultLogPath;
        static readonly object fileLock = new object();

        static Action<string> Logger
        {
            get { return logger.Value; }
        }

        static VsOutputLogger()
        {
            //Debug.Listeners.Add(new VSTraceListener());
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationInfo.ApplicationName);
            defaultLogPath = Path.Combine(dir, ApplicationInfo.ApplicationName + "-te.log");
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch { }
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

        public static void WriteLine(string format, params object[] args)
        {
            var message = string.Format(CultureInfo.CurrentCulture, format, args);
            WriteLine(message);
        }

        public static void WriteLine(string message)
        {
            Logger(message + Environment.NewLine);
        }

        static async void DefaultLogger(string msg)
        {
            // this codepath is called multiple times on loading
            // doing a little delay so that calling code doesn't get slowed down by this
            await System.Threading.Tasks.Task.Delay(500);
            lock(fileLock)
            {
                // TODO: Fix this properly
                try
                {
                    File.AppendAllText(defaultLogPath, msg, Encoding.UTF8);
                }
                catch(Exception)
                {
                }
            }
        }
    }
}
