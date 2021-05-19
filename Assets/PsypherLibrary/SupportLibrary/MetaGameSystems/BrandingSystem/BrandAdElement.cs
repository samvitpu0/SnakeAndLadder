using UnityEngine;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.BrandingSystem
{
    [RequireComponent(typeof(BaseBAnimData))]
    public class BrandAdElement : MonoBehaviour
    {
        private bool _isInitialized = false;

        public string UID;
        public BaseBAnimData BAnimData;

        #region Initialization

        /*
        private void Awake()
        {
            Initialize();
        }
*/

        public virtual void Initialize()
        {
            if (_isInitialized) return;

            if (BAnimData == null)
            {
                BAnimData = GetComponent<BaseBAnimData>();
            }

            _isInitialized = true;
        }

        #endregion

        #region Actions

        public virtual void Play()
        {
            BAnimData.Play();
        }

        public virtual void Stop()
        {
            BAnimData.Stop();
        }

        public virtual void Restart()
        {
            BAnimData.Restart();
        }

        #endregion
    }
}