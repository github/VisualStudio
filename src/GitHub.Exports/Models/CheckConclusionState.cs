namespace GitHub.Models
{
    public enum CheckConclusionState
    {
        ActionRequired,
        TimedOut,
        Cancelled,
        Failure,
        Success,
        Neutral,
    }
}