using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem
{
    public enum AdsPositions
    {
        Top,
        Bottom,
        Top_Left,
        Top_Right,
        Bottom_Left,
        Bottom_Right
    }

    public enum AdsTypes
    {
        Banner,
        Interstitial,
        RewardedVideo
    }

    /// <summary>
    /// This has to added to the scene and all required Ads system has to attached to the GameObject
    /// </summary>
    public class AdsManager : GenericManager<AdsManager>
    {
        #region fields and properties

        public static Action OnAdsSystemInitialized;
        public static Action<AdsTypes, AdsPositions> OnAdsShown;
        public static Action<AdsTypes> OnAdsHidden;
        public static Action OnRewardVideoCompleted;

        [Tooltip(
            "When this is set to true, priority settings are retrive from supplied Json[if available] - Implementation pending")]
        //todo: to be integrated later
        public bool PriorityFromConfig;

        //to hold the last ads advertizers that supplied ad
        private List<AdsSystemBase> _engagedAdvertisers = new List<AdsSystemBase>();

        private bool _errorInInitialization;
        private List<AdsSystemBase> _availableAdsSystem;
        private List<AdsSystemBase> _systemsWithAdsAvailable;
        private bool _isRequestingAds;
        private bool _isInitialized;
        private bool _isPremiumUser;

        public bool IsRequestingAds
        {
            get { return _isRequestingAds; }
        }

        private bool _consentForShowingAds;

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        #endregion

        #region Initializing

        public virtual void AskForConsent(bool storedValue, Action<bool> onConsent)
        {
            if (_isInitialized) return;

            #region UI Correction

            EPanelSize panelSize = EPanelSize.MediumLandscape;

            Debug.Log("Current Resolution: " + Screen.width + " : " + Screen.height);

            if (Screen.height > Screen.width)
            {
                panelSize = EPanelSize.LargePortrait;
            }
            else if (Screen.currentResolution.height < Screen.currentResolution.width)
            {
                panelSize = EPanelSize.MediumLandscape;
            }

            #endregion

            if (!storedValue)
            {
                UIPopupBox.Instance.SetDataYesNo("We request your permission to show ads, \nPlease Accept.", () =>
                {
                    _consentForShowingAds = true;
                    onConsent.SafeInvoke(true);
                    Initialize();
                }, () =>
                {
                    _consentForShowingAds = false;
                    onConsent.SafeInvoke(false);
                }, shouldUseAcceptAndDecline: true, dSize: panelSize);
            }
            else //initialize if consent already given
            {
                _consentForShowingAds = true;
                Initialize();
            }
        }

        protected virtual void Initialize()
        {
            //note: initialize all the available ads system
            _availableAdsSystem = CheckForAttachedAdsSystem();
            _systemsWithAdsAvailable = new List<AdsSystemBase>();

            if (_availableAdsSystem.Count > 0)
            {
                _errorInInitialization = false;
                _availableAdsSystem.ForEach(service =>
                {
                    var adsInterface = service as IAdsInterface;
                    if (adsInterface != null)
                    {
                        adsInterface.SetConsent(_consentForShowingAds);
                        adsInterface.Initialize();
                    }
                });

                //note: once inits are done, invoke onInitialized action
                OnAdsSystemInitialized.SafeInvoke();
                _isInitialized = true;
            }
            else
            {
                _isInitialized = false;
                _errorInInitialization = true;

#if UNITY_EDITOR
                Debug.LogError(
                    "Ads system initialization error. Check is any ads system are attached to the Ads manager GameObject.");
#else
            Debug.LogWarning("Ads system initialization error. Check is any ads system are attached to the Ads manager GameObject.");
#endif
            }
        }

        /// <summary>
        /// to be use incase, normal initialization failed
        /// </summary>
        public void ForceInitialize()
        {
            _consentForShowingAds = true;
            Initialize();
        }

        #endregion

        #region Data

        private List<AdsSystemBase> CheckForAttachedAdsSystem()
        {
            var allAdsSystem = new List<AdsSystemBase>();

            //note: iterate and find all attached ads systems
            allAdsSystem.AddRange(GetComponents<AdsSystemBase>().Where(x => x.isActiveAndEnabled));
            return allAdsSystem;
        }

        #endregion

        #region Actions

        public void RequestAdvertisement(AdsTypes type, Action<int> onSuccess = null, Action onFail = null,
            AdsPositions position = AdsPositions.Top, float timeOutDuration = 5,
            Dictionary<string, object> extraOptions = null)
        {
            if (!LocalDataManager.Instance.IsConnectedToInternet)
            {
                Debug.Log("Request ads ignored, as there is not internet connectivity.");
                onFail.SafeInvoke();
                return; //early return if there is not internet connectivity
            }


            Debug.Log("Requesting Ad: Type- " + type + " Extra options: " + JsonConvert.SerializeObject(extraOptions));
            StartCoroutine(RequestAds(type, onSuccess, onFail, position, timeOutDuration, extraOptions));
        }

        protected virtual IEnumerator RequestAds(AdsTypes type, Action<int> onSuccess = null, Action onFail = null,
            AdsPositions position = AdsPositions.Top, float timeOutDuration = 8,
            Dictionary<string, object> extraOptions = null)
        {
            bool timeOut = false;

            //early exit when there is init error
            if (_errorInInitialization)
            {
                onFail.SafeInvoke();
                yield return new WaitForEndOfFrame();
                _isRequestingAds = false;
                yield break;
            }

            _isRequestingAds = true;
            var requestCount = 0;

            try
            {
                _availableAdsSystem.ForEach(x =>
                {
                    var adsInterface = x as IAdsInterface;
                    if (adsInterface != null)
                    {
                        adsInterface.RequestAd(type, () =>
                        {
                            _systemsWithAdsAvailable.AddUnique(x);

                            requestCount++;
                        }, () => { requestCount++; }, position, extraOptions);
                    }
                });
            }
            catch (Exception e)
            {
                Debug.Log("Ad Request error: " + e);
                timeOut = true;
            }


            this.InvokeAfter(() => timeOut = true, timeOutDuration);

            //wait till all the request are processed and callbacks received
            yield return new WaitUntil(() => requestCount.Equals(_availableAdsSystem.Count) || timeOut);

            // _systemsWithAdsAvailable = _systemsWithAdsAvailable.Distinct().ToList();

            var adsFound = _systemsWithAdsAvailable.Count(x => x.IsAdsAvailable(type, extraOptions));

            if (adsFound > 0)
                onSuccess.SafeInvoke(adsFound);
            else
            {
                onFail.SafeInvoke();
            }

            yield return new WaitForEndOfFrame();
            _isRequestingAds = false;
        }

        public void ShowAdvertisement(AdsTypes type, AdsPositions position = AdsPositions.Top, Action onAdShown = null,
            Action onAdClicked = null, Action<object> onRewarded = null, Action<string> onFail = null,
            bool forceAds = false, Dictionary<string, object> extraOptions = null, float invokeDelay = 0,
            Action onSkipped = null)
        {
            if (!LocalDataManager.Instance.IsConnectedToInternet)
            {
                Debug.Log("Show ads ignored, as there is not internet connectivity.");
                return;
            }

            Debug.LogFormat("Ads Manager::: -> Premium User: {0} \n-> Force Ads: {1}", _isPremiumUser, forceAds);

            //debug
            if (BaseSettings.Instance.ForceAds)
            {
                Debug.Log("Debug force Ads is true, ignoring premium status.");
            }

            if (!BaseSettings.Instance.ForceAds && !forceAds) //game setting->forceAds is use for debug
            {
                if (_isPremiumUser)
                {
                    return;
                }
            }

            Debug.Log("Showing Ad: Type- " + type + " Extra options: " + JsonConvert.SerializeObject(extraOptions) +
                      " with delay: " + invokeDelay);
            StartCoroutine(ShowAds(type, position, onAdShown, onAdClicked, onRewarded, onFail, extraOptions,
                invokeDelay, onSkipped));
        }

        protected virtual IEnumerator ShowAds(AdsTypes type, AdsPositions position = AdsPositions.Top,
            Action onAdShown = null, Action onAdClicked = null, Action<object> onRewarded = null,
            Action<string> onFail = null, Dictionary<string, object> extraOptions = null, float invokeDelay = 0,
            Action onSkipped = null)
        {
            yield return new WaitForSeconds(invokeDelay);

            //early exit when there is init error or if the user is a premium user
            if (_errorInInitialization) yield break;

            if (_systemsWithAdsAvailable.Count < 1)
            {
                Debug.LogWarning("Error: No Ads Available");
                onFail.SafeInvoke("No Ads Available");
                yield break;
            }

            //sort the system that has ads based on priority
            _systemsWithAdsAvailable.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            var firstPriority = _systemsWithAdsAvailable.Find(x => x.IsAdsAvailable(type, extraOptions));

            if (firstPriority == null)
            {
                Debug.LogWarning("Ad type of : " + type + " is not available in all systems.");
                yield break;
            }

            if (!firstPriority.IsApiInitialized() || !firstPriority.IsAdsAvailable(type, extraOptions))
            {
                Debug.LogWarningFormat("System Initialized: {0}.\nAds available in the system: {1}",
                    firstPriority.IsApiInitialized(), firstPriority.IsAdsAvailable(type, extraOptions));
                yield break;
            }

            var adsInterface = firstPriority as IAdsInterface;

            if (adsInterface != null)
            {
                //adding the set status method to the on ad shown action
                onAdShown += () =>
                {
                    firstPriority.SetAdServedStatus(type, true);
                    OnAdsShown.SafeInvoke(type, position);
                };

                onRewarded += obj => { OnRewardVideoCompleted.SafeInvoke(); };

                adsInterface.ShowAd(type, position, onAdShown, onAdClicked, onRewarded, extraOptions, onSkipped);
                _engagedAdvertisers.AddUnique(firstPriority);
            }
        }

        public virtual void HideAdvertisement(AdsTypes type, Action onAdHidden = null,
            bool shouldReleaseFromCache = false)
        {
            if (_engagedAdvertisers == null || !_engagedAdvertisers.Any()) return;

            Debug.Log("Hiding Ad: Type- " + type);

            //early exit when there is init error
            if (_errorInInitialization) return;

            _engagedAdvertisers.ForEach(service =>
            {
                var adsInterface = service as IAdsInterface;
                if (adsInterface != null)
                {
                    //adding the set status method to the on ad shown action
                    onAdHidden += () =>
                    {
                        service.SetAdServedStatus(type, false);
                        OnAdsHidden.SafeInvoke(type);
                    };

                    adsInterface.HideAd(type, onAdHidden, shouldReleaseFromCache);
                }
            });
        }

        /// <summary>
        /// Hide all Showing/Active Ads, it will not trigger OnHideAds event
        /// </summary>
        /// <param name="onAdHidden"></param>
        /// <param name="shouldReleaseFromCache"></param>
        public virtual void HideAllAdvertisements(Action onAdHidden = null, bool shouldReleaseFromCache = false)
        {
            if (_engagedAdvertisers == null || !_engagedAdvertisers.Any()) return;

            //early exit when there is init error
            if (_errorInInitialization) return;

            _engagedAdvertisers.ForEach(service =>
            {
                var adsInterface = service as IAdsInterface;
                if (adsInterface != null)
                {
                    adsInterface.HideAllAds(onAdHidden, shouldReleaseFromCache);
                }
            });

            //_engagedAdvertisers.Clear();
        }

        public virtual bool IsAdsAvailable(AdsTypes type, Dictionary<string, object> extraOptions = null)
        {
            if (_systemsWithAdsAvailable == null || !_systemsWithAdsAvailable.Any() ||
                !LocalDataManager.Instance.IsConnectedToInternet)
                return false;

            //sort the system that has ads based on priority
            _systemsWithAdsAvailable.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            var firstPriority = _systemsWithAdsAvailable.Find(x => x.IsAdsAvailable(type, extraOptions));

            if (firstPriority != null)
            {
                Debug.LogFormat($"Ad system type - {type} is available");
                return true;
            }

            Debug.LogWarningFormat("Ad system type - {0} is not Available", type);
            return false;
        }

        public virtual void SetPremiumUser(bool isPremium)
        {
            Debug.Log("Premium user: " + isPremium);

            if (isPremium)
            {
                HideAllAdvertisements(null, true);
            }

            _isPremiumUser = isPremium;
        }

        #endregion
    }
}