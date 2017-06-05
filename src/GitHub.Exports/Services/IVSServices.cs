using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSServices
    {
        string VSVersion { get; }

        void ActivityLogMessage(string message);
        void ActivityLogWarning(string message);
        void ActivityLogError(string message);
        bool TryOpenRepository(string directory, bool logErrors = true);
    }
}