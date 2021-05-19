#if ENABLE_CATEGORY
using PsypherLibrary.SupportLibrary.CategorySystem;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.CrossPromotion.OtherApps
{
    public class ClickOnAppLinks : MonoBehaviour, ICategoryClickable
    {
        public void OnClick(CategorySystemInfo categoryInfo, ThumbnailHelper clickedOn)
        {
            var iData = ((Item) clickedOn.Data).Data;
            var data = iData.TryAndFind<CrossMetaData.OtherAppsData>(x => x.HasValues);
            

            var activeLink = data.GooglePlayLink; //for editor
#if UNITY_ANDROID
            activeLink = data.GooglePlayLink;

#elif UNITY_IOS
       activeLink = data.AppStoreLink;
#endif
            if (!string.IsNullOrEmpty(activeLink))
            {
                Application.OpenURL(activeLink);
                //AnalyticsManager.Instance.LogLinkClick(activeLink, "Promo App");
            }
        }
    }
}
#endif