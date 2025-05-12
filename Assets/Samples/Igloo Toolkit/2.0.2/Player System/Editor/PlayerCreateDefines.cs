using UnityEditor;
using System;
using System.Linq;

namespace Igloo.Player.Editor
{
    [InitializeOnLoad]
    public static class PlayerCreateDefines
    {
        #region Static Private Vars
        private const string DefaultDefine = "IGLOO_PLAYER";
        #endregion

        #region Constructor Methods

        static PlayerCreateDefines()
        {
            if (!HasDefine(DefaultDefine))
                SetDefine(DefaultDefine, true);
        }

        #endregion

        #region Static Public Methods

        public static void SetDefine(string define, bool active)
        {
            // Get all defines groups.
            var buildTargets = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));
            foreach (var targetGroup in buildTargets)
            {
                if (!CheckBuildTarget(targetGroup)) continue;

                // Get all defines.
                var definesString = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup));
                var defines = definesString.Split(';').ToList();

                // Setup defines.
                if (active)
                {
                    if (!defines.Contains(define))
                        defines.Add(define);
                } else
                {
                    defines.Remove(define);
                }

                // Store new defines.
                PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup), string.Join(";", defines));
            }
        }

        public static bool HasDefine(string define)
        {
            // Get current define group.
            var currentBuildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            var definesString = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(currentBuildTarget));
            var defines = definesString.Split(';');

            // Check contain defines.
            return defines.Contains(define);
        }

        #endregion

        #region Static Private Methods

        private static bool CheckBuildTarget(BuildTargetGroup buildTarget)
        {
            // Not available in Unknown.
            if (buildTarget == BuildTargetGroup.Unknown)
                return false;

            // Or Obsolete.
            var buildTargetString = buildTarget.ToString();
            var field = typeof(BuildTargetGroup).GetField(buildTargetString);
            return !Attribute.IsDefined(field, typeof(ObsoleteAttribute), true);
        }

        #endregion
    }
}

