using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels
{
    public class UILoader : UIPanel
    {
        #region Singleton

        protected static bool IsSpawnedAlready = false;
        static GameObject _container;
        protected static int UID;

        protected static GameObject Container
        {
            get
            {
                if (_container == null)
                {
                    var container = GameObject.Find("LoaderContainer");
                    if (container == null)
                    {
                        container = new GameObject("LoaderContainer");
                        container.AddComponent<DontDestroyOnLoad>();
                    }

                    _container = container;
                }

                return _container;
            }
            set { _container = value; }
        }

        protected static UILoader _instance = null;

        public static UILoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(UILoader)) as UILoader;
                    if (_instance == null)
                    {
                        var go = Instantiate(Resources.Load("DefinedPanels/Loader"), Container.transform) as GameObject;
                        _instance = go.GetComponent<UILoader>();
                        //  _instance.GetCanvas.worldCamera = Camera.main;
                        _instance.gameObject.name = _instance.GetType().Name;
                    }

                    _instance.transform.SetParent(Container.transform);
                }

                IsSpawnedAlready = true;
                return _instance;
            }
            set { _instance = value; }
        }

        int CriticalLayerSortingOrder = 1000;

        #endregion

        protected bool isLoaderOn = false;
        protected int _LoaderTriggerCount;

        protected Sequence _loadingSeq;

        [SerializeField]
        [Header("UI Loader")]
        private float AngleIncreaseFactor = -45f;

        [SerializeField]
        private float DelayFactor = 0.1f;

        [SerializeField]
        private Image Spinner = null;

        [SerializeField]
        private Text LoaderText = null;


        protected override void Awake()
        {
            if (LoaderText)
                LoaderText.Deactivate();

            if (IsSpawnedAlready)
            {
                Debug.Log("Deleting duplicate");
                Destroy(gameObject);
            }

            if (transform.root == transform) //only if this is the root transform, else it gives a runtime warning
                DontDestroyOnLoad(this);

            IsSpawnedAlready = true;
            UID = GetInstanceID();
            InitializeLoader();
            GetCanvas.sortingOrder = CriticalLayerSortingOrder;
            base.Awake();
        }

        protected virtual void InitializeLoader()
        {
            _loadingSeq = DOTween.Sequence();
            _loadingSeq.Append(Spinner.transform.DOLocalRotate(new Vector3(0, 0, -AngleIncreaseFactor), 0f, RotateMode.FastBeyond360).SetEase(Ease.Flash)
                    .SetUpdate(UpdateType.Fixed))
                .SetDelay(DelayFactor)
                .SetLoops(-1, LoopType.Incremental);

            _loadingSeq.Pause(); //small hack to properly display the loading indicator
        }

        public virtual void StopLoader(bool forceStop = false)
        {
            _LoaderTriggerCount--;
            _LoaderTriggerCount = Mathf.Clamp(_LoaderTriggerCount, 0, _LoaderTriggerCount);

            //Debug.Log("Loader Count: " + _LoaderTriggerCount);

            if (_LoaderTriggerCount > 0 && forceStop == false)
            {
                return;
            }

            isLoaderOn = false;
            _loadingSeq.Pause();

            DeactivatePanel();
            LoaderText.Deactivate();
        }

        public virtual void StartLoader()
        {
            _LoaderTriggerCount++;
            LoaderText.Deactivate();
            //  Debug.Log("Loader Count: " + _LoaderTriggerCount);

            if (isLoaderOn)
            {
                return;
            }

            if (GetCanvas)
            {
                GetCanvas.sortingOrder = CriticalLayerSortingOrder;
            }

            isLoaderOn = true;
            ActivatePanel();

            _loadingSeq.Restart(true);
        }

        public virtual void SetText(string inText)
        {
            if (!LoaderText.IsActive())
                LoaderText.Activate();

            if (LoaderText)
            {
                LoaderText.SetText(inText);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //only if the original singleton is destroyed
            if (GetInstanceID().Equals(UID))
            {
                _instance = null;
                IsSpawnedAlready = false;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
        }

        //void OnLevelWasLoaded(int level)
        //{
        //    Instance.GetCanvas.worldCamera = Camera.main;
        //}
    }
}