using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem.DemoAdsSystem
{
    public class DemoAdsUI : UIPanel
    {
        #region Singleton

        protected static bool IsSpawnedAlready = false;
        static GameObject _container;
        protected static int UID;

        protected static GameObject Container
        {
            get
            {
                if (_container == null)
                {
                    var container = GameObject.Find("DemoAdsContainer");
                    if (container == null)
                    {
                        container = new GameObject("DemoAdsContainer");
                        container.AddComponent<DontDestroyOnLoad>();
                    }

                    _container = container;
                }

                return _container;
            }
            set { _container = value; }
        }

        protected static DemoAdsUI _instance = null;

        public static DemoAdsUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(DemoAdsUI)) as DemoAdsUI;
                    if (_instance == null)
                    {
                        var go = Instantiate(Resources.Load("DemoAds/DemoAds"), Container.transform) as GameObject;
                        _instance = go.GetComponent<DemoAdsUI>();
                        //  _instance.GetCanvas.worldCamera = Camera.main;
                        _instance.gameObject.name = _instance.GetType().Name;
                    }

                    _instance.transform.SetParent(Container.transform);
                }

                IsSpawnedAlready = true;
                return _instance;
            }
            set { _instance = value; }
        }

        int CriticalLayerSortingOrder = 3000;

        #endregion

        [Header("Demo Ads UI")]
        public DemoAdsElement BannerAd;

        public DemoAdsElement InterstitialAd;
        public DemoAdsElement RewardingVideoAd;

        protected override void Awake()
        {
            if (IsSpawnedAlready)
            {
                Debug.Log("Deleting duplicate");
                Destroy(gameObject);
            }

            if (transform.root == transform) //only if this is the root transform, else it gives a runtime warning
                DontDestroyOnLoad(this);

            IsSpawnedAlready = true;
            UID = GetInstanceID();
            GetCanvas.sortingOrder = CriticalLayerSortingOrder;
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BannerAd.OnAdShown += ShowAds;
            BannerAd.OnAdClosed += HideAds;

            InterstitialAd.OnAdShown += ShowAds;
            InterstitialAd.OnAdClosed += HideAds;

            RewardingVideoAd.OnAdShown += ShowAds;
            RewardingVideoAd.OnAdClosed += HideAds;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            BannerAd.OnAdShown -= ShowAds;
            BannerAd.OnAdClosed -= HideAds;

            InterstitialAd.OnAdShown -= ShowAds;
            InterstitialAd.OnAdClosed -= HideAds;

            RewardingVideoAd.OnAdShown -= ShowAds;
            RewardingVideoAd.OnAdClosed -= HideAds;
        }

        private void ShowAds()
        {
            ActivatePanel(true);
        }

        private void HideAds()
        {
            DeactivatePanel(true);
        }
    }
}