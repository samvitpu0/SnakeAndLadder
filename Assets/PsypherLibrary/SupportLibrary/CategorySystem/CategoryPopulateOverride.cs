using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    [RequireComponent(typeof(CategorySystemInfo))]
    public abstract class CategoryPopulateOverride : MonoBehaviour
    {
        private CategorySystemInfo _categoryInfo;
        public GameObject OverrideThumbnail;
        protected CategorySystemInfo CategoryInfo
        {
            get { return _categoryInfo ?? (_categoryInfo = transform.GetComponent<CategorySystemInfo>()); }
        }

        public virtual void OverrideCreateAndFill(UIPanel panelContainer, GameObject thumbnailPrefab, List<JToken> jData, bool containsOnlyItems, bool forceNumberThumbnail = false)
        {
            Debug.Log("Overriding the populate rule for @" + CategoryInfo + "...");

        }



    }
}
#endif