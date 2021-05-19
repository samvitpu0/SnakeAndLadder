#if ENABLE_NGCENTER
using System;
using JetBrains.Annotations;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using SA.iOS.GameKit;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.StoreIntegration.GameCenterHelper
{
    public class GCenterLeaderboardHelper : GenericManager<GCenterLeaderboardHelper>
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
                ISN_GKLeaderboard.LoadLeaderboards(result =>
                {
                    if (result.IsSucceeded)
                    {
                        foreach (var leaderboard in result.Leaderboards)
                        {
                            Debug.Log("Leaderboard.ID: " + leaderboard.Identifier);
                            Debug.Log("Leaderboard.GroupID: " + leaderboard.GroupIdentifier);
                            Debug.Log("Leaderboard.Title: " + leaderboard.Title);
                        }

                        _isInitialized = true;
                        _viewController = new ISN_GKGameCenterViewController();
                        resultCallback.SafeInvoke();
                    }
                    else
                    {
                        Debug.Log("Load Leaderboards failed! Error code: " + result.Error.Code + ", Message: " +
                                  result.Error.Message);

                        _isInitialized = false;
                        _viewController = null;
                    }
                });
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Shows the default Game Center leaderboard UI
        /// </summary>
        /// <param name="leaderboardId"></param>
        /// <param name="timeScope"></param>
        public void ShowDefaultLeaderboardUI([CanBeNull] string leaderboardId = null,
            ISN_GKLeaderboardTimeScope timeScope = ISN_GKLeaderboardTimeScope.Today)
        {
            if (GCenterLoginHelper.Instance.IsSignedIn() && _isInitialized)
            {
                _viewController.ViewState = ISN_GKGameCenterViewControllerState.Leaderboards;

                if (_viewController != null)
                {
                    if (string.IsNullOrEmpty(leaderboardId))
                    {
                        _viewController.Show();
                    }
                    else
                    {
                        _viewController.LeaderboardIdentifier = leaderboardId;
                        _viewController.LeaderboardTimeScope = timeScope;
                        _viewController.Show();
                    }
                }
            }
            else
            {
                Debug.Log("Not signed into Game Center.");
            }
        }

        /// <summary>
        /// Report leaderboard data
        /// </summary>
        /// <param name="leaderboardId"></param>
        /// <param name="incrementalData"></param>
        /// <param name="leaderboardName"></param>
        public void SubmitLeaderboardData(string leaderboardId, int incrementalData,
            [CanBeNull] string leaderboardName = null)
        {
            if (GCenterLoginHelper.Instance.IsSignedIn() && _isInitialized)
            {
                var scoreReporter = new ISN_GKScore(leaderboardId) {Value = incrementalData, Context = 1};

                scoreReporter.Report(result =>
                {
                    if (result.IsSucceeded)
                    {
                        Debug.Log("Leaderboard Submit -> " + (leaderboardName ?? leaderboardId) + ", value: " +
                                  incrementalData);
                    }
                    else
                    {
                        Debug.Log("Score Report failed! Code: " + result.Error.Code + " Message: " +
                                  result.Error.Message);
                    }
                });
            }
        }

        #endregion
    }
}
#endif