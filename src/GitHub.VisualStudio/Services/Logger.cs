using System;
using System.Diagnostics;
using System.Globalization;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

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
        static Lazy<Action<string>> logger = new Lazy<Action<string>>(() => GetWindow().OutputString);

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

        static OutputWindowPane GetWindow()
        {
            var dte = Services.Dte2;
            return dte.ToolWindows.OutputWindow.ActivePane;
        }

        public static void LogToGeneralOutput(string msg)
        {
            Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
            IVsOutputWindowPane generalPane;

            if (Services.OutputWindow == null)
                return;

            int hr = Services.OutputWindow.GetPane(ref generalPaneGuid, out generalPane);
            if (ErrorHandler.Failed(hr))
            {
                hr = Services.OutputWindow.CreatePane(ref generalPaneGuid, "Log", 1, 0);
                if (ErrorHandler.Succeeded(hr))
                    hr = Services.OutputWindow.GetPane(ref generalPaneGuid, out generalPane);
            }

            Debug.Assert(ErrorHandler.Succeeded(hr), "Failed to get log window");

            if (ErrorHandler.Succeeded(hr))
            {
                hr = generalPane.OutputString(msg);
                Debug.Assert(ErrorHandler.Succeeded(hr), "Failed to print to log window");
            }

            if (ErrorHandler.Succeeded(hr))
            {
                hr = generalPane.Activate(); // Brings this pane into view
                Debug.Assert(ErrorHandler.Succeeded(hr), "Failed to activate log window");
            }
        }


    }
}
