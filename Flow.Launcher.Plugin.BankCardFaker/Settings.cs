using System.Collections.Generic;

namespace Flow.Launcher.Plugin.BankCardFaker
{
    public class Settings
    {
        public List<string> CardTypes { get; set; } = new();
        public List<string> SelectedBinList { get; set; } = new();
    }
}