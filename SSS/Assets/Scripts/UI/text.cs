using System.Xml;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class text : MonoBehaviour, ILocalizableText
{
    private TextMeshProUGUI textComponent;

    public string baseLocalizationKey;
    public string additionalLocalizationKey;
    private void Awake()
    {
        textComponent = gameObject.GetComponent<TextMeshProUGUI>();
        baseLocalizationKey = gameObject.name;
        Text = "";

    }
    private void Start()
    {

    }

    // по идее этому свойству мы должны присваивать не сам текст, а ключ его локализации, по нему уже само свойство будет находить нужную строку 
    public string Text
    {
        get { return additionalLocalizationKey; }
        set
        {
            string baseText = LocalizationManager.Instance.GetText(baseLocalizationKey);
            if (baseText == "")
            {
                Debug.Log(textComponent);
                Debug.Log(textComponent.text);
                Debug.Log(LocalizationManager.Instance.GetText(value));
                Debug.Log(value);
                textComponent.text = LocalizationManager.Instance.GetText(value);
            }
            else
                textComponent.text = LocalizationManager.Instance.GetText(baseLocalizationKey) + " " + LocalizationManager.Instance.GetText(value);
            additionalLocalizationKey = value;
        }
    }

    public void UpdateText()
    {
        Text = LocalizationManager.Instance.GetText(additionalLocalizationKey);
    }
}