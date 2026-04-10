using System.Collections.Generic;

namespace Flow.Launcher.Plugin.BankCardFaker
{
    public class Settings
    {
        /// <summary>
        /// 输入过滤条件为空时, 返回结果乱序
        /// </summary>
        public bool InputEmptyRandom { get; set; } = true;

        public bool DebitCardType { get; set; } = true;

        public bool CreditCardType { get; set; } = true;

        // public List<string> CardTypes { get; set; } = new();

        public List<string> SelectedBinList { get; set; } = new();
    }
}