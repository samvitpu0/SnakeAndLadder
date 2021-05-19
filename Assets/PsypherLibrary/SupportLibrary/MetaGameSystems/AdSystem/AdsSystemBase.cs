using System;
using System.Collections;
using System.Collections.Generic;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem
{
    #region SupportiveClasses

    [Serializable]
    public class Apikey : KeyValue<string, string>
    {
    }

    [Serializable]
    public class AdStatus
    {
        public AdsTypes Type;
        public bool IsShowing;

        public AdStatus(AdsTypes type, bool isShowing)
        {
            Type = type;
            IsShowing = isShowing;
        }
    }

    public class Actions
    {
        public enum ActionType
        {
            Success,
            Fail
        }

        public enum EventType
        {
            Request,
            Show,
            Hide,
            Reward,
            Click
        }

        /*public class ActionCategory
    {
        public EventType EventType;

        public Dictionary<AdsTypes, Action> ActionList = new Dictionary<AdsTypes, Action>();

        public ActionCategory(EventType eventType, AdsTypes type, Action inAction)
        {
            EventType = eventType;
            if (inAction != null)
                ActionList.SafeAdd(type, inAction);
        }
    }*/

        public class ActionCategory
        {
            public EventType EventType;

            public Dictionary<AdsTypes, Action> ActionList = new Dictionary<AdsTypes, Action>();
            public Dictionary<AdsTypes, Action<object>> ActionDataList = new Dictionary<AdsTypes, Action<object>>();

            public ActionCategory(EventType eventType, AdsTypes type, Action inAction)
            {
                EventType = eventType;
                if (inAction != null)
                    ActionList.SafeAdd(type, inAction);
            }

            public ActionCategory(EventType eventType, AdsTypes type, Action<object> inAction)
            {
                EventType = eventType;
                if (inAction != null)
                    ActionDataList.SafeAdd(type, inAction);
            }
        }

        private List<ActionCategory> _successActions = new List<ActionCategory>();
        private List<ActionCategory> _failActions = new List<ActionCategory>();

        public void CreateActions(AdsTypes type, Action inAction, ActionType actionType, EventType eventType)
        {
            if (actionType == ActionType.Success)
            {
                var addedElement = _successActions.Find(x => x.EventType.Equals(eventType));

                if (inAction == null) //early exit if inAction is null
                    return;

                //if already present
                if (addedElement != null)
                {
                    addedElement.ActionList.SafeAdd(type, inAction);
                }
                else
                {
                    var elementToAdd = new ActionCategory(eventType, type, inAction);

                    _successActions.Add(elementToAdd);
                }
            }
            else
            {
                var addedElement = _failActions.Find(x => x.EventType.Equals(eventType));

                if (inAction == null) //early exit if inAction is null
                    return;

                //if already present
                if (addedElement != null)
                {
                    addedElement.ActionList.SafeAdd(type, inAction);
                }
                else
                {
                    var elementToAdd = new ActionCategory(eventType, type, inAction);

                    _failActions.Add(elementToAdd);
                }
            }
        }

        public void CreateActions(AdsTypes type, Action<object> inAction, ActionType actionType, EventType eventType)
        {
            if (actionType == ActionType.Success)
            {
                var addedElement = _successActions.Find(x => x.EventType.Equals(eventType));

                if (inAction == null) //early exit if inAction is null
                    return;

                //if already present
                if (addedElement != null)
                {
                    addedElement.ActionDataList.SafeAdd(type, inAction);
                }
                else
                {
                    var elementToAdd = new ActionCategory(eventType, type, inAction);

                    _successActions.Add(elementToAdd);
                }
            }
            else
            {
                var addedElement = _failActions.Find(x => x.EventType.Equals(eventType));

                if (inAction == null) //early exit if inAction is null
                    return;

                //if already present
                if (addedElement != null)
                {
                    addedElement.ActionDataList.SafeAdd(type, inAction);
                }
                else
                {
                    var elementToAdd = new ActionCategory(eventType, type, inAction);

                    _failActions.Add(elementToAdd);
                }
            }
        }

        public void ActionInvoke(AdsTypes type, ActionType actionType, EventType eventType, object data = null)
        {
            if (actionType == ActionType.Success)
            {
                if (_successActions.Count < 1)
                    return;

                var item = _successActions.Find(x => x.EventType.Equals(eventType));
                if (item != null)
                {
                    item.ActionList.SafeRetrieve(type).SafeInvoke();
                    item.ActionList.Remove(type);

                    if (data != null)
                    {
                        item.ActionDataList.SafeRetrieve(type).SafeInvoke(data);
                        item.ActionDataList.Remove(type);
                    }
                }
            }
            else
            {
                if (_failActions.Count < 1)
                    return;

                var item = _failActions.Find(x => x.EventType.Equals(eventType));
                if (item != null)
                {
                    item.ActionList.SafeRetrieve(type).SafeInvoke();
                    item.ActionList.Remove(type);

                    if (data != null)
                    {
                        item.ActionDataList.SafeRetrieve(type).SafeInvoke(data);
                        item.ActionDataList.Remove(type);
                    }
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Base class for any ads system initialization
    /// </summary>
    public abstract class AdsSystemBase : MonoBehaviour
    {
        #region Variables

        [Tooltip("Priority of this system respect to others, lesser the value higher the priority")]
        public int Priority;

        [Tooltip("List of Api keys with respect to platform")]
        public List<Apikey> ApiKeys;

        [Tooltip("Ads currently activated by this system. Dont modify here")]
        protected List<AdStatus> AdsServedStatus = new List<AdStatus>();

        protected Actions AssignedActions = new Actions();

        protected bool IsConsentGiven;

        protected bool Isinitialized;

        protected bool _isBannerReady;
        protected bool _isInterstitialReady;
        protected bool _isRewardVideoReady;

        #endregion

        #region Data

        public void SetPriority(int newPriority)
        {
            Priority = newPriority;
        }

        #endregion

        #region Event Callbacks

        //banner
        protected virtual void OnBannerAdCached()
        {
            _isBannerReady = true;

            AssignedActions.ActionInvoke(AdsTypes.Banner, Actions.ActionType.Success, Actions.EventType.Request);

            /*//calling it from here, as no events for showing banner ads
        OnBannerAdShown();*/
        }

        protected virtual void OnBannerAdShown()
        {
            AssignedActions.ActionInvoke(AdsTypes.Banner, Actions.ActionType.Success, Actions.EventType.Show);
        }

        protected virtual void OnBannerError<T>(T errorCode)
        {
            //_isBannerReady = false;
            AssignedActions.ActionInvoke(AdsTypes.Banner, Actions.ActionType.Fail, Actions.EventType.Request);

            Debug.Log("Banner System Error: " + errorCode);
        }

        //note: ad Click events

        protected virtual void OnBannerClick()
        {
            AssignedActions.ActionInvoke(AdsTypes.Banner, Actions.ActionType.Success, Actions.EventType.Click);
        }

        //interstitial
        protected virtual void OnInterstitialAdCached()
        {
            _isInterstitialReady = true;

            AssignedActions.ActionInvoke(AdsTypes.Interstitial, Actions.ActionType.Success, Actions.EventType.Request);
        }

        protected virtual void OnInterstitialAdShown()
        {
            AssignedActions.ActionInvoke(AdsTypes.Interstitial, Actions.ActionType.Success, Actions.EventType.Show);
        }

        protected virtual void OnInterstitialErrorRequest<T>(T errorCode)
        {
            _isInterstitialReady = false;
            AssignedActions.ActionInvoke(AdsTypes.Interstitial, Actions.ActionType.Fail, Actions.EventType.Request);

            Debug.Log("Interstitial System Error: " + errorCode);
        }

        protected virtual void OnInterstitialErrorShow<T>(T errorCode)
        {
            _isInterstitialReady = false;
            AssignedActions.ActionInvoke(AdsTypes.Interstitial, Actions.ActionType.Fail, Actions.EventType.Show);

            Debug.Log("Interstitial System Error: " + errorCode);
        }

        //note: ad Click events

        protected virtual void OnInterstitialClick()
        {
            AssignedActions.ActionInvoke(AdsTypes.Interstitial, Actions.ActionType.Success, Actions.EventType.Click);
        }

        //reward video
        protected virtual void OnRewardVideoAdCached(bool cached)
        {
            _isRewardVideoReady = cached;

            AssignedActions.ActionInvoke(AdsTypes.RewardedVideo, cached ? Actions.ActionType.Success : Actions.ActionType.Fail, Actions.EventType.Request);
        }

        protected virtual void OnRewardAdShown()
        {
            AssignedActions.ActionInvoke(AdsTypes.RewardedVideo, Actions.ActionType.Success, Actions.EventType.Show);
        }

        protected virtual void OnRewardAdRewarded<T>(T optionParam)
        {
            AssignedActions.ActionInvoke(AdsTypes.RewardedVideo, Actions.ActionType.Success, Actions.EventType.Reward, optionParam);
        }

        protected virtual void OnRewardVideoError<T>(T errorCode)
        {
            _isRewardVideoReady = false;
            AssignedActions.ActionInvoke(AdsTypes.RewardedVideo, Actions.ActionType.Fail, Actions.EventType.Request);

            Debug.Log("Reward Video System Error: " + errorCode);
        }

        protected virtual void OnRewardAdSkipped()
        {
            AssignedActions.ActionInvoke(AdsTypes.RewardedVideo, Actions.ActionType.Fail, Actions.EventType.Reward);

            Debug.Log("Reward Video Skipped");
        }

        //todo: ad Click events

        #endregion

        #region Actions

        public void SetAdServedStatus(AdsTypes type, bool isActive)
        {
            var entry = new AdStatus(type, isActive);
            if (isActive)
            {
                AdsServedStatus.AddUnique(entry, item => !item.Type.Equals(entry.Type));
            }
            else
            {
                AdsServedStatus.FindAndRemove(x => x.Type.Equals(entry.Type));
            }
        }

        public bool IsApiInitialized()
        {
            var adsInterface = this as IAdsInterface;
            return adsInterface != null && adsInterface.IsInitialized();
        }

        public bool IsAdsAvailable(AdsTypes type, Dictionary<string, object> extraOptions)
        {
            var adsInterface = this as IAdsInterface;
            return adsInterface != null && adsInterface.IsAdAvailable(type, extraOptions);
        }

        #endregion
    }
}