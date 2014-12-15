using Splat;

namespace GitHub.Infrastructure
{
    public class AppModeDetector : IModeDetector
    {
        public bool? InUnitTestRunner()
        {
            return false;
        }

        public bool? InDesignMode()
        {
            return false;
        }
    }
}
