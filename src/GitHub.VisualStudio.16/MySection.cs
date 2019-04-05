using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GitHub.Models;
using GitHub.Services;
using Microsoft;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.GitCommits, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MySection : TeamExplorerSectionBase
    {
        public const string SectionId = "DA9CEE5D-B433-4FB6-AC35-5F8FEE2747A7";

        public MySection()
        {
        }

        protected override object CreateView(SectionInitializeEventArgs e)
        {
            Assembly.Load("GitHub.UI");
            Assembly.Load("GitHub.VisualStudio.UI");
            Assembly.Load("GitHub.UI.Reactive");

            var componentModel = ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(componentModel);

            var compositionServices = new CompositionServices();
            var compositionContainer = compositionServices.CreateCompositionContainer(componentModel.DefaultExportProvider);
            compositionContainer.ComposeExportedValue<ITeamExplorerContext>(new MyTeamExplorerContext());

            var section = CreateTeamExplorerSection(compositionContainer, TeamExplorer.Sync.GitHubPublishSection.GitHubPublishSectionId);
            return section.SectionContent;
        }

        //protected override ITeamExplorerSection CreateViewModel(SectionInitializeEventArgs e)
        //{
        //    return new MySectionViewModel { IsVisible = true };
        //}

        static ITeamExplorerSection CreateTeamExplorerSection(CompositionContainer compositionContainer, string sectionId)
        {
            var exports = compositionContainer.GetExports<ITeamExplorerSection, IDictionary<string, object>>();
            var export = exports
                .Where(e => e.Metadata[nameof(TeamExplorerSectionAttribute.Id)] as string == sectionId)
                .First();
            var section = export.Value;
            return section;
        }

        class MyTeamExplorerContext : ITeamExplorerContext
        {
            public LocalRepositoryModel ActiveRepository => throw new NotImplementedException();

            public event EventHandler StatusChanged;
            public event PropertyChangedEventHandler PropertyChanged;
        }
    }

    //internal class MySectionViewModel : TeamExplorerSectionViewModelBase
    //{
    //}
}
