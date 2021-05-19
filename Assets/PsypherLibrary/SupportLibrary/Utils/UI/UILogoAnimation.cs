using System.Collections.Generic;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.UIHelperScripts;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    public class UILogoAnimation : UIPanel
    {
        #region properties and fields

        [Header("Logo Animations \n")]
        public bool AutoPlay;

        public float AnimationDuration = 2;
        public Image LogoImage;

        public List<Sprite> LogoSprites;

        private Sequence _logoAnimSequence;

        #endregion

        #region initialization

        private void Start()
        {
            Initialize();

            if (AutoPlay) //if autoPlay is true, play the tween after initialization
                Play();
        }

        private void Initialize()
        {
            _logoAnimSequence = DOTween.Sequence();

            _logoAnimSequence.AppendCallback(() => ActivatePanel());

            foreach (var logoSprite in LogoSprites)
            {
                _logoAnimSequence.Append(LogoImage.DOFade(0, 0))
                    .AppendCallback(() =>
                    {
                        LogoImage.overrideSprite = logoSprite;
                        LogoImage.preserveAspect = true;
                    })
                    .Append(LogoImage.DOFade(1, AnimationDuration / LogoSprites.Count))
                    .Append(LogoImage.DOFade(0, 0.3f));
            }

            //_logoAnimSequence.AppendCallback(DeactivatePanel);
        }

        #endregion

        #region Actions

        /// <summary>
        /// plays the logo animation
        /// </summary>
        /// <returns></returns>
        public Tween Play()
        {
            _logoAnimSequence.Play();
            return _logoAnimSequence;
        }

        #endregion
    }
}