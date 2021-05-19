using System.Collections.Generic;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary._ExampleScripts
{
    using PsypherLibrary.SupportLibrary.CategorySystem;

    public class SearchManagerExample : MonoBehaviour
    {
        public UIScrollable Container;
        public CategorySystemInfo CategoryInfo;

        public GameObject Prefab;

        // Use this for initialization
        public void Initialize()
        {
            SearchManager.SetData(CategoryInfo.AllItems);
        }

        public void OnTextEnter(string text)
        {
            var retData = SearchManager.GetItemForTag(text);


            Container.SetData(retData).SetPrefab(CategoryInfo.ItemPrefab).SetFunction((data, currentIndex, gObj) =>
            {
                var currentData = ((data as List<object>)[currentIndex]) as Item;
                gObj.GetComponentInChildren<Text>().SetText(currentData.Name);
            }).Initialize();
        }
    }
}
#endif