using System;
using System.ComponentModel;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPopupBox : UIPanel
    {
        #region Singleton

        private static UIPopupBox _instance = null;
        static GameObject _container;
        private static bool _isSpawnedAlready = false;
        protected static int UID;

        static GameObject Container
        {
            get
            {
                if (_container == null)
                {
                    var container = GameObject.Find("PopUpContainer");
                    if (container == null)
                    {
                        container = new GameObject("PopUpContainer");
                        container.AddComponent<DontDestroyOnLoad>();
                    }

                    _container = container;
                }

                return _container;
            }
            set { _container = value; }
        }

        public static UIPopupBox Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(UIPopupBox)) as UIPopupBox;
                    if (_instance == null)
                    {
                        var go = Instantiate(Resources.Load("DefinedPanels/PopUpBox"), Container.transform) as GameObject;
                        _instance = go.GetComponent<UIPopupBox>();
                        // _instance.GetCanvas.worldCamera = Camera.main;
                        _instance.gameObject.name = _instance.GetType().Name;
                    }

                    _instance.transform.SetParent(Container.transform);
                }

                _isSpawnedAlready = true;
                return _instance;
            }
            set { _instance = value; }
        }

        int CriticalLayerSortingOrder = 1001;

        #endregion

        private Action OnClickYes;
        private Action OnClickNo;
        private Action OnClickOk;
        private Action OnClickClose;

        [Header("PopupBox UI")]
        public GameObject OKButton;

        public GameObject YesButton;
        public GameObject NoButton;
        public GameObject CloseButton;

        public Image ImageAnnouncement;
        public Text Description;
        public Image ImageOnlyAnnouncement;

        public RectTransform ContainerPanel;

        public RectTransform DescriptionPanel;
        public RectTransform ImageOnlyPanel;


        #region Initialization

        protected override void Awake()
        {
            if (_isSpawnedAlready)
            {
                Debug.Log("Deleting duplicate");
                Destroy(gameObject);
            }

            //DontDestroyOnLoad(this);
            _isSpawnedAlready = true;
            UID = GetInstanceID();
            GetCanvas.sortingOrder = CriticalLayerSortingOrder;
            base.Awake();

            Initialize();
        }

        protected void Initialize()
        {
            //deactivating the content panels
            DescriptionPanel.Deactivate();
            ImageOnlyPanel.Deactivate();
        }

        #endregion

        #region Actions

        public void SetDataYesNo(string displayText, Action yesImplementation, Action noImplementation, EPanelSize dSize = EPanelSize.MediumPortrait, bool reset = false,
            bool shouldUseAcceptAndDecline = false, UIType uiType = UIType.NormalView)
        {
            if (reset) ResetFormat();

            DescriptionPanel.Activate();
            ImageOnlyPanel.Deactivate();

            Description.SetText(displayText);
            OnClickYes = yesImplementation;
            OnClickNo = noImplementation;
            OnClickOk = null;
            OnClickClose = null;

            if (YesButton != null)
            {
                if (shouldUseAcceptAndDecline)
                {
                    YesButton.GetComponentInChildren<Text>().SetText("Accept");
                }

                YesButton.Activate();
            }

            if (NoButton != null)
            {
                if (shouldUseAcceptAndDecline)
                {
                    NoButton.GetComponentInChildren<Text>().SetText("Decline");
                }

                NoButton.Activate();
            }

            if (OKButton != null)
            {
                OKButton.Deactivate();
            }

            if (CloseButton != null)
            {
                CloseButton.Deactivate();
            }

            Description.Activate();
            ImageAnnouncement.Deactivate();

            AdjustDialogSize(dSize);

            switch (uiType)
            {
                case UIType.StackedView:
                {
                    ViewController.ActivateWithStackView(this);
                }
                    break;
                case UIType.NormalView:
                {
                    ViewController.ActivateWithNormalView(this);
                }
                    break;
                case UIType.BaseView:
                {
                    Debug.LogWarning("base type is not supported");
                }
                    break;
            }
        }

        public void SetDataOk(string displayText, Action OkImplementation, EPanelSize dSize = EPanelSize.MediumPortrait, bool reset = false, UIType uiType = UIType.NormalView)
        {
            if (reset) ResetFormat();

            DescriptionPanel.Activate();
            ImageOnlyPanel.Deactivate();

            Description.SetText(displayText);
            OnClickOk = OkImplementation;
            OnClickYes = null;
            OnClickNo = null;
            OnClickClose = null;

            if (YesButton != null)
                YesButton.Deactivate();
            if (NoButton != null)
                NoButton.Deactivate();
            if (OKButton != null)
                OKButton.Activate();
            if (CloseButton != null)
                CloseButton.Deactivate();

            Description.Activate();
            ImageAnnouncement.Deactivate();

            AdjustDialogSize(dSize);

            switch (uiType)
            {
                case UIType.StackedView:
                {
                    ViewController.ActivateWithStackView(this);
                }
                    break;
                case UIType.NormalView:
                {
                    ViewController.ActivateWithNormalView(this);
                }
                    break;
                case UIType.BaseView:
                {
                    Debug.LogWarning("base type is not supported");
                }
                    break;
            }
        }

        public void SetDataImageAnnouncement(Sprite toDisplay, Action closeImplementation, EPanelSize dSize = EPanelSize.MediumLandscape, UIType uiType = UIType.NormalView)
        {
            if (toDisplay == null) return;

            DescriptionPanel.Deactivate();
            ImageOnlyPanel.Activate();

            ImageOnlyAnnouncement.sprite = toDisplay;

            OnClickClose = closeImplementation;
            OnClickOk = null;
            OnClickYes = null;
            OnClickNo = null;

            if (YesButton != null)
                YesButton.Deactivate();
            if (NoButton != null)
                NoButton.Deactivate();
            if (OKButton != null)
                OKButton.Deactivate();
            if (CloseButton != null)
                CloseButton.Activate();

            AdjustDialogSize(dSize);

            switch (uiType)
            {
                case UIType.StackedView:
                {
                    ViewController.ActivateWithStackView(this);
                }
                    break;
                case UIType.NormalView:
                {
                    ViewController.ActivateWithNormalView(this);
                }
                    break;
                case UIType.BaseView:
                {
                    Debug.LogWarning("base type is not supported");
                }
                    break;
            }
        }

        public void SetImageProperties(Sprite imageSprite = null, Color? newColor = null, bool keepAspectRatio = false)
        {
            ImageAnnouncement.Activate();
            ImageAnnouncement.sprite = imageSprite;
            ImageAnnouncement.DOColor(newColor ?? Color.white, 0.5f);
            ImageAnnouncement.preserveAspect = keepAspectRatio;
        }

        public void SetTextProperties(TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal, Color? newColor = null)
        {
            Description.alignment = alignment;
            Description.fontStyle = style;
            Description.color = newColor ?? Description.color;
            //Description.alignment;
        }

        void ResetFormat()
        {
            SetTextProperties();
        }

        #endregion

        #region Internal Actions

        void AdjustDialogSize(EPanelSize dSize)
        {
            var desc = dSize.GetDescription().Split('|', (char) StringSplitOptions.RemoveEmptyEntries);
            var sizeVec = new Vector2(desc[0].ToInt(), desc[1].ToInt());

            //can change to percentage based, if problems occured

            ContainerPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetCanvas.GetComponent<RectTransform>().sizeDelta.x * (sizeVec.x / 100));
            ContainerPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetCanvas.GetComponent<RectTransform>().sizeDelta.y * (sizeVec.y / 100));
        }

        #endregion

        #region Events

        public void OnClose()
        {
            CallBackOnDeactivate += () =>
            {
                OnClickClose.SafeInvoke();
                CallBackOnDeactivate = null;
            };
            ViewController.Back();
        }

        public void OnYes()
        {
            CallBackOnDeactivate += () =>
            {
                OnClickYes.SafeInvoke();
                CallBackOnDeactivate = null;
            };
            ViewController.Back();
        }

        public void OnNo()
        {
            CallBackOnDeactivate += () =>
            {
                OnClickNo.SafeInvoke();
                CallBackOnDeactivate = null;
            };
            ViewController.Back();
        }

        public void OnOk()
        {
            CallBackOnDeactivate += () =>
            {
                OnClickOk.SafeInvoke();
                CallBackOnDeactivate = null;
            };
            ViewController.Back();
        }


        //void OnLevelWasLoaded(int level)
        //{
        //    Instance.GetCanvas.worldCamera = Camera.main;
        //}

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //only if the original singleton is destroyed
            if (GetInstanceID().Equals(UID))
            {
                _instance = null;
                _isSpawnedAlready = false;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
        }

        #endregion
    }
}