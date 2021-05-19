// Inspired by http://forum.unity3d.com/threads/automatic-version-increment-script.144917/

using System;
using System.IO;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Editor
{
    [InitializeOnLoad]
    public class VersionIncrementor : GenericScriptableObject<VersionIncrementor>, IPreprocessBuildWithReport
    {
        public bool IncrementOnBuild = false;
        public int MajorVersion;
        public int MinorVersion;
        public int BuildVersion;
        public string CurrentVersion;

        public int callbackOrder
        {
            get { return 0; }
        }

        void IncrementVersion(int majorIncr, int minorIncr, int buildIncr)
        {
            MajorVersion += majorIncr;
            MinorVersion += minorIncr;
            BuildVersion += buildIncr;

            UpdateVersionNumber();
        }


        [MenuItem("Tools/Build Version Manager/Open Build Version Manager")]
        private static void OpenBuildManager()
        {
            Selection.activeObject = Instance;
        }

        [MenuItem("Tools/Build Version Manager/Increase Major Version")]
        private static void IncreaseMajor()
        {
            Instance.MajorVersion++;
            Instance.MinorVersion = 0;
            Instance.BuildVersion = 0;
            Instance.UpdateVersionNumber();
        }

        [MenuItem("Tools/Build Version Manager/Increase Minor Version")]
        private static void IncreaseMinor()
        {
            Instance.MinorVersion++;
            Instance.BuildVersion = 0;
            Instance.UpdateVersionNumber();
        }

        [MenuItem("Tools/Build Version Manager/Increase Build Number")]
        private static void IncreaseBuild()
        {
            Instance.BuildVersion++;
            Instance.UpdateVersionNumber();
        }

        void UpdateVersionNumber()
        {
            //Make your custom version layout here.
            CurrentVersion = MajorVersion.ToString("0") + "." + MinorVersion.ToString("00") + "." +
                             BuildVersion.ToString("000");

            PlayerSettings.Android.bundleVersionCode = MajorVersion * 10000 + MinorVersion * 1000 + BuildVersion;
            PlayerSettings.iOS.buildNumber = (MajorVersion * 10000 + MinorVersion * 1000 + BuildVersion).ToString();
            PlayerSettings.bundleVersion = CurrentVersion;
            EditorUtility.SetDirty(Instance);
        }

        #region Events/Processes

        [PostProcessBuild(1)]
        public static void OnPreprocessBuild2(BuildTarget target, string pathToBuiltProject)
        {
            //Debug.Log("Build v" + Instance.CurrentVersion);
            // IncreaseBuild();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Build v" + Instance.CurrentVersion);
            if (IncrementOnBuild)
            {
                IncreaseBuild();
            }

#if UNITY_WEBGL
            PlayerSettings.WebGL.memorySize = BaseSettings.Instance.MemorySize;
            Debug.Log("Building WebGL build with memory size: " + PlayerSettings.WebGL.memorySize);
#endif
        }

        #endregion
    }
}