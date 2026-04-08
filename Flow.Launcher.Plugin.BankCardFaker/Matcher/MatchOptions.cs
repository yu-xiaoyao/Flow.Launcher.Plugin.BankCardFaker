namespace Flow.Launcher.Plugin.BankCardFaker.Matcher;

public class MatchOptions
{
    public static MatchOptions Default = new();

    /// <summary>
    /// 全词匹配
    /// </summary>
    public bool FullEqual { get; set; }

    /// <summary>
    /// 启用拼音
    /// </summary>
    public bool PinyinSearch { get; set; } = true;

    /// <summary>
    /// 启用全拼音
    /// </summary>
    public bool FullPinyin { get; set; }
}