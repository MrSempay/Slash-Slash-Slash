using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static StaticClassForAdditionalFunctions;

public class ParameterInternetSettings : MonoBehaviour, IControlLifeCicleFunctions
{
    public string selfName;

    [SerializeField] private TextEdit _textDisplayName;
    [SerializeField] private TextEdit _textEmail;
    [SerializeField] private TMP_InputField _textInputFieldEmail;
    [SerializeField] private TMP_InputField _textInputFieldDisplayName;

    private string _email;
    private string _displayName;
    private string _nameEmailUpdateFunction;
    private string _nameDisplayNameUpdateFunction;


    public bool AwakeWasCalledAlready { get; set; }
    public bool StartWasCalledAlready { get; set; } 
    public string Email
    {
        get { return _email; }
        set
        {
            _email = value;
            _textEmail.Text = value;
            object[] parameters = new object[] { value, (RectTransform)transform };
            CallFunctionByName(_nameEmailUpdateFunction, EventBus.Instance, parameters);
        }
    }
    public string DisplayName // Не при всяком изменении DisplayName необходимо пытаться изменить его на сервере. Поэтому логика разделена для загрузки/визуализации и логики вызова сервера
    {
        get { return _displayName; }
        set
        {
            _displayName = value;
            _textDisplayName.Text = value;
            object[] parameters = new object[] { value, (RectTransform)transform };
            CallFunctionByName(_nameDisplayNameUpdateFunction, EventBus.Instance, parameters);
        }
    }
    public string EmailLoaded
    {
        set
        {
            _email = value;
            _textInputFieldEmail.text = value;
        }
    }
    public string DisplayNameLoaded
    {
        set
        {
            _displayName = value;
            _textInputFieldDisplayName.text = value;
        }
    }



    public void Awake()
    {
        PlayFabManager.Instance.OnGetDisplayNameFromEmailLogin += OnEmailLoginSuccess;
        if (!AwakeWasCalledAlready)
        {
            selfName = gameObject.name;
            _nameEmailUpdateFunction = C.NameFunc.TriggerEmailForLinkWasChanged;
            _nameDisplayNameUpdateFunction = C.NameFunc.TriggerDisplayNameWasChanged;
            AwakeWasCalledAlready = true;
        }

    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayNameWasChanged()
    {
        bool isLongEnoughDisplayName = PlayFabManager.Instance.ChangeDisplayName(_textDisplayName.Text);
        //DisplayName = _textDisplayName.Text;
        DisplayName = GetCleanText(_textDisplayName.Text);
        if (!isLongEnoughDisplayName)
        {
            //Debug.Log("Таки тут...");
        }
    }
    public void EmailWasChanged() // только для сохранения в настройках сделали это. Вызывается при окончании редактирования
    {
        //Email = _textEmail.Text;
        Email = GetCleanText(_textEmail.Text);
    }

    public void ButtonLinkEmailWasPressed()
    {
        Email = GetCleanText(_textEmail.Text);
        PlayFabManager.Instance.LinkEmail(Email);
    }
    public void ButtonLoginEmail()
    {
        Email = GetCleanText(_textEmail.Text);
        PlayFabManager.Instance.LoginOrRegisterEmailIfFailureLoginMobile(Email); 
    }

    private void OnEmailLoginSuccess(string displayName)
    {
        
        _textInputFieldDisplayName.text = displayName; 
    }

    private string GetCleanText(string wrongText)
    {
        string raw = wrongText;
        // Удаляем zero-width символы
        string cleaned = Regex.Replace(raw, "[\u200B-\u200D\uFEFF]", "");
        return cleaned.Trim();
    }

    private void OnDestroy()
    {
        PlayFabManager.Instance.OnGetDisplayNameFromEmailLogin -= OnEmailLoginSuccess;
    }

}
