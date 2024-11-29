using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PortalHunter.GameTools.I18n
{
    /// <summary>
    /// Languages, i hate you!
    /// https://developer.mozilla.org/en-US/docs/Mozilla/Localization/Localization_and_Plurals
    /// </summary>
    public sealed class PluralService
    {
        public static PluralService Instance { get; } = new PluralService();

        public const string KeyPattern = @"\{[a-zA-Z]+[a-zA-Z0-9_]*\}";
        private const string FormulaPartPattern = @"((_|=)(([0-9]+)|(\[[0-9]+-[0-9]+\]))(\|(_|=)(([0-9]+)|(\[[0-9]+\-[0-9]+\])))?{[А-Яа-я0-9\.\,\ \%_A-Za-z]*}|(other{[А-Яа-я0-9\.\,\ \%_A-Za-z{}]*}))";

        public string Language { get; private set; }

        private readonly Dictionary<string, WordRules> _rules;

        private PluralService()
        {
            _rules = new Dictionary<string, WordRules>();
        }

        public void Init(string language, IDictionary<string, string> pairs)
        {
            if (Language == language)
            {
                return;
            }
            Language = language;

            if (_rules.Count > 0) _rules.Clear();

            foreach (var pair in pairs)
            {
                _rules.Add(pair.Key, new WordRules(pair.Value));
            }
        }

        public string ParseString(string origin, params float[] args)
        {
            var mathces = Regex.Matches(origin, KeyPattern);
            int count = Math.Min(mathces.Count, args.Length);
            
            for (int i = 0; i < count; i++)
            {
                var constructed = Round(args[i]);
                var word = GetWordFromKey(mathces[i].Value, args[i]);
                if (!string.IsNullOrEmpty(word)) constructed += word;
                origin = new Regex(Regex.Escape(mathces[i].Value)).Replace(origin, constructed, 1);
            }

            if (mathces.Count != args.Length)
            {
                UnityEngine.Debug.LogError($"Localization Error: {origin} needs {mathces.Count} params, but {args.Length} given");
            }

            return origin;
        }

        private string GetWordFromKey(string key, float val)
        {
            WordRules rule;

            if (!_rules.TryGetValue(key, out rule))
            {
                return null;
            }

            return rule.GetWord(key, val);
        }

        public static string Round(double val)
        {
            var res = val < 0d ? "-" : "";
            val = Math.Abs(val);

            res += Math.Round(val, val < 1d ? 2 : val < 10d ? 1 : 0).ToString(CultureInfo.InvariantCulture);

            return res;
        }

        private class WordRules
        {
            private readonly List<RulePart> _parts;
            private readonly RulePart _otherWord;

            public WordRules(string formula)
            {
                _parts = new List<RulePart>();

                var rawParts = Regex.Matches(formula, FormulaPartPattern);

                foreach (Match rawPart in rawParts)
                {
                    if (rawPart.Value.StartsWith("other"))
                    {
                        _otherWord = new RulePart(rawPart.Value.Replace("other{", "").Replace("}", ""), true, null, true, null);
                        continue;
                    }
                    _parts.Add(ParsePart(rawPart.Value));
                }
            }

            public string GetWord(string key, float val)
            {
                var strVal = Round(val).Replace("-", "");

                if (strVal.IndexOf(".") != -1)
                {
                    strVal = strVal.Substring(strVal.IndexOf(".") + 1);
                }

                foreach (var part in _parts)
                {
                    if (part.IsFit(strVal))
                    {
                        return part.Word;
                    }
                }

                if (_otherWord == null)
                {
                    return string.Empty;
                }

                return _otherWord.Word;
            }

            private static RulePart ParsePart(string formula)
            {
                var wordIndex = formula.IndexOf('{');
                var word = formula.Substring(wordIndex + 1, formula.Length - wordIndex - 2);

                formula = formula.Substring(0, wordIndex);

                string includeRaw;
                string excludeRaw = null;

                var index = formula.IndexOf('|');
                if (index != -1)
                {
                    includeRaw = formula.Substring(0, index);
                    excludeRaw = formula.Substring(index + 1);
                }
                else
                {
                    includeRaw = formula;
                }

                List<string> includeList = null;
                List<string> excludeList = null;
                var isIncludeEqual = true;
                var isExcludeEqual = true;

                if (includeRaw.Contains('_'))
                {
                    isIncludeEqual = false;
                }
                includeRaw = includeRaw.Substring(1);
                includeList = ParseSubpart(includeRaw);

                if (!string.IsNullOrEmpty(excludeRaw))
                {
                    if (excludeRaw.Contains('_'))
                    {
                        isExcludeEqual = false;
                    }
                    excludeRaw = excludeRaw.Substring(1);
                    excludeList = ParseSubpart(excludeRaw);
                }

                return new RulePart(word,
                    isIncludeEqual, includeList.ToArray(),
                    isExcludeEqual, excludeList != null && excludeList.Count > 0 ? excludeList.ToArray() : null);
            }

            private static List<string> ParseSubpart(string includeRaw)
            {
                var list = new List<string>();

                if (includeRaw.IndexOf('[') != -1)
                {
                    includeRaw = includeRaw.Replace("[", "").Replace("]", "");

                    string[] split;
                    if (includeRaw.IndexOf('-') != -1)
                    {
                        split = includeRaw.Split('-');
                        var max = int.Parse(split[1]);
                        for (int i = int.Parse(split[0]); i <= max; i++)
                        {
                            list.Add(i.ToString());
                        }
                    }
                    else if (includeRaw.IndexOf(',') != -1)
                    {
                        split = includeRaw.Split(',');
                        list.AddRange(split);
                    }
                    else
                    {
                        list.Add(includeRaw);
                    }
                }
                else
                {
                    list.Add(includeRaw);
                }

                return list;
            }

            private class RulePart
            {
                public readonly string Word;

                private readonly RuleSubPart _include;
                private readonly RuleSubPart _exclude;

                public RulePart(string word, bool includeEquals, string[] include, bool excludeEquals, string[] exclude)
                {
                    Word = word;
                    if (include != null)
                    {
                        _include = new RuleSubPart(includeEquals, include);
                    }
                    if (exclude != null)
                    {
                        _exclude = new RuleSubPart(excludeEquals, exclude);
                    }
                }

                public bool IsFit(string val)
                {
                    if (_exclude != null && _exclude.IsExist(val))
                    {
                        return false;
                    }
                    if (_include == null)
                    {
                        return true;
                    }
                    return _include.IsExist(val);
                }

                private class RuleSubPart
                {
                    private readonly bool _isEqual;
                    private readonly HashSet<string> _values;

                    public RuleSubPart(bool isEqual, IEnumerable<string> values)
                    {
                        _isEqual = isEqual;
                        _values = new HashSet<string>(values);
                    }

                    public bool IsExist(string val)
                    {
                        if (_isEqual) return _values.Contains(val);

                        var first = _values.First();

                        if (val.Length < first.Length)
                        {
                            return false;
                        }
                        val = val.Substring(val.Length - first.Length, first.Length);

                        return _values.Contains(val);
                    }
                }
            }
        }
    }
}