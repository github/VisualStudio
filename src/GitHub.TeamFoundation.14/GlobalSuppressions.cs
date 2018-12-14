// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings")]
[assembly: SuppressMessage("Design", "CA1056:Uri properties should not be strings")]
[assembly: SuppressMessage("Reliability", "CA2007:Do not directly await a Task", Justification = "Discouraged for VSSDK projects.")]
