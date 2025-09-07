using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.PfEditor.Json;
using PlayFab.SharedModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static GameManager;
using static StaticClassForAdditionalFunctions;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;


public class PlayFabManager : MonoBehaviour
{
    private readonly string _generalPassword = "DefaultPass123";

    private static PlayFabManager _instance;
    private string _userEmail = "";
    //private string _userPassword;
    private string _userName = "DefaultName";
    private string _displayName = "";
    private string _successEmail = "";

    public event Action<string> OnGetDisplayNameFromEmailLogin;
    public event Action<string> OnGetIDTitleAccountLogin;
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
        //var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
        //PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

    }

    private IEnumerator StupidDelay()
    {
        yield return null;
        LoginOrRegisterEmailIfFailureLoginMobile(_userEmail);
         
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

    public void LinkEmail(string email)
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
            var linkEmail = new AddUsernamePasswordRequest { Email = email, Password = _generalPassword, Username = _userName };
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
        Debug.Log(error.ToString() + " Ошибка привязки почты к аккаунту!");
        //Debug.Log(error.Error);
        if (error.Error == PlayFabErrorCode.AccountAlreadyLinked)
        {
            GetEmailFromServer(); // это - если мы ещё не залогинились, не знаем наш локальный email и хотим получить его с сервера и проверить, то ли мы пытаемся привязать аккаунт к какому-то
                                  // уже привязанному email-у, то ли хотим привязать к email-у, к которому этот аккаунт уже привязан - в таком случае скажем, что просто залогиньтесь.
            return;
        }

        if (error.Error == PlayFabErrorCode.InvalidParams) // подразумевается, что тут будет проблема только с электронной почтой, ибо пароль у нас по-умолчанию
        {
            Debug.Log("Неправильный формат электронной почты!");
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.InvalidEmailAddress, TYPE_NOTIFICATION.Failure, 4, false);
            return;
        }
        //_OnLinkEmailFailure?.Invoke();
        //Debug.Log(error.GetType());
        //Debug.Log(error.GenerateErrorReport());

    }

    public void LoginOrRegisterEmailIfFailureLoginMobile(string email)
    {
        if (email != "")
        {
            _userEmail = email;
            var request = new LoginWithEmailAddressRequest { Email = email, Password = _generalPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnEmailLoginSuccess, OnEmailLoginFailure);
        }
        else
        {
            LoginOrRegisterMobile();
        }
    }
    private void OnEmailLoginSuccess(LoginResult result)
    {
        Debug.Log("Залогинились (Email)"); 
        GameManager.Instance.InvokeAppearingNotification(C.Notifications.SignInEmail, TYPE_NOTIFICATION.Success, 4, false);
        _successEmail = _userEmail;
        GetDisplayNameFromServer();

        OnLoginSuccess?.Invoke();
        //OnGetDisplayNameFromEmailLogin.Invoke(result.InfoResultPayload.AccountInfo.Username); // подписываемся в ParameterLinkEmail, будем обновлять там текстовое поле DisplayName
        //contextCurrentSession = result.AuthenticationContext;
    }
    private void OnEmailLoginFailure(PlayFabError error)
    {
        Debug.Log(error.ToString());
        Debug.Log(error.Error);
        if (error.Error == PlayFabErrorCode.InvalidParams) // подразумевается, что тут будет проблема только с электронной почтой, ибо пароль у нас по-умолчанию
        {
            Debug.Log("Неправильный формат электронной почты!");
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.InvalidEmailAddress, TYPE_NOTIFICATION.Failure, 4, false);
        }
        if (error.Error == PlayFabErrorCode.AccountNotFound) // подразумевается, что тут будет проблема только с электронной почтой, ибо пароль у нас по-умолчанию
        {
            Debug.Log("Для данной электронной почты аккаунт не найден!"); 
            GameManager.Instance.InvokeAppearingNotification(C.Notifications.AccountNotFound, TYPE_NOTIFICATION.Failure, 4, false);
        }
        LoginOrRegisterMobile();
        
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
        Debug.Log(error.GenerateErrorReport());
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
            Debug.Log(OnGetDisplayNameFromEmailLogin);
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

