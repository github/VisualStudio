
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming", "CA1707:Identifiers should not contain underscores",
    Justification = "Unit test names can contain a _")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design", "CA1034:Nested types should not be visible",
    Justification = "Nested unit test classes should be visible")]
