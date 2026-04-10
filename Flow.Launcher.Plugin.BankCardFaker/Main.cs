using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Flow.Launcher.Plugin.BankCardFaker.Logger;
using Flow.Launcher.Plugin.BankCardFaker.Matcher;

namespace Flow.Launcher.Plugin.BankCardFaker
{
    public class BankCardFaker : IPlugin, ISettingProvider, IContextMenu
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

            if (_settings.InputEmptyRandom && string.IsNullOrEmpty(query.Search.Trim()))
            {
                var rng = new Random();
                var infos = bankInfos.OrderBy(x => rng.Next()).ToList();
                return buildBankResults(query, infos);
            }

            var bankNameFilter = string.Empty;
            var bankCodeFilter = string.Empty;
            var bankCardFilter = string.Empty;

            foreach (var querySearchTerm in query.SearchTerms)
            {
                if (int.TryParse(querySearchTerm, out var bnStart))
                {
                    if (bnStart > 0)
                    {
                        if (string.IsNullOrEmpty(bankCodeFilter))
                        {
                            bankCodeFilter = querySearchTerm;
                            continue;
                        }
                    }
                }

                if (string.IsNullOrEmpty(bankNameFilter))
                {
                    bankNameFilter = querySearchTerm;
                }
                else
                {
                    if (string.IsNullOrEmpty(bankCardFilter))
                        bankCardFilter = querySearchTerm;
                }
            }

            InnerLogger.Logger.Debug(
                $"bankNameFilter: {bankNameFilter}.  bankCodeFilter: {bankCodeFilter}. bankCardFilter: {bankCardFilter}");

            var primaryList = new List<BankCardInfo>();
            foreach (var f in bankInfos)
            {
                var matchBankName = false;
                var matchBankCode = false;
                var matchBankType = false;

                // 名称
                if (!string.IsNullOrEmpty(bankNameFilter))
                {
                    if (BcBuilder.EnBankCode.ContainsKey(bankNameFilter.ToLower()))
                    {
                        var rn = BcBuilder.EnBankCode[bankNameFilter.ToLower()];
                        var match = string.Equals(rn, f.BankName, StringComparison.OrdinalIgnoreCase);
                        if (match)
                        {
                            InnerLogger.Logger.Debug(
                                $"Match En Code: {bankNameFilter}. Bin: {f.Bin} - Card Type: {f.CardType} - Bank Name: {f.BankName}");
                            matchBankName = true;
                        }
                    }

                    if (!matchBankName)
                    {
                        if (f.BankName.Contains(bankNameFilter, StringComparison.OrdinalIgnoreCase))
                            matchBankName = true;

                        if (!matchBankName)
                        {
                            if (_stringMatcher.Match(bankNameFilter, f.BankName, _options))
                                matchBankName = true;
                        }
                    }
                }
                else matchBankName = true;

                // 卡类型
                if (!string.IsNullOrEmpty(bankCardFilter))
                {
                    var cardType = BcBuilder.GetCardTypeDescription(f.CardType);
                    if (cardType.Contains(bankCardFilter, StringComparison.OrdinalIgnoreCase))
                        matchBankType = true;

                    if (!matchBankType)
                    {
                        if (_stringMatcher.Match(bankCardFilter, cardType, _options))
                            matchBankType = true;
                    }
                }
                else matchBankType = true;

                // 卡号前缀
                if (!string.IsNullOrEmpty(bankCodeFilter))
                {
                    if (f.Bin.StartsWith(bankCodeFilter))
                        matchBankCode = true;
                }
                else matchBankCode = true;

                if (matchBankName && matchBankType && matchBankCode)
                {
                    primaryList.Add(f);
                }
            }

            return buildBankResults(query, primaryList);
        }

        public Control CreateSettingPanel()
        {
            return new SettingsUserControl { DataContext = _settings };
        }

        private List<Result> buildBankResults(Query query, List<BankCardInfo> bankCardInfos)
        {
            var result = new List<Result>();
            foreach (var bankCardInfo in bankCardInfos)
            {
                var bankCardNum = BcBuilder.BuildCardNum(bankCardInfo);
                var cardTypeName = BcBuilder.GetCardTypeDescription(bankCardInfo.CardType);
                result.Add(new Result()
                {
                    // Title = $"{bankCardInfo.BankName}",
                    // SubTitle = $"{cardTypeName}: {bankCardNum}",
                    Title = $"{bankCardInfo.BankName} {cardTypeName}",
                    SubTitle = bankCardNum,
                    IcoPath = IcoPath,
                    CopyText = bankCardNum,
                    ContextData = new BankContextData
                    {
                        BankCardInfo = bankCardInfo,
                        BankCardNum = bankCardNum
                    },
                    AutoCompleteText = $"{query.ActionKeyword} {bankCardInfo.BankName} {cardTypeName}",
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

            foreach (var bankCardInfo in BcBuilder.BankConfig)
            {
                var cardType = BcBuilder.GetCardTypeByName(bankCardInfo.CardType);
                if (cardType == CardType.DebitCard)
                {
                    if (_settings.DebitCardType)
                    {
                        if (_settings.SelectedBinList.Count != 0)
                        {
                            if (_settings.SelectedBinList.Contains(bankCardInfo.Bin))
                                result.Add(bankCardInfo);
                        }
                        else
                            result.Add(bankCardInfo);
                    }
                }
                else if (cardType == CardType.CreditCard)
                {
                    if (_settings.CreditCardType)
                    {
                        if (_settings.SelectedBinList.Count != 0)
                        {
                            if (_settings.SelectedBinList.Contains(bankCardInfo.Bin))
                                result.Add(bankCardInfo);
                        }
                        else
                            result.Add(bankCardInfo);
                    }
                }
            }

            return result;
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            var contextData = selectedResult.ContextData as BankContextData;
            if (contextData == null) return [];

            var bankCardInfo = contextData.BankCardInfo;
            var fullInfo =
                $"{bankCardInfo.BankName}\n{BcBuilder.GetCardTypeDescription(bankCardInfo.CardType)}\n{contextData.BankCardNum}\n{bankCardInfo.CardType}\n{bankCardInfo.CardName}";
            return
            [
                new Result
                {
                    Title = "复制卡号",
                    SubTitle = contextData.BankCardNum,
                    IcoPath = IcoPath,
                    CopyText = contextData.BankCardNum,
                    Action = _ =>
                    {
                        _context.API.CopyToClipboard(contextData.BankCardNum, showDefaultNotification: false);
                        return true;
                    }
                },

                new Result
                {
                    Title = "复制全部银行信息",
                    SubTitle = fullInfo.Replace("\n", "  "),
                    IcoPath = IcoPath,
                    CopyText = fullInfo,
                    Action = _ =>
                    {
                        _context.API.CopyToClipboard(fullInfo, showDefaultNotification: false);
                        return true;
                    }
                }
            ];
        }
    }


    public class BankContextData
    {
        public BankCardInfo BankCardInfo { set; get; }
        public string BankCardNum { set; get; }
    }
}