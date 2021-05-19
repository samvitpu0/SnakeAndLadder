#if ENABLE_CATEGORY
using System;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.CrossPromotion
{
    public class CrossPromoPanel : UIPanel
    {
        [Header("Promo Panel")]
        public UIRemoteImage Image;

        public Image TimerImage;
        public Text TimerText;

        private Action _onClick;
        private Tweener _timer;

        protected override void OnEnable()
        {
            base.OnEnable();

            CanvasGroup.interactable = false;

            //initializing the fill image and text
            ResetTimer();
        }

        public void SetPromoPanel(Action onClick, string thumbnail, float duration = 3, Action onDurationEnd = null)
        {
            Image.ImageURL = thumbnail;
            _onClick = onClick;


            _timer = DOVirtual.Float(0, duration, duration, (value) =>
            {
                var cTime = duration - value.RoundTo(0);
                var ratio = value / duration;

                if (TimerImage && TimerText)
                {
                    TimerImage.fillAmount = ratio;
                    TimerText.SetText(cTime);
                }
            }).OnComplete(onDurationEnd.SafeInvoke).SetDelay(0.5f).SetEase(Ease.Linear);
        }

        public override Tweener ShowAnimation()
        {
            CanvasGroup.interactable = true;

            if (CanvasGroup && CanvasGroup.interactable)
            {
                EventTrigger trigger = CanvasGroup.GetComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.RemoveAllListeners();
                entry.callback.AddListener((data) =>
                {
                    if (CanvasGroup.interactable)
                    {
                        _onClick.SafeInvoke();
                        _onClick = null; //safe for not calling it twice
                        DeactivatePanel();
                    }
                });
                trigger.triggers.RemoveAll(x => x != null);
                trigger.triggers.Add(entry);
            }

            return base.ShowAnimation();
        }

        public override Tweener HideAnimation()
        {
            CanvasGroup.interactable = false;

            if (_timer != null && _timer.IsPlaying())
            {
                _timer.Kill();
                _timer = null;
            }

            ResetTimer();
            return base.HideAnimation();
        }

        void ResetTimer()
        {
            if (TimerImage && TimerText)
            {
                TimerImage.fillAmount = 0;
                TimerText.SetText("");
            }
        }
    }
}
#endif