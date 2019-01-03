using GitHub.Helpers;

/// <summary>
/// Tests of the <see cref="MentionsAutoCompleteSource"/>. Test implementations are in
/// <see cref="AutoCompleteSourceTests{TAutoCompleteSource, TCacheInterface}"/>
/// </summary>
/// <remarks>
/// THIS CLASS SHOULD ONLY CONTAIN TESTS SPECIFIC TO <see cref="MentionsAutoCompleteSource"/> that deviate from the
/// behavior in common with all <see cref="IAutoCompleteSource"/> implementations.
/// </remarks>
public class MentionsAutoCompleteSourceTests : AutoCompleteSourceTests<MentionsAutoCompleteSource, IMentionsCache>
{
}
