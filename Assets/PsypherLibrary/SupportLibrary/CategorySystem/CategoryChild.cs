using UnityEngine;


#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    public abstract class CategoryChild : MonoBehaviour
    {

        #region variable declaration

        private CategorySystemInfo _categoryInfo;

        public CategorySystemInfo CategoryInfo
        {
            get { return _categoryInfo ?? (_categoryInfo = transform.root.GetComponent<CategorySystemInfo>()); }
        }
        #endregion
    }
}
#endif