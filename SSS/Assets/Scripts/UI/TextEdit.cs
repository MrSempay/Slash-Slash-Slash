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

    public bool awakeWasCalledAlready { get; set; }

    public void Awake()
    {
        if (!awakeWasCalledAlready)
        {
            awakeWasCalledAlready = true;

            textComponent = gameObject.GetComponent<TextMeshProUGUI>();

            textComponent.font = GameManager.Instance.globalFont;

            if (baseLocalizationKey == "")
            {
            //    baseLocalizationKey = gameObject.name;
            }
            Text = ""; // просто начальна€ инициализаци€ текста
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
            if (!awakeWasCalledAlready)
            {
                Awake();
            }

            additionalLocalizationKey = value; 
            string baseText = LocalizationManager.Instance.GetText(baseLocalizationKey);
            if (baseText == "")
            {   
                //Debug.Log("ћƒјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјј");
                //Debug.Log(value);
                //Debug.Log(textComponent);
                textComponent.text = LocalizationManager.Instance.GetText(value);
            }
            else
            {
                string settingValue = LocalizationManager.Instance.GetText(value);
                if (settingValue != "")
                {
                    //Debug.Log("≈банина1");
                    //Debug.Log("ћƒјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјј");
                    //Debug.Log(value);
                    //Debug.Log(textComponent);
                    //Debug.Log(textComponent.text);
                    //Debug.Log(LocalizationManager.Instance.GetText(value));
                    textComponent.text = LocalizationManager.Instance.GetText(baseLocalizationKey) + " " + LocalizationManager.Instance.GetText(value);
                }
                else
                {
                    textComponent.text = LocalizationManager.Instance.GetText(baseLocalizationKey);
                }
            }
        }
    }

    public void UpdateText()
    {
        //Debug.Log("SHITTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT");
        //Debug.Log(additionalLocalizationKey);
        Text = additionalLocalizationKey;
        //textComponent.text += " " + notLocalizableText;
        if (notLocalizableText != "")
        {
            if (additionalLocalizationKey != "")
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
        //Debug.Log("SHITTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT");
        //Debug.Log(additionalLocalizationKey);
        notLocalizableText = text;
        UpdateText();
    }
    public void SetBaseText(string text)
    {
        //Debug.Log("≈банина");
        baseLocalizationKey = text;
        UpdateText();
    }
}