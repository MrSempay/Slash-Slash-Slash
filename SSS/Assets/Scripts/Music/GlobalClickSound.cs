using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalClickSound : MonoBehaviour
{
    private static GlobalClickSound _instance;

    public static GlobalClickSound Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("GlobalClickSound");
                _instance = obj.AddComponent<GlobalClickSound>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public void Initialize() { }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // клик мыши / тап
        {
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new System.Collections.Generic.List<RaycastResult>(); 
            EventSystem.current.RaycastAll(pointer, results);

            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    //Debug.Log(" нопку нашли");
                    //AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(C.MusicSounds.PlayerGotDamage, audioEmitter???, AudioManager.TYPE_SOUND.Default, AudioManager.TYPE_AUDIO_SOURCE._2DStandard);
                    AudioManager.Instance.StartSoundEffect(C.MusicSounds.PlayerGotDamage);
                    break;
                }
            }
        }
    }
}