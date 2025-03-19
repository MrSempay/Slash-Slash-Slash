using System;
using UnityEngine;
using UnityEngine.Events;


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
    }

    public UnityEvent<bool> DoorWasDestroyed { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleSonicOfSettingsMenuWasToggled { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleGameOfSettingsMenuWasToggled { get; } = new();
    public UnityEvent<bool, RectTransform> ToggleVideoOfSettingsMenuWasToggled { get; } = new();


    public void TriggerDoorWasDestroyed(bool wasDestroyed) { DoorWasDestroyed.Invoke(wasDestroyed); }

    public void TriggerToggleSonicOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleSonicOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleGameOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleGameOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }
    public void TriggerToggleVideoOfSettingsMenu(bool wasToggled, RectTransform rectTransformToggle) { ToggleVideoOfSettingsMenuWasToggled.Invoke(wasToggled, rectTransformToggle); }

}
