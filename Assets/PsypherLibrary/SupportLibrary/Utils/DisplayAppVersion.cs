using PsypherLibrary._ExternalLibraries.MyBox.Attributes;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Utils
{
    /// <summary>
    /// Use to display app version. Especially QA purpose
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class DisplayAppVersion : MonoBehaviour
    {
        public bool GetAppVersion;

        [ConditionalField("GetAppVersion", true)]
        public string CustomVersion;

        public string Prefix;

        private Text InfoText
        {
            get { return GetComponent<Text>(); }
        }

        // Use this for initialization
        private void Start()
        {
            if (GetAppVersion)
                InfoText.SetText(Prefix + Application.version);
            else
                InfoText.SetText(Prefix + CustomVersion);
        }
    }
}