using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;

namespace PsypherLibrary.SupportLibrary.MetaGameSystems.BrandingSystem.DemoBranding
{
    public class DemoBAnimData : BaseBAnimData
    {
        #region Defining Abstract

        protected override void Initialize(BrandingManager.BrandingData brandingData)
        {
            PlaySequence = DOTween.Sequence();
            StopSequence = DOTween.Sequence();

            switch (brandingData.Type)
            {
                case BrandingManager.BrandingType.Image:
                {
                    var imageDisplay = brandingData.ImageAdDisplay;

                    brandingData.AdsImages.ForEach(x =>
                    {
                        PlaySequence.Append(imageDisplay.DOFade(0, 0))
                            .AppendCallback(() =>
                            {
                                imageDisplay.sprite = x;
                                imageDisplay.Activate();
                            })
                            .Append(imageDisplay.DOFade(1, brandingData.AnimationTime))
                            .AppendInterval(brandingData.DisplayTime)
                            .Append(imageDisplay.DOFade(0, brandingData.TransitionTime));
                    });

                    PlaySequence.SetLoops(LoopCount);

                    StopSequence.Append(imageDisplay.DOFade(0, brandingData.TransitionTime))
                        .AppendCallback(imageDisplay.Deactivate);
                }
                    break;
                case BrandingManager.BrandingType.Video:
                {
                    //todo:implement later
                    var videoDisplay = brandingData.VideoAdDisplay;
                    videoDisplay.Activate();
                }
                    break;
            }
        }

        public override void Play()
        {
            PlaySequence.Play();
        }

        public override void Stop()
        {
            StopSequence.Play();
        }

        public override void Restart()
        {
            PlaySequence.Restart();
        }

        #endregion
    }
}