using System;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;
using UnityEngine.Events;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    public class UIShowHideAnimations : MonoBehaviour
    {
        private enum FadeOutDirection
        {
            X_Axis,
            X_Axis_Negative,
            Y_Axis,
            Y_Axis_Negative
        }

        public Vector3 TargetPosition;
        public Vector3 BasePosition;
        public float FadeInTime = 0.5f;
        public float FadeOutTime = 0.5f;

        [Tooltip("If the default position should be out of screen")]
        public bool BaseIsOutOfScreen = true;

        [SerializeField]
        [Tooltip("Only Applicable if the Base Is Out Of Screen is set to true")]
        private FadeOutDirection _fadeOutDirection = FadeOutDirection.X_Axis;

        public UnityEvent CallBackOnFadeIn;
        public UnityEvent CallBackOnFadeOut;

        private RectTransform _rectTransform;

        private UIPanel _uiPanel;

        #region Initialization

        void Awake()
        {
            _rectTransform = transform.GetComponent<RectTransform>();
            _uiPanel = GetComponent<UIPanel>();
        }

        private void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            if (_uiPanel)
            {
                _uiPanel.CallBackOnReadyToActivate += ShowAnimation;
                _uiPanel.CallBackOnReadyToDeactivate += HideAnimation;
            }
        }

        private void OnDestroy()
        {
            if (_uiPanel)
            {
                _uiPanel.CallBackOnReadyToActivate -= ShowAnimation;
                _uiPanel.CallBackOnReadyToDeactivate -= HideAnimation;
            }
        }

        #endregion

        #region Actions

        public virtual void ShowAnimation()
        {
            if (_rectTransform)
            {
                _rectTransform.DOAnchorPos(TargetPosition, FadeInTime).SetEase(Ease.InOutSine).OnComplete(() => CallBackOnFadeIn.Invoke());
            }
            else
            {
                transform.DOLocalMove(TargetPosition, FadeInTime).SetEase(Ease.InOutSine).OnComplete(() => CallBackOnFadeIn.Invoke());
            }
        }

        public virtual void HideAnimation()
        {
            Vector3 outDir = Vector3.zero;
            var screenHeight = Screen.currentResolution.height;
            var screenWidth = Screen.currentResolution.width;

            if (BaseIsOutOfScreen)
            {
                switch (_fadeOutDirection)
                {
                    case FadeOutDirection.X_Axis:
                        outDir.Set(1.5f * screenWidth, 0, 0);
                        break;
                    case FadeOutDirection.X_Axis_Negative:
                        outDir.Set(-1.5f * screenWidth, 0, 0);
                        break;
                    case FadeOutDirection.Y_Axis:
                        outDir.Set(0, 1.5f * screenHeight, 0);
                        break;
                    case FadeOutDirection.Y_Axis_Negative:
                        outDir.Set(0, -1.5f * screenHeight, 0);
                        break;
                }
            }
            else
            {
                outDir = BasePosition;
            }

            if (_rectTransform)
            {
                _rectTransform.DOAnchorPos(outDir, FadeOutTime).SetEase(Ease.InOutSine).OnComplete(() => CallBackOnFadeOut.Invoke());

                //Debug.Log(screenWidth + ", " + screenHeight);
            }
            else
            {
                transform.DOLocalMove(outDir, FadeOutTime).SetEase(Ease.InOutSine).OnComplete(() => CallBackOnFadeOut.Invoke());
            }
        }

        #endregion
    }
}