using System;
using System.IO;
using System.Linq;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using PsypherLibrary.SupportLibrary.Utils.FileManager;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace PsypherLibrary.SupportLibrary.Utils.Generics
{
    public class GenericScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        #region BaseRequirements

        protected static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        var allScriptableObjects = Resources.LoadAll("", typeof(T));

                        var objToLoad = allScriptableObjects.First();
                        Debug.Log("SO:: " + objToLoad);
                        _instance = objToLoad as T;
                    }
                    catch (Exception)
                    {
                        _instance = Resources.Load<T>(nameof(T));
                    }


                    if (_instance == null)
                    {
                        _instance = CreateInstance<T>();
#if UNITY_EDITOR

                        FileStaticAPI.CreateFolder(BaseConstants.ProjectSettingsPath);

                        string fullPath = Path.Combine(Path.Combine("Assets", BaseConstants.ProjectSettingsPath),
                            typeof(T).Name + BaseConstants.ProjectSettingsAssetExtension
                        );

                        AssetDatabase.CreateAsset(_instance, fullPath);
#endif
                    }
                }

                return _instance;
            }
        }

        #endregion
    }
}