# endregion




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
        this._userEmail = userEmail;
    }
    public void GetUserPassword(string userPassword)
    {
        //this._userPassword = userPassword;
    }

    public void GetUserName(string userName)
    {
        this._userName = userName;
    }
    public void GetDisplayName(string displayName)
    {
        this._displayName = displayName;
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
                { "CurrentScore" + nameLevel, ScoreManager.Instance.CurrentScore },
                { "maxKillCombo" + nameLevel, ScoreManager.Instance.maxKillCombo },
                { "timeFromStartLevel" + nameLevel, (int)ScoreManager.Instance.timeFromStartLevel },
                { "currentYear" + nameLevel, DateTime.Now.Year },
                { "currentMonth" + nameLevel, DateTime.Now.Month },
            };

            var request = new ExecuteCloudScriptRequest()
            {
                FunctionName = "UpdatePlayerStatsNEW",
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
                { "MaxReachedLevel", GameManager.Instance.MaxReachedLevel },
            };

            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "UpdateMaxReachedLevel", // Имя Cloud Script функции
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


    // в асинхронной среде не будет ждать получения результатов с сервера. Асинхронная среда продолжит выполнение кода не дожидаясь результатов.
    public void GetScoreLeaderboarder()
    {
        //Debug.Log("УРЯЯЯЯ");
        string nameLevel = LevelBuilder.instance.selfName;
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "CurrentScore" + nameLevel, MaxResultsCount = 10 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboard, OnErrorLeaderbpard);
    }

    // асинхронная среда будет приостановлена до тех пор, пока придут результаты с сервера и метод полностью не закончит свою работу. До тех пор асинхронный контекст будет ждать
    public async Task GetScoreLeaderboarderAsync()
    {

        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogWarning("Not logged in to PlayFab!");
            return;
        }

        //Debug.Log("УРЯЯЯЯ");
        string nameLevel = LevelBuilder.instance.selfName;

        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "CurrentScore" + nameLevel, MaxResultsCount = 10 };
        
        var taskCompletionSource = new TaskCompletionSource<GetLeaderboardResult>();

        PlayFabClientAPI.GetLeaderboard(requestLeaderboard,
            result => { taskCompletionSource.SetResult(result); },
            error => {
                Debug.LogError(error.GenerateErrorReport());
                taskCompletionSource.SetException(new Exception(error.GenerateErrorReport()));
            });

        try
        {
            GetLeaderboardResult result = await taskCompletionSource.Task;
            //Debug.Log(result);
            //Debug.Log(taskCompletionSource);
            //Debug.Log(taskCompletionSource.Task);
            OnGetLeaderboard(result);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in GetScoreLeaderboarderAsync: " + e.Message);
            // Handle the error here
        }


    }

    public Dictionary<string, int> lastLeaderboardStatsInfo = new(); // string_ИМЯ_ИГРОКА: int_ЗНАЧЕНИЕ_СТАТИСТИКИ

    private void OnGetLeaderboard(GetLeaderboardResult result)
    {
        Debug.Log("mdaaaaaaaaaaaaaaaaaa"); 
        //Debug.Log(result);
        //Debug.Log(result.Leaderboard);
        //Debug.Log(result.Leaderboard.Count);

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
        Debug.Log("Чё за херота1?"); 
        OnGetIDTitleAccountLogin?.Invoke(IDTitleAccount);
        
    }
    private void OnGetIDTitleAccountFailure(PlayFabError error)
    {
        Debug.LogError("Ошибка при получении инфо об аккаунте при попытке получить Title account ID: " + error.GenerateErrorReport());
    }



    private void OnDestroy()
    {
        OnLoginSuccess -= LoginSuccess;
    }
}