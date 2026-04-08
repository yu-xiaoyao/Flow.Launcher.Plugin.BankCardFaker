using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.BankCardFaker
{
    public class BankCardFaker : IPlugin, ISettingProvider
    {
        public readonly string IcoPath = "Images\\BankCardFaker.png";

        private PluginInitContext _context;
        private Settings _settings;

        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();

            InnerLogger.SetAsFlowLauncherLogger(_context, LoggerLevel.DEBUG);
        }

        public List<Result> Query(Query query)
        {
            var bankInfos = _getConfigBankCardInfoList();

            var search = query.Search.Trim();

            InnerLogger.Logger.Debug($"search: {search}");

            if (!string.IsNullOrEmpty(search))
            {
                bankInfos = bankInfos.Where(f =>
                {
                    if (BcBuilder.EnBankCode.ContainsKey(search.ToLower()))
                    {
                        var rn = BcBuilder.EnBankCode[search.ToLower()];
                        InnerLogger.Logger.Debug(
                            $"Match En Code: {search}. Bin: {f.Bin} - Card Type: {f.CardType} - Bank Name: {f.BankName}");
                        return string.Equals(rn, f.BankName, StringComparison.OrdinalIgnoreCase);
                    }

                    var m = _context.API.FuzzySearch(search, f.BankName);
                    if (m.Success)
                        return true;
                    m = _context.API.FuzzySearch(search, f.CardType);
                    if (m.Success)
                        return true;
                    m = _context.API.FuzzySearch(search, $"{f.BankName}-{f.CardType}");
                    if (m.Success)
                        return true;
                    return false;
                }).ToList();
            }

            if (!bankInfos.Any())
                return new List<Result>();

            return buildBankResults(bankInfos);
        }

        public Control CreateSettingPanel()
        {
            return new SettingsUserControl { DataContext = _settings };
        }

        private List<Result> buildBankResults(List<BankCardInfo> bankCardInfos)
        {
            var result = new List<Result>();
            foreach (var bankCardInfo in bankCardInfos)
            {
                var bankCardNum = BcBuilder.BuildCardNum(bankCardInfo);
                result.Add(new Result()
                {
                    Title = $"{bankCardInfo.BankName}-{bankCardInfo.CardType}",
                    SubTitle = $"{bankCardNum} - ({bankCardInfo.CardName})",
                    IcoPath = IcoPath,
                    CopyText = bankCardNum,
                    ContextData = bankCardInfo,
                    Action = _ =>
                    {
                        _context.API.CopyToClipboard(bankCardNum, showDefaultNotification: false);
                        return true;
                    },
                });
            }

            return result;
        }

        private List<BankCardInfo> _getConfigBankCardInfoList()
        {
            var result = new List<BankCardInfo>();

            if (!_settings.SelectedBinList.Any())
            {
                foreach (var bankCardInfo in BcBuilder.BankConfig)
                {
                    if (_settings.CardTypes.Any())
                    {
                        if (_settings.CardTypes.Contains(bankCardInfo.CardType))
                            result.Add(bankCardInfo);
                    }
                    else result.Add(bankCardInfo);
                }
            }
            else
            {
                foreach (var bankCardInfo in BcBuilder.BankConfig)
                {
                    if (!_settings.SelectedBinList.Contains(bankCardInfo.Bin)) continue;

                    if (_settings.CardTypes.Any())
                    {
                        if (_settings.CardTypes.Contains(bankCardInfo.CardType))
                            result.Add(bankCardInfo);
                    }
                    else result.Add(bankCardInfo);
                }
            }

            return result;
        }
    }
}