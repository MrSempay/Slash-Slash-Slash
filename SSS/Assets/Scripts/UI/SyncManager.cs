using PlayFab.ClientModels;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Overlays;
using UnityEngine;

public class SyncManager : ICleanUp
{
    public static SyncManager Instance => _instance ??= new SyncManager();

    private static SyncManager _instance;

    public void Initialize()
    {
        PlayFabManager.Instance.OnLoginSuccess += SyncroniseGeneralData;

        CleanupManager.Register(this);
    }


    public void Dispose()
    {
        PlayFabManager.Instance.OnLoginSuccess -= SyncroniseGeneralData;
        Debug.Log("Нещщадно уничтожаем наш SyncManager! Даже жалко как-то...");
    }


    public async void SyncroniseGeneralData()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            await GetMaxReachedLevel();
        }
        
    }

    public async Task GetMaxReachedLevel()
    {
        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { "MaxReachedLevel" }
        };

        var taskCompletionSource = new TaskCompletionSource<GetPlayerStatisticsResult>();

        PlayFabClientAPI.GetPlayerStatistics(
            request,
            result => { taskCompletionSource.SetResult(result); },
            error => Debug.LogError(error.GenerateErrorReport())
        );

        try
        {
            GetPlayerStatisticsResult result = await taskCompletionSource.Task;
            OnGetMaxReachedLevel(result);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in StartCloudUpdatePlayerStatsNEWAsync: " + e.Message);
            // Handle the error here
        }
    }

    private void OnGetMaxReachedLevel(GetPlayerStatisticsResult result)
    {
        if (result.Statistics != null && result.Statistics.Count > 0)
        {
            var stat = result.Statistics[0]; // так как мы запрашивали только одну
            Debug.Log($"MaxReachedLevel = {stat.Value}");
        }
        else
        {
            Debug.Log("Статистика MaxReachedLevel отсутствует.");
        }
    }


}