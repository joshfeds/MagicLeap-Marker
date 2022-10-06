#region

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#endregion

namespace MagicLeapSetupTool.Editor.Utilities
{
    /// <summary>
    /// Utility Adds ability to add and remove DefineSymbols
    /// </summary>
    public static class DefineSymbolsUtility
    {

        /// <summary>
        /// Check if the current define symbols contain a definition
        /// </summary>
        public static bool ContainsDefineSymbol(string symbol)
        {
            var definesString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            return definesString.Contains(symbol);
        }


        /// <summary>
        /// Remove a define from the scripting define symbols for every build target.
        /// </summary>
        /// <param name="define"></param>
        public static void RemoveDefineSymbol(string define)
        { 

            foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (defineSymbols.Contains(define))
                {
                    defineSymbols = defineSymbols.Replace($"{define};", "");
                    defineSymbols = defineSymbols.Replace($"{define}", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
                }
            }
        }

        /// <summary>
        /// Checks if build target has the Attribute [ObsoleteAttribute]
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private static bool IsObsolete(BuildTargetGroup group)
        {
            var field = typeof(BuildTargetGroup).GetField(group.ToString());
            if (field != null)
            {
                var attributes = field.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                return attributes.Length > 0;
            }

            return false;
        }

        /// <summary>
        /// Add define symbol as soon as Unity gets done compiling.
        /// </summary>
        public static void AddDefineSymbol(string define)
        {
            foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup)) continue;

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (!defineSymbols.Contains(define))
                {
                    if (defineSymbols.Length < 1)
                        defineSymbols = define;
                    else if (defineSymbols.EndsWith(";"))
                        defineSymbols = $"{defineSymbols}{define}";
                    else
                        defineSymbols = $"{defineSymbols};{define}";

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
                }
            }
        }

        /// <summary>
        /// Find Wild Card Directory Path
        /// </summary>
        public static bool DirectoryPathExistsWildCard(string path, string searchPattern)
        {
            foreach (var directory in Directory.EnumerateDirectories(path))
                if (directory.Contains(searchPattern))
                    return true;

            return false;
        }

    }
}