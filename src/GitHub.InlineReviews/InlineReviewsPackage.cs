using System;
using System.Runtime.InteropServices;
using GitHub.InlineReviews.Commands;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.InlineReviewsPackageId)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public class InlineReviewsPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();
            PackageResources.Register(this);
        }
    }
}
