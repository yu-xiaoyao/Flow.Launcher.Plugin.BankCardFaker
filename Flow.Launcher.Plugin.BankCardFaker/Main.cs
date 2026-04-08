using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Flow.Launcher.Plugin.BankCardFaker.Logger;
using Flow.Launcher.Plugin.BankCardFaker.Matcher;

namespace Flow.Launcher.Plugin.BankCardFaker
{
    public class BankCardFaker : IPlugin, ISettingProvider
    {
        public readonly string IcoPath = "Images\\BankCardFaker.png";

        private PluginInitContext _context;
        private Settings _settings;
        private MatchOptions _options;
        private IStringMatcher _stringMatcher;

        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();

            _options = MatchOptions.Default;
            _stringMatcher = StringMatcherHelper.NewStringMatcher(context.API);

            InnerLogger.SetAsFlowLauncherLogger(_context, LoggerLevel.DEBUG);
        }

        public List<Result> Query(Query query)
        {
            var bankInfos = _getConfigBankCardInfoList();

            var search = query.Search.Trim();

            InnerLogger.Logger.Debug($"search: {search}");

            var result = new List<Result>();

            if (!string.IsNullOrEmpty(search))
            {
                var primaryList = new List<BankCardInfo>();
                var secondaryList = new List<BankCardInfo>();

                foreach (var f in bankInfos)
                {
                    if (BcBuilder.EnBankCode.ContainsKey(search.ToLower()))
                    {
                        var rn = BcBuilder.EnBankCode[search.ToLower()];
                        var match = string.Equals(rn, f.BankName, StringComparison.OrdinalIgnoreCase);
                        if (match)
                        {
                            InnerLogger.Logger.Debug(
                                $"Match En Code: {search}. Bin: {f.Bin} - Card Type: {f.CardType} - Bank Name: {f.BankName}");
                            primaryList.Add(f);
                        }
                        continue;
                    }

                    if (f.BankName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        primaryList.Add(f);
                        continue;
                    }

                    if (_stringMatcher.Match(search, f.BankName, _options))
                    {
                        primaryList.Add(f);
                        continue;
                    }


                    if ($"{f.BankName}{f.CardType}".Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        secondaryList.Add(f);
                        continue;
                    }

                    if (f.CardType.Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        secondaryList.Add(f);
                        continue;
                    }

                    if (_stringMatcher.Match(search, $"{f.BankName}{f.CardType}", _options))
                    {
                        secondaryList.Add(f);
                        continue;
                    }

                    if (_stringMatcher.Match(search, f.CardType, _options))
                    {
                        secondaryList.Add(f);
                        continue;
                    }
                }

                if (primaryList.Count != 0)
                {
                    var list = buildBankResults(primaryList);
                    result.AddRange(list);
                }

                if (secondaryList.Count != 0)
                {
                    var list = buildBankResults(secondaryList);
                    result.AddRange(list);
                }
            }

            return result;
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