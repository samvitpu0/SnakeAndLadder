using System;
using System.Collections;
using System.Collections.Generic;
using PsypherLibrary.SupportLibrary.Managers;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem.DemoAdsSystem
{
    /// <summary>
    /// Use this class as a reference for writing Actual Ads initializer
    /// </summary>
    public class DemoAdsInitializer : MonoBehaviour
    {
        #region Initialization

        private void OnEnable()
        {
            AdsManager.OnAdsSystemInitialized += RequestInitialAdvertisements;
        }

        private void OnDisable()
        {
            AdsManager.OnAdsSystemInitialized -= RequestInitialAdvertisements;
        }

        void Start()
        {
            //note: disabled to test, re-enable for final build
            if (LocalDataManager.Instance.SaveData.GetPremiumStatus())
            {
                AdsManager.Instance.SetPremiumUser(true);
            }

            //using legal agreement acceptance for Ads permissions
            AdsManager.Instance.AskForConsent(LocalDataManager.Instance.SaveData.IsConsentGivenForAds, (consent) =>
            {
                LocalDataManager.Instance.SaveData.IsConsentGivenForAds = consent;
                LocalDataManager.Instance.Save();
                Time.timeScale = 1;
            });

            if (!LocalDataManager.Instance.SaveData.IsConsentGivenForAds)
            {
                Time.timeScale = 0;
            }
        }

        void RequestInitialAdvertisements()
        {
            Debug.Log("Requesting initial ads");

            StartCoroutine(RequestInitialAds());
        }

        IEnumerator RequestInitialAds()
        {
            yield return new WaitUntil(() => LocalDataManager.Instance.IsConnectedToInternet);

            //initial ads loading -- can be done in later stage
            try
            {
                AdsManager.Instance.RequestAdvertisement(AdsTypes.Banner, bannerCount => { Debug.Log("Banner system: " + bannerCount); }, position: AdsPositions.Bottom);

                AdsManager.Instance.RequestAdvertisement(AdsTypes.Interstitial, interstitialCount => { Debug.Log("interstitial system available: " + interstitialCount); });

                AdsManager.Instance.RequestAdvertisement(AdsTypes.RewardedVideo, videoCount => { Debug.Log("Reward video system: " + videoCount); });
            }
            catch (Exception e)
            {
                Debug.Log("ads initialize error: " + e);
            }

            yield return new WaitForFixedUpdate();
        }

        #endregion
    }
}