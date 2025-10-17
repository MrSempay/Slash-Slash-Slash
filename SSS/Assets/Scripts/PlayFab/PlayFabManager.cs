using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.SharedModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;
using static StaticClassForAdditionalFunctions;


public class PlayFabManager : MonoBehaviour
{
    private readonly string _generalPassword = "DefaultPass123";

    private static PlayFabManager _instance;
    private string _userEmail = "";
    private string _userPassword = "";
    //private string _userPassword;
    private string _userName = "DefaultName"; // увы, не используем. Эта штука должна быть уникальной
    private string _displayName = "";
    private string _successEmail = "";
    private float _timeRepeatTryingLogin = 30f;

    public event Action<string> OnGetDisplayNameFromEmailLogin;
    public event Action<string> OnGetIDTitleAccountAfterLogin;
    public event Action OnLoginSuccess;
    public string IDTitleAccountLast = "";
    public GetAccountInfoResult accountInfoResult;

    public static PlayFabManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("PlayFabManager");
                _instance = obj.AddComponent<PlayFabManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public void Initialize() { }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        OnLoginSuccess += LoginSuccess;
    }



    public void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            /*
            Please change the titleId below to your own titleId from PlayFab Game Manager.
            If you have already set the value in the Editor Extensions, this can be skipped.
            */
            PlayFabSettings.staticSettings.TitleId = "1C0876";
        }
        //Debug.Log(ReturnMobileID());
        StartCoroutine(StupidDelay());
        StartCoroutine(RepeatLogin());
        //var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
        //PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

    }

    private IEnumerator StupidDelay()
    {
        yield return null;
        LoginOrRegisterEmailIfFailureLoginMobile(_userEmail, _userPassword, OnEmailLoginFailureAtStart);
         
    }

    #region Change credentials settings

    public void LoginOrRegisterDeviceID()
    {

    }
    public void DisplayNameWasChanged(string displayName)
    {

    }

# endregion


