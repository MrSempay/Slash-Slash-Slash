using PlayFab;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static StaticClassForAdditionalFunctions;

public class ParameterInternetSettings : MonoBehaviour, IControlLifeCicleFunctions
{
    public string selfName;
    public ButtonText recoveryButton;

    [SerializeField] private TextEdit _textDisplayName;
    [SerializeField] private TextEdit _textEmail;
    [SerializeField] private TextEdit _textPassword;
    [SerializeField] private TMP_InputField _textInputFieldEmail;
    [SerializeField] private TMP_InputField _textInputFieldDisplayName;
    [SerializeField] private TMP_InputField _textInputFieldPassword;

    private int _cooldownRecoverButton = 30;
    private bool _lockRecoveryButton = false;
    private string _email;
    private string _password;
    private string _displayName;
    private string _nameEmailUpdateFunction;
    private string _namePasswordUpdateFunction;
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
    public string Password
    {
        get { return _password; }
        set
        {
            _password = value;
            _textPassword.Text = value;
            object[] parameters = new object[] { value, (RectTransform)transform };
            CallFunctionByName(_namePasswordUpdateFunction, EventBus.Instance, parameters);
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
    public string PasswordLoaded
    {
        set
        {
            _password = value;
            _textInputFieldPassword.text = value;
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
    public void PasswordWasChanged() // только для сохранения в настройках сделали это. Вызывается при окончании редактирования
    {
        //Email = _textEmail.Text;
        Password = GetCleanText(_textInputFieldPassword.text); // нужно из InputField брать напрямую, иначе я получаю *********
    }

    public void ButtonLinkEmailWasPressed()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            if (ApplyCredentials())
            {
                PlayFabManager.Instance.LinkEmail(Email, Password);
            }
        }
        else
        {
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.ServiceUnavailable, TYPE_NOTIFICATION.Failure, 3, false);
        }   
    }

    public void ButtonLoginEmail()
    {
        if (ApplyCredentials())
        {
            PlayFabManager.Instance.LoginOrRegisterEmailIfFailureLoginMobile(Email, Password);
        }
        else
        {
            PlayFabManager.Instance.LoginOrRegisterMobile();
        }
    }
    public void ButtonRecoveryPasswordWasPressed()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            if (!_lockRecoveryButton) // рудиментная защита, мы делаем кнопку не кликабельной при нажатии ибо
            {
                PlayFabManager.Instance.RecoverPassword();
                CoroutineManager.Instance.StartManagedCoroutine(gameObject, CooldownTickRecoveryButton());
            }
        }
        else
        {
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.ServiceUnavailable, TYPE_NOTIFICATION.Failure, 3, false);
        }
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
    private bool ApplyCredentials()
    {
        string passwordForCheck = GetCleanText(_textInputFieldPassword.text);
        if (passwordForCheck.Length < 7 && passwordForCheck.Length != 0)
        {
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.PasswordTooShort, TYPE_NOTIFICATION.Failure, 4, false);
            return false;
        }

        Email = GetCleanText(_textEmail.Text);
        Password = passwordForCheck; // нужно из InputField брать напрямую, иначе я получаю *********

        return true;
    }

    private IEnumerator CooldownTickRecoveryButton()
    {
        _lockRecoveryButton = true;

        recoveryButton.buttonComponent.interactable = false;

        int timePassedInSeconds = 0;

        while (timePassedInSeconds < _cooldownRecoverButton)
        {
            recoveryButton.textButton.SetNotLocalizableText("(" + (_cooldownRecoverButton - timePassedInSeconds).ToString() + ")");
            timePassedInSeconds++;
            yield return new WaitForSecondsRealtime(1); // каждую секунду на 1 уменьшаем счётчик и обновляем UI-ку
        }
        recoveryButton.textButton.SetNotLocalizableText("");

        recoveryButton.buttonComponent.interactable = true;

        _lockRecoveryButton = false;
    }


    public void Awake()
    {
        PlayFabManager.Instance.OnGetDisplayNameFromEmailLogin += OnEmailLoginSuccess;
        if (!AwakeWasCalledAlready)
        {
            selfName = gameObject.name;
            _nameEmailUpdateFunction = C.NameFunc.TriggerEmailForLinkWasChanged;
            _nameDisplayNameUpdateFunction = C.NameFunc.TriggerDisplayNameWasChanged;
            _namePasswordUpdateFunction = C.NameFunc.TriggerPasswordWasChanged;
            AwakeWasCalledAlready = true;
        }

    }
    void Start()
    {

    }
    void Update()
    {

    }


    private void OnDestroy()
    {
        PlayFabManager.Instance.OnGetDisplayNameFromEmailLogin -= OnEmailLoginSuccess;

        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
    }

}
