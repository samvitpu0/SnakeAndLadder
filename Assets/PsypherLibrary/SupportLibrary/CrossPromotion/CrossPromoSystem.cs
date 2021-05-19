using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;
using Random = UnityEngine.Random;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CrossPromotion
{
    using PsypherLibrary.SupportLibrary.CategorySystem;

    [Serializable]
    public enum PromoAdsStyle
    {
        Fullscreen,
        Banner
    }

    [Serializable]
    public class CrossPromoData
    {
        public string Banner;
        public string Fullscreen;
        public string AppStoreLink;
        public string GooglePlayLink;
    }

    [Serializable]
    public class PromoAdsContainer
    {
        public PromoAdsStyle AdsStyle;
        public CrossPromoPanel AdsPanel;
    }

    public class CrossPromoSystem : GenericManager<CrossPromoSystem>
    {
        public static bool ExternalAdsAvailable = false;

        public List<PromoAdsContainer> AvailableAdsType;
        public List<CrossPromoData> PromoData;
        public CrossPromoData CurrentCrossPromo;

        private CrossPromoPanel _currentPromoPanel;
        private Action _onClick;
        private float _lastDuration;

        private CategoryCollection _rawCollection;
        private JArray _collection;

        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            _rawCollection = LocalDataManager.Instance.GetConfig<CategoryCollection>(typeof(CrossPromoData).Name);
            _collection = new JArray(_rawCollection.Collections);

            PromoData = _collection.TryAndFindList<CrossPromoData>(x => x != null);
        }

        public void ShowCrossPromoAds(PromoAdsStyle type, float duration)
        {
            if (ExternalAdsAvailable) //when external ads are present cross promo ads are ignored
            {
                Debug.Log("External Ads is available, cross promo ads are not shown.");
                return;
            }

            var initIndex = Random.Range(0, PromoData.Count);
            CurrentCrossPromo = PromoData.JumpBy(initIndex, 1, out initIndex);
            _lastDuration = duration;

            if (CurrentCrossPromo != null)
            {
                var correctType = AvailableAdsType.Find(x => x.AdsStyle == type);
                if (correctType != null)
                {
                    _currentPromoPanel = correctType.AdsPanel;
                }

                if (_currentPromoPanel == null) return;

                switch (type)
                {
                    case PromoAdsStyle.Banner:
                    {
                        _currentPromoPanel.SetPromoPanel(() => OnClick(CurrentCrossPromo), CurrentCrossPromo.Banner,
                            duration, ResetBanner); //force deactivate when necessary
                    }
                        break;
                    case PromoAdsStyle.Fullscreen:
                    {
                        _currentPromoPanel.SetPromoPanel(() => OnClick(CurrentCrossPromo), CurrentCrossPromo.Fullscreen,
                            duration, ForceCloseAds);
                    }
                        break;
                }

                _currentPromoPanel.ActivatePanel();
            }
        }

        public void ForceCloseAds()
        {
            CurrentCrossPromo = null;
            if (_currentPromoPanel != null)
            {
                _currentPromoPanel.DeactivatePanel();
                _currentPromoPanel = null;
            }
        }

        void ResetBanner()
        {
            ForceCloseAds();
            ShowCrossPromoAds(PromoAdsStyle.Banner, _lastDuration);
        }

        void OnClick(CrossPromoData data)
        {
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