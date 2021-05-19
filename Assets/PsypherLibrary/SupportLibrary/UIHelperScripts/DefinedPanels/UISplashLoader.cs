using DG.Tweening;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels
{
    public class UISplashLoader : UIPanel
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
                    var container = GameObject.Find("SplashLoaderContainer");
                    if (container == null)
                    {
                        container = new GameObject("SplashLoaderContainer");
                        container.AddComponent<DontDestroyOnLoad>();
                    }

                    _container = container;
                }

                return _container;
            }
            set { _container = value; }
        }

        protected static UISplashLoader _instance = null;

        public static UISplashLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(UISplashLoader)) as UISplashLoader;
                    if (_instance == null)
                    {
                        var go = Instantiate(Resources.Load("DefinedPanels/SplashLoader"), Container.transform) as GameObject;
                        _instance = go.GetComponent<UISplashLoader>();
                        //  _instance.GetCanvas.worldCamera = Camera.main;
                        _instance.gameObject.name = _instance.GetType().Name;
                    }

                    _instance.transform.SetParent(Container.transform);
                }

                return _instance;
            }
            set { _instance = value; }
        }

        int CriticalLayerSortingOrder = 1000;

        #endregion

        bool isLoaderOn = false;

        int _LoaderTriggerCount;
        Sequence _rotationSeq;

        [SerializeField]
        [Header("Splash Loader")]
        protected float AngleIncreaseFactor = -45f;

        [SerializeField]
        protected float DelayFactor = 0.1f;

        [SerializeField]
        protected Image Loader;

        [SerializeField]
        protected Text LoaderText;

        [SerializeField]
        protected Text VersionText;

        [SerializeField]
        protected Slider LoaderSlider;


        protected override void Awake()
        {
            if (IsSpawnedAlready)
            {
                Debug.Log("Deleting duplicate @ " + name);
                Destroy(gameObject);
            }

            if (transform.root == transform) //only if this is the root transform, else it gives a runtime warning
                DontDestroyOnLoad(this);

            if (LoaderText)
                LoaderText.Deactivate();

            IsSpawnedAlready = true;
            UID = GetInstanceID();
            InitializeLoader();
            base.Awake();

            //scene management
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        protected virtual void InitializeLoader()
        {
            if (LoaderSlider) LoaderSlider.value = 0;

            if (!Loader) return; //if loader do not exists

            _rotationSeq = DOTween.Sequence();
            _rotationSeq.Append(Loader.transform.DOLocalRotate(new Vector3(0, 0, -AngleIncreaseFactor), 0f, RotateMode.FastBeyond360).SetEase(Ease.Flash)
                    .SetUpdate(UpdateType.Fixed, true))
                .SetDelay(DelayFactor)
                .SetLoops(-1, LoopType.Incremental);

            _rotationSeq.Pause(); //small hack to properly display the loading indicator
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

            if (Loader)
                _rotationSeq.Pause();

            DeactivatePanel();
            LoaderText.Deactivate();

            if (GetCanvas)
            {
                GetCanvas.sortingOrder = -CriticalLayerSortingOrder;
            }
        }

        public virtual void StartLoader()
        {
            _LoaderTriggerCount++;
            LoaderText.Deactivate();
            VersionText.Deactivate();
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

            if (Loader)
                _rotationSeq.Restart();
        }

        public virtual void SetText(string inText)
        {
            if (!LoaderText.IsActive())
                LoaderText.Activate();

            LoaderText.SetText(inText);
        }

        public virtual void SetLoadingProgress(float progress)
        {
            if (LoaderSlider)
            {
                LoaderSlider.DOValue(progress, 0);
            }

            SetText(progress + "%");
        }

        public void SetVersion(string versionInfo)
        {
            if (!VersionText.IsActive())
            {
                VersionText.Activate();
            }

            VersionText.SetText("Content Version: " + versionInfo);
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


        //hacky fix
        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals(EScenes.Splash.GetDescription()))
            {
                this.Activate();
            }
            else
            {
                this.Deactivate();
            }
        }
    }
}