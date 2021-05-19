#if ENABLE_NGCENTER
using System;
using JetBrains.Annotations;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using SA.iOS.GameKit;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.StoreIntegration.GameCenterHelper
{
    public class GCenterAchievementHelper : GenericManager<GCenterAchievementHelper>
    {
        #region Fields and Properties

        private bool _isInitialized;
        private ISN_GKGameCenterViewController _viewController;

        #endregion

        #region Initialization

        public void Initialization(Action resultCallback = null)
        {
            if (!_isInitialized)
            {
                ISN_GKAchievement.LoadAchievements((result) =>
                {
                    if (result.IsSucceeded)
                    {
                        foreach (ISN_GKAchievement achievement in result.Achievements)
                        {
                            Debug.Log("Achievement.ID: " + achievement.Identifier);
                            Debug.Log("Achievement.PercentCompleted: " + achievement.PercentComplete);
                            Debug.Log("Achievement.LastReportDate: " + achievement.LastReportedDate);
                            Debug.Log("Achievement.Completed: " + achievement.Completed);
                        }

                        _isInitialized = true;
                        _viewController = new ISN_GKGameCenterViewController();
                        resultCallback.SafeInvoke();
                    }
                    else
                    {
                        Debug.Log("LoadAchievements failed! Code: " + result.Error.Code + " Message: " +
                                  result.Error.Message);

                        _isInitialized = false;
                        _viewController = null;
                    }
                });
            }
        }

        #endregion

        public void ShowDefaultAchievementUI()
        {
            if (GCenterLoginHelper.Instance.IsSignedIn() && _isInitialized)
            {
                _viewController.ViewState = ISN_GKGameCenterViewControllerState.Achievements;

                _viewController?.Show();
            }
            else
            {
                Debug.Log("Not signed into Game Center.");
            }
        }

        public void IncrementAchievement(string achievementId, int percentage,
            [CanBeNull] string achievementName = null)
        {
            if (GCenterLoginHelper.Instance.IsSignedIn() && _isInitialized)
            {
                var achievement = new ISN_GKAchievement(achievementId) {PercentComplete = percentage};

                achievement.Report(result =>
                {
                    if (result.IsSucceeded)
                    {
                        Debug.Log("Incrementing Achievement -> " + (achievementName ?? achievementId) +
                                  ", percentage: " +
                                  percentage);
                    }
                    else
                    {
                        Debug.Log("Achievement report failed! Code: " + result.Error.Code + " Message: " +
                                  result.Error.Message);
                    }
                });
            }
        }

        public void UnlocksAchievement(string achievementId, [CanBeNull] string achievementName = null)
        {
            if (GCenterLoginHelper.Instance.IsSignedIn() && _isInitialized)
            {
                var achievement = new ISN_GKAchievement(achievementId) {PercentComplete = 100};

                achievement.Report(result =>
                {
                    if (result.IsSucceeded)
                    {
                        Debug.Log("Unlocks Achievement -> " + (achievementName ?? achievementId));
                    }
                    else
                    {
                        Debug.Log("Achievement report failed! Code: " + result.Error.Code + " Message: " +
                                  result.Error.Message);
                    }
                });
            }
        }
    }
}
#endif