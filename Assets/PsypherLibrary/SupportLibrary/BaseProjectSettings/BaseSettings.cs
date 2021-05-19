using System;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEditor;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.BaseProjectSettings
{
    [Serializable]
    public enum FileNameType
    {
        GUID,
        FileName
    }

    [Serializable]
    public struct KeystoreData
    {
        public string Password;
        public string AliasName;
        public string AliasPassword;
    }

    [Serializable]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class BaseSettings : GenericScriptableObject<BaseSettings>
    {
        [Header("Data")]
        [Tooltip("To use this feature, use GetUniqueId to your filename or URL when saving using Utility [Class Utilities] methods")]
        public FileNameType FileNameSaveType;

        public PiUtilities.ESaveTypes DataSaveType;

        [Header("Build")] public KeystoreData Keystore;

        [Header("Asset Bundles")]
        [Tooltip(
            "Should compile the asset bundles into [StreamingAssets] Folder, else it will create a folder named [AssetBundle] inside [Assets]")]
        public bool ToStreamingAsset;

        [Tooltip("Path relative to the asset bundle compilation folder")]
        public string RelativePath;

        [Header("Config End Points")] [SerializeField]
        public EEndPoints EndPoints;

        [Header("Meta Settings")]
        [Tooltip("Make the user as premium user; to be use during development")]
        public bool IsPremiumUser = false;

        [Tooltip("Enables mock ads; to be use during development, before ads SDKs are integrated")]
        public bool EnableMockAds = false;

        [Tooltip("Force ads regardless of Premium state, use for debug")]
        public bool ForceAds = false;

        [Tooltip("Should store analytical data in phone storage")]
        public bool LocalAnalytics = false;

#if UNITY_WEBGL
        [Header("WebGL")] [Tooltip("Memory size of the webGL build")]
        public int MemorySize = 256;
#endif
        [Header("Debug")]
        public bool IsDebugBuild = false;

        public bool EnableLogs = true;
    }
}