# region Login/Registration/Linking email


    public static PlayFabAuthenticationContext contextCurrentSession;
    private string _lastEmail = "";

    public void LinkEmail(string email, string password)
    {
        if (email != "")
        {
            //GetEmailFromServer();
            if (email == _successEmail) // это - если мы уже залогинились, знаем наш локальный email и говорим, что наш аккаунт уже привязан
            {
                Debug.Log("Текущий аккаунт УЖЕ привязан к заданному Email-у!");
                GameManager.Instance.InvokeAppearingNotification(C.Notifications.AccountAlreadyLinkedToSpecifiedEmail, TYPE_NOTIFICATION.Warning, 4, false);
                return;
            }
            _userEmail = email;
            var linkEmail = new AddUsernamePasswordRequest { Email = email, Password = password, Username = UserNameGenerator.GenerateRandomName(4) };
            PlayFabClientAPI.AddUsernamePassword(linkEmail, OnEmailLinkSuccess, OnEmailLinkFailure);
        }
    }
    private void OnEmailLinkSuccess(AddUsernamePasswordResult result)
    {
        Debug.Log("Привязали к аккаунту почту...");
        GameManager.Instance.InvokeAppearingNotification(C.Notifications.AccountLinked, TYPE_NOTIFICATION.Success, 4, false);

        //LoginOrRegisterEmailIfFailureLoginMobile(_userEmail);
        //_OnLinkEmailSuccess?.Invoke(); 
        //contextCurrentSession = result.AuthenticationContext;
    }
    private void OnEmailLinkFailure(PlayFabError error)
    {
        //_userEmail = ""; // если привязать к почте не удалось, то сбрасываем текущее значение Email-а до значения по умолчанию
        Debug.Log(error.ToString() + " Ошибка привязки почты к аккаунту!" + " Код ошибки: " + error.Error);
        //Debug.Log(error.Error);
        if (error.Error == PlayFabErrorCode.AccountAlreadyLinked)
        {
            GetEmailFromServer(); // это - если мы ещё не залогинились, не знаем наш локальный email и хотим получить его с сервера и проверить, то ли мы пытаемся привязать аккаунт к какому-то
                                  // уже привязанному email-у, то ли хотим привязать к email-у, к которому этот аккаунт уже привязан - в таком случае скажем, что просто залогиньтесь.
            return;
        }

        if (error.Error == PlayFabErrorCode.InvalidParams) // пароль али почта не корректный формат имеют
        {
            Debug.Log("Неправильный формат электронной почты или пароля!");
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.InvalidFormatEmailAddressOrPassword, TYPE_NOTIFICATION.Failure, 4, false);
            return;
        }
        if (error.Error == PlayFabErrorCode.EmailAddressNotAvailable) // если мы УЖЕ залогинены и пытаемся привязать к аккаунту почту, которая уже привязана к какому-то другому аккаунту
        {
            Debug.Log("Данная электронная почту уже привязана к другому аккаунту");
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.EmailAddressNotAvailable, TYPE_NOTIFICATION.Failure, 4, false);
            return;
        }
        if (error.Error == PlayFabErrorCode.UsernameNotAvailable)
        {
            Debug.LogWarning("Имя пользователя уже занято, пробуем другое.");
            var linkEmail = new AddUsernamePasswordRequest { Email = _userEmail, Password = _generalPassword, Username = UserNameGenerator.GenerateRandomName(4) };
            PlayFabClientAPI.AddUsernamePassword(linkEmail, OnEmailLinkSuccess, OnEmailLinkFailure);
        }
        //_OnLinkEmailFailure?.Invoke();
        //Debug.Log(error.GetType());
        //Debug.Log(error.GenerateErrorReport());

    }


    public void LoginOrRegisterEmailIfFailureLoginMobile(string email, string password) // для публичного доступа к методу логина со стандартной CallBack функцией OnEmailLoginFailure
    {
        LoginOrRegisterEmailIfFailureLoginMobile(email, password, OnEmailLoginFailure);
    }
    private void LoginOrRegisterEmailIfFailureLoginMobile(string email, string password, Action<PlayFabError> FailureEmailFunction) // для возможности задания в Start своей CallBack функции
    {
        if (email != "")
        {
            _userEmail = email;
            var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnEmailLoginSuccess, FailureEmailFunction);
        }
        else
        {
            LoginOrRegisterMobile();
        }
    } 
    private void OnEmailLoginSuccess(LoginResult result)
    {
        Debug.Log("Залогинились (Email)"); 
        OnLoginSuccess?.Invoke();
        GameManager.Instance.InvokeAppearingNotification(C.Notifications.SignInEmail, TYPE_NOTIFICATION.Success, 4, false);
        _successEmail = _userEmail;

        GetDisplayNameFromServer();



        //OnGetDisplayNameFromEmailLogin.Invoke(result.InfoResultPayload.AccountInfo.Username); // подписываемся в ParameterLinkEmail, будем обновлять там текстовое поле DisplayName
        //contextCurrentSession = result.AuthenticationContext;
    }
    private void OnEmailLoginFailure(PlayFabError error)
    {
        Debug.Log(error.ToString());
        Debug.Log(error.Error);
        if (error.Error == PlayFabErrorCode.InvalidParams) // пароль али почта не корректный формат имеют
        {
            Debug.Log(_userPassword);
            Debug.Log(_userEmail);
            Debug.Log("Неправильный формат электронной почты или пароля!" + " Пароль: " + _userPassword + ", Email: " + _userEmail);
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.InvalidFormatEmailAddressOrPassword, TYPE_NOTIFICATION.Failure, 4, false);
        }
        if (error.Error == PlayFabErrorCode.InvalidEmailOrPassword) // не удаётся (но формат верный!) найти в базе данных аккаунт с таким Email-ом и паролем
        { // Я НЕ ПОНИМАЮ, ЧТО ЗА ДИЧЬ. ПО ИДЕЕ ЭТО ТОЛЬКО КОГДА НЕПРАВИЛЬНЫЙ ПАРОЛЬ. Ибо на несуществующий (но по формату верный!) Email оно выдаёт ошибку PlayFabErrorCode.AccountNotFound
            Debug.Log("Не удаётся найти аккаунт с заданными Email-ом и паролем!" + " Пароль: " + _userPassword + ", Email: " + _userEmail);
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.InvalidEmailAddress, TYPE_NOTIFICATION.Failure, 4, false);
        }
        if (error.Error == PlayFabErrorCode.AccountNotFound) // подразумевается, что тут будет проблема только с электронной почтой, ибо пароль у нас по-умолчанию
        {
            Debug.Log("Для данной электронной почты аккаунт не найден!"); 
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.AccountNotFound, TYPE_NOTIFICATION.Failure, 4, false);
        }
        if (!PlayFabClientAPI.IsClientLoggedIn()) // нужно для того, чтобы если мы уже в каком-то аккаунте находимся (условно в третьем) и пытаемся зайти в другой (условно второй),
                                                  // и у нас это не получилось - нас автоматом не логинило на базовый аккаунт этого телефона (первый), а осталвляло в текущем (третьем)
        {
            LoginOrRegisterMobile();
        }
        
    }
    private void OnEmailLoginFailureAtStart(PlayFabError error)
    {
        Debug.Log(error.ToString());
        Debug.Log(error.Error);
        if (error.Error == PlayFabErrorCode.InvalidParams) // пароль али почта не корректный формат имеют
        {
            Debug.Log("Неправильный формат электронной почты или пароля!" + " Пароль: " + _userPassword + ", Email: " + _userEmail);

            if (_userPassword != "" || _userEmail != "") // собсна, только ради этой проверки функцию другую и бахнули
            {
                GameManager.Instance.InvokeAppearingNotification(C.Notifications.InvalidFormatEmailAddressOrPassword, TYPE_NOTIFICATION.Failure, 4, false);
            }

        }
        if (error.Error == PlayFabErrorCode.AccountNotFound) // в этой функции такой проблемы вообще быть не может. А может и может)
        {
            Debug.Log("Для данной электронной почты аккаунт не найден!"); 
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.AccountNotFound, TYPE_NOTIFICATION.Failure, 4, false);
        }
        if (!PlayFabClientAPI.IsClientLoggedIn()) // нужно для того, чтобы если мы уже в каком-то аккаунте находимся (условно в третьем) и пытаемся зайти в другой (условно второй),
                                                  // и у нас это не получилось - нас автоматом не логинило на базовый аккаунт этого телефона (первый), а осталвляло в текущем (третьем)
        {
            LoginOrRegisterMobile();
        }
        
    }


    public void LoginOrRegisterMobile()
    {

#if UNITY_ANDROID
        var requestAndroid = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileID(), CreateAccount = true };
        PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginMobileSuccess, OnLoginMobileFailure);

