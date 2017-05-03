using System;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.TeamFoundation
{
    class TeamFoundationVersion
    {
        internal static int Major => Version.Major;
        static Version Version => typeof(ITeamExplorer).Assembly.GetName().Version;
    }
}
