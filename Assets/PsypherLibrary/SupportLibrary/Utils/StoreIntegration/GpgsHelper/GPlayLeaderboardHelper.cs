#if ENABLE_NGPGS
using PsypherLibrary.SupportLibrary.Utils.Generics;
using System.Collections;
using JetBrains.Annotations;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using SA.Android.App;
using SA.Android.GMS.Common;
using SA.Android.GMS.Games;
using SA.Android.Utilities;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.StoreIntegration.GpgsHelper
{
    public class GPlayLeaderboardHelper : GenericManager<GPlayLeaderboardHelper>
    {
        #region supports

        #endregion

        #region fields and properties

        private AN_LeaderboardsClient _leaderBoardsClient;

        #endregion

        #region Initialization

        public void Initialization()
        {
            if (_leaderBoardsClient == null)
            {
                _leaderBoardsClient = AN_Games.GetLeaderboardsClient();
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// use this to submit leader board data
        /// </summary>
        public void SubmitLeaderboardData(string leaderboardId, int incrementalData,
            [CanBeNull] string leaderboardName = null)
        {
            if (!CheckGPlayReady())
            {
                Debug.Log("GMS api not available...");
                return;
            }

            _leaderBoardsClient.SubmitScore(leaderboardId, incrementalData);

            Debug.Log("Leaderboard Submit -> " + (leaderboardName ?? leaderboardId) + ", value: " + incrementalData);
        }

        /// <summary>
        /// use this to submit leader board data, with call back
        /// </summary>
        public void SubmitLeaderboardImmediate(string leaderboardId, int incrementalData, string scoreTag,
            [CanBeNull] string leaderboardName = null)
        {
            if (!CheckGPlayReady())
            {
                Debug.Log("GMS api not available...");
                return;
            }

            _leaderBoardsClient.SubmitScoreImmediate(leaderboardId, incrementalData, scoreTag, (result) =>
            {
                if (result.IsSucceeded)
                {
                    var scoreSubmissionData = result.Data;
                    Debug.Log("SubmitScoreImmediate completed");
                    Debug.Log("scoreSubmissionData.PlayerId: " + scoreSubmissionData.PlayerId);
                    Debug.Log("scoreSubmissionData.LeaderboardId: " + scoreSubmissionData.LeaderboardId);

                    foreach (AN_Leaderboard.TimeSpan span in (AN_Leaderboard.TimeSpan[]) System.Enum.GetValues(
                        typeof(AN_Leaderboard.TimeSpan)))
                    {
                        var scoreSubmissionResult = scoreSubmissionData.GetScoreResult(span);
                        Debug.Log("scoreSubmissionData.FormattedScore: " + scoreSubmissionResult.FormattedScore);
                        Debug.Log("scoreSubmissionData.NewBest: " + scoreSubmissionResult.NewBest);
                        Debug.Log("scoreSubmissionData.RawScore: " + scoreSubmissionResult.RawScore);
                        Debug.Log("scoreSubmissionData.ScoreTag: " + scoreSubmissionResult.ScoreTag);
                    }
                }
                else
                {
                    Debug.Log("Failed to Submit Score Immediate " + result.Error.FullMessage);
                }
            });

            //Debug.Log("Leaderboard Submit -> " + (leaderboardName ?? leaderboardId) + ", value: " + incrementalData);
        }

        public void ShowDefaultLeaderboardUI()
        {
            StartCoroutine(ShowLeaderboards());
        }

        private IEnumerator ShowLeaderboards()
        {
            bool signInCancelled = false;

            if (!CheckGPlayReady()) //if GPlay not signed in
            {
                var isGPlaySignedIn = GPlayLoginHelper.Instance.IsSignedIn();

                Debug.Log("GPlay not signed in. Requesting sign in...");
                GPlayLoginHelper.Instance.SignInRequest((isSignedIn, statusCode) =>
                {
                    if (isSignedIn && statusCode == AN_CommonStatusCodes.SUCCESS)
                    {
                        isGPlaySignedIn = true;
                        Initialization(); //initializing, after sign in
                    }
                    else
                    {
                        signInCancelled = true;
                    }
                }, true);

                if (signInCancelled)
                {
                    yield break;
                }

                yield return new WaitUntil(() => isGPlaySignedIn); //wait till GPlay gets sign in
            }

            _leaderBoardsClient.GetAllLeaderboardsIntent((result) =>
            {
                if (result.IsSucceeded)
                {
                    var intent = result.Intent;
                    var proxy = new AN_ProxyActivity();
                    proxy.StartActivityForResult(intent, (intentResult) => { proxy.Finish(); });
                    Debug.Log("Showing Default Leaderboard UI");
                }
                else
                {
                    Debug.Log("Failed to get leader boards intent :: " + result.Error.FullMessage);
                }
            });

            yield return true;
        }

        bool CheckGPlayReady()
        {
            if (GPlayLoginHelper.Instance.IsSignedIn())
            {
                Initialization();

                return true;
            }

            return false;
        }

        #endregion
    }
}
#endif