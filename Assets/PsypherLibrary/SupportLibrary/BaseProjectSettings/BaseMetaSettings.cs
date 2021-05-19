using System;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif


namespace PsypherLibrary.SupportLibrary.BaseProjectSettings
{
    [Serializable]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class BaseMetaSettings : GenericScriptableObject<BaseMetaSettings>
    {
        [Header("Notifications")]
        public bool EnableLocalNotifications;
    }
}