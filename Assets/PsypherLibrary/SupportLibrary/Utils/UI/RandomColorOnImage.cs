using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    [RequireComponent(typeof(Image))]
    public class RandomColorOnImage : MonoBehaviour
    {
        private Image _image;
        public Gradient Colors;

        void Awake()
        {
            _image = GetComponent<Image>();
        }

        void OnEnable()
        {
            if (_image)
            {
                var sColor = Colors.colorKeys.PickRandom().color;
                _image.color = sColor;
            }
        }
    }
}