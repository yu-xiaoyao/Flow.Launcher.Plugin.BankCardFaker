using System;
using System.Linq;
using Flow.Launcher.Plugin.BankCardFaker.Logger;
using Flow.Launcher.Plugin.BankCardFaker.Matcher;

namespace Flow.Launcher.Plugin.BankCardFaker;

public class Main_Test
{
    public static void Main()
    {
        // Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
        // check_code_dup();

        check_pinyin();
    }

    private static void check_pinyin()
    {
        InnerLogger.SetAsConsoleLogger(LoggerLevel.TRACE);

        var version = "app-2.1.1";
        var appData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        var sm = StringMatcherHelper.NewStringMatcher(null, $@"{appData}\FlowLauncher\{version}\");

        var m = sm.Match("WD", "我的主要Github", new MatchOptions());
        Console.WriteLine(m);
    }

    public static void check_code_dup()
    {
        var config = BcBuilder.BankConfig;
        var duplicates = config
            .GroupBy(c => c.Bin)
            .Where(g => g.Count() > 1)
            .Select(g => (g.Key, Count: g.Count(), Infos: g.Select(c => $"{c.BankName}-{c.CardName}").ToArray()))
            .ToArray();

        if (duplicates.Length == 0)
        {
            Console.WriteLine("No duplicate Bin codes found.");
            return;
        }

        Console.WriteLine($"Found {duplicates.Length} duplicate Bin code(s):");
        foreach (var dup in duplicates)
        {
            Console.WriteLine($"  Bin: {dup.Key}, Count: {dup.Count}");
            foreach (var info in dup.Infos)
            {
                Console.WriteLine($"    - {info}");
            }
        }
    }
}