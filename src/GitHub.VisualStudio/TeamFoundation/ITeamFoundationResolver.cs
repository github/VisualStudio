using System.Diagnostics.CodeAnalysis;

namespace GitHub.VisualStudio.TeamFoundation
{
    /// <summary>
    /// This interface should be imported by any MEF part that uses types that
    /// reference the `Microsoft.TeamFoundation.*` assemblies.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces",
        Justification = "This is a MEF export used to resolve `Microsoft.TeamFoundation.*` assemblies")]
    public interface ITeamFoundationResolver
    {
    }
}
