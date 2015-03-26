using System;
using Xunit;

namespace GitHub.Tests.Helpers
{
    public class ConditionalFactAttribute : FactAttribute
    {
        public bool RunOnJanky { get; set; }

        public override string Skip
        {
            get
            {
                if (!RunOnJanky && IsJanky())
                {
                    return Message ?? "This test may not be run on Janky";
                }
                return base.Skip;
            }
            set { base.Skip = value; }
        }

        public string Message { get; set; }

        static bool IsJanky()
        {
            return !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JANKY_SHA1"));
        }
    }

    public class ConditionalTheoryAttribute : TheoryAttribute
    {
        public bool RunOnJanky { get; set; }

        public override string Skip
        {
            get
            {
                if (!RunOnJanky && IsJanky())
                {
                    return Message ?? "This test may not be run on Janky";
                }
                return base.Skip;
            }
            set { base.Skip = value; }
        }

        public string Message { get; set; }

        static bool IsJanky()
        {
            return !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JANKY_SHA1"));
        }
    }
}
