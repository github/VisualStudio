using System;

namespace GitHub.Exports
{
    /// <summary>
    /// Only expose export when executing in Visual Studio (devenv) process.
    /// </summary>
    /// <remarks>
    /// This attribute is used to mark exports that mustn't be loaded into Blend.
    /// See: https://github.com/github/VisualStudio/pull/1055
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ExportForVisualStudioProcessAttribute : ExportForProcessAttribute
    {
        const string VisualStudioProcessName = "devenv";

        /// <summary>
        /// Define an export that is only exposed in a Visual Studio (devenv) process.
        /// </summary>
        /// <param name="contractType">The contract type to expose.</param>
        public ExportForVisualStudioProcessAttribute(Type contractType = null) :
            base(VisualStudioProcessName, contractType)
        {
        }

        public static bool IsVisualStudioProcess() => IsProcess(VisualStudioProcessName);
    }
}
