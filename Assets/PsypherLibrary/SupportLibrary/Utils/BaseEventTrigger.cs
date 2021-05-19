using UnityEngine;
using UnityEngine.Events;

namespace PsypherLibrary.SupportLibrary.Utils
{
    public class BaseEventTrigger : MonoBehaviour
    {
        //unity events
        public UnityEvent eStart;

        public UnityEvent eEnable;
        public UnityEvent eDisable;
        public UnityEvent eDestroy;


        void Start()
        {
            if (eStart != null)
            {
                eStart.Invoke();
            }
        }

        void OnEnable()
        {
            if (eEnable != null)
            {
                eEnable.Invoke();
            }
        }

        void OnDisable()
        {
            if (eDisable != null)
            {
                eDisable.Invoke();
            }
        }

        void OnDestroy()
        {
            if (eDestroy != null)
            {
                eDestroy.Invoke();
            }
        }
    }
}