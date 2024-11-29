using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PortalHunter.GameTools.I18n
{
    public static class I18nHelpers
    {

#if UNITY_EDITOR
        public static string[] GetAllFilesPathsByExtension(string extension, string[] exclude = null)
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where((path) =>
                {
                    var mainCheck = path.StartsWith("Assets/") && path.EndsWith(extension);
                    if (mainCheck && exclude != null && exclude.Any(item => path.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        return false;
                    }
                    return mainCheck;
                }).ToArray();
        }

        public static string BuildCSV(List<string[]> list)
        {
            var csv = new string[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Length < 4)
                {
                    Debug.LogError("wrong column count");
                    continue;
                }

                var row = new List<string>();
                foreach (var item in list[i])
                {
                    row.Add("\"" + (string.IsNullOrEmpty(item) ? "Empty" : item) + "\"");
                }
                csv[i] = string.Join(",", row.ToArray());
            }
            return string.Join("\n", csv);
        }
#endif
    }
}