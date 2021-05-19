#if UNITY_EDITOR
using System.IO;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using UnityEditor;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.AssetBundle
{
    public class AssetBundleBuilder : EditorWindow
    {
        [MenuItem("Tools/Asset Bundle/Build Asset Bundle for Android")]
        private static void BuildAssetBundleAndroid()
        {
            BuildAssetBundles(BuildTarget.Android);
        }

        [MenuItem("Tools/Asset Bundle/Build Asset Bundle for iOS")]
        private static void BuildAssetBundleiOS()
        {
            BuildAssetBundles(BuildTarget.iOS);
        }

        [MenuItem("Tools/Asset Bundle/Build Asset Bundle for Windows")]
        private static void BuildAssetBundleWindows()
        {
            BuildAssetBundles(BuildTarget.StandaloneWindows);
        }

        private static void BuildAssetBundles(BuildTarget buildTarget)
        {
            var relativePath = BaseSettings.Instance.RelativePath;
            var finalRelativePath = string.IsNullOrEmpty(relativePath) ? buildTarget.ToString() : relativePath;

            var path = Path.Combine("Assets/AssetBundle", finalRelativePath);

            if (BaseSettings.Instance.ToStreamingAsset)
                path = Path.Combine(Application.streamingAssetsPath, finalRelativePath);

            CheckDirectory(path);
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, buildTarget);
            AssetDatabase.Refresh();
        }

        private static void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
#endif