#endif

#if UNITY_IOS
        var requestIOS = new LoginWithIOSDeviceIDRequest { DeviceId = ReturnMobileID(), CreateAccount = true };
        PlayFabClientAPI.LoginWithIOSDeviceID(requestIOS, OnLoginMobileSuccess, OnLoginMobileFailure);
  

#endif

    }
    private void OnLoginMobileSuccess(LoginResult result)
    {
        Debug.Log("Залогинились (Mobile)");

        OnLoginSuccess?.Invoke();

        GameManager.Instance.InvokeAppearingNotification(C.Notifications.SignInIDMobile, TYPE_NOTIFICATION.Success, 4, false);
        //Debug.Log(OnGetDisplayNameFromEmailLogin);
        //Debug.Log(result);
        // Получаем инфо об аккаунте
        GetDisplayNameFromServer();
        //contextCurrentSession = result.AuthenticationContext;
    }
    private void OnLoginMobileFailure(PlayFabError error)
    {
        //Debug.Log(error.ToString());
        //Debug.Log(error.GetType());
        Debug.Log(error.Error);
        Debug.Log(error.GenerateErrorReport());

        if (error.Error == PlayFabErrorCode.ServiceUnavailable) // если сервис PlayFab недоступен по той или иной причине (хоть даже из-за интернета)
        {
            Debug.Log("Сервис PlayFab недоступен!");
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.ServiceUnavailable, TYPE_NOTIFICATION.Failure, 4, false);
        }
    }

