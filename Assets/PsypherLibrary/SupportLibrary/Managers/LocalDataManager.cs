using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PsypherLibrary.SupportLibrary.AudioManager;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.BaseProjectSettings;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using PsypherLibrary.SupportLibrary.Utils.InternetReachabilityVerifier;
using UnityEngine;
using Object = System.Object;

namespace PsypherLibrary.SupportLibrary.Managers
{
    [Serializable]
    public class TaggedItemInfo
    {
        public List<string> TagItemList = new List<string>();

        public void AddItem(string itemID)
        {
            TagItemList.AddUnique(itemID);
        }

        public void RemoveItem(string itemID)
        {
            TagItemList.FindAndRemove(x => x.Equals(itemID));
        }
    }

    [Serializable]
    public class LocalSaveData
    {
        #region Fields and Properties

        #region Settings

        public Dictionary<AudioController.EAudioLayer, bool> AudioLayerToggle =
            new Dictionary<AudioController.EAudioLayer, bool>()
                {{AudioController.EAudioLayer.Music, true}, {AudioController.EAudioLayer.Sound, true}};

        public bool VibrationSetting = true;
        public bool AllowContentDownload = true;

        public string CurrentLanguage = BaseConstants.DEFAULT_LANGUAGE;
        public bool NotificationToggle = true;

        #endregion

        #region GameData

        public JObject MainConfigData = new JObject();
        public float ConfigVersion = 0;

        #endregion

        #region Save Records

        public string InstallDate;
        public int LocalSessions;

        #region Tagged Item lists

        public TaggedItemInfo FavoriteItems;

        #endregion

        #region Ads System

        public bool IsConsentGivenForAds;

        #endregion

        #endregion

        #region InAppPurchase

        public bool IsPremiumUser = false;

        #endregion

        #region AdditionalData

        [SerializeField] public Dictionary<string, object> AdditionalData;

        #endregion

        #endregion

        public LocalSaveData()
        {
            InstallDate = DateTimeExtensions.GetCurrentUnixTime().ToString();
        }

        public int GetDaySinceInstall()
        {
            return (int) (DateTimeExtensions.GetCurrentUnixTime().FromUnixTime() -
                          InstallDate.FromUnixTime()).TotalDays;
        }

        public void SetPremiumUser(bool isPremium)
        {
            IsPremiumUser = isPremium;
        }

        public bool GetPremiumStatus()
        {
            return IsPremiumUser;
        }

        public object GetAdditionalData(string uid)
        {
            if (AdditionalData != null && AdditionalData.Any())
            {
                return AdditionalData.SafeRetrieve(uid);
            }

            return null;
        }

        public void UpdateAdditionalData(string uid, object data)
        {
            if (AdditionalData == null)
            {
                AdditionalData = new Dictionary<string, object>();
            }

            AdditionalData.SafeAdd(uid, data);
        }
    }

    public class LocalDataManager : GenericManager<LocalDataManager>

    {
        //Application wide events
        public static Action<bool> OnAppFocus;
        public static Action<InternetReachabilityVerifier.Status> OnNetworkChange;

        public LocalSaveData SaveData;
        public bool IsConnectedToInternet;


        protected override void Awake()
        {
            base.Awake();
            //right now directly setting the screen to prevent dimming,
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //Logs enable/disable
            Debug.unityLogger.logEnabled = BaseSettings.Instance.EnableLogs;

            //internet verifier settings
            InternetReachabilityVerifier.Instance.captivePortalDetectionMethod =
                InternetReachabilityVerifier.CaptivePortalDetectionMethod.Google204HTTPS;
        }

        #region ILocalData implementation

        void OnEnable()
        {
            SaveData = Load() ?? Create();

            //incrementing the number of app sessions the moment local data manager is loaded 
            SaveData.LocalSessions++;

            //reference to hold the internet connection status
            InternetReachabilityVerifier.Instance.statusChangedDelegate += OnNetworkStatusChange;

            //Test premium user - this has no effect in release builds
            if (BaseSettings.Instance.IsDebugBuild)
            {
                SaveData.SetPremiumUser(BaseSettings.Instance.IsPremiumUser);
            }
        }

