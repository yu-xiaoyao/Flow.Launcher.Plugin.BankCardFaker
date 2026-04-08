using System;
using System.Linq;

namespace Flow.Launcher.Plugin.BankCardFaker;

public class Main_Test
{
    public static void Main()
    {
        check_code_dup();
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