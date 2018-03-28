using System;
using System.Threading.Tasks;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.Services.Vssdk.Commands;
using System.ComponentModel.Composition;
using EnvDTE;
using GitHub.Settings;

namespace GitHub.InlineReviews.Commands
{
    [Export(typeof(IEnableInlineCommentsCommand))]
    public class EnableInlineCommentsCommand : VsCommand, IEnableInlineCommentsCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.EnableInlineCommentsId;

        readonly IGitHubServiceProvider gitHubServiceProvider;
        readonly IPullRequestEditorService pullRequestEditorService;
        readonly IPackageSettings packageSettings;

        [ImportingConstructor]
        public EnableInlineCommentsCommand(IGitHubServiceProvider gitHubServiceProvider, IPullRequestEditorService pullRequestEditorService,
            IPackageSettings packageSettings)
            : base(CommandSet, CommandId)
        {
            this.pullRequestEditorService = pullRequestEditorService;
            this.gitHubServiceProvider = gitHubServiceProvider;
            this.packageSettings = packageSettings;
        }

        public override Task Execute()
        {
            var view = pullRequestEditorService.FindActiveView();
            string file = FindActiveFile();

            using (EnableEditorComments())
            {
                pullRequestEditorService.NavigateToEquivalentPosition(view, file);
            }

            return Task.CompletedTask;
        }

        string FindActiveFile()
        {
            var dte = gitHubServiceProvider.GetService<DTE>();
            var file = dte.ActiveDocument.FullName;
            return file;
        }

        IDisposable EnableEditorComments() => new EnableEditorCommentsContext(packageSettings);

        // Editor comments will be enabled when a file is opened from inside this context.
        class EnableEditorCommentsContext : IDisposable
        {
            readonly IPackageSettings settings;
            bool editorComments;

            internal EnableEditorCommentsContext(IPackageSettings settings)
            {
                this.settings = settings;

                editorComments = settings.EditorComments;
                settings.EditorComments = true;
            }

            public void Dispose()
            {
                settings.EditorComments = editorComments;
            }
        }
    }
}
