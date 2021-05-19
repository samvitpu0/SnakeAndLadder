using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    public class UIPagination : MonoBehaviour
    {
        public bool IsHorizontal = false;
        public CanvasScaler canvasScaler;
        public float PageSlideSpeed = 0.2f;


        void Start()
        {
            OrderPages();
        }

        private void OrderPages()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild((transform.childCount - 1) - i).GetComponent<RectTransform>().anchoredPosition =
                    IsHorizontal
                        ? new Vector2(i * canvasScaler.referenceResolution.x, 0)
                        : new Vector2(0, i * canvasScaler.referenceResolution.y);
            }
        }

        public void BringPageToView(Transform t)
        {
            var pagesToShift = IsHorizontal
                ? t.GetComponent<RectTransform>().anchoredPosition.x / canvasScaler.referenceResolution.x
                : t.GetComponent<RectTransform>().anchoredPosition.y / canvasScaler.referenceResolution.y;

            foreach (RectTransform child in GetComponent<RectTransform>())
            {
                var finalPos = IsHorizontal
                    ? new Vector2(child.anchoredPosition.x - (pagesToShift * canvasScaler.referenceResolution.x),
                        child.anchoredPosition.y)
                    : new Vector2(child.anchoredPosition.x,
                        child.anchoredPosition.y - (pagesToShift * canvasScaler.referenceResolution.y));

                child.DOAnchorPos(finalPos, PageSlideSpeed).OnStart(() => { child.GetComponent<CanvasGroup>().interactable = false; }).OnComplete(() => { child.GetComponent<CanvasGroup>().interactable = true; });
            }
        }
    }
}