using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels
{
    public class UIToastNotification : UIPanel
    {
        #region Singleton

        private static bool _isSpawnedAlready = false;
        protected static int UID;
        private static UIToastNotification _instance = null;

        public static UIToastNotification Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(UIToastNotification)) as UIToastNotification;
                    if (_instance == null)
                    {
                        var go = Instantiate(Resources.Load("DefinedPanels/Toast"), Container.transform) as GameObject;
                        _instance = go.GetComponent<UIToastNotification>();
                        //_instance.GetCanvas.worldCamera = Camera.main;
                        _instance.gameObject.name = _instance.GetType().Name;
                    }

                    _instance.transform.SetParent(Container.transform);
                }

                _isSpawnedAlready = true;
                return _instance;
            }
            set { _instance = value; }
        }

        static GameObject _container;

        static GameObject Container
        {
            get
            {
                if (_container == null)
                {
                    var container = GameObject.Find("ToastContainer");
                    if (container == null)
                    {
                        container = new GameObject("ToastContainer");
                        container.AddComponent<DontDestroyOnLoad>();
                    }

                    _container = container;
                }

                return _container;
            }
            set { _container = value; }
        }

        int CriticalLayerSortingOrder = 1005;

        #endregion

        [SerializeField]
        [Header("UI Toast Notification")]
        private Text _toastText = null;

        public Text GetToastText
        {
            get { return _toastText; }
        }


        #region Initialization

        protected override void Awake()
        {
            if (_isSpawnedAlready)
            {
                Debug.Log("Deleting duplicate");
                Destroy(gameObject);
            }

            // DontDestroyOnLoad(this);
            _isSpawnedAlready = true;
            UID = GetInstanceID();
            GetCanvas.sortingOrder = CriticalLayerSortingOrder;
            base.Awake();
        }

        #endregion

        #region Actions

        public void TriggerToast(string text = null, float time = -1.0f)
        {
            KillToast();
            GetToastText.SetText(text);
            ActivatePanel();
            CancelInvoke();
            if (time > 0)
                Invoke("KillToast", time);
        }

        public void KillToast()
        {
            DeactivatePanel();
            GetToastText.SetText();
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