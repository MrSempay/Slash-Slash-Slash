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
                var obj = new GameObject("EventBas");
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

    public UnityEvent<bool, RectTransform> ValueVibrationWasChanged { get; } = new();
    public UnityEvent<bool, RectTransform> ValueCameraShakingWasChanged { get; } = new();
    public UnityEvent<float, RectTransform> ValueBrightnessWasChanged { get; } = new();
    public UnityEvent<float, RectTransform> ValueVolumMusicWasChanged { get; } = new();
    public UnityEvent<float, RectTransform> ValueVolumEffectsWasChanged { get; } = new();
    public UnityEvent<ENUM, RectTransform> ValueLanguageWasChanged { get; } = new();
    public UnityEvent<ENUM, RectTransform> ValueOrientationWasChanged { get; } = new();


    public UnityEvent<int> OnKillKomboWasChanged { get; } = new();
    public UnityEvent<STYLE_RANK> OnRankWasChanged { get; } = new();


    public void TriggerDoorWasDestroyed(bool wasDestroyed) { DoorWasDestroyed.Invoke(wasDestroyed); }

    public void TriggerToggleSonicOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleSonicOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleGameOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleGameOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleVideoOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleVideoOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleLanguageOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleLanguageOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }

    public void TriggerToggleParameterVibration(bool vibrationOn, RectTransform rectTransformToggle) { ValueVibrationWasChanged.Invoke(vibrationOn, rectTransformToggle); }
    public void TriggerToggleParameterCameraShaking(bool cameraShakingOn, RectTransform rectTransformToggle) { ValueCameraShakingWasChanged.Invoke(cameraShakingOn, rectTransformToggle); }
    public void TriggerParameterBrightness(float value, RectTransform rectTransformToggle) { ValueBrightnessWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterVolumMusic(float value, RectTransform rectTransformToggle) { ValueVolumMusicWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterVolumEffects(float value, RectTransform rectTransformToggle) { ValueVolumEffectsWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterLanguage(ENUM value, RectTransform rectTransformToggle) { ValueLanguageWasChanged.Invoke(value, rectTransformToggle); }
    public void TriggerParameterOrientation(ENUM value, RectTransform rectTransformToggle) { ValueOrientationWasChanged.Invoke(value, rectTransformToggle); }

    public void KillComboWasChanged(int value) { OnKillKomboWasChanged.Invoke(value); }
    public void RankWasChanged(STYLE_RANK value) { OnRankWasChanged.Invoke(value); }
}