# region Check Email From Server

    private void GetEmailFromServer()
    {
        var request = new GetAccountInfoRequest(); // пустой → значит про текущего игрока
        PlayFabClientAPI.GetAccountInfo(request, OnGetAccountEmailSuccess, OnGetAccountEmailFailure);
    }
    private void OnGetAccountEmailSuccess(GetAccountInfoResult result)
    {
        string email = result.AccountInfo?.PrivateInfo?.Email;
        //string username = result.AccountInfo?.Username; // Username тоже может быть, если задавался при регистрации
        Debug.Log("Чё за дичь?");
        Debug.Log(email);
        Debug.Log(_userEmail);


        if (email == _userEmail)
        {
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.AccountAlreadyLinkedToSpecifiedEmail, TYPE_NOTIFICATION.Warning, 4, false);
            Debug.Log("Данный аккаунт уже привязан к заданному Email-у: " + email + ". Нажмите Логин.");
            return;
        }
        else
        {
            Debug.Log("Текщий аккаунт уже привязан к другой почте!");
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.AccountAlreadyLinked, TYPE_NOTIFICATION.Failure, 4, false);
            return;
        }


        //if (!string.IsNullOrEmpty(username))
        //    Debug.Log("Username: " + username);
        //else
        //    Debug.Log("Username ещё не задан");
    }
    private void OnGetAccountEmailFailure(PlayFabError error)
    {
        Debug.LogError("Ошибка при получении инфо об аккаунте: " + error.GenerateErrorReport());
    }

# endregion

