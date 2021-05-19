#if ENABLE_CATEGORY
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.CategorySystem;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.CrossPromotion.OtherApps
{
    public class OtherAppsPopulateOverride : CategoryPopulateOverride
    {
        public override void OverrideCreateAndFill(UIPanel panelContainer, GameObject thumbnailPrefab, List<JToken> jData, bool containsOnlyItems, bool forceNumberThumbnail = false)
        {
            base.OverrideCreateAndFill(panelContainer, thumbnailPrefab, jData, containsOnlyItems, forceNumberThumbnail);

            panelContainer.ContentHolder.SetPrefab(thumbnailPrefab).SetData(jData).SetFunction((data, index, obj) =>
            {
                var cData = (List<JToken>) data;

                var itemObj = cData[index].TryAndFind<Item>(item => item["Data"] != null);

                if (itemObj != null)
                {
                    //if data only contains items, no need for hybrid
                    var tHelper = obj.GetComponent<ThumbnailHelper>();
                    var itemData = itemObj.Data.TryAndFind<CrossMetaData.OtherAppsData>(item => item.HasValues);

                    tHelper.SetContentType(ThumbnailTypes.Item, itemObj.ContentType);
                    tHelper.SetDetails(itemObj.Name, itemData.AppImage, () => CategoryInfo.OnInitialize(itemObj, tHelper), () => CategoryInfo.OnClick(cData[index], tHelper));
                }
            }).Initialize();
        }
    }
}
#endif