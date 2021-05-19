using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AdSystem.DemoAdsSystem
{
    public class DemoAdsElement : MonoBehaviour
    {
        public Action OnAdShown;
        public Action OnAdClosed;

        public LayoutGroup ParentLayout;
        public RectTransform DemoText;
        public Button CloseButton;
        public Image TimerImage;

        private Action<object> _onRewarded;
        private Action _onSkipped;
        private float _remainingTimer;
        private Tween _tween;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _remainingTimer = 0;
            _tween = null;

            CloseButton.interactable = false;
            CloseButton.onClick.RemoveAllListeners();
            CloseButton.onClick.AddListener(() => CloseAd());
        }


        public void ShowAd(AdsTypes type, float duration = -1, AdsPositions position = AdsPositions.Bottom, Action<object> onRewarded = null, bool skippable = false, Action onSkipped = null)
        {
            this.Activate();

            switch (type)
            {
                case AdsTypes.Banner:
                {
                    this.InvokeAfter(() => CloseButton.interactable = true, 0.1f, ignoreUnityTimeScale: true);

                    switch (position)
                    {
                        case AdsPositions.Top:
                        {
                            ParentLayout.childAlignment = TextAnchor.UpperCenter;
                        }
                            break;
                        case AdsPositions.Bottom:
                        {
                            ParentLayout.childAlignment = TextAnchor.LowerCenter;
                        }
                            break;
                    }
                }
                    break;
                case AdsTypes.Interstitial:
                case AdsTypes.RewardedVideo:
                {
                    _remainingTimer = duration;
                    _onRewarded = onRewarded;
                    _onSkipped = onSkipped;

                    if (skippable)
                    {
                        this.InvokeAfter(() => CloseButton.interactable = true, 0.1f, ignoreUnityTimeScale: true);
                    }

                    this.InvokeAfter(() => CloseButton.interactable = true, duration, (elapsedTime) =>
                    {
                        _remainingTimer = duration - elapsedTime;
                        TimerImage.fillAmount = elapsedTime / duration;
                    }, 0.1f, ignoreUnityTimeScale: true);

                    Time.timeScale = 0;
                }
                    break;
            }

            _tween = DemoText.DOPunchRotation(Vector3.one, 0.3f).SetEase(Ease.InOutElastic).SetLoops(-1, LoopType.Incremental).SetUpdate(true);


            OnAdShown.SafeInvoke();
        }

        public void CloseAd(Action onAdHidden = null)
        {
            Time.timeScale = 1;
            if (!CloseButton) return;

            if (_onRewarded != null || _onSkipped != null)
            {
                if (_remainingTimer <= 0)
                {
                    _onRewarded.SafeInvoke(Random.Range(1, 5));
                }
                else
                {
                    _onSkipped.SafeInvoke();
                }
            }

            if (_tween != null && _tween.IsActive())
            {
                _tween.Complete();
                _tween.Kill();
            }

            CloseButton.interactable = false;

            onAdHidden.SafeInvoke();
            OnAdClosed.SafeInvoke();

            this.Deactivate();
        }
    }
}