using UnityEngine;
using static GameManager;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static AudioManager;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{

    private static AudioManager _instance;

    private float _timeTickOfChangingVolumeBetweenMusic = 0.03f;
    private float _timeCleanUpSoundSourcesDictionary = 60f;
    private bool _musicWasEndedByItself = true;
    private Dictionary<string, AudioClip> _sourcesSounds = new();
    private Dictionary<string, AudioClip> _dictionaryFightMusic = new();
    private Dictionary<string, AudioClip> _dictionaryAmbientMusic = new();
    private Dictionary<string, AudioClip> _dictionaryOtherMusic = new();
    private AudioClip _currentMusic; // —сылка на аудиофайл дл€ взрыва
    private AudioClip _beginningMusic; // —сылка на аудиофайл дл€ взрыва
    private AudioClip _transitionMusic; // —сылка на аудиофайл дл€ взрыва
    private AudioClip _certainMusic; // —сылка на аудиофайл дл€ какой-то конкретной музыки
    private string _pathToMusicFolder = "Music/Musics/";
    private string _pathToSoundsEffect = "Music/Effects/";
    private string _nameBeginningMusic = "BeginningLevelMusic";
    private string _nameTransitionMusic = "TransitionMusic";

    public enum TYPE_SOUND { Walk, AttackPeak, GetDamage, Default, Death, Destroy};
    public enum TYPE_AUDIO_SOURCE { _2DStandard, _3DStandard };
    public Dictionary<GameObject, Dictionary<TYPE_SOUND, AudioSource>> dictionaryObjectsAndTheirAudioSourcesByTypes = new();
    public AudioSource audioMusicComponent; // —сылка на AudioSource дл€ музыки
    public AudioSource audioEffectsComponent; // —сылка на AudioSource дл€ звуковых эффектов



    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("AudioManager");
                _instance = obj.AddComponent<AudioManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    // метод вообще ничего не делает, но как-то инициализировать наш синглтон надо, создавать переменную и присваивать ей ненужную ссылку на наш объект желани€ нет. 
    // ”вы, просто GameManager.Instance сделать нельз€
    public void Initialize() { }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        audioMusicComponent = gameObject.AddComponent<AudioSource>();
        audioMusicComponent.loop = false;

        audioEffectsComponent = gameObject.AddComponent<AudioSource>();

        CoroutineManager.Instance.StartManagedCoroutine(gameObject, ControlSoundSourcesDictionary());


        LoadSoundsDictionary(_pathToSoundsEffect, _sourcesSounds);
    }

    // 14.08.2025 - возможно уже рудиментна€ функци€
    //public void StartMusic(string nameMusic)
    //{
    //    Debug.Log(nameMusic);
    //    StopAllCoroutines();
    //    if (_sourcesSounds.ContainsKey(nameMusic))
    //    {
    //        if (_sourcesSounds[nameMusic] != _audioMusicComponent.clip)
    //        {
    //            Debug.Log(1);
    //            FadeCurrentMusicAndAfterRise(_sourcesSounds[nameMusic]);
    //        }
    //        else
    //        {
    //            Debug.Log(2);
    //            RiseCurrentMusic(_sourcesSounds[nameMusic]);
    //        }
    //    }
    //    else
    //    {
    //        _currentMusic = Resources.Load<AudioClip>(_pathToMusicFile + nameMusic);
    //        _sourcesSounds[nameMusic] = _currentMusic;

    //        if ( _sourcesSounds.Count == 1)
    //        {
    //            Debug.Log(3);
    //            _audioMusicComponent.volume = 0;
    //            RiseCurrentMusic(_currentMusic);
    //        }
    //        else
    //        {
    //            Debug.Log(4);
    //            FadeCurrentMusicAndAfterRise(_currentMusic);
    //        }

    //    }
    //}



    public void StartSoundEffect(string nameEffect)
    {
        if (string.IsNullOrEmpty(nameEffect))
        {
            return;
        }

        if (_sourcesSounds.ContainsKey(nameEffect))
        {
            audioEffectsComponent.PlayOneShot(_sourcesSounds[nameEffect]);

        }
        else
        {
            AudioClip _currentEffect = Resources.Load<AudioClip>(_pathToSoundsEffect + nameEffect);
            _sourcesSounds[nameEffect] = _currentEffect;
            audioEffectsComponent.PlayOneShot(_currentEffect);
        }
    }


    private void FadeCurrentMusicAndAfterRise(AudioClip explosionSound)
    {
        StartCoroutine(FadeCurrentMusicTickAndAfterRise(explosionSound));
    }

    private void FadeCurrentMusic(AudioClip explosionSound)
    {
        StartCoroutine(FadeCurrentMusicTick(explosionSound));
    }

    private void RiseCurrentMusic(AudioClip explosionSound)
    {
        StartCoroutine(RiseCurrentMusicTick(explosionSound));
    }

    IEnumerator FadeCurrentMusicTickAndAfterRise(AudioClip explosionSound)
    {
        while (audioMusicComponent.volume > 0.03f)
        {
            audioMusicComponent.volume -= 0.015f;
            yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        }
        StartCoroutine(RiseCurrentMusicTick(explosionSound));
    }

    IEnumerator FadeCurrentMusicTick(AudioClip explosionSound)
    {
        while (audioMusicComponent.volume > 0.03f)
        {
            audioMusicComponent.volume -= 0.015f;
            yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        }
    }

    IEnumerator RiseCurrentMusicTick(AudioClip explosionSound)
    {
        yield return null;

        audioMusicComponent.clip = explosionSound;
        audioMusicComponent.Play();
        while (audioMusicComponent.volume < GameManager.Instance.currentSettings.VolumeMusic)
        {
            //Debug.Log("???");
            audioMusicComponent.volume += 0.015f;
            yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        }
    }


    //            ------------------------------------ Ќќ¬џ… ”ѕ–ј¬Ћя“ќ– ј”ƒ»ќ !!! ----------------------------------------               //

    public void UpdateMusicLevelSet()
    {
        _beginningMusic = Resources.Load<AudioClip>(_pathToMusicFolder + LevelBuilder.instance.selfName + "/" + _nameBeginningMusic);
        _transitionMusic = Resources.Load<AudioClip>(_pathToMusicFolder + LevelBuilder.instance.selfName + "/" + _nameTransitionMusic);

        if (_beginningMusic == null || _transitionMusic == null)
        {
            Debug.LogError("ќтсутствует музыка перехода или начальна€ музыка дл€ уровн€!");
        }

        string fightMusicFolder = _pathToMusicFolder + LevelBuilder.instance.selfName + "/FightMusic/";
        string ambientMusicFolder = _pathToMusicFolder + LevelBuilder.instance.selfName + "/AmbientMusic/";

        LoadSoundsDictionary(fightMusicFolder, _dictionaryFightMusic);
        LoadSoundsDictionary(ambientMusicFolder, _dictionaryAmbientMusic);
    }

    public void StartBeginningMusic()
    {
        //Debug.Log(nameMusic);
        StopAllCoroutines();

        if (!_musicWasEndedByItself)
        {
            StartCoroutine(FadePreviousMusicAndStartBeginning());
            return;
        }

        _musicWasEndedByItself = false;

        //audioMusicComponent.volume = 0;
        StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(_beginningMusic, null));
    }

    public void PlayFightOrAmbientMusic(bool isFightMusic)
    {
        StopAllCoroutines();

        if (!_musicWasEndedByItself)
        {
            StartCoroutine(FadePreviousMusicAndStartAmbientOrFight(isFightMusic));
            return;
        }

        _musicWasEndedByItself = false;

        List<string> keys;

        //Debug.Log()
        if (isFightMusic)
        {
            keys = new List<string>(_dictionaryFightMusic.Keys);
            if (keys.Count > 0)
            {
                string randomKey = keys[Random.Range(0, keys.Count)];
                StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(_dictionaryFightMusic[randomKey], isFightMusic)); 
            }
            else
            {
                Debug.Log("ѕјѕ ј — ћ”«џ ќ… !!!!!!!!!!!!!!!!!!!!! ѕ”—“јјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјј");
            }
        }
        else
        {
            keys = new List<string>(_dictionaryAmbientMusic.Keys);
            if (keys.Count > 0)
            {
                string randomKey = keys[Random.Range(0, keys.Count)];
                StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(_dictionaryAmbientMusic[randomKey], isFightMusic));
            }
            else
            {
                Debug.Log("ѕјѕ ј — ћ”«џ ќ… !!!!!!!!!!!!!!!!!!!!! ѕ”—“јјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјј");
            }
        }


    }

    public void StartCertainMusicInLoop(string nameMusic)
    {
        StopAllCoroutines();


        if (!_dictionaryOtherMusic.ContainsKey(nameMusic))
        {
            _certainMusic = Resources.Load<AudioClip>(_pathToMusicFolder + nameMusic);

            if (_certainMusic == null)
            {
                Debug.LogError("ћузыки с именем " + nameMusic + " в заданной директории " + _pathToMusicFolder + " не найдено!");
                return;
            }

            _dictionaryOtherMusic[nameMusic] = _certainMusic;
        }
        else
        {
            _certainMusic = _dictionaryOtherMusic[nameMusic];
        }

        if (!_musicWasEndedByItself)
        {
            StartCoroutine(FadePreviousMusicCertain(nameMusic));
            return;
        }

        _musicWasEndedByItself = false;

        _currentMusic = _certainMusic;
        audioMusicComponent.clip = _certainMusic;
        audioMusicComponent.Play();
        StartCoroutine(WaitForMusicEndByItself(_certainMusic, nameMusic));
    }

    public void StartSoundEffectAtSpecifiedObject(string nameEffect, GameObject obj, TYPE_SOUND typeSound, TYPE_AUDIO_SOURCE typeAudioSource, List<TYPE_SOUND> typeSoundsToStop = null)
    {
        if (string.IsNullOrEmpty(nameEffect) || !_sourcesSounds.ContainsKey(nameEffect))
        {
            return;
        }

        AudioSource audioSourceTarget;

        if (dictionaryObjectsAndTheirAudioSourcesByTypes.ContainsKey(obj))
        {
            if (dictionaryObjectsAndTheirAudioSourcesByTypes[obj].ContainsKey(typeSound))
            {
                audioSourceTarget = dictionaryObjectsAndTheirAudioSourcesByTypes[obj][typeSound];
            }
            else
            {
                audioSourceTarget = AttachToObjectAndCashAudioSource(obj, typeSound, typeAudioSource);
            }
        }
        else
        {
            dictionaryObjectsAndTheirAudioSourcesByTypes[obj] = new Dictionary<TYPE_SOUND, AudioSource>();

            audioSourceTarget = AttachToObjectAndCashAudioSource(obj, typeSound, typeAudioSource);
        }

        if (typeSoundsToStop != null)
        {
            foreach (var typeSoundToStop in typeSoundsToStop)
            {
                if (dictionaryObjectsAndTheirAudioSourcesByTypes[obj].ContainsKey(typeSoundToStop))
                {
                    dictionaryObjectsAndTheirAudioSourcesByTypes[obj][typeSoundToStop].Stop();
                }
            }
        }

        audioSourceTarget.PlayOneShot(_sourcesSounds[nameEffect]);
    }

    public void StopSomeTypeSoundOnObject(TYPE_SOUND typeSound, GameObject obj)
    {
        if (dictionaryObjectsAndTheirAudioSourcesByTypes.ContainsKey(obj))
        {
            if (dictionaryObjectsAndTheirAudioSourcesByTypes[obj].ContainsKey(typeSound))
            {
                dictionaryObjectsAndTheirAudioSourcesByTypes[obj][typeSound].Stop();
            }
        }
    }



    private void JustStartAmbientOrFightMusic(AudioClip targetMusic, bool isFightMusic)
    {
        audioMusicComponent.clip = targetMusic;
        _currentMusic = targetMusic;
        audioMusicComponent.Play();
        StartCoroutine(WaitForAmbientOrFightMusicEnd(targetMusic, isFightMusic));
    }

    private void JustStartBeginningMusic()
    {
        audioMusicComponent.clip = _beginningMusic;
        _currentMusic = _beginningMusic;
        audioMusicComponent.Play();
        StartCoroutine(WaitForBeginningMusicEnd());
    }

    private void LoadSoundsDictionary(string path, Dictionary<string, AudioClip> dict)
    {
        // «агружаем все AudioClip из указанной папки
        AudioClip[] clipsFight = Resources.LoadAll<AudioClip>(path);

        // ƒобавл€ем в словарь по имени
        foreach (AudioClip clip in clipsFight)
        {
            if (clip != null)
            {
                // clip.name Ч это им€ файла без расширени€
                dict[clip.name] = clip;
                Debug.Log($"«агружен трек: {clip.name}");
            }
        }
    }

    private AudioSource AttachToObjectAndCashAudioSource(GameObject obj, TYPE_SOUND typeSound, TYPE_AUDIO_SOURCE typeAudioSource)
    {
        AudioSource audioSourceTarget = obj.AddComponent<AudioSource>();
        audioSourceTarget.volume = GameManager.Instance.currentSettings.volumeEffects;

        dictionaryObjectsAndTheirAudioSourcesByTypes[obj][typeSound] = audioSourceTarget;

        switch (typeAudioSource)
        {
            case TYPE_AUDIO_SOURCE._3DStandard:
                audioSourceTarget.spatialBlend = 1;
                audioSourceTarget.rolloffMode = AudioRolloffMode.Linear;
                audioSourceTarget.minDistance = 4f;
                audioSourceTarget.maxDistance = 21f;
                break;
            case TYPE_AUDIO_SOURCE._2DStandard:
                break; // по умолчанию тот компонент, который создаЄтс€, нас устраивает
        }

        return audioSourceTarget;
    }

    private IEnumerator FadePreviousMusicAndStartBeginning()
    {
        while (audioMusicComponent.volume > 0.03f)
        {
            audioMusicComponent.volume -= 0.015f;
            yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        }
        audioMusicComponent.volume = GameManager.Instance.currentSettings.volumeMusic;

        StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(_beginningMusic, null));
    }

    private IEnumerator FadePreviousMusicAndStartAmbientOrFight(bool isFightMusic)
    {
        while (audioMusicComponent.volume > 0.03f)
        {
            audioMusicComponent.volume -= 0.015f;
            yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        }
        audioMusicComponent.volume = GameManager.Instance.currentSettings.volumeMusic;

        List<string> keys;

        if (isFightMusic)
        {
            keys = new List<string>(_dictionaryFightMusic.Keys);
            if (keys.Count > 0)
            {
                string randomKey = keys[Random.Range(0, keys.Count)];
                StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(_dictionaryFightMusic[randomKey], isFightMusic));
            }
            else
            {
                Debug.Log("ѕјѕ ј — ћ”«џ ќ… !!!!!!!!!!!!!!!!!!!!! ѕ”—“јјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјј");
            }
        }
        else
        {
            keys = new List<string>(_dictionaryAmbientMusic.Keys);
            if (keys.Count > 0)
            {
                string randomKey = keys[Random.Range(0, keys.Count)];
                StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(_dictionaryAmbientMusic[randomKey], isFightMusic));
            }
            else
            {
                Debug.Log("ѕјѕ ј — ћ”«џ ќ… !!!!!!!!!!!!!!!!!!!!! ѕ”—“јјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјј");
            }
        }
    }

    private IEnumerator FadePreviousMusicCertain(string nameMusic)
    {
        while (audioMusicComponent.volume > 0.03f)
        {
            audioMusicComponent.volume -= 0.015f;
            yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        }
        audioMusicComponent.volume = GameManager.Instance.currentSettings.volumeMusic;

        StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetCertainMusic(_currentMusic, nameMusic));

        _currentMusic = _certainMusic;
        audioMusicComponent.clip = _certainMusic;
        audioMusicComponent.Play();
        StartCoroutine(WaitForMusicEndByItself(_certainMusic, nameMusic));
    }

    private IEnumerator FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(AudioClip targetMusic, bool? isFightMusic)
    {
        Debug.Log($"FadeCurrentMusicTickStartTransitionAndStartTargetMusic вызвана с: targetMusic={targetMusic?.name}, isFightMusic={isFightMusic}");
        yield return null;
        //while (audioMusicComponent.volume > 0.03f)
        //{
        //    audioMusicComponent.volume -= 0.015f;
        //    yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        //}
        audioMusicComponent.volume = GameManager.Instance.currentSettings.VolumeMusic; // на данный момент у нас после затухани€ предыдущей музыки музыка перехода начинаетс€ в полную силу. ћожет, нужно сделать плавное нарастание
        if (audioMusicComponent.clip != _transitionMusic)
        {
            audioMusicComponent.clip = _transitionMusic;
            audioMusicComponent.Play();
        }
        StartCoroutine(WaitForTransitionBetweenFightOrAmbientMusicEnd(targetMusic, isFightMusic)); // здесь должно быть transition
    }
    private IEnumerator FadeCurrentMusicTickStartTransitionAndStartTargetCertainMusic(AudioClip targetMusic, string nameMusic)
    {
        Debug.Log($"FadeCurrentMusicTickStartTransitionAndStartTargetMusic вызвана с: targetMusic={targetMusic?.name}");
        yield return null;
        //while (audioMusicComponent.volume > 0.03f)
        //{
        //    audioMusicComponent.volume -= 0.015f;
        //    yield return new WaitForSecondsRealtime(_timeTickOfChangingVolumeBetweenMusic);
        //}
        audioMusicComponent.volume = GameManager.Instance.currentSettings.VolumeMusic; // на данный момент у нас после затухани€ предыдущей музыки музыка перехода начинаетс€ в полную силу. ћожет, нужно сделать плавное нарастание
        if (_transitionMusic != null) // по идее у нас музыка перехода подгружаетс€ только при UpdateLevelSet, который вызываетс€ из LevelBuilder.Instance. Ќа сценах без него у нас еЄ...
                                      // в целом нету. ћожет потом добавлю. » музыка без перехода одна в другую переливаетс€
        {
            if (audioMusicComponent.clip != _transitionMusic)
            {
                Debug.Log("„то-то странное");
                audioMusicComponent.clip = _transitionMusic;
                audioMusicComponent.Play();
            }
            StartCoroutine(WaitForTransitionBetweenCertainMusicEnd(targetMusic, nameMusic)); // здесь должно быть transition
        }
        else
        {
            Debug.Log("ћузыка перехода отсутствует! ѕропускаем логику с ней и переходим к следующей музыке!");

            _musicWasEndedByItself = true;

            StartCertainMusicInLoop(nameMusic);
        }
    }

    private IEnumerator WaitForTransitionBetweenFightOrAmbientMusicEnd(AudioClip targetMusic, bool? isFightMusic = null)
    {
        //Debug.Log($"¬ызов из: {System.Environment.StackTrace}");
        if (_transitionMusic != null) // хот€ если это у нас боева€ или музыка поко€, музыка перехода у нас всегда будет, но да ладно, оставлю проверку
        {
            yield return new WaitForSecondsRealtime(_transitionMusic.length);

            Debug.Log("ћузыка перехода закончилась!");

            _musicWasEndedByItself = true;

            if (isFightMusic != null) // вообще, дичь это. Ќужно помен€ть на какой-нибудь enum. Ќа данный момент у нас может быть лишь 3 состо€ни€: true (боева€), false (мирна€) и null - начальна€
            {
                JustStartAmbientOrFightMusic(targetMusic, (bool)isFightMusic);
            }
            else
            {
                JustStartBeginningMusic();
            }
        }
        else
        {
            Debug.Log("ћузыка перехода отсутствует! ѕропускаем логику с ней и переходим к следующей музыке!");

            _musicWasEndedByItself = true;

            if (isFightMusic != null) // вообще, дичь это. Ќужно помен€ть на какой-нибудь enum. Ќа данный момент у нас может быть лишь 3 состо€ни€: true (боева€), false (мирна€) и null - начальна€
            {
                JustStartAmbientOrFightMusic(targetMusic, (bool)isFightMusic);
            }
            else
            {
                JustStartBeginningMusic();
            }
        }
    }
    private IEnumerator WaitForTransitionBetweenCertainMusicEnd(AudioClip targetMusic, string nameMusic)
    {
        yield return new WaitForSecondsRealtime(_transitionMusic.length);

        Debug.Log("ћузыка перехода закончилась!");

        _musicWasEndedByItself = true;

        StartCertainMusicInLoop(nameMusic);
    }

    private IEnumerator WaitForAmbientOrFightMusicEnd(AudioClip targetMusic, bool isFightMusic)

    {
        yield return new WaitForSecondsRealtime(targetMusic.length);

        Debug.Log("«акончивша€с€ мелоди€: " + targetMusic.name);
        Debug.Log("ƒлилась " + targetMusic.length + " секунд");
        Debug.Log("Ќачинаем музыкальный цикл заново!");

        _musicWasEndedByItself = true;

        PlayFightOrAmbientMusic(isFightMusic);
    }
    private IEnumerator WaitForBeginningMusicEnd()
    {
        yield return new WaitForSecondsRealtime(_beginningMusic.length);

        _musicWasEndedByItself = true;

        Debug.Log("ѕереходим из начальной музыки в эмбиент");
        PlayFightOrAmbientMusic(false);
    }
    private IEnumerator WaitForMusicEndByItself(AudioClip music, string nameMusic)
    {
        yield return new WaitForSecondsRealtime(music.length);

        _musicWasEndedByItself = true;

        Debug.Log("«акончивша€с€ мелоди€ в главном меню: " + music.name);

        StartCertainMusicInLoop(nameMusic);
    }

    // --------------------------------------------  ќЌ≈÷ Ќќ¬ќ√ќ ”ѕ–ј¬Ћя“ќ–ј --------------------------------------------- // 

    private IEnumerator ControlSoundSourcesDictionary()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(_timeCleanUpSoundSourcesDictionary);

            var toRemove = new List<GameObject>();

            foreach (var kvp in dictionaryObjectsAndTheirAudioSourcesByTypes)
            {
                if (kvp.Key == null)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var dead in toRemove)
            {
                dictionaryObjectsAndTheirAudioSourcesByTypes.Remove(dead);
            }
        }
    }



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
