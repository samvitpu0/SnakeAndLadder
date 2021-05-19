using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsypherLibrary.SupportLibrary.Utils.Generics
{
    public abstract class GenericSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance = null;
        protected static bool IsSpawnedAlready = false;
        protected static int UID;
        static GameObject _persistantContainer;
        static GameObject _nonPersistantContainer;

        protected virtual bool IsPersistent
        {
            get { return false; }
        }


        private static GameObject PersistentContainer
        {
            get
            {
                if (_persistantContainer == null)
                {
                    var container = GameObject.Find("PersistentSingletons");
                    if (container == null)
                    {
                        container = new GameObject("PersistentSingletons");
                        container.AddComponent<Extensions.DontDestroyOnLoad>();
                    }

                    _persistantContainer = container;
                }

                return _persistantContainer;
            }
            set { _persistantContainer = value; }
        }

        private static GameObject NonPersistentContainer
        {
            get
            {
                if (_nonPersistantContainer == null)
                {
                    var container = GameObject.Find("NonPersistentSingletons");
                    if (container == null)
                    {
                        container = new GameObject("NonPersistentSingletons");
                    }

                    _nonPersistantContainer = container;
                }

                return _nonPersistantContainer;
            }
            set { _nonPersistantContainer = value; }
        }


        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;
                    if (_instance == null)
                    {
                        var go = new GameObject();
                        _instance = go.AddComponent<T>();
                        _instance.gameObject.name = _instance.GetType().Name;

                        //IsSpawnedAlready = true;
                    }
                }

                return _instance;
            }
        }


        public static bool HasInstance
        {
            get { return !IsDestroyed; }
        }

        public static bool IsDestroyed
        {
            get { return _instance == null; }
        }

        protected virtual void Awake()
        {
            //scene management
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            if (IsPersistent)
            {
                if (IsSpawnedAlready)
                {
                    Debug.Log("Deleting duplicate @" + name + ", IID: " + GetInstanceID());
                    DestroyImmediate(gameObject);
                }
                else
                {
                    gameObject.transform.SetParent(PersistentContainer.transform);
                    IsSpawnedAlready = true;
                    UID = GetInstanceID();
                }
            }
            else
            {
                gameObject.transform.SetParent(NonPersistentContainer.transform);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
        }

        protected virtual void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

            if (IsPersistent)
            {
                //only if the original singleton is destroyed
                if (GetInstanceID().Equals(UID))
                {
                    _instance = null;
                    IsSpawnedAlready = false;
                    RefreshStaticOnDestroy();
                }
            }
            else
            {
                _instance = null;
                IsSpawnedAlready = false;
            }
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnLevelLoaded(scene, mode);
            OnLevelLoaded(scene.buildIndex);
        }


        protected virtual void OnLevelLoaded(Scene scene, LoadSceneMode mode)
        {
        }

        protected virtual void OnLevelLoaded(int levelIndex)
        {
        }

        #region extra layer of control

        /// <summary>
        /// Implement if required to clean static references
        /// </summary>
        protected virtual void RefreshStaticOnDestroy()
        {
        }

        #endregion
    }
}