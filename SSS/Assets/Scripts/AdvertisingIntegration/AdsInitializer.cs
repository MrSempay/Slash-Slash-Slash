using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{

    [SerializeField] string androidGameID = "5812008";
    [SerializeField] string iOSGameID = "5812009";
    [SerializeField] bool testMode = false;
    [SerializeField] private TextMeshProUGUI currentComboUI;
    private string gameID;

    void Awake()
    {
        InitializeAds();
    }
    public void InitializeAds()
    {
        gameID = (Application.platform == RuntimePlatform.IPhonePlayer) ? iOSGameID : androidGameID;
        currentComboUI.text += gameID;
        Advertisement.Initialize(gameID, testMode, this);
    }

    public void OnInitializationComplete()
    {
        currentComboUI.text += " Unity Ads initialization complete. ";
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        currentComboUI.text += $"Unity Ads Initialization Failed: {error.ToString()} - {message}";
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}