using System.Xml;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TextEdit : MonoBehaviour, ILocalizableText, IControlLifeCicleFunctions
{
    private TextMeshProUGUI textComponent;

    public string baseLocalizationKey = "";
    public string additionalLocalizationKey = "";
    public string notLocalizableText = "";

    public bool AwakeWasCalledAlready { get; set; }
    public bool StartWasCalledAlready { get; set; }

    public void Awake()
    {
        if (!AwakeWasCalledAlready)
        {
            AwakeWasCalledAlready = true;

            textComponent = gameObject.GetComponent<TextMeshProUGUI>();

            textComponent.font = GameManager.Instance.globalFont;

            if (baseLocalizationKey == "")
            {
            //    baseLocalizationKey = gameObject.name;
            }
            Text = ""; // просто начальная инициализация текста
        }
    }
    private void Start()
    {

    }

    // по идее этому свойству мы должны присваивать не сам текст, а ключ его локализации, по нему уже само свойство будет находить нужную строку 
    public string Text
    {
        get { return textComponent.text; } // было return additionalLocalizationKey;
        set
        {
            if (!AwakeWasCalledAlready)
            {
                Awake();
            }

            additionalLocalizationKey = value; 
            string baseText = LocalizationManager.Instance.GetText(baseLocalizationKey);
            if (baseText == "")
            {   
                textComponent.text = LocalizationManager.Instance.GetText(value);
            }
            else
            {
                string settingValue = LocalizationManager.Instance.GetText(value);
                if (settingValue != "")
                {
                    textComponent.text = baseText + " " + settingValue;
                }
                else
                {
                    textComponent.text = baseText;
                }
            }
        }
    }

    public void UpdateText()
    {
        Text = additionalLocalizationKey;
        //textComponent.text += " " + notLocalizableText;
        if (notLocalizableText != "")
        {
            if (additionalLocalizationKey != "" || baseLocalizationKey != "")
            {
                textComponent.text += " " + notLocalizableText;
            }
            else
            {
                textComponent.text += notLocalizableText;
            }
        }
    }
    public void SetNotLocalizableText(string text)
    {
        notLocalizableText = text;
        UpdateText();
    }
    public void SetBaseText(string text)
    {
        baseLocalizationKey = text;
        UpdateText();
    }
}