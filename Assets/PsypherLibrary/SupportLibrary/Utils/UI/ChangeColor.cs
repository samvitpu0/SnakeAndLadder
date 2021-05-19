using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    public class ChangeColor : MonoBehaviour
    {
        public Image Image;

        private Color _loadedColor;

        void Awake()
        {
            Image = Image ?? GetComponent<Image>();
        }

        public void SetColor(string hexColor)
        {
            if (!ColorUtility.TryParseHtmlString(hexColor, out _loadedColor))
            {
                _loadedColor = Color.white;
            }
            else
            {
                Image.DOColor(_loadedColor, 0);
            }
        }
    }
}