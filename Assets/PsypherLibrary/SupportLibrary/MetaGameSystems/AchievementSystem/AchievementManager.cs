using System;
using System.Collections;
using System.Collections.Generic;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.AchievementSystem
{
    #region DataStructure

    [Serializable]
    public class Mission
    {
        private enum NotificationFlag
        {
            Ready,
            Notified
        }


        //to be filled from json
        public string MissionID;
        public string IconUri;
        public int InitialValue;
        public int IncrementalValue;
        public int RewardCoins;
        public string MissionMessage;

        //current stored values
        public int CurrentTarget;
        public int Currentprogress;
        public int ClaimedCount;
        public int PendingClaim;

        private NotificationFlag _notifyStatus;

        private bool _isInitialized;

        public Mission(string missionId, string iconUri, int initialValue, int incrementalValue, int rewardCoins, string missionMessage)
        {
            MissionID = missionId;
            IconUri = iconUri;
            InitialValue = initialValue;
            IncrementalValue = incrementalValue;
            RewardCoins = rewardCoins;
            MissionMessage = missionMessage;
        }

        public void RefreshData(string iconUri, int initialValue, int incrementalValue, int rewardCoins, string missionMessage)
        {
            IconUri = iconUri;
            InitialValue = initialValue;
            IncrementalValue = incrementalValue;
            RewardCoins = rewardCoins;
            MissionMessage = missionMessage;
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                CurrentTarget = InitialValue;
                Currentprogress = 0;
                ClaimedCount = 0;
                PendingClaim = 0;

                _isInitialized = true;
                _notifyStatus = NotificationFlag.Ready;
            }
        }

        public int GetCummulitiveReward()
        {
            return (PendingClaim > 1) ? (PendingClaim * RewardCoins) : RewardCoins;
        }

        public void SetProgress(int progress, Action<int> onObjectiveComplete = null, Action fireNotification = null)
        {
            Currentprogress += progress;
            if (Currentprogress >= CurrentTarget)
            {
                CurrentTarget += IncrementalValue;
                PendingClaim++;

                onObjectiveComplete.SafeInvoke(PendingClaim);

                if (_notifyStatus == NotificationFlag.Ready)
                {
                    fireNotification.SafeInvoke();
                    _notifyStatus = NotificationFlag.Notified;
                }
            }
        }

        public float GetProgress()
        {
            if (CurrentTarget > 0)
                return ((float) ((float) Currentprogress / (float) CurrentTarget));
            else
                return 0.0f;
        }

        public void Claim(Action<int> onClaim)
        {
            if (PendingClaim > 0)
            {
                PendingClaim = 0;
                ClaimedCount++;
                _notifyStatus = NotificationFlag.Ready;

                onClaim.SafeInvoke(GetCummulitiveReward());
            }
        }
    }

    [Serializable]
    public class MissionData
    {
        public List<Mission> Missions;

        public Mission GetMission(string missionId)
        {
            if (Missions != null && Missions.Count > 0)
            {
                var fObj = Missions.Find(item => item.MissionID.Equals(missionId));
                if (fObj != null)
                    return fObj;
            }

            return null;
        }

        public void RefreshMissionsData(List<Mission> missions)
        {
            if (Missions == null || Missions.Count < 1)
            {
                Missions = new List<Mission>();
                Missions = missions;
            }
            else
            {
                missions.ForEach(item =>
                {
                    //refreshing old data
                    var alreadyAdded = Missions.Find(x => x.MissionID.Equals(item.MissionID));

                    if (alreadyAdded != null) //if the record exists
                    {
                        alreadyAdded.RefreshData(item.IconUri, item.InitialValue, item.IncrementalValue, item.RewardCoins, item.MissionMessage);
                    }

                    //adding new missions to runtime config
                    Missions.AddUnique(item, x => !x.MissionID.Equals(item.MissionID));
                });
            }
        }
    }

    #endregion

    [Serializable]
    public class AchievementManager : GenericManager<AchievementManager>
    {
        #region Fields and Properties

        public MissionData MissionData;
        private MissionData _savedMissionData;

        #endregion

        #region Initialize

        void OnEnable()
        {
            ConfigManager.OnConfigLoaded += ReloadMissionConfig;
        }

        void OnDisable()
        {
            ConfigManager.OnConfigLoaded -= ReloadMissionConfig;
        }

        void ReloadMissionConfig()
        {
            _savedMissionData.RefreshMissionsData(LocalDataManager.Instance.GetConfig<MissionData>().Missions);

            Initialize();
        }

        void Initialize()
        {
            if (_savedMissionData.Missions != null && _savedMissionData.Missions.Count > 0)
            {
                MissionData = _savedMissionData;
            }

            if (MissionData != null)
            {
                MissionData.Missions.ForEach(mission => mission.Initialize());
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Setting the saved mission data, this has to be called before initialization
        /// </summary>
        /// <param name="mData"></param>
        public void SetMissionData(MissionData mData)
        {
            _savedMissionData = mData;
            Initialize();
        }

        /// <summary>
        /// To set progress of respective mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="progress"></param>
        /// <param name="onObjectiveComplete"> This action also returns the pending claim count</param>
        /// <param name="fireNotification"></param>
        public void SetProgress(string missionId, int progress = 1, Action<int> onObjectiveComplete = null, Action fireNotification = null)
        {
            Debug.Log("Mission Type :" + missionId);
            MissionData.GetMission(missionId).SetProgress(progress, onObjectiveComplete, fireNotification);
        }

        /// <summary>
        /// To claim reward on successfull objective complete
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="onClaim">this action has return of reward coins</param>
        public void Claim(string missionId, Action<int> onClaim)
        {
            Debug.Log("Mission Claim Call");
            MissionData.GetMission(missionId).Claim(onClaim);
        }

        /// <summary>
        /// Gets the mission for the given missionID
        /// </summary>
        /// <param name="missionID"></param>
        /// <returns></returns>
        public Mission GetMission(string missionID)
        {
            return MissionData.GetMission(missionID);
        }

        #endregion
    }
}