#if ENABLE_NGCENTER
using PsypherLibrary.SupportLibrary.Extensions;
using System;
using PsypherLibrary.SupportLibrary.Managers;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using SA.Foundation.Templates;
using SA.iOS.GameKit;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.StoreIntegration.GameCenterHelper
{
    public class GCenterLoginHelper : GenericManager<GCenterLoginHelper>
    {
        #region Fields and Properties

        public static Action<bool> OnGCenterAuth;

        private bool _isSigninIgnored;
        private ISN_GKLocalPlayer _currentPlayerData;

        public bool IsPlayerDataLoaded => _currentPlayerData != null;

        #endregion

        #region Initialization

        protected override void Awake()
        {
            base.Awake();
            Initialization();
        }

        private void Initialization()
        {
            //check prefs for ignore signin
            _isSigninIgnored = PlayerPrefs.GetInt("IsSigninIgnored", 0) > 0;
        }

        #endregion

        #region Actions

        public void SignInRequest(Action<bool> resultCallback = null, bool ignorePrefs = false)
        {
            if (IsSignedIn()) //if already signed in
            {
                _currentPlayerData = ISN_GKLocalPlayer.LocalPlayer;
                GetSignedInPlayerData();

                _isSigninIgnored = false;
                PlayerPrefs.SetInt("IsSigninIgnored", 0);

                resultCallback.SafeInvoke(true);
                OnGCenterAuth.SafeInvoke(true);

                return;
            }

            if (!ignorePrefs)
            {
                if (_isSigninIgnored)
                {
                    _currentPlayerData = null;

                    resultCallback.SafeInvoke(false);
                    return;
                }
            }

            //note: new authentication method introduced in 2020.6
            ISN_GKLocalPlayer.SetAuthenticateHandler(result =>
            {
                if (result.IsSucceeded)
                {
                    Debug.Log("Authenticate is succeeded!");
                    _currentPlayerData = ISN_GKLocalPlayer.LocalPlayer;

                    _isSigninIgnored = false;
                    PlayerPrefs.SetInt("IsSigninIgnored", 0);

                    resultCallback.SafeInvoke(true);
                    OnGCenterAuth.SafeInvoke(true);
                }
                else
                {
                    Debug.Log("Authenticate is failed! Error with code: " + result.Error.Code + " and description: " +
                              result.Error.Message);
                    _currentPlayerData = null;

                    _isSigninIgnored = true;
                    PlayerPrefs.SetInt("IsSigninIgnored", 1);

                    resultCallback.SafeInvoke(false);
                    OnGCenterAuth.SafeInvoke(false);
                }
            });

            //note:removed in the new update 2020.6
            /*ISN_GKLocalPlayer.Authenticate(result =>
            {
                if (result.IsSucceeded)
                {
                    Debug.Log("Authenticate is succeeded!");
                    _currentPlayerData = ISN_GKLocalPlayer.LocalPlayer;

                    _isSigninIgnored = false;
                    PlayerPrefs.SetInt("IsSigninIgnored", 0);

                    resultCallback.SafeInvoke(true);
                    OnGCenterAuth.SafeInvoke(true);
                }
                else
                {
                    Debug.Log("Authenticate is failed! Error with code: " + result.Error.Code + " and description: " +
                              result.Error.Message);
                    _currentPlayerData = null;

                    _isSigninIgnored = true;
                    PlayerPrefs.SetInt("IsSigninIgnored", 1);

                    resultCallback.SafeInvoke(false);
                    OnGCenterAuth.SafeInvoke(false);
                }
            });*/
        }

        public bool IsSignedIn()
        {
            return ISN_GKLocalPlayer.LocalPlayer.Authenticated;
        }

        /// <summary>
        /// Get Current Player's Data
        /// </summary>
        /// <returns></returns>
        public ISN_GKPlayer GetSignedInPlayerData()
        {
            if (!IsSignedIn())
            {
                return null;
            }
            else
            {
                //Printing player info:
                Debug.Log("player.Id: " + _currentPlayerData.PlayerID);
                Debug.Log("player.Alias: " + _currentPlayerData.Alias);
                Debug.Log("player.DisplayName: " + _currentPlayerData.DisplayName);
                Debug.Log("player.Authenticated: " + _currentPlayerData.Authenticated);
                Debug.Log("player.Underage: " + _currentPlayerData.Underage);

                return _currentPlayerData;
            }
        }

        /// <summary>
        /// call this method only after authentication is complete and has player data ready
        /// </summary>
        /// <param name="resultCallback"></param>
        /// <param name="imageSize"></param>
        public void GetPlayerImage(Action<Texture2D> resultCallback, GKPhotoSize imageSize = GKPhotoSize.Normal)
        {
            var playerData = ISN_GKLocalPlayer.LocalPlayer;
            Texture2D texture;

            try
            {
                playerData.LoadPhoto(imageSize, result =>
                {
                    if (result.IsSucceeded)
                    {
                        resultCallback.SafeInvoke(result.Image);
                        Debug.Log("Player photo: " + result.Image);
                    }
                    else
                    {
                        resultCallback.SafeInvoke(null);
                        Debug.Log("Failed to load player's photo: " + result.Error.FullMessage);
                    }
                });
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e}");
                resultCallback.SafeInvoke(null);
            }
        }

        #endregion
    }
}
#endif