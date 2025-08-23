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


    public bool awakeWasCalledAlready { get; set; }
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
        if (!awakeWasCalledAlready)
        {
            selfName = gameObject.name;
            _nameEmailUpdateFunction = C.NameFunc.TriggerEmailForLinkWasChanged;
            _nameDisplayNameUpdateFunction = C.NameFunc.TriggerDisplayNameWasChanged;
            awakeWasCalledAlready = true;
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
        DisplayName = _textDisplayName.Text;
        if (!isLongEnoughDisplayName)
        {
            Debug.Log("Таки тут...");
        }
    }
    public void EmailWasChanged() // только для сохранения в настройках сделали это. Вызывается при окончании редактирования
    {
        Email = _textEmail.Text;
    }

    public void ButtonLinkEmailWasPressed()
    {
        PlayFabManager.Instance.LinkEmail(_textEmail.Text);
        Email = _textEmail.Text;
    }
    public void ButtonLoginEmail()
    {
        PlayFabManager.Instance.LoginOrRegisterEmailIfFailureLoginMobile(_textEmail.Text);
        Email = _textEmail.Text;
    }

    private void OnEmailLoginSuccess(string displayName)
    {
        
        _textInputFieldDisplayName.text = displayName; 
    }

    private void OnDestroy()
    {
        PlayFabManager.Instance.OnGetDisplayNameFromEmailLogin -= OnEmailLoginSuccess;
    }

}
