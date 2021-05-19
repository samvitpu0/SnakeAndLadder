using System.Collections;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils
{
    public class selfDestroy : MonoBehaviour
    {
        public float triggerTimer = 0.5f; //self destroy after
        public bool shouldDisable = false;


        // Use this for initialization
        void OnEnable()
        {
            if (!shouldDisable)
                Destroy(transform.root.gameObject, triggerTimer);
            else
            {
                StartCoroutine(disable(triggerTimer));
            }
        }

        IEnumerator disable(float after)
        {
            yield return new WaitForSeconds(after);

            gameObject.SetActive(false);
        }
    }
}