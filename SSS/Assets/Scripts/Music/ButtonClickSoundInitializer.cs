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
                AudioManager.Instance.StartSoundEffect(nameSound);
            });
        }
    }
}