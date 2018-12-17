// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project. Project-level
// suppressions either have no target or are given a specific target
// and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Error List, point to "Suppress Message(s)", and click "In Project
// Suppression File". You do not need to add suppressions to this
// file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "GitHub", Scope = "resource", Target = "VSPackage.resources")]
[assembly: SuppressMessage("Reliability", "CA2007:Do not directly await a Task", Justification = "Discouraged for VSSDK projects.")]
[assembly: SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
