using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.PfEditor.Json;
using PlayFab.SharedModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static GameManager;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;


public class PlayFabManager : MonoBehaviour
{
    private static PlayFabManager _instance;
    private string _userEmail;
    private string _userPassword;
    private string _userName;

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
        //var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
        //PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }



#region Login/Registration

    public static PlayFabAuthenticationContext contextCurrentSession;

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Залогинились");
        //contextCurrentSession = result.AuthenticationContext;
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Зарегались");
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = _userName }, OnDisplayName, OnLoginFailure);
    }


    private void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " is your display name");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.Log(error.ToString());
        Debug.Log(error.ToString());
        Debug.Log(error.GetType());
        var registerRequest = new RegisterPlayFabUserRequest { Email = _userEmail, Password = _userPassword, Username = _userName };

        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }



    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void GetUserEmail(string userEmail)
    {
        this._userEmail = userEmail;
    }
    public void GetUserPassword(string userPassword)
    {
        this._userPassword = userPassword;
    }

    public void GetUserName(string userName)
    {
        this._userName = userName;
    }

    public void OnClickLogin()
    {
        var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword  };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);

    }

    #endregion


#region PlayerStatistic

    public int oneStat;
    public int secondStat;
    

    // Легаси-код. Изменяет статистику игрока напрямую из клиента. Теперь используем для этого вызов с серверга. Обращаемся к соответствующему API сервера в StartCloudUpdatePlayerStats
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

    // Ещё раз: StartCloudUpdatePlayerStatsNEW - вызывается из контексте, в котором нет await, который не будет ждать прихода результатов с сервера для успешнего (или нет) вызова 
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



    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result)
    {
        //Debug.Log(result);
        //Debug.Log(result.FunctionResult);
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
            OnGetLeaderboard(result);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in GetScoreLeaderboarderAsync: " + e.Message);
            // Handle the error here
        }


    }





    public Dictionary<string, int> lastLeaderboardStatsInfo; // string_ИМЯ_ИГРОКА: int_ЗНАЧЕНИЕ_СТАТИСТИКИ

    void OnGetLeaderboard(GetLeaderboardResult result)
    {
        lastLeaderboardStatsInfo = new();

        foreach (PlayerLeaderboardEntry fieldLeaderboard in result.Leaderboard)
        {
            lastLeaderboardStatsInfo[fieldLeaderboard.DisplayName] = fieldLeaderboard.StatValue;
            //Debug.Log(fieldLeaderboard.DisplayName + ": " + fieldLeaderboard.StatValue);
            //Debug.Log(fieldLeaderboard.PlayFabId + ": " + fieldLeaderboard.StatValue);
        }
    }

    void OnErrorLeaderbpard(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }


#endregion

}