using System.Collections;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels
{
    public class UISceneLoader : UIPanel
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
                        var cDontDestroy = container.AddComponent<DontDestroyOnLoad>();

                        //this is very useful when full resetting games (clear data and reload from first scene)
                        cDontDestroy.CanBeDestroyed = false;
                    }

                    _container = container;
                }

                return _container;
            }
            set { _container = value; }
        }

        protected static UISceneLoader _instance = null;

        public static UISceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(UISceneLoader)) as UISceneLoader;
                    if (_instance == null)
                    {
                        var go = Instantiate(Resources.Load("DefinedPanels/UISceneLoader"), Container.transform) as GameObject;
                        _instance = go.GetComponent<UISceneLoader>();
                        //  _instance.GetCanvas.worldCamera = Camera.main;
                        _instance.gameObject.name = _instance.GetType().Name;
                    }

                    _instance.transform.SetParent(Container.transform);
                }

                //IsSpawnedAlready = true;
                return _instance;
            }
            set { _instance = value; }
        }

        int CriticalLayerSortingOrder = 2000;

        #endregion


        protected bool isLoaderOn = false;
        protected int _LoaderTriggerCount;
        protected Sequence _loadingSeq;

        [SerializeField]
        [Header("UI Scene Loader")]
        protected float AngleIncreaseFactor = -45f;

        [SerializeField]
        protected float DelayFactor = 0.1f;

        [SerializeField]
        protected Image Loader;

        protected string PreviousSceneName;

        #region Initialization

        protected override void Awake()
        {
            if (IsSpawnedAlready)
            {
                Debug.Log("Deleting duplicate @ " + name);
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
            GetCanvas.sortingOrder = CriticalLayerSortingOrder;

            _loadingSeq = DOTween.Sequence();
            _loadingSeq.Append(Loader.transform.DOLocalRotate(new Vector3(0, 0, -AngleIncreaseFactor), 0f, RotateMode.FastBeyond360).SetEase(Ease.Flash))
                .SetDelay(DelayFactor)
                .SetLoops(-1, LoopType.Incremental)
                .SetUpdate(UpdateType.Fixed, true);


            _loadingSeq.Pause(); //small hack to properly display the loading indicator
        }

        #endregion

        #region Actions

        protected void StartLoader()
        {
            _LoaderTriggerCount++;

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

        public void StopLoader(bool forceStop = false)
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
        }


        public void LoadScene(string sceneName, bool showLoader = true)
        {
            if (SceneManager.GetSceneByName(sceneName).buildIndex == 0)
            {
                //getting singletons
                var gSingletons = Resources.FindObjectsOfTypeAll<DontDestroyOnLoad>();

                //destroying singletons
                gSingletons.ForEach(x =>
                {
                    if (x.CanBeDestroyed)
                        Destroy(x.gameObject);
                });
            }

            if (showLoader)
                StartLoader();

            PreviousSceneName = SceneManager.GetActiveScene().name;
            StartCoroutine(LoadSceneInternal(sceneName));
        }

        public void LoadSceneIndex(int index, bool showLoader = true)
        {
            //if splash is loaded in-game, clear out all the managers
            if (index == 0)
            {
                //getting singletons
                var gSingletons = Resources.FindObjectsOfTypeAll<DontDestroyOnLoad>();

                //detroying singletons
                gSingletons.ForEach(x =>
                {
                    if (x.CanBeDestroyed)
                        Destroy(x.gameObject);
                });
            }

            if (showLoader)
                StartLoader();

            PreviousSceneName = SceneManager.GetActiveScene().name;
            StartCoroutine(LoadSceneInternal(index));
        }

        public void LoadPreviousScene(bool showLoader = true)
        {
            LoadScene(PreviousSceneName, showLoader);
        }

        public void ReloadCurrentScene(bool showLoader = true)
        {
            var currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene, showLoader);
        }

        #endregion

        #region Internal

        IEnumerator LoadSceneInternal(string sceneName)
        {
            Debug.Log("Loaded scene name: " + sceneName);
            yield return new WaitForSeconds(0.5f);
            var scene = SceneManager.LoadSceneAsync(sceneName);
            yield return scene;
            yield return new WaitForFixedUpdate();
            StopLoader();
        }

        IEnumerator LoadSceneInternal(int sceneIndex)
        {
            Debug.Log(sceneIndex);
            yield return new WaitForSeconds(0.5f);
            var scene = SceneManager.LoadSceneAsync(sceneIndex);
            yield return scene;
            yield return new WaitForFixedUpdate();
            StopLoader();
        }

        IEnumerator LoadSceneAdditiveInternal(string sceneName)
        {
            yield return new WaitForSeconds(0.5f);
            var scene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return scene;
            yield return new WaitForEndOfFrame();
            StopLoader();
        }

        #endregion

        #region Events

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

        #endregion
    }
}