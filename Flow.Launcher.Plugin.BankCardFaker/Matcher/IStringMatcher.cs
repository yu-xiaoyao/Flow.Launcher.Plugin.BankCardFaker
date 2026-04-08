namespace Flow.Launcher.Plugin.BankCardFaker.Matcher;

public interface IStringMatcher
{
    public bool Match(string query, string stringToCompare, MatchOptions options);
}