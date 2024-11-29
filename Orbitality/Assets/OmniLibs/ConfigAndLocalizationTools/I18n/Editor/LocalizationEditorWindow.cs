#if UNITY_EDITOR && ADMIN
using PortalHunter.GameMeta;
using PortalHunter.GameRoot;
using PortalHunter.GameTools.ConfigUploader;
using PortalHunter.GameTools.CustomEditor;
using PortalHunter.I18n;
using PortalHunter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PortalHunter.GameTools.I18n.CustomEditor
{
    public class LocalizationEditorWindow : EditorWindow
    {
        [MenuItem("Game tools/Localization", priority = 10)]
        public static void ShowWindow()
        {
            GetWindow(typeof(LocalizationEditorWindow), false, typeof(LocalizationEditorWindow).Name, true);
        }

        private const string PathToFile = "Assets/GameTools/CLIENT/I18n/Resources/ru/i18n.csv";
        private static string CrowdinExecPath => $"{Directory.GetCurrentDirectory()}/ExternalTools/CrowdinCLI/crowdin.bat".Replace("\\", "/");
        private static string CrowdinJarPath => $"{Directory.GetCurrentDirectory()}/ExternalTools/CrowdinCLI/crowdin-cli.jar".Replace("\\", "/");
        private static string ConfigPath => $"{Directory.GetCurrentDirectory()}/ExternalTools/CrowdinCLI/crowdin.yml".Replace("\\", "/");

        private Tabs _tab = Tabs.Keys;

        // from file
        private SortedDictionary<string, LocalizationRecord> _data = new SortedDictionary<string, LocalizationRecord>();

        // from now founded
        private SortedDictionary<string, LocalizationRecord> _foundedData;

        // file + now founded
        private SortedDictionary<string, LocalizationRecord> _mixedData = new SortedDictionary<string, LocalizationRecord>();
        private Vector2 _scroll;
        private Source _searchSource = Source.Variable;

        // filtered data
        private SortedDictionary<string, LocalizationRecord> _filteredData;
        private bool _isFilterShown;
        private string _searchQuery;
        private bool _newFilter;
        private bool _changedFilter;
        private bool _removedFilter;

        private Source _sourceFilter = (Source)~1;

        private int _branchName;
        private string _crowdinOutput;

        private Dictionary<string, string> _translationVars;

        [Flags]
        public enum Source
        {
            Code = 1,
            PrefabAndScene = 2,
            Config = 4,
            Variable = 8,
        }
        public enum PairStatus { NoneChanged, New, Changed, Removed }
        private enum Tabs { Keys = 0, UploadAndDownload = 1 }

        private void OnGUI()
        {
            _tab = (Tabs)GUILayout.Toolbar((int)_tab, Enum.GetNames(typeof(Tabs)));

            switch (_tab)
            {
                case Tabs.Keys:
                    RenderKeysTab();
                    break;
                case Tabs.UploadAndDownload:
                    RenderUpDownloadTab();
                    break;
            }
        }

        #region Keys tab
        private void RenderKeysTab()
        {
            GUILayout.Space(7);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Load from file"))
                {
                    LoadFromFile();
                }

                EditorGUI.BeginDisabledGroup(!_searchSource.HasFlag(Source.Code) && !_searchSource.HasFlag(Source.PrefabAndScene) && !_searchSource.HasFlag(Source.Config));
                {
                    if (GUILayout.Button("Search all keys"))
                    {
                        SearchAllKeys();
                        ProcessSearchResult();
                        Filter();
                    }
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Apply changes"))
                {
                    ApplyChanges();
                }

                if (GUILayout.Button("Revert changes"))
                {
                    RevertChanges();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                RenderTogle(Source.Code);
                RenderTogle(Source.Config);
                RenderTogle(Source.PrefabAndScene);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            _isFilterShown = EditorGUILayout.Foldout(_isFilterShown, "Filter");
            if (_isFilterShown)
            {
                RenderFilterPanel();
            }
            else
            {
                ResetFilter();
            }

            RenderList(_isFilterShown ? _filteredData : _mixedData ?? _foundedData);
        }

        private void ResetFilter()
        {
            _newFilter = _changedFilter = _removedFilter = false;
            _searchQuery = string.Empty;
            _sourceFilter = 0;
            _filteredData = null;
        }

        private void RenderTogle(Source flag)
        {
            if (EditorGUILayout.Toggle(flag.ToString(), _searchSource.HasFlag(flag))) _searchSource |= flag;
            else _searchSource &= ~flag;
        }

        private void RenderFilterPanel()
        {
            GUILayout.Label("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Query", GUILayout.MaxWidth(70));
                    _searchQuery = EditorGUILayout.TextField(_searchQuery);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    _sourceFilter = (Source)EditorGUILayout.EnumFlagsField("Source", _sourceFilter);

                    _newFilter = EditorGUILayout.Toggle("New", _newFilter);
                    _changedFilter = EditorGUILayout.Toggle("Changed", _changedFilter);
                    _removedFilter = EditorGUILayout.Toggle("Removed", _removedFilter);
                }
                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Filter();
            }

            GUILayout.Label("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);
        }

        private void RenderList(SortedDictionary<string, LocalizationRecord> source)
        {
            if (source == null || source.Count == 0)
            {
                ShowNotification(new GUIContent("No data found"));
                return;
            }

            RemoveNotification();

            _scroll = GUILayout.BeginScrollView(_scroll);
            {
                source.ForEach(RenderPairItem);
            }
            GUILayout.EndScrollView();
        }

        private static void RenderPairItem(KeyValuePair<string, LocalizationRecord> item)
        {
            var defaultColor = GUI.color;

            switch (item.Value.Status)
            {
                case PairStatus.NoneChanged:
                    break;
                case PairStatus.New:
                    GUI.color = Color.green;
                    break;
                case PairStatus.Changed:
                    GUI.color = Color.yellow;
                    break;
                case PairStatus.Removed:
                    GUI.color = Color.red;
                    break;
            }

            GUILayout.BeginVertical();
            {
                EditorGUI.BeginChangeCheck();
                {
                    string value, hint;
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Key", GUILayout.MaxWidth(40));
                        EditorGUILayout.SelectableLabel(item.Key);

                        GUILayout.Label("Source:", GUILayout.MaxWidth(50));
                        EditorGUILayout.SelectableLabel(item.Value.Source.ToString());
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Value", GUILayout.MaxWidth(40));
                        value = EditorGUILayout.TextField("", item.Value.Value);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Hint", GUILayout.MaxWidth(40));
                        hint = EditorGUILayout.TextField("", item.Value.Hint);
                    }
                    GUILayout.EndHorizontal();

                    if (item.Value.Status != PairStatus.Removed)
                    {
                        item.Value.Value = value;
                        item.Value.Hint = hint;
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    item.Value.Status = PairStatus.Changed;
                }

                EditorHelper.HorizontalLine();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
            GUI.color = defaultColor;
        }

        private void LoadFromFile()
        {
            AssetDatabase.Refresh();
            var source = EditorGUIUtility.Load(PathToFile).ToString();

            var r = new CsvReader(source.Split('\n'));

            _data = new SortedDictionary<string, LocalizationRecord>();
            foreach (var item in r)
            {
                if (item.Count == 1) continue;
                if (item.Count < 4)
                {
                    UnityEngine.Debug.LogError($"cant parse localization file, item[0] = {item[0]}");
                }
                Source enumVal;
                var keyCell = item[0];
                var valueCell = item[3];
                if (!Enum.TryParse(valueCell, out enumVal))
                    UnityEngine.Debug.LogError($"localization parsing error: enum Source cant parse value {valueCell} and key {keyCell}");
                _data.Add(keyCell, new LocalizationRecord(item[1], item[2], enumVal, PairStatus.NoneChanged));
            }


            _mixedData = new SortedDictionary<string, LocalizationRecord>();
            foreach (var keyValuePair in _data)
            {
                _mixedData.Add(keyValuePair.Key, (LocalizationRecord)keyValuePair.Value.Clone());

            }

            InitVars();
            _foundedData = null;
        }

        private void SearchAllKeys()
        {
            _foundedData = new SortedDictionary<string, LocalizationRecord>();

            if (_searchSource.HasFlag(Source.PrefabAndScene))
            {
                GetKeysFromPrefabs(_foundedData);
            }

            if (_searchSource.HasFlag(Source.Code))
            {
                GetKeysFromCode(_foundedData);
            }

            if (_searchSource.HasFlag(Source.Config))
            {
                GetKeysFromGameConfig(_foundedData);
            }
        }

        static void AddToStorage(SortedDictionary<string, LocalizationRecord> storage,
            string key, string value, string hint, Source source, PairStatus status, string context)
        {
            if (string.IsNullOrEmpty(hint)) hint = "Empty";
            if (storage.ContainsKey(key))
            {
                var hintOld = storage[key].Hint;

                if (hintOld == "Empty" && hint != "Empty")
                {
                    storage[key].Hint = hint;
                }
                else if (hintOld != hint && hint != "Empty")
                {
                    UnityEngine.Debug.LogError($"Key {key} hint \"{hint}\" from {context} is not same as hint \"{hintOld}\" from {storage[key].context}");
                }
                return;
            }
            storage.Add(key, new LocalizationRecord(value, hint, source, status, context));
        }

        private static void GetKeysFromPrefabs(SortedDictionary<string, LocalizationRecord> storage)
        {
            var allPrefabs = I18nHelpers.GetAllFilesPathsByExtension(".prefab");
            foreach (var prefab in allPrefabs)
            {
                var o = AssetDatabase.LoadMainAssetAtPath(prefab) as GameObject;
                var scripts = o.GetComponentsInChildren<LocalizeLabel>(true);
                foreach (var item in scripts)
                {
                    if (string.IsNullOrEmpty(item.Key))
                    {
                        continue;
                    }
                    AddToStorage(storage, item.Key, item.GetText(), item.GetHint(), Source.PrefabAndScene, PairStatus.New, prefab);
                }
            }
        }

        private static void GetKeysFromCode(SortedDictionary<string, LocalizationRecord> storage)
        {
            LocalizationUtils.RegisterAllHardcodedLocalizeProperties((key, t, hint) => AddToStorage(storage, key, t, hint, Source.Code, PairStatus.New, "From Code"));
        }

        private void GetKeysFromGameConfig(SortedDictionary<string, LocalizationRecord> storage)
        {
            ConfigsLoader.LoadForEditorToolsIfNeeded();
            var config = GameConfig.Instance;
            Action<string, string> action = (key, value) => AddToStorage(storage, key, value,
                "From confluence or google tables, modify Ru there", Source.Config, PairStatus.New, "GameConfig");
            GameConfigBuilder.FillConfigWithLocalization(config, ConfigsLoader.GetConfigsSourcePath(ConfigsSourcePathType.Dev), action);
            // Hack since max edited all text in crowdin
            GameConfigBuilder.FillHackedTutorLevelLocalizations(config, ConfigsLoader.GetConfigsSourcePath(ConfigsSourcePathType.Dev),
                (key, value) =>
                {
                    AddToStorage(storage, key, value, "Modifiable from crowdin", Source.Code, PairStatus.New,
                        "GameConfig");
                });
        }

        private void ProcessSearchResult()
        {
            _mixedData = new SortedDictionary<string, LocalizationRecord>();

            ParseAndUpdateVariables(_foundedData);

            var translationsFromFile = _data;
            var newSearchedKeys = _foundedData;

            foreach (var fromSavedFile in translationsFromFile)
            {
                var mergeResult = (LocalizationRecord)fromSavedFile.Value.Clone();

                _mixedData.Add(fromSavedFile.Key, mergeResult);

                if (!_searchSource.HasFlag(mergeResult.Source))
                {
                    mergeResult.Status = PairStatus.NoneChanged;
                    continue;
                }

                if (mergeResult.Source.HasFlag(Source.Variable))
                {
                    continue;
                }

                LocalizationRecord foundedLocalizationRecord;
                newSearchedKeys.TryGetValue(fromSavedFile.Key, out foundedLocalizationRecord);

                if (foundedLocalizationRecord != null)
                {
                    if (!_searchSource.HasFlag(foundedLocalizationRecord.Source))
                    {
                        continue;
                    }

                    if (foundedLocalizationRecord.Source == Source.Code || foundedLocalizationRecord.Source == Source.PrefabAndScene)
                    {
                        mergeResult.Source = foundedLocalizationRecord.Source;

                        if (mergeResult.Hint != I18nService.UnScreen(foundedLocalizationRecord.Hint))
                        {
                            mergeResult.Hint = foundedLocalizationRecord.Hint;
                            mergeResult.Status = PairStatus.Changed;
                            foundedLocalizationRecord.Status = PairStatus.Changed;
                        }
                        continue;
                    }
                    else if (foundedLocalizationRecord.Source == Source.Config)
                    {
                        if (mergeResult.Value != foundedLocalizationRecord.Value || mergeResult.Hint != I18nService.UnScreen(foundedLocalizationRecord.Hint))
                        {
                            mergeResult.Value = foundedLocalizationRecord.Value;
                            mergeResult.Hint = foundedLocalizationRecord.Hint;
                            mergeResult.Status = PairStatus.Changed;
                        }
                    }

                    continue;
                }
                mergeResult.Status = PairStatus.Removed;
            }

            foreach (var founded in newSearchedKeys)
            {
                if (founded.Value.Status == PairStatus.Changed)
                    continue;

                if (!_searchSource.HasFlag(founded.Value.Source) && !(founded.Key.StartsWith("$") && founded.Key.EndsWith("_Name")))
                    continue;

                if (!translationsFromFile.ContainsKey(founded.Key))
                {
                    founded.Value.Status = PairStatus.New;
                    _mixedData.Add(founded.Key, founded.Value);
                }
            }
        }

        private void ApplyChanges()
        {
            _data = new SortedDictionary<string, LocalizationRecord>();
            foreach (var keyValuePair in _mixedData)
            {
                if (keyValuePair.Value.Status != PairStatus.Removed)
                    _data.Add(keyValuePair.Key, keyValuePair.Value);
            }

            var rawData = new List<string[]>();
            foreach (var item in _data)
            {
                item.Value.Status = PairStatus.NoneChanged;

                rawData.Add(new[] {
                    item.Key,
                    I18nService.Screen(string.IsNullOrEmpty(item.Value.Value) ? item.Key + "_Value" : item.Value.Value),
                    I18nService.Screen(item.Value.Hint),
                    item.Value.Source.ToString()
                });
            }

            var source = I18nHelpers.BuildCSV(rawData);
            source = I18nService.UnScreen(source);

            var writer = new StreamWriter(PathToFile, false);
            writer.Write(source);
            writer.Close();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RevertChanges();
        }

        private void RevertChanges()
        {
            _mixedData = new SortedDictionary<string, LocalizationRecord>();
            foreach (var keyValuePair in _data)
            {
                if (keyValuePair.Value.Status != PairStatus.Removed)
                    _mixedData.Add(keyValuePair.Key, keyValuePair.Value);
            }
            if (_isFilterShown)
            {
                Filter();
            }
        }

        private void Filter()
        {
            _filteredData = new SortedDictionary<string, LocalizationRecord>();

            foreach (var pair in _mixedData)
            {
                var query = pair.Key?.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) != -1
                    || pair.Value.Value?.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) != -1
                    || pair.Value.Hint?.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) != -1;

                query &= _sourceFilter.HasFlag(pair.Value.Source);

                if (!_newFilter && !_changedFilter && !_removedFilter)
                {
                    if (query) { _filteredData.Add(pair.Key, pair.Value); }
                    else continue;
                }

                if (query && (_newFilter && pair.Value.Status == PairStatus.New
                              || _changedFilter && pair.Value.Status == PairStatus.Changed
                              || _removedFilter && pair.Value.Status == PairStatus.Removed))
                {
                    _filteredData.Add(pair.Key, pair.Value);
                }
            }
        }

        private void InitVars()
        {
            _translationVars = _data
                .Where(data => Regex.IsMatch(data.Key, PluralService.KeyPattern) || Regex.IsMatch(data.Key, ProducerOfRich.GDTypePattern))
                .ToDictionary(item => item.Key, item => item.Value.Value);
        }

        private void ParseAndUpdateVariables(SortedDictionary<string, LocalizationRecord> storage)
        {
            var newVars = new HashSet<string>();

            if (_translationVars == null)
            {
                InitVars();
            }

            foreach (var data in storage)
            {
                if (Regex.Match(data.Key, PluralService.KeyPattern).Success) continue;
                if (Regex.Match(data.Key, ProducerOfRich.GDTypePattern).Success) continue;

                foreach (Match match in Regex.Matches(data.Value.Value, ProducerOfRich.GDTypePattern))
                {
                    if (!_translationVars.Keys.Contains(match.Value))
                    {
                        _translationVars.Add(match.Value, $"Tooltip info for {match.Value}");
                        newVars.Add(match.Value);
                    }
                }

                foreach (Match match in Regex.Matches(data.Value.Value, LocalizationUtils.GDVariablePattern))
                {
                    var key = "{var}";
                    if (match.Value.IndexOf(':') != -1)
                    {
                        key = "{" + match.Value.Substring(match.Value.IndexOf(':') + 1) + "}";
                    }
                    if (!_translationVars.ContainsKey(key))
                    {
                        _translationVars.Add(key, $"other{{\"ADD PLURAL FORMULA FOR {key}\"}}");
                        newVars.Add(key);
                    }
                    if (match.Value.Contains("%"))
                        key += "%";
                    data.Value.Value = new Regex(match.Value).Replace(data.Value.Value, key, 1);
                }
            }

            foreach (var newData in newVars)
            {
                foreach (var mixedData in storage)
                {
                    if (mixedData.Key == newData) break;
                }

                var hint = newData.StartsWith("$") ? "Its tooltip" : "its formula field for " + newData + " word";
                storage.Add(newData, new LocalizationRecord(_translationVars[newData], hint, Source.Variable));
            }

            foreach (var var in _translationVars)
            {
                if (!var.Key.StartsWith("$") || var.Key.Contains("_Name")) continue;
                if (storage.ContainsKey($"{var.Key}_Name")) continue;
                storage.Add($"{var.Key}_Name", new LocalizationRecord(var.Key, $"Translation for type {var.Key}", Source.Code));
            }
        }
        #endregion

        #region Upload & download tab
        private void RenderUpDownloadTab()
        {
            GUILayout.Space(7);

            _branchName = EditorGUILayout.Popup("Branch name", _branchName, new[] { "Development", "Release" });

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Upload source"))
            {
                UploadSource();
            }
            if (GUILayout.Button("Download translations"))
            {
                DownloadTranslations();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField(_crowdinOutput, EditorStyles.textArea);
        }

        private void UploadSource()
        {
            RunCrowdin(GetUploadCommandTemplate(_branchName == 0 ? "Development" : "Release"));
        }

        private void DownloadTranslations()
        {
            RunCrowdin(GetDownloadCommandTemplate(_branchName == 0 ? "Development" : "Release"));
        }

        private void RunCrowdin(string arguments)
        {
            _crowdinOutput = "";
            try
            {
                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = CrowdinExecPath,
                        Arguments = arguments,
                        //WorkingDirectory = Application.dataPath,
                        //RedirectStandardOutput = true,
                        //UseShellExecute = false,
                        //CreateNoWindow = true,
                        //Verb = "runas",
            }
                };
                p.Start();

                //StreamReader str = p.StandardOutput;
                //var output = str.ReadToEnd();
                //p.WaitForExit();

                _crowdinOutput = "Successfully launched app";
            }
            catch (Exception e)
            {
                _crowdinOutput = "Unable to send data: " + e.Message;
            }
        }

        private static string GetUploadCommandTemplate(string branch)
        {
            return $"{CrowdinJarPath} upload sources -c {ConfigPath}"; //-b {branch}";
        }

        private static string GetDownloadCommandTemplate(string branch)
        {
            return $"{CrowdinJarPath} pull -c {ConfigPath}"; //-b {branch}";
        }
        #endregion

        [Serializable]
        public class LocalizationRecord : ICloneable
        {
            public string Value;
            public string Hint;
            public Source Source;
            public PairStatus Status;
            public string context;

            public LocalizationRecord(string value = "", string hint = "",
                Source source = Source.Code, PairStatus status = PairStatus.New, string context = "")
            {
                Value = value;
                Hint = hint;
                Status = status;
                Source = source;
                this.context = context;
            }

            public object Clone()
            {
                return new LocalizationRecord(Value, Hint, Source, Status);
            }
        }
    }
}
#endif