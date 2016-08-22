using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSServices
    {
        void ActivityLogMessage(string message);
        void ActivityLogWarning(string message);
        void ActivityLogError(string message);
    }
}