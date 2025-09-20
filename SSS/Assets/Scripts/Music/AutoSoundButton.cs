using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AutoSoundButton : MonoBehaviour // пока что решил не добавлять звук нажатия для кнопок из панельки выбора и OneWayToggleFixed. НЕ ЗНАЮ ПОЧЕМУ, ВОТ НЕ ХОЧЕТСЯ ТУДА
{
    private Button _btn;

    private void OnEnable()
    {
        if (_btn == null)
        {
            _btn = GetComponent<Button>();
        }
        _btn.onClick.AddListener(PlaySound);
    }

    private void OnDisable()
    {
        if (_btn == null)
        {
            _btn = GetComponent<Button>();
        }
        _btn.onClick.RemoveListener(PlaySound);
    }

    private void PlaySound()
    {
        AudioManager.Instance.StartSoundEffect(C.MusicSounds.OnButtonClick);
    }
}