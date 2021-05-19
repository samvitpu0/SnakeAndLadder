using PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace PsypherLibrary.SupportLibrary.Managers
{
    public class GeneralSceneManager : GenericSingleton<GeneralSceneManager>
    {
        public bool UseLoadingAnim;
        private Scene _currentScene;

        protected override void OnLevelLoaded(int levelIndex)
        {
            base.OnLevelLoaded(levelIndex);

            _currentScene = SceneManager.GetSceneByBuildIndex(levelIndex);
        }

        public void RestartScene()
        {
            if (UseLoadingAnim)
            {
                UISceneLoader.Instance.ReloadCurrentScene();
            }
            else
            {
                SceneManager.LoadScene(_currentScene.name);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (UseLoadingAnim)
            {
                UISceneLoader.Instance.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        public void LoadScene(int index)
        {
            if (UseLoadingAnim)
            {
                UISceneLoader.Instance.LoadSceneIndex(index);
            }
            else
            {
                SceneManager.LoadScene(index);
            }
        }
    }
}