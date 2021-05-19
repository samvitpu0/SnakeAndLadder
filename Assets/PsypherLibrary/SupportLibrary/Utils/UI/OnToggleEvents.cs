using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    [RequireComponent(typeof(Toggle))]
    public class OnToggleEvents : MonoBehaviour
    {
        public UnityEvent OnToggleOn;
        public UnityEvent OnToggleOff;

        private Toggle Toggle
        {
            get { return GetComponent<Toggle>(); }
        }

        void OnEnable()
        {
            if (Toggle)
            {
                Toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        OnToggleOn.SafeInvoke();
                    }
                    else
                    {
                        OnToggleOff.SafeInvoke();
                    }
                });
            }
        }

        void OnDisable()
        {
            Toggle.onValueChanged.RemoveAllListeners();
        }
    }
}