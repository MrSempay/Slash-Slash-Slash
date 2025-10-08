using UnityEngine;
using UnityEngine.UI;

public static class ButtonClickSoundInitializer
{    
    public static void SetButtonsSound(string nameSound)
    {
        foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            button.onClick.AddListener(() =>
            {
                //AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(nameSound, button.gameObject, AudioManager.TYPE_SOUND.Default, AudioManager.TYPE_AUDIO_SOURCE._2DStandard);
                //AudioManager.Instance.StartSoundEffect(nameSound);
            });
        }
    }
}