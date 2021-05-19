using PsypherLibrary.SupportLibrary.Utils.Generics;
#if ENABLE_NUTILS
using System;
using JetBrains.Annotations;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using SA.Android.App;
using UnityEngine;


namespace PsypherLibrary.SupportLibrary.Utils.Native.NativePopups
{
    public class NativePopups : GenericManager<NativePopups>
    {
        #region Actions

#if UNITY_ANDROID
        public void CreateOkPopup(string title, string message, Action onOk)
        {
            AN_AlertDialog popup = new AN_AlertDialog(AN_DialogTheme.Material_Light);
            popup.Title = title;
            popup.Message = message;
            popup.Cancelable = false;

            popup.SetPositiveButton("OK", () =>
            {
                popup.Hide();
                Debug.Log("Ok on " + title + " is pressed.");
                onOk.SafeInvoke();
            });
            popup.Show();
        }

        public void CreateYesNo(string title, string message, Action onYes, Action onNo, [CanBeNull] string yesButtonName = null, [CanBeNull] string noButtonName = null)
        {
            AN_AlertDialog popup = new AN_AlertDialog(AN_DialogTheme.Material_Light);
            popup.Title = title;
            popup.Message = message;
            popup.Cancelable = false;

            popup.SetPositiveButton(yesButtonName ?? "YES", () =>
            {
                popup.Hide();
                Debug.Log("YES on " + title + " is pressed.");
                onYes.SafeInvoke();
            });

            popup.SetNegativeButton(noButtonName ?? "NO", () =>
            {
                popup.Hide();
                Debug.Log("NO on " + title + " is pressed.");
                onNo.SafeInvoke();
            });

            popup.Show();
        }
#endif

        #endregion
    }
}
#endif