# region Get Display Name From Server

    private void GetDisplayNameFromServer()
    {
        var request = new GetAccountInfoRequest(); // пустой → значит про текущего игрока
        PlayFabClientAPI.GetAccountInfo(request, OnGetAccountDisplayNameSuccess, OnGetAccountDisplayNameFailure);
    }
    private void OnGetAccountDisplayNameSuccess(GetAccountInfoResult result)
    {
        string displayName = result.AccountInfo?.TitleInfo?.DisplayName; 
        //string username = result.AccountInfo?.Username; // Username тоже может быть, если задавался при регистрации


        if (!string.IsNullOrEmpty(displayName)) 
        { 
            Debug.Log("DisplayName: " + displayName);
            OnGetDisplayNameFromEmailLogin?.Invoke(displayName); // подписываемся в ParameterLinkEmail, будем обновлять там текстовое поле DisplayName
        }
        else
        {
            OnGetDisplayNameFromEmailLogin?.Invoke(""); // подписываемся в ParameterLinkEmail, будем обновлять там текстовое поле DisplayName
            Debug.Log("DisplayName ещё не задан");
        }

        //if (!string.IsNullOrEmpty(username))
        //    Debug.Log("Username: " + username);
        //else
        //    Debug.Log("Username ещё не задан");
    }
    private void OnGetAccountDisplayNameFailure(PlayFabError error)
    {
        Debug.LogError("Ошибка при получении инфо об аккаунте: " + error.GenerateErrorReport());
    }

    #endregion

    #region RecoverPassword

    public void RecoverPassword()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.Log(_userEmail);
            Debug.Log(PlayFabSettings.TitleId);
            var request = new SendAccountRecoveryEmailRequest
            {
                Email = _userEmail,          // почта, на которую зарегистрирован аккаунт
                TitleId = PlayFabSettings.TitleId
            };

            PlayFabClientAPI.SendAccountRecoveryEmail(request, result =>
            {
                Debug.Log("Письмо для восстановления пароля отправлено!");
                GameManager.Instance.InvokeAppearingNotification(C.Notifications.EmailPasswordRecoveyrWasSent, TYPE_NOTIFICATION.Success, 4, false);
            }, error =>
            {
                Debug.Log("Ошибка при запросе восстановления пароля: " + error.GenerateErrorReport() + " Тип ошибки: " + error.Error);
                GameManager.Instance.InvokeAppearingNotification(C.Notifications.EmailPasswordRecoverFailure, TYPE_NOTIFICATION.Failure, 4, false);
            });
        }
    }

    #endregion


    //private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    //{
    //    Debug.Log("Зарегались");
    //    PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = _userName }, OnDisplayName, OnUpdatingDisplayNameFailure);
    //}



    //private void OnRegisterFailure(PlayFabError error)
    //{
    //    Debug.LogError(error.GenerateErrorReport());
    //}

    public bool ChangeDisplayName(string displayName)
    {
        Debug.Log("Меняем Display Name");
        if (displayName.Length < 4)
        {
            Debug.Log("Display Name должен состоять из не менее 3-ёх символов!");
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.DisplayNameTooShort, TYPE_NOTIFICATION.Failure, 4, false);
            return false;
        }
        _displayName = displayName;
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = displayName }, OnUpdateDisplayNameSuccess, OnUpdateDisplayNameFailure);
        return true;
    }

    private void OnUpdateDisplayNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " is your display name");
    }

    private void OnUpdateDisplayNameFailure(PlayFabError error)
    {
        Debug.Log(error.ToString() + "Не удалось обновить DISPLAY NAME!");
        //Debug.Log(error.ToString());
        //Debug.Log(error.GetType());
    }




    public void GetUserEmail(string userEmail)
    {
        _userEmail = userEmail;
    }
    public void GetUserPassword(string userPassword)
    {
        _userPassword = userPassword;
    }

    public void GetUserName(string userName)
    {
        _userName = userName;
    }
    public void GetDisplayName(string displayName)
    {
        _displayName = displayName;
    }


    public static string ReturnMobileID()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        return deviceID;
    }

