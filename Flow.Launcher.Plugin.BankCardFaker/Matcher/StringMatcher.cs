using System;

namespace Flow.Launcher.Plugin.BankCardFaker.Matcher;

public class StringMatcher : IStringMatcher
{
    private readonly IPublicAPI _publicApi;

    public StringMatcher(IPublicAPI publicApi)
    {
        _publicApi = _publicApi;
    }

    public bool Match(string query, string stringToCompare, MatchOptions options)
    {
        if (options.FullEqual)
            return string.Equals(query, stringToCompare, StringComparison.OrdinalIgnoreCase);

        if (_publicApi == null) return false;
        
        var m = _publicApi.FuzzySearch(query, stringToCompare);
        return m.Success;
    }
}