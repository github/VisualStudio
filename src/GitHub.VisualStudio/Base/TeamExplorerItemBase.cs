using NullGuard;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

namespace GitHub.VisualStudio.Exports
{
    [Export(typeof(Octokit.IGitHubClient))]
    public class GHClient : Octokit.GitHubClient
    {
        [ImportingConstructor]
        public GHClient(Models.IProgram program)
            : base(program.ProductHeader)
        {

        }
    }
}
