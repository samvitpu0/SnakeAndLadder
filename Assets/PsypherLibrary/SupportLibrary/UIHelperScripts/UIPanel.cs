using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        public bool IsPanelActive;
        public bool DisableGameObjectOnDeactivation = false;
        public bool IsDirty = true;
        public bool IsActiveOnStart = true;
        public bool CheckForVisiblity = false;
        public bool IsVisible = false;
        public bool CheckOnlyBoundsVisibility = false;

        //[HelpBox("This is help")]

        public float FadeInTime = 0.2f;
        public float FadeOutTime = 0.2f;

        [Header("Back Functionality")]
        [Tooltip("Always use ViewController to activate a UIScene, for this back buttons to work.")]
        public List<Button> BackButtons = new List<Button>(); //if there is more than 2 back buttons viz. back and cancel button

        public bool DisableOnBack = false;
        public bool DestroyOnBack = false;
        public bool UseHardwareBack;

        [Tooltip("Should this panel closes if click is triggered outside its boundary?")]
        public bool CloseOnOutClick;

        [Tooltip("A green rectangle to show the approximate extent of the local bounds")]
        public bool DrawBoundGizmo;

        public UIScrollable ContentHolder;
        public Text PanelTitle;
        public List<UIPanel> ChildUiPanelCache = new List<UIPanel>();

        //unity actions
        [Header("Call backs")] public Action OnBecomeVisible;

        public Action OnBecomeInvisible;

        [SerializeField] private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null && gameObject != null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
            set { _canvasGroup = value; }
        }


        [SerializeField] private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
            set { _rectTransform = value; }
        }

        [SerializeField] private RectTransform _canvasRectTransform;

        public RectTransform CanvasRectTransform
        {
            get
            {
                if (_canvasRectTransform == null)
                    _canvasRectTransform = GetCanvas.GetComponent<RectTransform>();

                return _canvasRectTransform;
            }
            set { _canvasRectTransform = value; }
        }

        //call back helpers
        public bool InvokeEventsOnInitialCall = true;
        public int ActivateCount { get; set; }
        public int DeactivateCount { get; set; }

        //system action
        public Action CallBackOnActivate = null;
        public Action CallBackOnReadyToActivate = null;
        public Action CallBackOnReadyToDeactivate = null;
        public Action CallBackOnDeactivate = null;

        public UnityEvent OnReadyToActivate;
        public UnityEvent OnActivate;
        public UnityEvent OnReadyToDeactivate;
        public UnityEvent OnDeactivate;


        [SerializeField] private Canvas _canvas;

        private Vector3 _curPos;
        private Vector3 _lastPos;
        private float _prevAlpha;
        private int _oldSortOrder;

        public Canvas GetCanvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = transform.root.GetComponent<Canvas>();
                    if (_canvas == null)
                    {
                        _canvas = transform.root.GetComponentInChildren<Canvas>();
                    }

                    _oldSortOrder = _canvas.sortingOrder;
                }

                return _canvas;
            }
        }

        public Canvas SetCanvas
        {
            set { _canvas = value; }
        }

        public bool UseSceneViewController;

        [SerializeField] private UIViewController _uiViewController;

        public UIViewController ViewController
        {
            get
            {
                if (_uiViewController == null)
                {
                    if (UseSceneViewController)
                    {
                        var sViewController = FindObjectOfType<SceneUIViewController>();
                        if (sViewController)
                        {
                            _uiViewController = sViewController.ViewController;
                        }
                        else
                        {
                            if (_uiViewController == null)
                            {
                                var gObj = new GameObject("SceneUIViewController", typeof(SceneUIViewController));
                                _uiViewController = gObj.GetComponent<SceneUIViewController>().ViewController;
                            }
                        }
                    }
                    else
                    {
                        _uiViewController = transform.root.GetComponent<UIViewController>();
                        if (_uiViewController == null)
                        {
                            _uiViewController = transform.root.gameObject.AddComponent<UIViewController>();
                        }
                    }
                }

                return _uiViewController;
            }
        }

        protected virtual void Awake()
        {
            try
            {
                if (IsActiveOnStart)
                    ActivatePanel();
                else
                {
                    CanvasGroup.alpha = 0;
                    DeactivatePanel();
                }

                if (BackButtons.Any()) //if there should be more than 1 back button
                {
                    BackButtons.ForEach(x => { x.onClick.AddListener(PressBack); });
                }

                _lastPos = transform.position;
                _prevAlpha = CanvasGroup.alpha;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected virtual void OnDestroy()
        {
            //when this gameobject is destroy [or during scene change], make sure to kill the tweens
            DOTween.Kill(name + "@" + GetInstanceID() + "Activate");
            DOTween.Kill(name + "@" + GetInstanceID() + "Deactivate");
        }

        protected virtual void OnEnable()
        {
            if (CloseOnOutClick)
            {
                ViewController.OnMouseDownOnObject += OnClick_General;
            }
        }

        protected virtual void OnDisable()
        {
            if (UseHardwareBack)
            {
                HardwareInputManager.OnBack -= PressBack;
            }

            if (_uiViewController != null && CloseOnOutClick)
            {
                ViewController.OnMouseDownOnObject -= OnClick_General;
            }
        }


        public virtual Tweener ShowAnimation()
        {
            if (UseHardwareBack)
            {
                HardwareInputManager.OnBack += PressBack;
            }

            return _canvasGroup ? _canvasGroup.DOFade(1, FadeInTime).SetUpdate(UpdateType.Fixed, true) : null;
        }

        public virtual Tweener HideAnimation()
        {
            if (UseHardwareBack)
            {
                HardwareInputManager.OnBack -= PressBack;
            }

            return _canvasGroup ? _canvasGroup.DOFade(0, FadeOutTime).SetUpdate(UpdateType.Fixed, true) : null;
        }

        public void DeactivateControl()
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            IsPanelActive = false;

            if (DisableGameObjectOnDeactivation)
                gameObject.Deactivate();
        }

        public void OnToggle(bool isOn)
        {
            if (isOn) ActivatePanel();
            else DeactivatePanel();
        }

        public void OnToggleOpposite(bool isOn)
        {
            if (isOn) DeactivatePanel();
            else ActivatePanel();
        }

        public void OnToggleSwitch()
        {
            if (IsPanelActive) DeactivatePanel();
            else ActivatePanel();
        }

        public void ActivateControl()
        {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            IsPanelActive = true;
        }

        private void CacheUiPanels()
        {
            if (!IsDirty)
                return;
            ChildUiPanelCache.Clear();
            GetChildUiPanels(gameObject);
            IsDirty = false;
        }

        private void GetChildUiPanels(GameObject go)
        {
            Transform t = go.transform;
            foreach (Transform child in t)
            {
                var childPanel = child.GetComponent<UIPanel>();
                if (childPanel != null)
                {
                    ChildUiPanelCache.Add(childPanel);
                    GetChildUiPanels(child.gameObject);
                }
            }
        }

        public virtual void DeactivatePanel(bool ignoreUnityTimeScale = true)
        {
            CacheUiPanels();

            DOTween.Kill(name + "@" + GetInstanceID() + "Activate");

            DeactivateCount++;

            HideAnimation().OnStart(() =>
            {
                DeactivateControl();

                if (!InvokeEventsOnInitialCall && DeactivateCount <= 1)
                    return;

                //system action
                CallBackOnReadyToDeactivate.SafeInvoke();
                //unity action
                OnReadyToDeactivate.SafeInvoke();
            }).OnComplete(() =>
            {
                IsVisible = false;

                if (DisableOnBack)
                    this.Deactivate();

                if (DestroyOnBack)
                    Destroy(gameObject);

                if (!InvokeEventsOnInitialCall && DeactivateCount <= 1)
                    return;

                //system action
                CallBackOnDeactivate.SafeInvoke();
                //unity Action
                OnDeactivate.SafeInvoke();
            }).SetId(name + "@" + GetInstanceID() + "Deactivate").SetUpdate(ignoreUnityTimeScale);
        }

        public virtual void ActivatePanel(bool ignoreUnityTimeScale = true)
        {
            if (gameObject)
                gameObject.Activate();

            CacheUiPanels();
            DOTween.Kill(name + "@" + GetInstanceID() + "Deactivate");

            ActivateCount++;

            ShowAnimation().OnStart(() =>
                {
                    if (!InvokeEventsOnInitialCall && ActivateCount <= 1)
                        return;

                    //system Action
                    CallBackOnReadyToActivate.SafeInvoke();
                    //unity action
                    OnReadyToActivate.SafeInvoke();
                })
                .OnComplete(() =>
                {
                    IsVisible = true;
                    ActivateControl();

                    if (!InvokeEventsOnInitialCall && ActivateCount <= 1)
                        return;

                    //system Action
                    CallBackOnActivate.SafeInvoke();
                    //unity action
                    OnActivate.SafeInvoke();
                }).SetId(name + "@" + GetInstanceID() + "Activate").SetUpdate(ignoreUnityTimeScale);
        }

        public virtual void OnVisible()
        {
        }

        public virtual void OnInvisible()
        {
        }

        public bool IsAlphaOn()
        {
            return CheckOnlyBoundsVisibility || CanvasGroup.alpha > 0;
        }

        private void CheckVisibility()
        {
            var screenBounds = new Bounds(CanvasRectTransform.rect.center, CanvasRectTransform.rect.size);

            Bounds myBounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(CanvasRectTransform.transform,
                    transform);
            bool isOutOfBounds = myBounds.max.x < screenBounds.min.x || myBounds.min.x > screenBounds.max.x ||
                                 myBounds.min.y > screenBounds.max.y || myBounds.max.y < screenBounds.min.y;
            if (!isOutOfBounds && IsAlphaOn())
            {
                if (!IsVisible)
                {
                    IsVisible = true;
                    CacheUiPanels();
                    ChildUiPanelCache.ForEach(x =>
                    {
                        x.IsVisible = true;
                        x.OnBecomeVisible.SafeInvoke();
                        x.OnVisible();
                    });
                    OnVisible();
                    OnBecomeVisible.SafeInvoke();
                }
            }
            else if (isOutOfBounds || !IsAlphaOn())
            {
                if (IsVisible)
                {
                    IsVisible = false;
                    CacheUiPanels();
                    ChildUiPanelCache.ForEach(x =>
                    {
                        x.IsVisible = false;
                        x.OnBecomeInvisible.SafeInvoke();
                        x.OnInvisible();
                    });
                    OnInvisible();
                    OnBecomeInvisible.SafeInvoke();
                }
            }
        }

        private bool IsMoving()
        {
            if (transform.position == _lastPos)
            {
                return false;
            }
            else
            {
                _lastPos = transform.position;
                return true;
            }
        }

        private bool HasAlphaChanged()
        {
            if (CanvasGroup.alpha == _prevAlpha)
            {
                return false;
            }
            else
            {
                _prevAlpha = CanvasGroup.alpha;
                return true;
            }
        }

        protected virtual void Update()
        {
            if (CheckForVisiblity)
            {
                //   if (IsMoving() || HasAlphaChanged())
                CheckVisibility();
            }
        }

        //to set basic datas
        public virtual void SetBasicData(Dictionary<string, object> data)
        {
        }

        public virtual void PressBack()
        {
            if (ViewController != null && ViewController.CurrentUI.mPanel == this)
            {
                ViewController.Back();
                var backPanelInfo = ViewController.LastClosedPanel;
                if (backPanelInfo == null) //if no information received from viewController
                {
                    DeactivatePanel();
                }
            }
            else // if this is not a part of the view controller
            {
                DeactivatePanel();
            }
        }

        private void OnClick_General(Vector2 mousePos, GameObject currentSelectedUI)
        {
            if (!CloseOnOutClick || !IsPanelActive) return;

            var root = transform.root;
            var rootCanvas = root.GetComponent<Canvas>() != null ? root.GetComponent<Canvas>() : root.gameObject.FindComponentInAllChildren<Canvas>();
            var rootCanvasScale = rootCanvas.transform.localScale.x;
            if (!RectTransform.GetTotalBounds(rootCanvasScale).Contains(mousePos))
            {
                PressBack();
            }
        }

        //debug to check bounds
        private void OnDrawGizmos()
        {
            if (DrawBoundGizmo)
            {
                var root = transform.root;
                var rootCanvas = root.GetComponent<Canvas>() != null ? root.GetComponent<Canvas>() : root.gameObject.FindComponentInAllChildren<Canvas>();
                var rootCanvasScale = rootCanvas.transform.localScale.x;
                Gizmos.color = Color.green;
                Gizmos.DrawCube(RectTransform.GetTotalBounds(rootCanvasScale).center,
                    RectTransform.GetTotalBounds(rootCanvasScale).size);
            }
        }

        public void SetLayerCritical(int newSortingOrder = 1000, bool relativeToProvidedValue = false)
        {
            var relativeOrder = newSortingOrder * _oldSortOrder;
            GetCanvas.sortingOrder = relativeToProvidedValue ? relativeOrder : newSortingOrder;
        }

        public UIViewController GetViewController()
        {
            return _uiViewController;
        }
    }
}