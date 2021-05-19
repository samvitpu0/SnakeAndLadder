using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using PsypherLibrary.SupportLibrary.UIHelperScripts.DefinedPanels;
using UnityEngine;
using UnityEngine.Events;

namespace PsypherLibrary.SupportLibrary.Utils
{
    [RequireComponent(typeof(UIViewController))]
    public class PressBackActions : MonoBehaviour
    {
        private int _backPressCount;
        public bool CanExit = false;
        public bool CanPause = false;
        public string WarningMessage = "You cannot go back at this stage!";

        [Tooltip("Time duration after which back count refreshes to 0")]
        public float RefreshTime = 2;

        [Tooltip("No. of Back Press to invoke exit confirmation")]
        public int BackCountToExit = 2;

        [Tooltip("Actions to perform on Pause trigger")]
        public UnityEvent OnPause;

        public void PressBackToExit()
        {
            if (!CanExit)
            {
                UIToastNotification.Instance.TriggerToast(WarningMessage, 2f);
            }
            else
            {
                _backPressCount++;
                if (_backPressCount < BackCountToExit)
                {
                    UIToastNotification.Instance.TriggerToast("Press back 1 more time to exit!", 1f);

                    this.InvokeAfter(() => _backPressCount = 0, RefreshTime);
                    return;
                }

                ShowConfirmation();
            }
        }

        public void PressBackToPause()
        {
            if (CanPause)
            {
                //note: implemented as unity action, connect it on the editor

                OnPause.SafeInvoke();
            }
        }

        private void ShowConfirmation()
        {
            UIToastNotification.Instance.KillToast(); // to immediately kill the toast once the confirmation appears
            UIPopupBox.Instance.SetDataYesNo("Do you want to quit the game?",
                Application.Quit,
                null, EPanelSize.SmallPortait);
        }
    }
}