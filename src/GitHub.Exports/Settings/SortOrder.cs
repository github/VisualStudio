namespace GitHub.Settings
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Scope = "type", Target = "GitHub.Settings.SortOrder",
        Justification = "Types get lost on json serialization, and they get read back as long (sigh)")]
    public enum SortOrder : long
    {
        Unspecified,
        UpdatedDescending,
        CreatedDescending,
        UpdatedAscending,
        CreatedAscending,
    }
}