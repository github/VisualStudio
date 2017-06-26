using System;

namespace GitHub.Models
{
    public enum DiffChangeType
    {
        None,
        Add,
        Delete,
        Control // Git uses this for "\ No newline at end of file".
    }
}
