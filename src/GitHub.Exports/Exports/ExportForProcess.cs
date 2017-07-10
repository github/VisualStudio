using System;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace GitHub.Exports
{
    /// <summary>
    /// Only expose export when executing in specific named process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ExportForProcessAttribute : ExportAttribute
    {
        /// <summary>
        /// Define an export that is only exposed in a specific named process.
        /// </summary>
        /// <param name="contractType">The contract type to expose.</param>
        /// <param name="processName">Name of the process to expose export from (e.g. 'devenv').</param>
        public ExportForProcessAttribute(Type contractType, string processName) : base(ExportForProcess(contractType, processName))
        {
            ProcessName = processName;
        }

        static Type ExportForProcess(Type contractType, string processName)
        {
            return Process.GetCurrentProcess().ProcessName == processName ? contractType : null;
        }

        /// <summary>
        /// The process name export will be exposed in.
        /// </summary>
        public string ProcessName { get; }
    }
}
