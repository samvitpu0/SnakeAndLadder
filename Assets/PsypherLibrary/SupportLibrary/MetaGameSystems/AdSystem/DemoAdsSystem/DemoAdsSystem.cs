using System;
using System.Collections.Generic;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem.DemoAdsSystem
{
    public class DemoAdsSystem : AdsSystemBase, IAdsInterface
    {
        private DemoAdsUI _demoAdsUi;

        #region Interface Implementation

        public void SetConsent(bool consent)
        {
            IsConsentGiven = consent;
            Debug.Log("Consent: " + IsConsentGiven);
        }

        public void Initialize()
        {
            Isinitialized = true;
            _demoAdsUi = DemoAdsUI.Instance;

            _demoAdsUi.BannerAd.Deactivate();
            _demoAdsUi.InterstitialAd.Deactivate();
            _demoAdsUi.RewardingVideoAd.Deactivate();
        }

        public void RequestAd(AdsTypes type, Action onSuccess, Action onFail, AdsPositions position, Dictionary<string, object> extraOptions = null)
        {
            if (!IsConsentGiven || !Isinitialized)
            {
                Debug.LogWarning("Consent: " + IsConsentGiven + ", Initialized: " + Isinitialized);
                return;
            }

            AssignedActions.CreateActions(type, onSuccess, Actions.ActionType.Success, Actions.EventType.Request);

            switch (type)
            {
                case AdsTypes.Banner:
                {
                    OnBannerAdCached();
                }
                    break;
                case AdsTypes.Interstitial:
                {
                    OnInterstitialAdCached();
                }
                    break;
                case AdsTypes.RewardedVideo:
                {
                    OnRewardVideoAdCached(true);
                }
                    break;
                default:
                {
                    Debug.Log("Invalid ad type!!!");
                    return;
                }
            }
        }

        public void ShowAd(AdsTypes type, AdsPositions position, Action onAdShown, Action onAdClicked, Action<object> onRewarded, Dictionary<string, object> extraOptions = null, Action onSkipped = null)
        {
            if (!IsConsentGiven || !Isinitialized)
            {
                Debug.LogWarning("Consent: " + IsConsentGiven + ", Initialized: " + Isinitialized);
                return;
            }

            switch (type)
            {
                case AdsTypes.Banner:
                {
                    if (_isBannerReady)
                    {
                        _demoAdsUi.BannerAd.ShowAd(AdsTypes.Banner, position: position);
                    }
                }
                    break;
                case AdsTypes.Interstitial:
                {
                    if (_isInterstitialReady)
                    {
                        _demoAdsUi.InterstitialAd.ShowAd(AdsTypes.Interstitial, 5, skippable: extraOptions != null && (bool) extraOptions.SafeRetrieve("skippable"), onSkipped: onSkipped);
                    }
                }
                    break;
                case AdsTypes.RewardedVideo:
                {
                    if (_isRewardVideoReady)
                    {
                        _demoAdsUi.RewardingVideoAd.ShowAd(AdsTypes.RewardedVideo, Random.Range(6, 10), onRewarded: onRewarded, skippable: extraOptions != null && (bool) extraOptions.SafeRetrieve("skippable"), onSkipped: onSkipped);
                    }
                }
                    break;
                default:
                {
                    Debug.Log("Invalid ad type!!!");
                }
                    return;
            }

            onAdShown.SafeInvoke();
        }

        public void HideAd(AdsTypes type, Action onAdHidden, bool shouldReleaseFromCache)
        {
            switch (type)
            {
                case AdsTypes.Banner:
                {
                    _demoAdsUi.BannerAd.CloseAd(onAdHidden);
                }
                    break;
                case AdsTypes.Interstitial:
                {
                    _demoAdsUi.InterstitialAd.CloseAd(onAdHidden);
                }
                    break;
                case AdsTypes.RewardedVideo:
                {
                    _demoAdsUi.RewardingVideoAd.CloseAd(onAdHidden);
                }
                    break;
                default:
                {
                    Debug.Log("Invalid ad type!!!");
                }
                    return;
            }
        }

        public void HideAllAds(Action onAdHidden, bool shouldReleaseFromCache)
        {
            _demoAdsUi.BannerAd.CloseAd();
            _demoAdsUi.InterstitialAd.CloseAd();
            _demoAdsUi.RewardingVideoAd.CloseAd();

            onAdHidden.SafeInvoke();
        }

        public bool IsInitialized()
        {
            return Isinitialized;
        }

        public bool IsAdAvailable(AdsTypes type, Dictionary<string, object> extraOptions = null)
        {
            var isReady = false;
            switch (type)
            {
                case AdsTypes.Banner:
                    isReady = _isBannerReady;
                    break;
                case AdsTypes.Interstitial:
                    isReady = _isInterstitialReady;
                    break;
                case AdsTypes.RewardedVideo:
                    isReady = _isRewardVideoReady;
                    break;
            }
            //for demo ads, ads become available as soon as it is requested

            return isReady;
        }

        #endregion
    }
}