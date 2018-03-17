using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSServices
    {
        string VSVersion { get; }
        bool TryOpenRepository(string directory);
    }
}