using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Extensions
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        public bool CanBeDestroyed = true;

        void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}