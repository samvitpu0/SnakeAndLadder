using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CategorySystem
{
    [Serializable]
    public class ThumbnailFrameDetail
    {
        public Sprite FrameSprite;
        public ThumbnailTypes ThumbnailType;
    }

    [Serializable]
    public class ThumbnailState
    {
        public Sprite ActiveSprite;
        public Sprite DeactiveSprite;
    }

    [Serializable]
    public class TEvent : UnityEvent<float>
    {
    }

    [Serializable]
    public class BEvent : UnityEvent<bool>
    {
    }

    public class ThumbnailHelper : MonoBehaviour
    {
        #region variable declaration

        private CategorySystemInfo _categorySystemInfo;

        public CategorySystemInfo CategorySystemInfo
        {
            get
            {
                if (_categorySystemInfo == null)
                {
                    var root = transform.root;
                    _categorySystemInfo = root.GetComponent<CategorySystemInfo>() ?? root.GetComponentInChildren<CategorySystemInfo>();
                }

                return _categorySystemInfo;
            }
        }

        private string _contentTypeName;

        [SerializeField]
        public string ContentTypeName
        {
            get { return _contentTypeName; }
            set
            {
                _contentTypeName = value;

                if (ContentTypeIcon == null) return;


                foreach (var contentType in CategorySystemInfo.SupportedContentTypes)
                {
                    if (value.Equals(contentType.Name))
                    {
                        ContentTypeIcon.SetImage(contentType.Icon);
                        ContentTypeIcon.Activate();
                        break; //break the loop after it found the match
                    }
                }
            }
        }

        private ThumbnailTypes _thumbnailType;

        public ThumbnailTypes ThumbnailType
        {
            get { return _thumbnailType; }
            set
            {
                _thumbnailType = value;

                if (ColorChangeText)
                {
                    switch (value)
                    {
                        case ThumbnailTypes.Category:
                            TitleText.color = CategoryTextColor;
                            break;

                        case ThumbnailTypes.Item:
                        case ThumbnailTypes.ItemMinimal:
                            TitleText.color = ItemTextColor;
                            break;
                    }
                }
            }
        }

        public Text TitleText;
        public Image ThumbnailImage;
        public float ClickResetTime = 1;
        public Color CategoryTextColor = Color.yellow;
        public Color ItemTextColor = Color.white;
        public bool ColorChangeText = false;
        public Text OptionalText;

        [Header("Item")]
        public CanvasGroup LoaderGroup;

        public Image LoaderImage;
        public GameObject PlayIcon;
        public GameObject DownloadIcon;
        public GameObject PremiumPanel;
        public Toggle FavToggle;
        public Image ContentTypeIcon;
        public object Data;
        public bool IsDownloading;

        public UnityEvent OnDownloadStarts;
        public TEvent Downloading;
        public BEvent IfDownloading;
        public UnityEvent OnDownloadCompleted;

        [Header("Frames")]
        public Image FrameR;

        public Image FrameL;

        [SerializeField]
        public List<ThumbnailFrameDetail> Frames;

        public Image HighlightImage;

        [SerializeField]
        public ThumbnailState Sprites;

        public Button ClickButton
        {
            get { return GetComponent<Button>(); }
        }

        public Toggle ClickToggle
        {
            get { return GetComponent<Toggle>(); }
        }

        private bool _isDownloaded = false;

        public bool IsDownloaded
        {
            get { return _isDownloaded; }
        }

        private bool _alreadyClicked = false;

        #endregion

        #region initialization

        void Awake()
        {
            if (FavToggle)
            {
                FavToggle.Deactivate();
            }

            if (LoaderGroup)
            {
                LoaderGroup.alpha = 0;
                PlayIcon.Deactivate();
                DownloadIcon.Deactivate();
                LoaderImage.Deactivate();
            }

            if (ContentTypeIcon)
            {
                ContentTypeIcon.Deactivate();
            }
        }

        #endregion

        #region actions

        public void SetContentType(ThumbnailTypes thumbType, string contentTypeName = "")
        {
            ThumbnailType = thumbType;
            ContentTypeName = contentTypeName;

            if (!FrameR || !FrameL)
                return;

            foreach (var frame in Frames)
            {
                if (frame.ThumbnailType == thumbType)
                {
                    FrameL.sprite = FrameR.sprite = frame.FrameSprite;
                }
            }
        }

        public virtual void SetDetails(string thumbnailName, string thumbnailUrl, Action onInitAction = null, Action onClick = null, Action onClickLock = null, Dictionary<string, object> optionalData = null)
        {
            onInitAction.SafeInvoke();

            TitleText.text = thumbnailName;
            var thumbnailImage = GetComponentInChildren<UIRemoteImage>();
            if (thumbnailImage)
                thumbnailImage.ImageURL = thumbnailUrl;

            if (OptionalText && optionalData != null)
            {
                OptionalText.SetText(optionalData["extraText"]);
            }

            if (ClickButton)
            {
                ClickButton.onClick.RemoveAllListeners(); //if any, removing previous listeners [safe mode]
                ClickButton.onClick.AddListener(() =>
                {
                    if (!_alreadyClicked)
                    {
                        if (PremiumPanel && PremiumPanel.activeSelf)
                        {
                            OpenStore(onClickLock);
                        }
                        else
                        {
                            CategorySystemInfo.AudioController.TriggerAudio("OnClick");
                            onClick.SafeInvoke();
                        }

                        //bug fix: double click - double item replication issue
                        _alreadyClicked = true;
                        StartCoroutine(ResetClick(ClickResetTime));
                    }
                });
            }
            else if (ClickToggle)
            {
                ClickToggle.onValueChanged.RemoveAllListeners(); //if any, removing previous listeners [safe mode]
                var tGroup = CategorySystemInfo.transform.GetComponent<ToggleGroup>();
                if (tGroup)
                {
                    ClickToggle.group = tGroup;
                }

                ClickToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (!_alreadyClicked && isOn)
                    {
                        if (PremiumPanel && PremiumPanel.activeSelf)
                        {
                            OpenStore(onClickLock);
                        }
                        else
                        {
                            CategorySystemInfo.AudioController.TriggerAudio("OnClick");
                            onClick.SafeInvoke();
                        }

                        //bug fix: double click - double item replication issue
                        _alreadyClicked = true;
                        StartCoroutine(ResetClick(ClickResetTime));
                    }
                });
            }
        }

        public void SetThumbnailState(bool isDownloaded, bool resetFavoriteButton = false, bool isFavorite = false, Action<bool> onFavoriteToggle = null)
        {
            _isDownloaded = isDownloaded;
            if (LoaderGroup)
            {
                LoaderGroup.DOFade(1, 0.5f).SetDelay(0.3f);
                LoaderImage.fillAmount = 0;
                LoaderImage.Deactivate();
                if (LocalDataManager.Instance.SaveData.AllowContentDownload)
                {
                    if (isDownloaded)
                    {
                        PlayIcon.Activate();
                        DownloadIcon.Deactivate();

                        if (OnDownloadCompleted != null)
                            OnDownloadCompleted.Invoke();

                        if (IfDownloading != null)
                            IfDownloading.Invoke(false);
                    }
                    else
                    {
                        PlayIcon.Deactivate();
                        DownloadIcon.Activate();
                    }
                }
                else
                {
                    LoaderImage.Deactivate();
                    PlayIcon.Activate();
                }

                if (FavToggle && resetFavoriteButton)
                {
                    FavToggle.onValueChanged.RemoveAllListeners();
                    FavToggle.isOn = isFavorite;
                    FavToggle.onValueChanged.AddListener((x) =>
                    {
                        onFavoriteToggle.SafeInvoke(x);
                        CategorySystemInfo.AudioController.TriggerAudio("OnArrow");
                    });

                    FavToggle.Activate();
                }

                _alreadyClicked = false;
            }
        }

        public void SetRelatedData(object data)
        {
            Data = data;
        }

        public void SetHighlighted(bool value)
        {
            if (!HighlightImage) return;

            HighlightImage.SetImage(value ? Sprites.ActiveSprite : Sprites.DeactiveSprite);
        }

        public void SetFreeContent(bool isFree)
        {
            if (!PremiumPanel) return;

            if (isFree)
            {
                PremiumPanel.Deactivate();

                if (LocalDataManager.Instance.SaveData.AllowContentDownload)
                {
                    if (_isDownloaded)
                    {
                        LoaderImage.Deactivate();
                        PlayIcon.Activate();
                    }
                    else
                    {
                        PlayIcon.Deactivate();
                        LoaderImage.Deactivate();
                        DownloadIcon.Activate();
                    }
                }
                else
                {
                    LoaderImage.Deactivate();
                    PlayIcon.Activate();
                }
            }
            else
            {
                PlayIcon.Deactivate();
                LoaderImage.Deactivate();
                DownloadIcon.Deactivate();
                PremiumPanel.Activate();
            }
        }

        public void ShowProgress(float progress)
        {
            if (!IsDownloading)
            {
                if (IfDownloading != null)
                    IfDownloading.Invoke(false);

                return;
            }

            var rProgress = progress / 100; //relative progress

            if (LoaderImage)
            {
                if (!LoaderImage.IsActive())
                {
                    LoaderImage.Activate();
                    DownloadIcon.Deactivate();

                    if (OnDownloadStarts != null)
                    {
                        OnDownloadStarts.Invoke();
                    }

                    if (IfDownloading != null)
                    {
                        IfDownloading.Invoke(true);
                    }
                }

                LoaderImage.fillAmount = rProgress;
                if (Downloading != null)
                    Downloading.Invoke(rProgress);
            }

            _alreadyClicked = true;
        }

        public void SetTitleColor(Color color)
        {
            TitleText.color = color;
        }

        /// <summary>
        /// Override to provide custom open store. It's a good place to check for parental lock
        /// </summary>
        /// <param name="onClickStore"></param>
        public virtual void OpenStore(Action onClickStore)
        {
            onClickStore.SafeInvoke();
        }

        //bug fix
        IEnumerator ResetClick(float time)
        {
            yield return new WaitForSeconds(time);

            if (LoaderImage)
            {
                if (!LoaderImage.IsActive())
                    _alreadyClicked = false;
            }
            else
            {
                _alreadyClicked = false;
            }
        }

        #endregion
    }
}
#endif