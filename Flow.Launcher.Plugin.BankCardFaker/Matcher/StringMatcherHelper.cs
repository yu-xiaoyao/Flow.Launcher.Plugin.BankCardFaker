using System;
using System.IO;
using System.Reflection;
using Flow.Launcher.Plugin.BankCardFaker.Logger;

namespace Flow.Launcher.Plugin.BankCardFaker.Matcher;

public class StringMatcherHelper
{
    public static IStringMatcher NewStringMatcher(IPublicAPI publicApi)
    {
        // AppDomain.CurrentDomain.SetupInformation.ApplicationBase lastWith /
        var flowBaseDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        return NewStringMatcher(publicApi, flowBaseDir);
    }

    /// <summary>
    /// 创建 Matcher
    /// </summary>
    /// <param name="publicApi"></param>
    /// <param name="flowBaseDir">基础目录, 必须以 / 结尾</param>
    /// <returns></returns>
    public static IStringMatcher NewStringMatcher(IPublicAPI publicApi, string flowBaseDir)
    {
        try
        {
            var dllPath = flowBaseDir + "ToolGood.Words.Pinyin.dll";
            if (File.Exists(dllPath))
            {
                var assembly = Assembly.LoadFile(dllPath);
                // var assembly = Assembly.LoadFrom(dllPath);
                var pinyinMatchType = assembly.GetType("ToolGood.Words.Pinyin.PinyinMatch");
                var wordsHelperType = assembly.GetType("ToolGood.Words.Pinyin.WordsHelper");

                if (pinyinMatchType != null && wordsHelperType != null)
                {
                    return new FlowReflectionPinyinStringMatcher(publicApi, pinyinMatchType, wordsHelperType);
                }

                InnerLogger.Logger.Warn(
                    $"ToolGood has null. pinyinMatchType: [{pinyinMatchType}]. wordsHelperType: [{wordsHelperType}]");
            }
            else
                InnerLogger.Logger.Warn($"Pinyin Dll File not found: {dllPath}");
        }
        catch (Exception e)
        {
            InnerLogger.Logger.Error($"InitPinyinLibError. message: {e.Message}", e);
        }

        return new StringMatcher(publicApi);
    }
}