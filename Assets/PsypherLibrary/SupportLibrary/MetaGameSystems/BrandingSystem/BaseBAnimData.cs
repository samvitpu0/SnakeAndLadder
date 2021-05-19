using System;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.BrandingSystem
{
    public abstract class BaseBAnimData : MonoBehaviour
    {
        #region Fields and Properties

        /// <summary>
        /// sequence to be played on play action
        /// </summary>
        protected Sequence PlaySequence;

        /// <summary>
        /// sequence to be played on stop action
        /// </summary>
        protected Sequence StopSequence;

        [SerializeField]
        protected BrandingManager.BrandingData BrandingData;

        public int LoopCount = 0;

        #endregion

        #region Initialization

        protected virtual void Awake()
        {
            //deactivating/fading out images and video display on awake
            switch (BrandingData.Type)
            {
                case BrandingManager.BrandingType.Image:
                {
                    BrandingData.ImageAdDisplay.Deactivate();
                }
                    break;
                case BrandingManager.BrandingType.Video:
                {
                    BrandingData.VideoAdDisplay.Deactivate();
                }
                    break;
            }


            Initialize(BrandingData);
        }

        protected virtual void OnDestroy()
        {
            //when this gameObject is destroy [or during scene change], make sure to kill the tweens
            if (PlaySequence != null && PlaySequence.IsActive())
            {
                PlaySequence.Kill();
            }

            if (StopSequence != null && StopSequence.IsActive())
            {
                StopSequence.Kill();
            }
        }

        #endregion

        #region Abstract Methods

        protected abstract void Initialize(BrandingManager.BrandingData brandingData);

        public abstract void Play();

        public abstract void Stop();

        public abstract void Restart();

        #endregion
    }
}