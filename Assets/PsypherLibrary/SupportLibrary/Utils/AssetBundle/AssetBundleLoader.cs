using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsypherLibrary.SupportLibrary.Utils.AssetBundle
{
    public class AssetBundleLoader : MonoBehaviour
    {
        [SerializeField]
        List<string> _assetBundleName_Scenes = new List<string>();

        private List<UnityEngine.AssetBundle> _sceneAssetBundles = new List<UnityEngine.AssetBundle>();

        [SerializeField]
        private List<string> _assetBundleName_Resources = new List<string>();

        private List<UnityEngine.AssetBundle> _resourcesAssetBundle = new List<UnityEngine.AssetBundle>();

        [SerializeField]
        private string _entrySceneName = "";

        IEnumerator Start()
        {
            yield return LoadAssetBundles();
        }

        /// <summary>
        ///     to be called from outside the class if loading bundles at a later time than Start
        /// </summary>
        public void LoadAssetsAndStart()
        {
            StartCoroutine(LoadAssetBundles());
        }

        private IEnumerator LoadAssetBundles()
        {
            var waitCount = _assetBundleName_Scenes.Count + _assetBundleName_Resources.Count;

            var buildPlatform = "Android";

#if UNITY_ANDROID
            buildPlatform = "Android";
#elif UNITY_IOS
        buildPlatform = "iOS";
#endif //todo: add conditions if targeting more platforms

            var dirPath = BaseSettings.Instance.ToStreamingAsset ? Path.Combine(Application.streamingAssetsPath, buildPlatform) : Application.streamingAssetsPath;

            //bundles for scenes
            foreach (var assetBundleName in _assetBundleName_Scenes)
            {
                var path = Path.Combine(dirPath, assetBundleName);
                var iQueue = UnityEngine.AssetBundle.LoadFromFileAsync(path);


                iQueue.completed += operation =>
                {
                    _sceneAssetBundles.Add(((AssetBundleCreateRequest) operation).assetBundle);
                    waitCount--;
                };
            }

            //bundles for game resources
            foreach (var assetBundleName in _assetBundleName_Resources)
            {
                var path = Path.Combine(dirPath, assetBundleName);
                var iQueue = UnityEngine.AssetBundle.LoadFromFileAsync(path);

                iQueue.completed += operation =>
                {
                    _resourcesAssetBundle.Add(((AssetBundleCreateRequest) operation).assetBundle);
                    waitCount--;
                };
            }

            yield return new WaitWhile(() => waitCount > 0);

            LoadEntryScene();
        }

        private void LoadEntryScene()
        {
            try
            {
                var sceneName = _sceneAssetBundles.SelectMany(element => element.GetAllScenePaths()).ToList();
                var entryScene = sceneName.Find(item => item.Contains(_entrySceneName));


                if (string.IsNullOrEmpty(_entrySceneName) || string.IsNullOrEmpty(entryScene))
                    SceneManager.LoadScene(sceneName.First());
                else
                    SceneManager.LoadScene(entryScene);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load Scene from AssetBundle!, error: " + e);
            }
        }
    }
}