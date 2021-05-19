using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsypherLibrary.SupportLibrary.Extensions
{
    /// <summary>
    /// To use this GenericDontDestroyOnLoad, the name of every GameObject, to which this component is added, has to be unique 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericDontDestroyOnLoad<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static List<string> _originalItemsIDs = new List<string>();
        private static List<int> _originalItemsInstanceIds = new List<int>();
        private string _uid;

        protected virtual void Awake()
        {
            _uid = gameObject.name.GetUniqueId();

            // prevent additional objects being created on level reloads
            if (_originalItemsIDs != null && _originalItemsIDs.Any() && _originalItemsIDs.Contains(_uid))
            {
                Debug.Log("Deleting duplicate @" + name + ", IID: " + GetInstanceID());
                DestroyImmediate(gameObject);
                return;
            }

            _originalItemsIDs.AddUnique(_uid);
            _originalItemsInstanceIds.AddUnique(GetInstanceID());
            gameObject.AddComponent<DontDestroyOnLoad>();

            //debug
            _originalItemsIDs.ForEach(x => Debug.Log("Generic Don't Destroy Object: " + x));

            //scene management
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnLevelLoaded(scene);
        }


        protected virtual void OnLevelLoaded(Scene scene)
        {
        }

        protected virtual void OnDestroy()
        {
            //only if the original singleton is destroyed
            if (_originalItemsInstanceIds != null && _originalItemsInstanceIds.Any() && _originalItemsInstanceIds.Contains(GetInstanceID()))
            {
                _originalItemsIDs.FindAndRemove(x => x.Equals(_uid));
                _originalItemsInstanceIds.FindAndRemove(x => x.Equals(GetInstanceID()));

                //scene management
                SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _originalItemsIDs = null;
            _originalItemsInstanceIds = null;
            //applicationIsQuitting = true;
        }
    }
}