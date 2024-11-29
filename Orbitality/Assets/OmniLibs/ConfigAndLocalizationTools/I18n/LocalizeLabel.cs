using System;
using PortalHunter.I18n;
using TMPro;
using UnityEngine;

namespace PortalHunter.GameTools.I18n
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizeLabel : MonoBehaviour
    {
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                UpdateText();
            }
        }
        [SerializeField] string _key;
        [SerializeField] string _hint;

        TextMeshProUGUI textMesh;

        private void Start()
        {
            Init();
        }

        public string GetHint()
        {
            return string.IsNullOrEmpty(_hint) ? _hint : GetText();
        }

        public string GetText()
        {
#if UNITY_EDITOR
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshProUGUI>();
            }
#endif
            return textMesh.text;
        }

        private void Init()
        {
            textMesh = textMesh ?? GetComponent<TextMeshProUGUI>();
            UpdateText();
        }

        private void OnLocaleChange()
        {
            UpdateText();
        }
        private static bool developMode = false; // Do not localize, localization was not yet created.
        private void UpdateText()
        {
            var translation = I18nService.Instance.GetText(Key);
            var richTranslation = ProducerOfRich.MakeRich(translation); // Tags and icons for textmeshpro.
            var translationWithValues = PluralService.Instance.ParseString(richTranslation);

        }

        private void OnEnable()
        {
            try
            {
                I18nService.Instance.OnLocaleChanged += OnLocaleChange;
            }
            catch (Exception e)
            {
            }
        }

        private void OnDisable()
        {
            try
            {
                I18nService.Instance.OnLocaleChanged -= OnLocaleChange;
            }
            catch (Exception e)
            {
            }
        }
    }
}