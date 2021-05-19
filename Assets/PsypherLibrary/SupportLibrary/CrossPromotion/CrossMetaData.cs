#if ENABLE_CATEGORY
using System;

namespace PsypherLibrary.SupportLibrary.CrossPromotion
{
    public class CrossMetaData
    {
        #region OtherAppData

        [Serializable]
        public class OtherAppsData
        {
            public string AppImage;
            public string AppStoreLink;
            public string GooglePlayLink;
            public string Info;

            public OtherAppsData()
            {
                AppImage = string.Empty;
                AppStoreLink = string.Empty;
                GooglePlayLink = string.Empty;
                Info = string.Empty;
            }

            public OtherAppsData(string appImage, string gLink, string iLink, string info)
            {
                AppImage = appImage;
                AppStoreLink = iLink;
                GooglePlayLink = gLink;
                Info = info;
            }
        }

        #endregion
    }
}
#endif