using System.IO;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.Utils.FileManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif


namespace PsypherLibrary.SupportLibrary.Editor
{
    public class EditorUtilities
    {
#if UNITY_EDITOR

        [MenuItem("Tools/Clear Cache")]
        private static void ClearPlayerPrefs()
        {
            FileManager.RemoveAllData();
        }

        [MenuItem("Tools/Settings/Psypher Config")]
        private static void OpenPsypherConfig()
        {
            Selection.activeObject = PsypherConfig.Instance;
        }


        [MenuItem("Tools/Settings/Project-Settings")]
        private static void OpenProjectSettings()
        {
            var subSettings = PsypherConfig.Instance.SubSettingsClass;

            if (subSettings)
            {
                var classType = subSettings.GetClass();
                Selection.activeObject = GetSettingObjectOfType(classType);
            }
            else
            {
                Selection.activeObject = BaseSettings.Instance;
            }
        }

        [MenuItem("Tools/Settings/MetaGame-Settings")]
        private static void OpenMetaGameSettings()
        {
            Selection.activeObject = BaseMetaSettings.Instance;
        }

        [MenuItem("Tools/Open Save Folder")]
        private static void ShowExplorer()
        {
            var itemPath = PiUtilities.SavePath.Replace(@"/", @"\");
            System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
        }

        [MenuItem("Tools/Settings/Load Keystore")]
        private static void LoadKeystore()
        {
            //todo:change this
            PlayerSettings.Android.keystorePass = BaseSettings.Instance.Keystore.Password;
            PlayerSettings.Android.keyaliasName = BaseSettings.Instance.Keystore.AliasName;
            PlayerSettings.Android.keyaliasPass = BaseSettings.Instance.Keystore.AliasPassword;
        }

        private static Object GetSettingObjectOfType(System.Type inType)
        {
            Object outObj = null;
            outObj = Resources.Load(Path.Combine(BaseConstants.RelativeSettingsPath, inType.Name));

            Debug.Log("Name: " + outObj);

            if (outObj != null)
            {
                return outObj;
            }

            outObj = ScriptableObject.CreateInstance(inType);

#if UNITY_EDITOR

            FileStaticAPI.CreateFolder(BaseConstants.ProjectSettingsPath);

            string fullPath = Path.Combine(Path.Combine("Assets", BaseConstants.ProjectSettingsPath),
                inType.Name + BaseConstants.ProjectSettingsAssetExtension
            );

            AssetDatabase.CreateAsset(outObj, fullPath);
#endif

            return outObj;
        }
#endif
    }
}