# endregion


    #region PlayerStatistic

    public int oneStat;
    public int secondStat;
    

    // Легаси-код. Изменяет статистику игрока напрямую из клиента. Теперь используем для этого вызов с сервера. Обращаемся к соответствующему API сервера в StartCloudUpdatePlayerStats
    public void StoreStats(string nameLevel)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {

            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "CurrentScore" + nameLevel, Value = ScoreManager.Instance.CurrentScore },
                new StatisticUpdate { StatisticName = "maxKillCombo" + nameLevel, Value = ScoreManager.Instance.maxKillCombo },
                new StatisticUpdate { StatisticName = "timeFromStartLevel" + nameLevel, Value = (int)ScoreManager.Instance.timeFromStartLevel },
                new StatisticUpdate { StatisticName = "currentYear" + nameLevel, Value = DateTime.Now.Year },
                new StatisticUpdate { StatisticName = "currentMonth" + nameLevel, Value = DateTime.Now.Month }
            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }


    // Не актуально, так как все параметры нужно передавать явно, а не по имени. Универсальная функция представлена ниже: StartCloudUpdatePlayerStatsNEW
    //public void StartCloudUpdatePlayerStats()
    //{
    //    PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
    //    {
    //        FunctionName = "UpdatePlayerStats", // Arbitrary function name (must exist in your uploaded cloud.js file)
    //        FunctionParameter = new { oneStat = this.oneStat, secondStat = this.secondStat }, // The parameter provided to your function
    //        GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
    //    }, OnCloudUpdateStats, OnErrorShared);
    //}


    // Функция для вызова Cloud Script. Вызывается не асинхронно. Для асинхронной обработки только что обновлённых результатов (взятия их с сервера) нужно использовать функцию,
    // представленную ниже: StartCloudUpdatePlayerStatsNEWAsync.

    // Ещё раз: StartCloudUpdatePlayerStatsNEW - вызывается из контекста, в котором нет await, который не будет ждать прихода результатов с сервера для успешнего (или нет) вызова 
    // данной функции.

    // StartCloudUpdatePlayerStatsNEWAsync - вызывается из асинхронного контекста. Выполнение кода в данном контексте продолжится после получения результата выполнения данной 
    // функции на сервере (успешного или нет).

    public void StartCloudUpdatePlayerStatsNEW()
    {
        string nameLevel = LevelBuilder.instance.selfName;
        // Создаем и заполняем словарь для хранения статистики
        Dictionary<string, object> stats = new Dictionary<string, object>()
        {
            { "CurrentScore" + nameLevel, ScoreManager.Instance.CurrentScore },
            { "maxKillCombo" + nameLevel, ScoreManager.Instance.maxKillCombo },
            { "timeFromStartLevel" + nameLevel, (int)ScoreManager.Instance.timeFromStartLevel },
            { "currentYear" + nameLevel, DateTime.Now.Year },
            { "currentMonth" + nameLevel, DateTime.Now.Month },
        };

        // Вызываем Cloud Script
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStatsNEW", // Имя Cloud Script функции
            FunctionParameter = stats, // Передаем словарь со статистикой
            GeneratePlayStreamEvent = true, // Опционально - Отображать событие в PlayStream
        }, OnCloudUpdateStats, OnErrorShared);
    }
    public async Task StartCloudUpdatePlayerStatsNEWAsync()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            //Debug.Log("Неее");
            string nameLevel = LevelBuilder.instance.selfName;
            Dictionary<string, object> stats = new Dictionary<string, object>()
            {
                { C.Other.CurrentScore + nameLevel, ScoreManager.Instance.CurrentScore },
                { C.Other.maxKillCombo + nameLevel, ScoreManager.Instance.maxKillCombo },
                { C.Other.timeFromStartLevel + nameLevel, (int)ScoreManager.Instance.timeFromStartLevel },
                { C.Other.currentYear + nameLevel, DateTime.Now.Year },
                { C.Other.currentMonth + nameLevel, DateTime.Now.Month },
            };

            var request = new ExecuteCloudScriptRequest()
            {
                FunctionName = C.NameFunc.UpdatePlayerStatsNEW,
                FunctionParameter = stats,
                GeneratePlayStreamEvent = true,
            };

            var taskCompletionSource = new TaskCompletionSource<ExecuteCloudScriptResult>();

            PlayFabClientAPI.ExecuteCloudScript(request,
                result => { taskCompletionSource.SetResult(result); },
                error => {
                    Debug.LogError(error.GenerateErrorReport());
                    taskCompletionSource.SetException(new Exception(error.GenerateErrorReport()));
                });

            try
            {
                ExecuteCloudScriptResult result = await taskCompletionSource.Task;
                OnCloudUpdateStats(result);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in StartCloudUpdatePlayerStatsNEWAsync: " + e.Message);
                // Handle the error here
            }
        }
        else
        {
            Debug.LogWarning("Not logged in to PlayFab!");
        }

    }
    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result)
    {
        //Debug.Log(result);
        //Debug.Log(result.FunctionResult);

        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogWarning("Not logged in to PlayFab!");
            return;
        }

        foreach (LogStatement log in result.Logs)
        {
            //Debug.Log(log.Data);   
            //Debug.Log(log.Message); // только эта штука выводит значение, переданное в log.info()
            //Debug.Log(log.Level);   // эта штука обозначает степень важности log - info, error и т.п
        }
        ISerializerPlugin serializer = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
        //Debug.Log(serializer);
        //Debug.Log("Не уря...");
        Dictionary<string, object> jsonResult = serializer.DeserializeObject<Dictionary<string, object>>(result.FunctionResult.ToString());

        if (jsonResult != null && jsonResult.ContainsKey("messageValue"))
        {
            object messageValue;
            jsonResult.TryGetValue("messageValue", out messageValue);
            Debug.Log("Message Value: " + (string)messageValue);
        }
    }
    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }


    // получаем напрямую всю статистику текущего пользователя, вообще все поля, напрямую
    void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStats(GetPlayerStatisticsResult result)
    {
        Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
        {
            switch (eachStat.StatisticName)
            {
                case "oneStat":
                    oneStat = eachStat.Value;
                    break;
                case "secondStat":
                    secondStat = eachStat.Value;
                    break;
            }

            Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
        }
    }

    public void StartCloudUpdateMaxReachedLevel()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            Dictionary<string, object> stats = new Dictionary<string, object>()
            {
                { C.Other.MaxReachedLevel, GameManager.Instance.MaxReachedLevel },
            };

            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = C.NameFunc.UpdateMaxReachedLevel, // Имя Cloud Script функции
                FunctionParameter = stats, // Передаем словарь со статистикой
                GeneratePlayStreamEvent = true, // Опционально - Отображать событие в PlayStream
            }, result => { Debug.Log("Успешно обновили на сервере максимальный достигнутый игроком уровень!"); },
                error => { Debug.Log("Не смогли успешно обновить на сервере максимальный достигнутый игроком уровень!"); });
        }
        else
        {
            Debug.LogWarning("Not logged in to PlayFab!");
        }

    }



    #endregion


    #region Leaderboard

    public Dictionary<string, int> lastLeaderboardStatsInfo = new(); // string_ИМЯ_ИГРОКА: int_ЗНАЧЕНИЕ_СТАТИСТИКИ

    // в асинхронной среде не будет ждать получения результатов с сервера. Асинхронная среда продолжит выполнение кода не дожидаясь результатов.
    public void GetScoreLeaderboarder()
    {
        //Debug.Log("УРЯЯЯЯ");
        string nameLevel = LevelBuilder.instance.selfName;
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = C.Other.CurrentScore + nameLevel, MaxResultsCount = 10 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboard, OnErrorLeaderbpard);
    }

    // асинхронная среда будет приостановлена до тех пор, пока придут результаты с сервера и метод полностью не закончит свою работу. До тех пор асинхронный контекст будет ждать
    public async Task GetScoreLeaderboarderAsync(CancellationToken token)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogWarning("Not logged in to PlayFab!");
            return;
        }

        token.ThrowIfCancellationRequested(); // сразу проверяем

        //Debug.Log("УРЯЯЯЯ");
        string nameLevel = LevelBuilder.instance.selfName;

        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = C.Other.CurrentScore + nameLevel, MaxResultsCount = 10 };
        
        var taskCompletionSource = new TaskCompletionSource<GetLeaderboardResult>();

        PlayFabClientAPI.GetLeaderboard(requestLeaderboard,
            result => { taskCompletionSource.TrySetResult(result); },
            error => {
                Debug.LogError(error.GenerateErrorReport());
                taskCompletionSource.TrySetException(new Exception(error.GenerateErrorReport()));
            });

        try
        {
            using (token.Register(() => taskCompletionSource.TrySetCanceled()))
            {
                GetLeaderboardResult result = await taskCompletionSource.Task;
                //Debug.Log(result);
                //Debug.Log(taskCompletionSource);
                //Debug.Log(taskCompletionSource.Task);
                OnGetLeaderboard(result);
            }
        }
        catch (OperationCanceledException e)
        {
            Debug.LogError("Error in GetScoreLeaderboarderAsync: " + e.Message);
            throw;
            // Handle the error here
        }
        catch (Exception e)
        {
            Debug.LogError("Error in GetScoreLeaderboarderAsync: " + e.Message);
            //throw; // не делаем это, ибо мы там более нигде оное не обрабатывает в более высоком контексте. Ну, пока что...
            // Handle the error here
        }



    }

    private void OnGetLeaderboard(GetLeaderboardResult result)
    {
        //Debug.Log("mdaaaaaaaaaaaaaaaaaa"); 
        //Debug.Log(result);
        //Debug.Log(result.Leaderboard);
        //Debug.Log(result.Leaderboard.Count);
        lastLeaderboardStatsInfo = new();
        foreach (PlayerLeaderboardEntry fieldLeaderboard in result.Leaderboard)
        {
            if (!string.IsNullOrEmpty(fieldLeaderboard.DisplayName))
            {
                lastLeaderboardStatsInfo[fieldLeaderboard.DisplayName] = fieldLeaderboard.StatValue;
                //Debug.Log(fieldLeaderboard.DisplayName + ": " + fieldLeaderboard.StatValue);
                //Debug.Log(fieldLeaderboard.PlayFabId + ": " + fieldLeaderboard.StatValue);
            }
            else
            {
                lastLeaderboardStatsInfo["DisplayName is null!" + UnityEngine.Random.RandomRange(-10000, 1000)] = fieldLeaderboard.StatValue;
            }
        }
    }

    private void OnErrorLeaderbpard(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    /// <summary>
    /// Запускает обновление лидерборда, отменяя все предыдущие попытки.
    /// </summary>
    public async Task UpdateLeaderboardServerAsync(CancellationToken token)
    {
        try
        {
            // Первый шаг — обновляем статистику
            await StartCloudUpdatePlayerStatsNEWAsync();
            token.ThrowIfCancellationRequested();

            // Подожди пару секунд (нужно, чтобы лидерборд успел применить обновления)
            await Task.Delay(2000, token);
            //Debug.Log("Или мы только тут?");
            token.ThrowIfCancellationRequested();

            Debug.Log("[Leaderboard] Successfully updated leaderboard stats.");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[Leaderboard] Previous update canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Leaderboard] Unexpected error: {ex}");
            //throw; // не делаем это, ибо мы там более нигде оное не обрабатывает в более высоком контексте. Ну, пока что...
        }
    }

    #endregion

    private void LoginSuccess()
    {
        GetIDTitleAccount();
    }


    private void GetIDTitleAccount()
    {
        var request = new GetAccountInfoRequest(); // пустой → значит про текущего игрока
        PlayFabClientAPI.GetAccountInfo(request, OnGetIDTitleAccountSuccess, OnGetIDTitleAccountFailure);
    }
    private void OnGetIDTitleAccountSuccess(GetAccountInfoResult result)
    {
        string IDTitleAccount = result.AccountInfo?.TitleInfo?.TitlePlayerAccount.Id;

        Debug.Log("Успешно получили Title account ID: " + IDTitleAccount);

        accountInfoResult = result; // на будущее, при получении информации об аккаунте будем обновлять локальную переменную

        this.IDTitleAccountLast = IDTitleAccount;

        OnGetIDTitleAccountAfterLogin?.Invoke(IDTitleAccount);
        
    }
    private void OnGetIDTitleAccountFailure(PlayFabError error)
    {
        Debug.LogError("Ошибка при получении инфо об аккаунте при попытке получить Title account ID: " + error.GenerateErrorReport());
    }

    private IEnumerator RepeatLogin()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(_timeRepeatTryingLogin);
            if (!PlayFabClientAPI.IsClientLoggedIn())
            {
                LoginOrRegisterEmailIfFailureLoginMobile(_userEmail, _userPassword);
            }
        }
    }

    private void OnDestroy()
    {
        OnLoginSuccess -= LoginSuccess;
        StopAllCoroutines();
    }
}