#if UNITY_EDITOR
using System;
using System.IO;
using PsypherLibrary.SupportLibrary.Utils.FileManager;
using UnityEditor;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.BaseProjectSettings
{
    [Serializable]
    [InitializeOnLoad]
    public class PsypherConfig : ScriptableObject
    {
        #region BaseRequirements

        const string GameSettingsAssetName = "Project-Config";

        private static PsypherConfig _instance;

        public static PsypherConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load(Path.Combine(BaseConstants.RelativeSettingsPath, GameSettingsAssetName)) as PsypherConfig;

                    if (_instance == null)
                    {
                        _instance = CreateInstance<PsypherConfig>();

                        if (!Application.isEditor) return _instance;

                        FileStaticAPI.CreateFolder(BaseConstants.ProjectSettingsPath);
                        var fullPath = Path.Combine(Path.Combine("Assets", BaseConstants.ProjectSettingsPath),
                            GameSettingsAssetName + BaseConstants.ProjectSettingsAssetExtension
                        );

                        AssetDatabase.CreateAsset(_instance, fullPath);
                    }
                }

                return _instance;
            }
        }

        #endregion

        [Header("Settings Override")]
        public MonoScript SubSettingsClass;

        public MonoScript SubMetaSettingsClass;
    }
}
#endif