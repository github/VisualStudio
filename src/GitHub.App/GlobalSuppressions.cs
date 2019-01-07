// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "GitHub.ViewModels.CreateRepoViewModel.#ResetState()")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "Git", Scope = "resource", Target = "GitHub.Resources.resources")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Scope = "member", Target = "GitHub.Caches.CredentialCache.#InsertObject`1(System.String,!!0,System.Nullable`1<System.DateTimeOffset>)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "Git", Scope = "resource", Target = "GitHub.App.Resources.resources")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)", Scope = "member", Target = "GitHub.Services.PullRequestService.#CreateTempFile(System.String,System.String,System.String)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)", Scope = "member", Target = "GitHub.Services.PullRequestService.#CreateTempFile(System.String,System.String,System.String,System.Text.Encoding)")]
[assembly: SuppressMessage("Design", "CA1056:Uri properties should not be strings")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings")]
[assembly: SuppressMessage("Reliability", "CA2007:Do not directly await a Task", Justification = "Discouraged for VSSDK projects.")]
