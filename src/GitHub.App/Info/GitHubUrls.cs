using System;
using System.Globalization;
using GitHub.Models;

namespace GitHub.Info
{
    public static class GitHubUrls
    {
        /// <summary>
        /// https://github.com/
        /// </summary>
        public const string GitHub = "https://github.com/";

        /// <summary>
        /// The url for a user's dashboard on github.com.
        /// </summary>
        public const string Dashboard = GitHub + "/dashboard";

        /// <summary>
        /// The url for contacting support
        /// </summary>
        public const string ContactSupport = GitHub + "/contact";

        /// <summary>
        /// The GitHub for Windows download and promo site
        /// </summary>
        public const string GitHubForWindows = "http://windows.github.com";

        /// <summary>
        /// The url for a user's billing information.
        /// </summary>
        public const string UserBilling = GitHub + "/account/billing";

        /// <summary>
        /// The URL for the Enterprise learn more page.
        /// </summary>
        public static readonly Uri LearnMore = new Uri("https://enterprise.github.com/");

        /// <summary>
        /// Url for Plans and Pricing
        /// </summary>
        public static readonly Uri Pricing = new Uri(GitHub + "/pricing?referral_code=GitHubExtensionForVisualStudio");

        /// <summary>
        /// The url for viewing signup information and plans on github.com.
        /// This includes a specific referral_code so we can track people
        /// coming to the site from the Windows App.
        /// </summary>
        public static readonly Uri Plans = new Uri(GitHub + "/plans?referral_code=GitHubWindows");

        /// <summary>
        /// The url for resetting your password on github
        /// </summary>
        public static readonly Uri ForgotPasswordPath = new Uri("/sessions/forgot_password", UriKind.Relative);

        /// <summary>
        /// The url to learn about GitHub:Enterprise
        /// </summary>
        /// <remarks>
        /// This doesn't change per enterprise repo.
        /// </remarks>
        public static readonly Uri GitHubEnterpriseWeb = new Uri("https://enterprise.github.com");

        /// <summary>
        /// The URL to learn more about two-factor authentication.
        /// </summary>
        public static readonly Uri TwoFactorLearnMore = new Uri("https://help.github.com/articles/about-two-factor-authentication");

        /// <summary>
        /// The GitHub maintained repo of common .gitignores.
        /// </summary>
        /// <remarks>
        /// This doesn't change per enterprise repo.
        /// </remarks>
        public const string GitIgnoreRepo = "https://github.com/github/gitignore";

        /// <summary>
        /// GitHub Help link about .gitattributes and line endings.
        /// </summary>
        public const string GitAttributesHelp = "https://help.github.com/articles/dealing-with-line-endings";

        /// <summary>
        /// The release notes page on the GitHub for Windows marketing site.
        /// </summary>
        public const string WindowsChangeLog = "https://windows.github.com/release-notes.html";

        /// <summary>
        /// The url for viewing billing information associated with a GitHub account.
        /// </summary>
        public static string Billing(this IAccount account)
        {
            return account.IsUser
                ? UserBilling
                : string.Format(CultureInfo.InvariantCulture,
                    GitHub + "/organizations/{0}/settings/billing", account.Login);
        }

        /// <summary>
        /// The URL for the GitHub for Visual Studio documentation.
        /// </summary>
        public const string Documentation = "https://github.com/github/VisualStudio/tree/master/docs";
    }
}
