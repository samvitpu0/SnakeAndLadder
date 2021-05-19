using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    [RequireComponent(typeof(Slider))]
    public class SliderFill : MonoBehaviour
    {
        public float showFillerThreshold = 5;
        private Slider _slider;

        void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(OnValueChanged);
        }


        void OnValueChanged(float value)
        {
            if (value > showFillerThreshold)
            {
                _slider.fillRect.Activate();
            }
            else
            {
                _slider.fillRect.Deactivate();
            }
        }
    }
}