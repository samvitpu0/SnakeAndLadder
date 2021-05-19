using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    public class CategorySystemRetainer : GenericDontDestroyOnLoad<CategorySystemRetainer>
    {
        public Canvas Canvas;
        public EScenes ActivateInScene;
        private CategorySystemInfo _categoryInfo;

        protected override void Awake()
        {
            if (_categoryInfo == null)
                _categoryInfo = GetComponent<CategorySystemInfo>();

            base.Awake();
        }

        protected override void OnLevelLoaded(Scene scene)
        {
            base.OnLevelLoaded(scene);

            if (Canvas)
            {
                Canvas.worldCamera = Camera.main;
            }

            if (scene.name.Equals(ActivateInScene.GetDescription()))
            {
                gameObject.Activate();

                //refresh contents
                _categoryInfo.RefreshCategorySystem();
            }
            else
            {
                gameObject.Deactivate();
            }
        }
    }
}
#endif