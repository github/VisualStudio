using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.VisualStudio.Helpers;
using NullGuard;
using Octokit;

namespace GitHub.VisualStudio.Base
{
    public abstract class TeamExplorerItemBase : TeamExplorerBase, INotifyPropertySource
    {
        bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; this.RaisePropertyChange(); }
        }

        bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; this.RaisePropertyChange(); }
        }

        string text;
        [AllowNull]
        public string Text
        {
            get { return text; }
            set { text = value; this.RaisePropertyChange(); }
        }

        public virtual void Execute()
        {
        }

        public virtual void Invalidate()
        {
        }
    }

    [Export(typeof(IGitHubClient))]
    public class GHClient : GitHubClient
    {
        [ImportingConstructor]
        public GHClient(IProgram program)
            : base(program.ProductHeader)
        {

        }
    }
}