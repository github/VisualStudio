namespace GitHub.Settings
{
    public static class AssemblyVersionInformation
    {
        // HACK: For some reason ThisAssembly.AssemblyFileVersion can't be referenced
        // directly from inside GitHub.VisualStudio.
        public const string Version = ThisAssembly.AssemblyFileVersion;
    }
}
