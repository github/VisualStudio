using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHub.Services
{
    /// <summary>
    /// Describes the login methods supported by an enterprise instance.
    /// </summary>
    [Flags]
    public enum EnterpriseLoginMethods
    {
        Token = 0x01,
        UsernameAndPassword = 0x02,
        OAuth = 0x04,
    }

    /// <summary>
    /// Services for checking the capabilities of enterprise installations.
    /// </summary>
    public interface IEnterpriseCapabilitiesService
    {
        /// <summary>
        /// Makes a request to the specified URL and returns whether or not the probe could definitively determine that a GitHub
        /// Enterprise Instance exists at the specified URL.
        /// </summary>
        /// <remarks>
        /// The probe checks the absolute path /site/sha at the specified <paramref name="enterpriseBaseUrl" />.
        /// </remarks>
        /// <param name="enterpriseBaseUrl">The URL to test</param>
        /// <returns>An <see cref="EnterpriseProbeResult" /> with either <see cref="EnterpriseProbeResult.Ok"/>,
        /// <see cref="EnterpriseProbeResult.NotFound"/>, or <see cref="EnterpriseProbeResult.Failed"/> in the case the request failed</returns>
        Task<EnterpriseProbeResult> Probe(Uri enterpriseBaseUrl);

        /// <summary>
        /// OtherChecks the login methods supported by an enterprise instance.
        /// </summary>
        /// <param name="enterpriseBaseUrl">The URL to test.</param>
        /// <returns>The supported login methods.</returns>
        Task<EnterpriseLoginMethods> ProbeLoginMethods(Uri enterpriseBaseUrl);
    }
}
