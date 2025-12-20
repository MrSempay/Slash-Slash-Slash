using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ScoreManager;
using static StaticClassForAdditionalFunctions;


public class EventBus : MonoBehaviour, IReadOnlyEventBus
{
    public static EventBus _instance;

    public static EventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("EventBus");
                _instance = obj.AddComponent<EventBus>();
                DontDestroyOnLoad(obj);

            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public UnityEvent<bool> DoorWasDestroyed { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleSonicOfSettingsMenuWasToggled { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleGameOfSettingsMenuWasToggled { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleVideoOfSettingsMenuWasToggled { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleLanguageOfSettingsMenuWasToggled { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleInternetOfSettingsMenuWasToggled { get; } = new();

    public UnityEvent<bool, RectTransform> ValueVibrationWasChanged { get; } = new();
    public UnityEvent<bool, RectTransform> ValueCameraShakingWasChanged { get; } = new();
    public UnityEvent<float, RectTransform> ValueBrightnessWasChanged { get; } = new();
    public UnityEvent<float, RectTransform> ValueVolumMusicWasChanged { get; } = new();
    public UnityEvent<bool, RectTransform> ValueShowNotificationsWasChanged { get; } = new();
    public UnityEvent<float, RectTransform> ValueVolumEffectsWasChanged { get; } = new();
    public UnityEvent<LANGUAGE, RectTransform> ValueLanguageWasChanged { get; } = new();
    public UnityEvent<LANGUAGE, RectTransform> ValueOrientationWasChanged { get; } = new();
    public UnityEvent<string, RectTransform> EmailForLinkWasChanged { get; } = new();
    public UnityEvent<string, RectTransform> PasswordWasChanged { get; } = new();
    public UnityEvent<string, RectTransform> DisplayNameWasChanged { get; } = new();


    public UnityEvent<int> OnKillKomboWasChanged { get; } = new();
    public UnityEvent<int> OnScoreWasChanged { get; } = new();
    public UnityEvent<STYLE_RANK> OnRankWasChanged { get; } = new();

    public UnityEvent<Enemy> OnEnemyWasKilledByPlayer { get; } = new();
    public UnityEvent<int> OnOneEnemyWasKilledByPlayer { get; } = new(); // нужно для подписи для тех специфических итераторов, которые увеличиваются на 1 при убийстве врага игроком

    public UnityEvent OnPlayerWasInstanced { get; } = new();

    public void TriggerDoorWasDestroyed(bool wasDestroyed) { DoorWasDestroyed.Invoke(wasDestroyed); }

    public void TriggerToggleSonicOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleSonicOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleGameOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleGameOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleVideoOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleVideoOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleLanguageOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleLanguageOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleInternetOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleInternetOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }

    public void TriggerToggleParameterVibration(bool vibrationOn, RectTransform rectTransformToggle) { ValueVibrationWasChanged.Invoke(vibrationOn, rectTransformToggle); }
    public void TriggerToggleParameterCameraShaking(bool cameraShakingOn, RectTransform rectTransformToggle) { ValueCameraShakingWasChanged.Invoke(cameraShakingOn, rectTransformToggle); }
    public void TriggerToggleParameterShowNotifications(bool value, RectTransform rectTransformToggle) { ValueShowNotificationsWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterBrightness(float value, RectTransform rectTransformToggle) { ValueBrightnessWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterVolumMusic(float value, RectTransform rectTransformToggle) { ValueVolumMusicWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterVolumEffects(float value, RectTransform rectTransformToggle) { ValueVolumEffectsWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterLanguage(LANGUAGE value, RectTransform rectTransformToggle) { ValueLanguageWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterOrientation(LANGUAGE value, RectTransform rectTransformToggle) { ValueOrientationWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerEmailForLinkWasChanged(string value, RectTransform rectTransformToggle) { EmailForLinkWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerPasswordWasChanged(string value, RectTransform rectTransformToggle) { PasswordWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerDisplayNameWasChanged(string value, RectTransform rectTransformToggle) { DisplayNameWasChanged.Invoke(value, rectTransformToggle); }

    public void KillComboWasChanged(int value) { OnKillKomboWasChanged.Invoke(value); }
    public void RankWasChanged(STYLE_RANK value) { OnRankWasChanged.Invoke(value); }
    public void ScoreWasChanged(int value) { OnScoreWasChanged.Invoke(value); }

    public void EnemyWasKilledByPlayer(Enemy enemy) { OnEnemyWasKilledByPlayer.Invoke(enemy);
                                                      OnOneEnemyWasKilledByPlayer.Invoke(1); }

    public void PlayerWasInstanced() { OnPlayerWasInstanced.Invoke(); }
}
