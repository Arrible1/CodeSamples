using PortalHunter.GameTools;
using PortalHunter.GameTools.I18n;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PortalHunter.I18n
{
    public class I18nService
    {
        public static I18nService Instance => instance ?? (instance = new I18nService());
        private static I18nService instance;

        public static readonly string[] SupportedLanguages = { "ru", "en" };

        public Action OnLocaleChanged;

        public string Locale
        {
            get { return locale; }
            set { SwitchLocale(value); }
        }
        private string locale;

        private readonly string fileName = "i18n";

        private Dictionary<string, string> _localizationData;

        private I18nService()
        {
        }
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            };

            string value;
            if (_localizationData.TryGetValue(key, out value))
                return value;
            return key;
        }

        public void SwitchLocale(string locale)
        {
            if (locale == null) locale = "ru";
            if (!SupportedLanguages.Contains(locale))
            {
                return;
            }

            LoadData(locale);

            this.locale = locale;

            OnLocaleChanged?.Invoke();
        }

        public static string Get(string key, bool makeRich = true, params float[] pluralValues)
        {
            var translation = Instance.GetText(key);
            if (makeRich)
            {
                translation = ProducerOfRich.MakeRich(translation); // Tags and icons for textmeshpro.
            }
            if (pluralValues.Length != 0)
            {
                translation = PluralService.Instance.ParseString(translation, pluralValues);
            }
            if (string.IsNullOrEmpty(translation))
            {
                Debug.Log($"localization key {key} not found");
                translation = $"<color=red>{key}</color>";
            }
            return translation;
        }

        public static string Get(string key, float[] pluralValues) => Get(key, true, pluralValues);
        public static string Get(string key) => Instance.GetText(key);

        private void LoadData(string locale)
        {
            var path = $"{locale}/{fileName}";
            var asset = Resources.Load<TextAsset>(path);

            if (asset == null || string.IsNullOrEmpty(asset.text))
            {
                throw new Exception($"Local {locale} not found!");
            }

            var rawData = new CsvReader(asset.text.Split('\n'));

            var formulas = new Dictionary<string, string>();
            _localizationData = new Dictionary<string, string>();
            foreach (var item in rawData)
            {
                if (item.Length <= 1)
                {
                    continue;
                }

                if (Regex.IsMatch(item[0], PluralService.KeyPattern))
                    formulas.Add(item[0], item[1]);

                if (_localizationData.ContainsKey(item[0]))
                {
                    Debug.LogError($"wtf {item[0]}");
                }
                _localizationData.Add(item[0], UnScreen(item[1]));
            }

            PluralService.Instance.Init(locale, formulas);
        }
        public static string Screen(string unscreened) => unscreened.Replace("\n", "\\n");
        public static string UnScreen(string screened) => screened.Replace("\\n", "\n");
    }
}