using System;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace GitHub.Exports
{
    /// <summary>
    /// Only expose export when executing in specific named process.
    /// </summary>
    /// <remarks>
    /// This attribute is used to mark exports that mustn't be loaded into Blend.
    /// See: https://github.com/github/VisualStudio/pull/1055
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ExportForProcessAttribute : ExportAttribute
    {
        // Unique name for exports that have been disabled
        const string DisabledContractName = "GitHub.Disabled";

        /// <summary>
        /// Define an export that is only exposed in a specific named process.
        /// </summary>
        /// <param name="contractType">The contract type to expose.</param>
        /// <param name="processName">Name of the process to expose export from (e.g. 'devenv').</param>
        public ExportForProcessAttribute(Type contractType, string processName) :
            base(ContractNameForProcess(contractType, processName), contractType)
        {
            ProcessName = processName;
        }

        static string ContractNameForProcess(Type contractType, string processName)
        {
            var enabled = Process.GetCurrentProcess().ProcessName == processName;
            return enabled ? null : DisabledContractName;
        }

        /// <summary>
        /// The process name export will be exposed in.
        /// </summary>
        public string ProcessName { get; }
    }
}
