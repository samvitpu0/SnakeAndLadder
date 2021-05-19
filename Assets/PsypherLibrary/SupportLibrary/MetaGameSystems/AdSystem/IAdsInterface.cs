using System;
using System.Collections.Generic;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem
{
    public interface IAdsInterface
    {
        void SetConsent(bool consent);
        void Initialize();
        void RequestAd(AdsTypes type, Action onSuccess, Action onFail, AdsPositions position, Dictionary<string, object> extraOptions = null);
        void ShowAd(AdsTypes type, AdsPositions position, Action onAdShown, Action onAdClicked, Action<object> onRewarded, Dictionary<string, object> extraOptions = null, Action onSkipped = null);
        void HideAd(AdsTypes type, Action onAdHidden, bool shouldReleaseFromCache);
        void HideAllAds(Action onAdHidden, bool shouldReleaseFromCache);

        bool IsInitialized();
        bool IsAdAvailable(AdsTypes type, Dictionary<string, object> extraOptions = null);
    }
}