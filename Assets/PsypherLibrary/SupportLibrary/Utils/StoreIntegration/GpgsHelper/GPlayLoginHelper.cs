#if ENABLE_NGPGS
using PsypherLibrary.SupportLibrary.Utils.Generics;
using System;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using SA.Android.App;
using SA.Android.App.View;
using SA.Android.GMS.Auth;
using SA.Android.GMS.Common;
using SA.Android.GMS.Common.Images;
using SA.Android.GMS.Drive;
using SA.Android.GMS.Games;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.StoreIntegration.GpgsHelper
{
    public class GPlayLoginHelper : GenericManager<GPlayLoginHelper>
    {
        #region Fields and Properties

        public enum ImageType
        {
            HiResImage,
            IconImage
        }

        public static Action<bool> OnGPlayAuth;

        public bool IsGoogleApiAvailable { get; private set; }
        public AN_GoogleSignInAccount GoogleSignedInAccount { get; private set; }

        private bool _isSigninIgnored;
        private AN_Player _currentPlayerData;

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
            var response = AN_GoogleApiAvailability.IsGooglePlayServicesAvailable();
            if (response == AN_ConnectionResult.SUCCESS)
            {
                IsGoogleApiAvailable = true;
                Debug.Log("Google APIs are available.");
            }
            else
            {
                Debug.Log("Google Api not available on current device, trying to resolve");
                AN_GoogleApiAvailability.MakeGooglePlayServicesAvailable(result =>
                {
                    if (result.IsSucceeded)
                    {
                        IsGoogleApiAvailable = true;
                        Debug.Log("Google APIs are now available.");
                    }
                    else
                    {
                        IsGoogleApiAvailable = false;
                    }
                });
            }

            //check prefs for ignore signin
            _isSigninIgnored = PlayerPrefs.GetInt("IsSigninIgnored", 0) > 0;
        }

        #endregion

        #region Actions

        public void SignInRequest(Action<bool, AN_CommonStatusCodes> resultCallback = null, bool ignorePrefs = false)
        {
            if (!IsGoogleApiAvailable) return;

            Debug.Log("Signing request...");

            if (IsSignedIn()) //if already signed in
            {
                resultCallback.SafeInvoke(true, AN_CommonStatusCodes.SUCCESS);
                GetSignedInPlayerData();
                ActivateGPlayPopup();

                _isSigninIgnored = false;
                PlayerPrefs.SetInt("IsSigninIgnored", 0);
                OnGPlayAuth.SafeInvoke(true);

                return;
            }

            if (!ignorePrefs)
            {
                if (_isSigninIgnored)
                {
                    resultCallback.SafeInvoke(false, AN_CommonStatusCodes.CANCELED);
                    Debug.Log("Signin Ignored!");
                    return;
                }
            }

            AN_GoogleSignInOptions.Builder builder =
                new AN_GoogleSignInOptions.Builder(AN_GoogleSignInOptions.DEFAULT_GAMES_SIGN_IN);
            builder.RequestId();
            builder.RequestEmail();
            builder.RequestProfile();
            //builder.RequestScope(AN_Drive.SCOPE_APPFOLDER);

            var gso = builder.Build();
            var client = AN_GoogleSignIn.GetClient(gso);

            var authStatus = AN_CommonStatusCodes.SIGN_IN_REQUIRED;

            //silent sign in
            client.SilentSignIn(signInResult =>
            {
                Debug.Log("Sign In StatusCode [silent]: " + signInResult.StatusCode);
                authStatus = signInResult.StatusCode;

                if (signInResult.IsSucceeded)
                {
                    Debug.Log("SignIn Succeeded [silent]");
                    GoogleSignedInAccount = signInResult.Account;
                    resultCallback.SafeInvoke(true, authStatus);
                    ActivateGPlayPopup();

                    //finally when signin is success, cache the player data for first time
                    if (authStatus == AN_CommonStatusCodes.SUCCESS)
                    {
                        GetSignedInPlayerData();

                        _isSigninIgnored = false;
                        PlayerPrefs.SetInt("IsSigninIgnored", 0);

                        OnGPlayAuth.SafeInvoke(true);
                    }
                }
                else
                {
                    GoogleSignedInAccount = null;
                    Debug.Log("Silent SignIn failed [silent] : " + signInResult.Error.FullMessage);

                    //interactive sign in
                    if (authStatus == AN_CommonStatusCodes.SIGN_IN_REQUIRED)
                    {
                        client.SignIn(interactiveSignInResult =>
                        {
                            authStatus = interactiveSignInResult.StatusCode;
                            Debug.Log("Sign In StatusCode [interactive]: " + interactiveSignInResult.StatusCode);

                            if (interactiveSignInResult.IsSucceeded)
                            {
                                Debug.Log("SignIn Succeeded [interactive]");
                                GoogleSignedInAccount = interactiveSignInResult.Account;
                                resultCallback.SafeInvoke(true, authStatus);
                                ActivateGPlayPopup();

                                //finally when signin is success, cache the player data for first time
                                if (authStatus == AN_CommonStatusCodes.SUCCESS)
                                {
                                    GetSignedInPlayerData();

                                    _isSigninIgnored = false;
                                    PlayerPrefs.SetInt("IsSigninIgnored", 0);

                                    OnGPlayAuth.SafeInvoke(true);
                                }
                            }
                            else
                            {
                                resultCallback.SafeInvoke(false, authStatus);
                                Debug.Log("SignIn failed [interactive]: " + interactiveSignInResult.Error.FullMessage);

                                if (authStatus == AN_CommonStatusCodes.CANCELED || authStatus == AN_CommonStatusCodes.ERROR)
                                {
                                    _isSigninIgnored = true;
                                    PlayerPrefs.SetInt("IsSigninIgnored", 1);
                                }
                            }
                        });
                    }
                }
            });

            //finally when signin is success, cache the player data for first time
            if (authStatus == AN_CommonStatusCodes.SUCCESS)
            {
                GetSignedInPlayerData();
                ActivateGPlayPopup();

                _isSigninIgnored = false;
                PlayerPrefs.SetInt("IsSigninIgnored", 0);

                OnGPlayAuth.SafeInvoke(true);
            }
        }

        public void SignOutRequest(Action<bool, AN_CommonStatusCodes> resultCallback = null, bool revokeAccess = false)
        {
            if (!IsGoogleApiAvailable) return;

            var gso = new AN_GoogleSignInOptions.Builder(AN_GoogleSignInOptions.DEFAULT_GAMES_SIGN_IN).Build();
            var client = AN_GoogleSignIn.GetClient(gso);

            //sign out
            client.SignOut(() =>
            {
                GoogleSignedInAccount = null;
                _currentPlayerData = null;
                resultCallback.SafeInvoke(false, AN_CommonStatusCodes.SUCCESS);

                _isSigninIgnored = true;
                PlayerPrefs.SetInt("IsSigninIgnored", 1);

                OnGPlayAuth.SafeInvoke(false);
            });

            //revoke access
            if (revokeAccess)
            {
                client.RevokeAccess(() =>
                {
                    GoogleSignedInAccount = null;
                    _currentPlayerData = null;
                });
            }
        }

        public bool IsSignedIn()
        {
            if (!IsGoogleApiAvailable) return false;

            return AN_GoogleSignIn.GetLastSignedInAccount() != null;
        }

        public void GetSignedInPlayerData(Action<AN_Player> resultCallback = null)
        {
            if (!IsGoogleApiAvailable || AN_GoogleSignIn.GetLastSignedInAccount() == null) return;

            if (_currentPlayerData != null) //early exit with existing player data
            {
                //Printing player info:
                Debug.Log("player.Id: " + _currentPlayerData.PlayerId);
                Debug.Log("player.Title: " + _currentPlayerData.Title);
                Debug.Log("player.DisplayName: " + _currentPlayerData.DisplayName);
                Debug.Log("player.HiResImageUri: " + _currentPlayerData.HiResImageUri);
                Debug.Log("player.IconImageUri: " + _currentPlayerData.IconImageUri);
                Debug.Log("player.HasIconImage: " + _currentPlayerData.HasIconImage);
                Debug.Log("player.HasHiResImage: " + _currentPlayerData.HasHiResImage);

                resultCallback.SafeInvoke(_currentPlayerData);

                return;
            }

            AN_PlayersClient client = AN_Games.GetPlayersClient();
            client.GetCurrentPlayer(result =>
            {
                if (result.IsSucceeded)
                {
                    _currentPlayerData = result.Data;

                    //Printing player info:
                    Debug.Log("player.Id: " + _currentPlayerData.PlayerId);
                    Debug.Log("player.Title: " + _currentPlayerData.Title);
                    Debug.Log("player.DisplayName: " + _currentPlayerData.DisplayName);
                    Debug.Log("player.HiResImageUri: " + _currentPlayerData.HiResImageUri);
                    Debug.Log("player.IconImageUri: " + _currentPlayerData.IconImageUri);
                    Debug.Log("player.HasIconImage: " + _currentPlayerData.HasIconImage);
                    Debug.Log("player.HasHiResImage: " + _currentPlayerData.HasHiResImage);

                    resultCallback.SafeInvoke(_currentPlayerData);
                }
                else
                {
                    Debug.Log("Failed to load Current Player " + result.Error.FullMessage);
                    resultCallback.SafeInvoke(null);
                }
            });
        }

        /// <summary>
        /// should always call after GetSignedInPlayerData [after successfully retrieve player data]. Its always best to use it inside GetSignedInPlayerData's resultCallback
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resultCallback"></param>
        public void GetPlayerImage(ImageType type, Action<Texture2D> resultCallback)
        {
            var playerData = _currentPlayerData;
            Texture2D texture;

            if (playerData != null)
            {
                switch (type)
                {
                    case ImageType.HiResImage:
                    {
                        if (playerData.HasHiResImage)
                        {
                            var url = playerData.HiResImageUri;
                            var manager = new AN_ImageManager();
                            manager.LoadImage(url, imageLoadResult =>
                            {
                                if (imageLoadResult.IsSucceeded)
                                {
                                    texture = imageLoadResult.Image;
                                    resultCallback.SafeInvoke(texture);
                                }
                                else
                                {
                                    resultCallback.SafeInvoke(null);
                                }
                            });
                        }
                    }
                        break;
                    case ImageType.IconImage:
                    {
                        if (playerData.HasIconImage)
                        {
                            var url = playerData.IconImageUri;
                            var manager = new AN_ImageManager();
                            manager.LoadImage(url, imageLoadResult =>
                            {
                                if (imageLoadResult.IsSucceeded)
                                {
                                    texture = imageLoadResult.Image;
                                    resultCallback.SafeInvoke(texture);
                                }
                                else
                                {
                                    resultCallback.SafeInvoke(null);
                                }
                            });
                        }
                    }
                        break;
                }
            }
        }


        public void ActivateGPlayPopup()
        {
            var gamesClient = AN_Games.GetGamesClient();
            gamesClient.SetViewForPopups(AN_MainActivity.Instance);

            //optionally
            gamesClient.SetGravityForPopups(AN_Gravity.TOP | AN_Gravity.CENTER_HORIZONTAL);
        }

        #endregion
    }
}
#endif