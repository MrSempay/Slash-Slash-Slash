using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonText : MonoBehaviour
{
    public TextEdit textButton;
    public Button buttonComponent;
    public GameObject buttonObject;

    private bool _soundListenerWasAttached = false;
    //private Button _btn;

    protected virtual void OnEnable()
    {
        if (!IsListenerAlreadyAdded(buttonComponent.onClick, PlaySound) && !_soundListenerWasAttached)
        {
            buttonComponent.onClick.AddListener(PlaySound);
            _soundListenerWasAttached = true;
        }
        ////Debug.Log(buttonComponent.onClick.GetPersistentEventCount());
    }
    private bool IsListenerAlreadyAdded(UnityEvent unityEvent, UnityEngine.Events.UnityAction action)
    {
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
        {
            if (unityEvent.GetPersistentTarget(i) == this &&
                unityEvent.GetPersistentMethodName(i) == action.Method.Name)
            {
                return true;
            }
        }
        return false;
    }

    protected virtual void OnDisable()
    {
        ////Debug.Log(GetInstanceID());
        ////Debug.Log("disable");
        ////Debug.Log(buttonComponent);
        ////Debug.Log(buttonComponent.onClick);
        ////Debug.Log(buttonComponent.onClick.GetPersistentEventCount());
        //if (_soundListenerWasAttached)
        //buttonComponent.onClick.RemoveListener(PlaySound);
    }

    protected void PlaySound()
    {
        ////Debug.Log("Ну что за параша такая-то");
        //AudioManager.Instance.StartSoundEffectAtSpecifiedObject(C.MusicSounds.OnButtonClick, gameObject, AudioManager.TYPE_SOUND.Default, AudioManager.TYPE_AUDIO_SOURCE._2DStandard);
        AudioManager.Instance.StartSoundEffect(C.MusicSounds.OnButtonClick); // ибо при удалении/деактивации кнопки верхний подход может отработать не корректно
    }
}
