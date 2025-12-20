using UnityEngine;
using UnityEngine.Events;

public class ToggleFixedOneWay : ToggleFixed
{
    float mudaDAYIOOO = 15;
    public override void OnMouseClicked()
    {
        if (!IsToggled) // если тумблер не прожат, то мы можем его прожать. В обратную сторону отжать нельзя, оттого и OneWay
        {
            IsToggled = true;
            PlaySound();
        }
    }

    private void PlaySound()
    {
        //Debug.Log("Ну что за параша такая-то");
        //AudioManager.Instance.StartSoundEffectAtSpecifiedObject(C.MusicSounds.OnButtonClick, gameObject, AudioManager.TYPE_SOUND.Default, AudioManager.TYPE_AUDIO_SOURCE._2DStandard);
        AudioManager.Instance.StartSoundEffect(C.MusicSounds.OnButtonClick);
    }


}
