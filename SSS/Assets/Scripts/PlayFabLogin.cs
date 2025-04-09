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
using UnityEngine;


public class PlayFabLogin : MonoBehaviour
{
    private string userEmail;
    private string userPassword;
    private string userName;
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
        GetStats();
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Зарегались");
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = userName }, OnDisplayName, OnLoginFailure);
        GetStats();
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
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = userName };

        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }



    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void GetUserEmail(string userEmail)
    {
        this.userEmail = userEmail;
    }
    public void GetUserPassword(string userPassword)
    {
        this.userPassword = userPassword;
    }

    public void GetUserName(string userName)
    {
        this.userName = userName;
    }

    public void OnClickLogin()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword  };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);

    }

    #endregion





#region PlayerStatistic

    public int oneStat;
    public int secondStat;


    // Легаси-код. Изменяет статистику игрока напрямую из клиента. Теперь используем для этого вызов с серверга. Обращаемся к соответствующему API сервера в StartCloudUpdatePlayerStats
    public void StoreStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "oneStat", Value = oneStat },
                new StatisticUpdate { StatisticName = "secondStat", Value = secondStat },
            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }


    // Build the request object and access the API
    public void StartCloudUpdatePlayerStats()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStats", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { oneStat = this.oneStat, secondStat = this.secondStat }, // The parameter provided to your function
            GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
        }, OnCloudUpdateStats, OnErrorShared);
    }
    // OnCloudHelloWorld defined in the next code block

    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result)
    {
        ISerializerPlugin serializer = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
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
    
    public void GetLeaderboarder()
    {
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "oneStat", MaxResultsCount = 10 };
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboard, OnErrorLeaderbpard);
        }

    void OnGetLeaderboard(GetLeaderboardResult result)
    {

        foreach (PlayerLeaderboardEntry fieldLeaderboard in result.Leaderboard)
        {
            Debug.Log(fieldLeaderboard.DisplayName + ": " + fieldLeaderboard.StatValue);
        }
    }

    void OnErrorLeaderbpard(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }


#endregion

}