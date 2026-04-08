using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flow.Launcher.Plugin.BankCardFaker.Logger;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.BankCardFaker.Matcher;

public class FlowReflectionPinyinStringMatcher : IStringMatcher
{
    private readonly IPublicAPI _publicApi;

    // private readonly FlowReflectionPinyinMatch _match;
    private readonly FlowReflectionWordsHelper _helper;

    public FlowReflectionPinyinStringMatcher(IPublicAPI publicApi, Type pinyinMatchType, Type wordsHelperType)
    {
        // _match = NewPinyinMatch(pinyinMatchType);
        // if (_match == null)
        // throw new Exception("Failed to create pinyin match");

        _helper = NewWordsHelper(wordsHelperType);
        if (_helper == null)
            throw new Exception("Failed to create words helper");
        _publicApi = publicApi;
    }

    public void SetKeywords(ICollection<string> keywords, bool force = false)
    {
        // _init = _match.SetKeywords(keywords, force);
    }


    public bool Match(string query, string stringToCompare, MatchOptions options)
    {
        if (options.FullEqual)
            return string.Equals(query, stringToCompare, StringComparison.OrdinalIgnoreCase);

        if (!options.PinyinSearch || !_helper.HasChinese(stringToCompare))
        {
            if (_publicApi == null) return false;

            var m = _publicApi.FuzzySearch(query, stringToCompare);
            return m.Success;
        }

        // 拼音搜索

        // 1. 首字母
        var firstPinyin = _helper.GetFirstPinyin(stringToCompare);
        if (string.IsNullOrEmpty(firstPinyin))
            return false;
        var match = firstPinyin.Contains(query, StringComparison.OrdinalIgnoreCase);
        if (match) return true;

        // 2. 全拼
        if (options.FullPinyin)
        {
            var result = _helper.GetPinyin(stringToCompare);
            if (result == null || !result.Any())
                return false;
            return result.Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }


    [CanBeNull]
    public static FlowReflectionWordsHelper NewWordsHelper(Type wordsHelperType)
    {
        var hasChineseMethod = wordsHelperType.GetMethod("HasChinese", new[] { typeof(string) });
        if (hasChineseMethod == null) return null;

        return new FlowReflectionWordsHelper
        {
            HasChineseMethod = hasChineseMethod,
            GetPinyinMethod = wordsHelperType.GetMethod("GetPinyin", new[] { typeof(string), typeof(bool) }),
            GetFirstPinyinMethod = wordsHelperType.GetMethod("GetFirstPinyin", new[] { typeof(string) }),
            IsAllChineseMethod = wordsHelperType.GetMethod("IsAllChinese", new[] { typeof(string) }),
        };
    }


    public class FlowReflectionWordsHelper
    {
        public MethodInfo HasChineseMethod { set; private get; }
        [CanBeNull] public MethodInfo GetPinyinMethod { set; private get; }
        [CanBeNull] public MethodInfo GetFirstPinyinMethod { set; private get; }
        [CanBeNull] public MethodInfo IsAllChineseMethod { set; private get; }

        /// <summary>
        /// 获取拼音全拼,支持多音,中文字符集为[0x4E00,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="text">原文本</param>
        /// <param name="tone">是否带声调</param>
        /// <returns></returns>
        [CanBeNull]
        public string GetPinyin(string text, bool tone = false)
        {
            if (GetPinyinMethod == null) return null;
            return (string)GetPinyinMethod.Invoke(null, new object[] { text, tone });
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        /// <param name="text">原文本</param>
        /// <returns></returns>
        [CanBeNull]
        public string GetFirstPinyin(string text)
        {
            if (GetFirstPinyinMethod == null) return null;
            try
            {
                return (string)GetFirstPinyinMethod.Invoke(null, [text]);
            }
            catch (TargetInvocationException ex)
            {
                var exInnerException = ex.InnerException;
                if (exInnerException != null)
                {
                    InnerLogger.Logger.Error(
                        $"FlowReflectionWordsHelper.GetFirstPinyin. ex: {exInnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                InnerLogger.Logger.Error($"FlowReflectionWordsHelper.GetFirstPinyin. ex: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 判断输入是否为中文  ,中文字符集为[0x4E00,0x9FA5][0x3400,0x4db5]
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool HasChinese(string content)
        {
            try
            {
                return (bool)HasChineseMethod.Invoke(null, [content])!;
            }
            catch (TargetInvocationException ex)
            {
                var exInnerException = ex.InnerException;
                if (exInnerException != null)
                {
                    InnerLogger.Logger.Error($"FlowReflectionWordsHelper.HasChinese. ex: {exInnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                InnerLogger.Logger.Error($"FlowReflectionWordsHelper.HasChinese. ex: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 判断输入是否为中文  ,中文字符集为[0x4E00,0x9FA5][0x3400,0x4db5]
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool IsAllChinese(string content)
        {
            if (IsAllChineseMethod == null) return false;
            try
            {
                return (bool)IsAllChineseMethod.Invoke(null, [content])!;
            }
            catch (TargetInvocationException ex)
            {
                var exInnerException = ex.InnerException;
                if (exInnerException != null)
                {
                    InnerLogger.Logger.Error($"FlowReflectionWordsHelper.IsAllChinese. ex: {exInnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                InnerLogger.Logger.Error($"FlowReflectionWordsHelper.IsAllChinese. ex: {ex.Message}");
            }

            return false;
        }
    }

    /*
    [CanBeNull]
    private static FlowReflectionPinyinMatch NewPinyinMatch(Type pinyinMatchType)
    {
        var setKeywordsMethod = pinyinMatchType.GetMethod("SetKeywords", [typeof(ICollection<string>)]);
        if (setKeywordsMethod == null) return null;

        var findIndexMethod = pinyinMatchType.GetMethod("FindIndex", [typeof(string)]);
        if (findIndexMethod == null) return null;
        if (findIndexMethod.ReturnType != typeof(List<int>)) return null;

        var findMethod = pinyinMatchType.GetMethod("Find", [typeof(string)]);
        if (findMethod == null) return null;
        if (findMethod.ReturnType != typeof(List<string>)) return null;

        var instance = Activator.CreateInstance(pinyinMatchType);
        if (instance == null) return null;

        return new FlowReflectionPinyinMatch
        {
            SetKeywordsMethod = setKeywordsMethod,
            FindIndexMethod = findIndexMethod,
            Instance = instance,
            FindMethod = findMethod
        };
    }


    public class FlowReflectionPinyinMatch
    {
        public MethodInfo SetKeywordsMethod { set; private get; }
        public MethodInfo FindIndexMethod { set; private get; }
        public object Instance { set; private get; }

        public MethodInfo FindMethod { set; private get; }

        private List<string> _keywords = new();


        public List<string> FindPinyin(string key)
        {
            if (!_keywords.Any()) return null;

            var indies = (List<int>)FindIndexMethod.Invoke(Instance, [key]);
            if (indies == null) return null;
            return !indies.Any() ? new List<string>() : indies.Select(index => _keywords[index]).ToList();
        }

        public bool SetKeywords(ICollection<string> keywords, bool force = false)
        {
            try
            {
                if (force)
                {
                    _keywords = keywords.ToList();
                    SetKeywordsMethod.Invoke(Instance, new object[] { _keywords });
                }
                else if (!_keywords.Any())
                {
                    _keywords = keywords.ToList();
                    SetKeywordsMethod.Invoke(Instance, new object[] { keywords });
                }

                return true;
            }
            catch (TargetInvocationException ex)
            {
                var exInnerException = ex.InnerException;
                if (exInnerException != null)
                {
                    InnerLogger.Logger.Error($"FlowReflectionPinyinMatch.SetKeywords. ex: {exInnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                InnerLogger.Logger.Error($"FlowReflectionPinyinMatch.SetKeywords. ex: {ex.Message}");
            }

            return false;
        }


        public List<string> Find(string key)
        {
            try
            {
                return (List<string>)FindMethod.Invoke(Instance, [key]);
            }
            catch (TargetInvocationException ex)
            {
                var exInnerException = ex.InnerException;
                if (exInnerException != null)
                {
                    InnerLogger.Logger.Error(
                        $"FlowReflectionPinyinMatch.Find. ex: {exInnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                InnerLogger.Logger.Error($"FlowReflectionPinyinMatch.Find. ex: {ex.Message}");
            }

            return null;
        }

        public List<int> FindIndex(string key)
        {
            try
            {
                return (List<int>)FindIndexMethod.Invoke(Instance, new object[] { key });
            }
            catch (TargetInvocationException ex)
            {
                var exInnerException = ex.InnerException;
                if (exInnerException != null)
                {
                    InnerLogger.Logger.Error(
                        $"FlowReflectionPinyinMatch.FindIndex. ex: {exInnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                InnerLogger.Logger.Error($"FlowReflectionPinyinMatch.FindIndex. ex: {ex.Message}");
            }

            return [];
        }
    }
    */
}