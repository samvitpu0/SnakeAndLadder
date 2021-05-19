using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.AudioManager;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsypherLibrary.SupportLibrary.Managers
{
    [Serializable]
    public class ConfigInfo
    {
        public float Ver = 0;
        public string MD5 = "0";
    }

    public class ConfigManager : GenericManager<ConfigManager>
    {
        public static Action<float> ProgressCounter;
        public static Action OnConfigLoaded;
        public static Action OnLocalizationTextLoaded;
        public static Action OnAudioCacheSuccess;
        public static Action OnAudioCacheFail;


        protected override void OnDestroy()
        {
            base.OnDestroy();
            FileManager.RemoveReference(this);
        }

        public void FetchLocalizedAudio(Action OnAudioFetchSuccess, Action OnAudioFetchFail,
            Action<float> Progress = null)
        {
            var config = LocalDataManager.Instance.GetConfig<LocalizationData>();
            var urls = new List<string>();
            var index =
                config.Languages.ToList()
                    .FindIndex(x => string.Equals(x, LocalDataManager.Instance.SaveData.CurrentLanguage));
            foreach (var key in config.LocalizationAudioBindings.Keys)
            {
                urls.Add(config.LocalizationAudioBindings[key][index]);
            }

            if (!string.Equals(LocalDataManager.Instance.SaveData.CurrentLanguage, BaseConstants.DEFAULT_LANGUAGE) &&
                urls.Any())
            {
                FileManager.Instance.GetAudio(urls: urls.ToArray(), onComplete: (audioFiles) =>
                {
                    AudioController.AudioClips.Clear();

                    foreach (var key in config.LocalizationAudioBindings.Keys)
                    {
                        AudioController.AudioClips.SafeAdd(key,
                            audioFiles.Find(x => x.name == config.LocalizationAudioBindings[key][index].GetUniqueId()));
                        urls.Add(config.LocalizationAudioBindings[key][index]);
                    }

                    AudioController.LocalizedAudioCached = true;
                    OnAudioCacheSuccess.SafeInvoke();
                    OnAudioFetchSuccess.SafeInvoke();
                }, owner: this, onFail: () =>
                {
                    Debug.Log("Couldn't Fetch Voice Overs");
                    OnAudioCacheFail.SafeInvoke();
                    OnAudioFetchFail.SafeInvoke();
                }, onProgress: Progress);
            }
            else
            {
                Progress.SafeInvoke(100);
                AudioController.LocalizedAudioCached = true;
                OnAudioCacheSuccess.SafeInvoke();
                OnAudioFetchSuccess.SafeInvoke();
            }
        }

        void FetchSubConfigs()
        {
            try
            {
                Debug.Log("Fetching Sub Configs");
                var urls = new List<string>();
                foreach (KeyValuePair<string, JToken> keyValuePair in LocalDataManager.Instance.SaveData.MainConfigData)
                {
                    urls.Add(keyValuePair.Value.ToString());
                }

                FileManager.Instance.GetData(urls: urls.ToArray(), onComplete: (data) =>
                {
                    Debug.Log("Fetched Sub Configs");
                    OnConfigReady();
                }, owner: this, onFail: () =>
                {
                    Debug.Log("Couldn't Fetch Sub Configs");
                    if (!LocalDataManager.Instance.IsConnectedToInternet)
                    {
                        UIToastNotification.Instance.TriggerToast("Check Your Connection!", 2f);
                        UIPopupBox.Instance.SetDataOk("You are offline.Check your internet connection to continue",
                            FetchSubConfigs);
                    }
                    else
                        UIPopupBox.Instance.SetDataOk("Oops!, There seems to be an issue....", FetchSubConfigs);
                }, onProgress: ProgressCounter);
            }
            catch (Exception e)
            {
                Debug.Log("Caught Exception @FetchSubConfigs Exception Msg: " + e.Message);
                Debug.Log("Caught Exception @FetchSubConfigs Exception StackTrace: " + e.StackTrace);
                Debug.Log("Couldn't Fetch Sub Configs");
                if (!LocalDataManager.Instance.IsConnectedToInternet)
                {
                    UIToastNotification.Instance.TriggerToast("Check Your Connection!", 2f);
                    UIPopupBox.Instance.SetDataOk("You are offline.Check your internet connection to continue",
                        FetchSubConfigs);
                }
                else
                    UIPopupBox.Instance.SetDataOk("Oops!, There seems to be an issue....", FetchSubConfigs);
            }
        }


        public void FetchMainConfig(bool handleFailures = true)
        {
            FileManager.RemoveFolder(EFileType.Data);

            try
            {
                FileManager.Instance.GetData(urls: BaseSettings.Instance.EndPoints.ConfigEndPoint, onComplete: (obj) =>
                {
                    Debug.Log("Fetched app Config");
                    LocalDataManager.Instance.SaveData.MainConfigData = JObject.Parse(obj[0]);

                    FetchSubConfigs();
                }, owner: this, onFail: () =>
                {
                    Debug.Log("Couldn't Fetch Main Config");

                    if (!handleFailures) return; //return, when failure handles are not needed

                    if (!LocalDataManager.Instance.IsConnectedToInternet)
                    {
                        UIToastNotification.Instance.TriggerToast("Check Your Connection!", 2f);
                        UIPopupBox.Instance.SetDataOk("You are offline.Check your internet connection to continue",
                            () => FetchMainConfig());
                    }
                    else
                    {
                        UIPopupBox.Instance.SetDataOk("Oops!, There seems to be an issue....", () => FetchMainConfig());
                    }
                }, onProgress: ProgressCounter);
            }
            catch (Exception e)
            {
                Debug.Log("Caught Exception @FetchMainConfig Exception Msg: " + e.Message);
                Debug.Log("Caught Exception @FetchMainConfig Exception StackTrace: " + e.StackTrace);
                Debug.Log("Couldn't Fetch Main Config");

                if (!handleFailures) return; //return, when failure handles are not needed

                if (!LocalDataManager.Instance.IsConnectedToInternet)
                {
                    UIToastNotification.Instance.TriggerToast("Check Your Connection!", 2f);
                    UIPopupBox.Instance.SetDataOk("You are offline.Check your internet connection to continue",
                        () => FetchMainConfig());
                }
                else
                    UIPopupBox.Instance.SetDataOk("Oops!, There seems to be an issue....", () => FetchMainConfig());
            }
        }

        public void FetchVersion(bool handleFailures = true)
        {
            try
            {
                Debug.Log("Fetching Version...");
                FileManager.RemoveFile(BaseSettings.Instance.EndPoints.VersionEndPoint);
                FileManager.Instance.GetData(urls: BaseSettings.Instance.EndPoints.VersionEndPoint, onComplete: (obj) =>
                {
                    Debug.Log("Fetched Version...");
                    var fetchedConfig = JsonConvert.DeserializeObject<ConfigInfo>(obj[0].ToString()).Ver;
                    Debug.Log("Current: " + fetchedConfig + ", Previous: " +
                              LocalDataManager.Instance.SaveData.ConfigVersion);

                    if (!LocalDataManager.Instance.SaveData.ConfigVersion.Equals(fetchedConfig)) //version check
                    {
                        LocalDataManager.Instance.SaveData.ConfigVersion = fetchedConfig;
                        FetchMainConfig(handleFailures);
                    }
                    else
                    {
                        OnConfigReady();
                    }
                }, owner: this, onFail: () =>
                {
                    if (!handleFailures)
                    {
                        return; //return, when failure handles are not needed
                    }

                    Debug.Log("LocalDataManager.Instance.SaveData.ConfigVersion : " +
                              LocalDataManager.Instance.SaveData.ConfigVersion);
                    if (LocalDataManager.Instance.IsConfigInitialized())
                    {
                        OnConfigReady();
                    }
                    else
                    {
                        if (!LocalDataManager.Instance.IsConnectedToInternet)
                        {
                            UIToastNotification.Instance.TriggerToast("Check Your Connection!", 2f);
                            UIPopupBox.Instance.SetDataOk("You are offline.Check your internet connection to continue",
                                () => FetchVersion());
                        }
                        else
                            UIPopupBox.Instance.SetDataOk("Oops!, There seems to be an issue....",
                                () => FetchVersion());
                    }
                }, onProgress: ProgressCounter);
            }
            catch (Exception e)
            {
                Debug.Log("Caught Exception @FetchVersion Exception Msg: " + e.Message);
                Debug.Log("Caught Exception @FetchVersion Exception StackTrace: " + e.StackTrace);
                Debug.Log("Couldn't Fetch Version");
                if (LocalDataManager.Instance.SaveData.ConfigVersion != 0)
                {
                    OnConfigReady();
                }
                else
                {
                    if (!handleFailures) return; //return, when failure handles are not needed

                    if (!LocalDataManager.Instance.IsConnectedToInternet)
                    {
                        UIToastNotification.Instance.TriggerToast("Check Your Connection!", 2f);
                        UIPopupBox.Instance.SetDataOk("You are offline.Check your internet connection to continue",
                            () => FetchVersion());
                    }
                    else
                        UIPopupBox.Instance.SetDataOk("Oops!, There seems to be an issue....", () => FetchVersion());
                }
            }
        }


        void OnConfigReady()
        {
            Debug.Log("OnConfigReady");
            LocalDataManager.Instance.Save();
            OnLocalizationTextLoaded.SafeInvoke();

            OnConfigLoaded.SafeInvoke();
        }
    }
}