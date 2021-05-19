using UnityEngine;

#if ENABLE_CATEGORY
namespace PsypherLibrary.SupportLibrary.CrossPromotion
{
    public class CrossPromoTest : MonoBehaviour
    {
        public PromoAdsStyle Type;
        public float Duration;

        public void ShowCrossPromoTest()
        {
            CrossPromoSystem.Instance.ShowCrossPromoAds(Type, Duration);
        }
    }
}
#endif