        void OnDisable()
        {
            Save();
            InternetReachabilityVerifier.Instance.statusChangedDelegate -= OnNetworkStatusChange;
        }

        public void Save()
        {
            JsonConvert.SerializeObject(SaveData).SaveJson(BaseConstants.SAVE_DATA_KEY);
            Debug.Log("Save Successful");
        }

        public LocalSaveData Load()
        {
            var data = PiUtilities.LoadJsonData<LocalSaveData>(BaseConstants.SAVE_DATA_KEY);
            if (data == default(LocalSaveData))
            {
                return null;
            }

            return data;
        }

        public LocalSaveData Create()
        {
            SaveData = new LocalSaveData();
            Debug.Log("Created New Save Data");

            return SaveData;
        }

        public LocalSaveData ClearData()
        {
            SaveData = null;
            SaveData = new LocalSaveData();
            Debug.Log("Cleared data");
            Save();
            return SaveData;
        }

        #endregion

        #region GetConfigs

        public T GetConfig<T>()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(GetConfigJson(typeof(T).Name));
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        //added this so that we can get jsons of same type but different names
        public T GetConfig<T>(string configName)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(GetConfigJson(configName));
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public string GetConfigJson(string configName)
        {
            string url;

            if (configName.Equals(typeof(AppConfigData).Name))
            {
                url = BaseSettings.Instance.EndPoints.ConfigEndPoint;
            }
            else if (configName.Equals(typeof(ConfigInfo).Name))
            {
                url = BaseSettings.Instance.EndPoints.VersionEndPoint;
            }
            else
            {
                url = SaveData.MainConfigData[configName].Value<string>();
            }

            if (FileManager.IsFileDownloaded(url))
            {
                Debug.Log("Config URL: " + url);
                return PiUtilities.LoadJsonData(url.GetUniqueId(),
                    Path.Combine(PiUtilities.SavePath, FileManager.GetFileType(url).ToString()),
                    "." + FileManager.GetFileFormat(url));
            }

            return string.Empty;
        }

        #endregion

        #region Events

        // Sent to all game objects when the player gets or looses focus
        private void OnApplicationFocus(bool focus)
        {
            OnAppFocus.SafeInvoke(focus);
        }

        void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                if (SaveData != null)
                    Save();
            }
        }

        void OnNetworkStatusChange(InternetReachabilityVerifier.Status netStatus)
        {
            OnNetworkChange.SafeInvoke(netStatus);

            switch (netStatus)
            {
                case InternetReachabilityVerifier.Status.NetVerified:
                {
                    IsConnectedToInternet = true;
                }
                    break;
                case InternetReachabilityVerifier.Status.PendingVerification:
                case InternetReachabilityVerifier.Status.Mismatch:
                case InternetReachabilityVerifier.Status.Error:
                case InternetReachabilityVerifier.Status.Offline:
                {
                    //offline
                    IsConnectedToInternet = false;
                }
                    break;
            }

            Debug.Log("Network status: " + netStatus);
        }

        #endregion

        #region Utilities

        public bool IsConfigInitialized()
        {
            return !SaveData.ConfigVersion.Equals(0);
        }

        public T GetData<T>(string uid)
        {
            var obj = SaveData.GetAdditionalData(uid);

            if (obj == null)
            {
                Debug.Log("Data not found, returning default - " + default(T));
                return default(T);
            }

            var data = obj.ToString();

            Debug.Log(name + " ::Data Retrieving - " + data);

            return JsonConvert.DeserializeObject<T>(data);
        }

        public void SetData(string uid, object data)
        {
            var jData = JsonConvert.SerializeObject(data);
            Debug.Log(name + " ::Data Setting - " + jData);
            SaveData.UpdateAdditionalData(uid, jData);
        }

        #endregion
    }
}