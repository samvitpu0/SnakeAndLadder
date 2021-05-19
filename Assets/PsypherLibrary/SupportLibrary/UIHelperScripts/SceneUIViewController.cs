using UnityEngine;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    [RequireComponent(typeof(UIViewController))]
    public class SceneUIViewController : MonoBehaviour
    {
        public UIViewController ViewController
        {
            get { return GetComponent<UIViewController>(); }
        }
    }
}