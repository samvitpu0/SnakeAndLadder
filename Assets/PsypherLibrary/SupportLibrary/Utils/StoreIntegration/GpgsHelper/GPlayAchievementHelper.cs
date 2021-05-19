#if ENABLE_NGPGS
using PsypherLibrary.SupportLibrary.Utils.Generics;
using System.Collections;
using JetBrains.Annotations;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using SA.Android.App;
using SA.Android.GMS.Common;
using SA.Android.GMS.Games;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.StoreIntegration.GpgsHelper
{
    public class GPlayAchievementHelper : GenericManager<GPlayAchievementHelper>
    {
        #region supports

        public enum AchievementState
        {
            Unlocked,
            Revealed,
            Hidden
        }

        public enum AchievementType
        {
            Standard,
            Incremental
        }

        #endregion

        #region fields and properties

        private AN_AchievementsClient _achievementsClient;

        #endregion

        #region initialization

        public void Initialization()
        {
            if (_achievementsClient == null)
            {
                _achievementsClient = AN_Games.GetAchievementsClient();
            }
        }

        #endregion

        #region actions

        public void UnlocksAchievement(string achievementId, [CanBeNull] string achievementName = null)
        {
            if (!CheckGPlayReady())
            {
                Debug.Log("GMS api not available...");
                return;
            }

            _achievementsClient.Unlock(achievementId);

            Debug.Log("Unlocks Achievement -> " + (achievementName ?? achievementId));
        }

        public void IncrementAchievement(string achievementId, int numSteps, [CanBeNull] string achievementName = null)
        {
            if (!CheckGPlayReady())
            {
                Debug.Log("GMS api not available...");
                return;
            }

            if (numSteps <= 0) return;

            _achievementsClient.Increment(achievementId, numSteps);

            Debug.Log("Incrementing Achievement -> " + (achievementName ?? achievementId) + ", by: " + numSteps);
        }

        public void ShowDefaultAchievementUI()
        {
            StartCoroutine(ShowAchievements());
        }

        private IEnumerator ShowAchievements()
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

            _achievementsClient.GetAchievementsIntent((result) =>
            {
                if (result.IsSucceeded)
                {
                    var intent = result.Intent;
                    var proxy = new AN_ProxyActivity();
                    proxy.StartActivityForResult(intent, (intentResult) => { proxy.Finish(); });
                    Debug.Log("Showing Default achievements UI");
                }
                else
                {
                    Debug.Log("Failed to get achievements intent :: " + result.Error.FullMessage);
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

        #region events

        #endregion
    }
}
#endif