using System.Collections;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    public class SizeFitter : MonoBehaviour
    {
        public RectTransform ContentsHolder;
        public bool AutoFitOnStart;

        [Tooltip("This will always follow the Anchors")]
        public Vector2 ExtraPadding;

        private Vector2 _newSizeDelta;

        IEnumerator Start()
        {
            if (AutoFitOnStart)
            {
                yield return new WaitForSeconds(0.5f);
                FitToContent();
            }
        }

        public void FitToContent()
        {
            Canvas.ForceUpdateCanvases();
            var thisRect = GetComponent<RectTransform>();
            float height = 0;

            if (ContentsHolder)
            {
                //getting the size of the childs
                for (int i = 0; i < ContentsHolder.childCount; i++)
                {
                    var childHeight = ContentsHolder.GetChild(i).GetComponent<RectTransform>().rect.size.y;
                    height += childHeight;
                }
            }

            _newSizeDelta = new Vector2(thisRect.sizeDelta.x + ExtraPadding.x, height + ExtraPadding.y);
            thisRect.sizeDelta = _newSizeDelta;
        }
    }
}