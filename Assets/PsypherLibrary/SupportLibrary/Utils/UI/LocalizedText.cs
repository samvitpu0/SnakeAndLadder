using System;
using System.Linq;
using PsypherLibrary.SupportLibrary.BaseDataStructure;
using PsypherLibrary.SupportLibrary.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    [RequireComponent(typeof(Text))]
    [Serializable]
    public class LocalizedText : MonoBehaviour
    {
        static LocalizationData data = null;

        public static LocalizationData Data
        {
            get
            {
                if (data == null)
                    data = LocalDataManager.Instance.GetConfig<LocalizationData>();
                if (data == null)
                    data = new LocalizationData();
                return data;
            }
        }

        private string TextTag;
        private Text _text = null;

        private Text TextComponent
        {
            get
            {
                if (_text == null)
                    _text = GetComponent<Text>();
                return _text;
            }
        }

        public static string GetLocalizedText(string tag)
        {
            if (Data.LocalizationTextBindings.ContainsKey(tag))
            {
                var index =
                    Data.Languages.ToList()
                        .FindIndex(x => string.Equals(x, LocalDataManager.Instance.SaveData.CurrentLanguage));
                if (index >= 0)
                {
                    return Data.LocalizationTextBindings[tag][index];
                }
            }

            return null;
        }

        void OnEnable()
        {
            TextTag = !string.IsNullOrEmpty(TextComponent.text) && TextComponent.text.Contains(":") ? TextComponent.text.Split(':')[0] : "";
            ConfigManager.OnLocalizationTextLoaded += SetText;
            TextComponent.raycastTarget = false;
            SetText();
        }

        void OnDisable()
        {
            ConfigManager.OnLocalizationTextLoaded -= SetText;
        }

        void SetText()
        {
            if (Data.LocalizationTextBindings.ContainsKey(TextTag))
            {
                var index =
                    Data.Languages.ToList()
                        .FindIndex(x => string.Equals(x, LocalDataManager.Instance.SaveData.CurrentLanguage));
                if (index >= 0)
                {
                    TextComponent.text = Data.LocalizationTextBindings[TextTag][index];
                    return;
                }
            }

            if (!string.IsNullOrEmpty(TextComponent.text))
                TextComponent.text = TextComponent.text.Contains(":") ? TextComponent.text.Split(':')[1] : TextComponent.text;
        }
    }
}