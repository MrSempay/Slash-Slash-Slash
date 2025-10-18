using UnityEngine;
using static StaticClassForAdditionalFunctions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;

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
    private AudioClip _currentMusic; // Ссылка на аудиофайл для взрыва
    private AudioClip _beginningMusic; // Ссылка на аудиофайл для взрыва
    private AudioClip _transitionMusic; // Ссылка на аудиофайл для взрыва
    private AudioClip _certainMusic; // Ссылка на аудиофайл для какой-то конкретной музыки
    private string _pathToMusicFolder = "Music/Musics/";
    private string _pathToSoundsEffect = "Music/Effects/";
    private string _nameBeginningMusic = "BeginningLevelMusic";
    private string _nameTransitionMusic = "TransitionMusic";
    private Coroutine _fadeEnvironmentSoundsCoroutine;
    private MusicManager _musicManager;
    private int _amountSoundEffectsInNearTime = 0;
    private int _maxSoundEffectsInNearTime = 3;
    private float _nearTime = 0.3f;
    private Coroutine _resetAmountSoundsEffectsCorotune;

    //DefaultAS - по сути тот же смысл, что и у Default, но он для AudioSource, у которых установлен AudioSource и проигрывается он как "музыка", зачастую в loop
    public enum TYPE_SOUND { Walk, AttackPeak, GetDamage, Default, DefaultAS, Death, Destroy}; 
    public enum TYPE_AUDIO_SOURCE { _2DStandard, _3DStandard }; 
    public Dictionary<GameObject, Dictionary<TYPE_SOUND, AudioSourceExtended>> dictionaryObjectsAndTheirAudioSourcesByTypes = new(); // возможно скоро будет Legacy, переходим на emitters
    public List<AudioEmitter> emitters = new();
    public AudioSource audioMusicComponent; // Ссылка на AudioSource для музыки
    public AudioSource audioEffectsUIComponent; // Ссылка на AudioSource для UI-звуковых эффектов
    public bool LockVolumeEnviromentSounds { get; set; }

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

    public class AudioSourceExtended
    {
        public AudioSource audioSource;
        public float maxVolume;

        public AudioSourceExtended(AudioSource audioSource, float maxVolume)
        {
            this.audioSource = audioSource;
            this.maxVolume = maxVolume;
        }
    }

    // метод вообще ничего не делает, но как-то инициализировать наш синглтон надо, создавать переменную и присваивать ей ненужную ссылку на наш объект желания нет. 
    // Увы, просто GameManager.Instance сделать нельзя
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

        _musicManager = gameObject.AddComponent<MusicManager>();
        _musicManager.musicSource = audioMusicComponent;
        _musicManager.musicSource.loop = false;

        UploadOtherMusic();


        audioEffectsUIComponent = gameObject.AddComponent<AudioSource>();

        CoroutineManager.Instance.StartManagedCoroutine(gameObject, ControlSoundSourcesDictionary());


        LoadSoundsDictionary(_pathToSoundsEffect, _sourcesSounds);
    }


    //            ------------------------------------ НОВЫЙ УПРАВЛЯТОР АУДИО !!! ----------------------------------------               //


    public void UpdateMusicLevelSetL()
    {
        _beginningMusic = Resources.Load<AudioClip>(_pathToMusicFolder + LevelBuilder.instance.selfName + "/" + _nameBeginningMusic);
        _transitionMusic = Resources.Load<AudioClip>(_pathToMusicFolder + LevelBuilder.instance.selfName + "/" + _nameTransitionMusic);

        if (_beginningMusic == null || _transitionMusic == null)
        {
            Debug.LogError("Отсутствует музыка перехода или начальная музыка для уровня!");
        }

        string fightMusicFolder = _pathToMusicFolder + LevelBuilder.instance.selfName + "/FightMusic/";
        string ambientMusicFolder = _pathToMusicFolder + LevelBuilder.instance.selfName + "/AmbientMusic/";

        LoadSoundsDictionary(fightMusicFolder, _dictionaryFightMusic);
        LoadSoundsDictionary(ambientMusicFolder, _dictionaryAmbientMusic);
    }
    public void UpdateMusicLevelSetL2()
    {
        if (_beginningMusic != LevelBuilder.instance.beginningMusic)
            _beginningMusic = LevelBuilder.instance.beginningMusic;
        if (_transitionMusic != LevelBuilder.instance.transitionMusic)
            _transitionMusic = LevelBuilder.instance.transitionMusic;

        if (_beginningMusic == null || _transitionMusic == null)
        {
            Debug.LogError("Отсутствует музыка перехода или начальная музыка для уровня!");
        }

        foreach (AudioClip clip in LevelBuilder.instance.listAmbientMusics)
        {
            _dictionaryAmbientMusic.TryAdd(clip.name, clip);
        }
        foreach (AudioClip clip in LevelBuilder.instance.listFightMusics)
        {
            _dictionaryFightMusic.TryAdd(clip.name, clip);
        }
    }

    #region Mega new Controller
    public void UpdateMusicLevelSet()
    {
        _musicManager.UpdateMusicLevelSet(
            beginning: LevelBuilder.instance.beginningMusic,
            transition: LevelBuilder.instance.transitionMusic,
            ambient: LevelBuilder.instance.listAmbientMusics,
            fight: LevelBuilder.instance.listFightMusics
        );
    }
    public void UploadOtherMusic()
    {
        List<AudioClip> musicList = new List<AudioClip>();

        // Загружаем все аудиоклипы из папки Resources
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Music/Musics/MusicPool/Other");

        // Преобразуем массив в List
        musicList = new List<AudioClip>(clips);

        _musicManager.UploadOtherMusic(musicList);
    }

    public void StartBeginningMusic()
    {
        _musicManager.PlayBeginningMusic();
    }
    public void PlayFightOrAmbientMusic(bool isFightMusic)
    {
        if (isFightMusic) _musicManager.PlayFightMusic();
        else _musicManager.PlayAmbientMusic();
    }
    public void StartCertainMusicInLoop(string nameMusic)
    {
        _musicManager.PlayCertainMusic(nameMusic);
    }


    #endregion Mega new Controller

    public void StartBeginningMusicL()
    {
        Debug.Log("Да как так " + _musicWasEndedByItself);
        StopAllCoroutines();

        if (!_musicWasEndedByItself)
        {
            Debug.Log("И что это?");
            StartCoroutine(FadePreviousMusicAndStartBeginning());
            return;
        }

        _musicWasEndedByItself = false;

        //audioMusicComponent.volume = 0;
        StartCoroutine(FadeCurrentMusicTickStartTransitionAndStartTargetMusicAmbientOrFight(_beginningMusic, null));
    }

    public void PlayFightOrAmbientMusicL(bool isFightMusic)
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
                Debug.Log("ПАПКА С МУЗЫКОЙ !!!!!!!!!!!!!!!!!!!!! ПУСТАААААААААААААААААААААААААААААААААААААААААААААААА");
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
                Debug.Log("ПАПКА С МУЗЫКОЙ !!!!!!!!!!!!!!!!!!!!! ПУСТАААААААААААААААААААААААААААААААААААААААААААААААА");
            }
        }


    }

    public void StartCertainMusicInLoopL(string nameMusic)
    {
        StopAllCoroutines();


        if (!_dictionaryOtherMusic.ContainsKey(nameMusic))
        {
            _certainMusic = Resources.Load<AudioClip>(_pathToMusicFolder + nameMusic);

            if (_certainMusic == null)
            {
                Debug.LogError("Музыки с именем " + nameMusic + " в заданной директории " + _pathToMusicFolder + " не найдено!");
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

    // StartSoundEffect вызывает проигрывание эффекта на общем SoundSource для эффектов, который находится на... AudioManager.Instance. Звук будет воспроизводиться даже если целевой
    // объект был disabled, ибо проигрывание звука к самому объекту по сути отношения не имеет. А вот при вызове метода StartSoundEffectAtSpecifiedObject звук проигрываться не будет,
    // ибо его AudioSource становится disabled вместе с самим объектом (с кнопками показательная ситуация). По идее можно disable только необходимые компоненты, чтоб её нельзя было
    // нажать, а оставлять только её AudioSource, но что-то это на мороку странную похоже
    public void StartSoundEffect(string nameEffect)
    {
        if (string.IsNullOrEmpty(nameEffect))
        {
            return;
        }

        if (_sourcesSounds.TryGetValue(nameEffect, out var audioClip))
        {
            audioEffectsUIComponent.PlayOneShot(audioClip);

        }
        else
        {
            AudioClip _currentEffect = Resources.Load<AudioClip>(_pathToSoundsEffect + nameEffect);
            _sourcesSounds[nameEffect] = _currentEffect;
            audioEffectsUIComponent.PlayOneShot(_currentEffect);
        }
    }

    public async Task FadeAllEnviromentSoundsAsync(float duration = 1f) // по идее, эту штуку мне не надо отменять, поэтому CanelationToken сюда не передаём. Может потом изменим сигнатуру...
    {
        if (_fadeEnvironmentSoundsCoroutine != null)
        {
            Debug.LogWarning("Fade already in progress!");
            return;
        }

        LockVolumeEnviromentSounds = true;

        IEnumerator FadeAllEnviromentSoundsCoroutine(float duration)
        {
            // Собираем все аудиосорсы, чтобы работать с копией
            List<AudioSourceExtended> allAudioSources = new List<AudioSourceExtended>();
            foreach (var objAudioSourcesCluster in dictionaryObjectsAndTheirAudioSourcesByTypes.Values) // поддержка Legacy
            {
                foreach (AudioSourceExtended audioSourceExtended in objAudioSourcesCluster.Values)
                {
                    if (audioSourceExtended.audioSource != null)
                    {
                        allAudioSources.Add(audioSourceExtended);
                    }
                }
            }
            //foreach (AudioEmitter emitter in emitters)
            //{
            //    Debug.Log(emitter);
            //    Debug.Log(emitter.gameObject);
            //    foreach (AudioSourceExtended audioSourceExtended in emitter.sources.Values)
            //    {
            //        if (audioSourceExtended.audioSource != null)
            //        {
            //            allAudioSources.Add(audioSourceExtended);
            //        }
            //    }
            //}
            // Сохраняем исходные значения объёмов
            float[] startVolumes = allAudioSources.Select(a => a.audioSource.volume).ToArray();

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                for (int i = 0; i < allAudioSources.Count; i++)
                {
                    var audioSource = allAudioSources[i].audioSource;
                    if (audioSource != null)
                    {
                        audioSource.volume = Mathf.Lerp(startVolumes[i], 0f, t);
                    }
                }

                yield return null;
            }

            // Финально обнуляем все значения
            foreach (var audioSourceExtended in allAudioSources)
            {
                if (audioSourceExtended.audioSource != null)
                    audioSourceExtended.audioSource.volume = 0f;
            }
        }

        AudioEmitter[] snapshot = emitters.ToArray();

        List<Task> fadeTasks = new List<Task>();

        foreach (var emitter in snapshot)
        {
            if (emitter != null)
            {
                Task safeTask = SafeIgnoreErrors(emitter.FadeOutAsync(duration));
                fadeTasks.Add(safeTask);
            }
        }


        var tcs = new TaskCompletionSource<bool>();

        _fadeEnvironmentSoundsCoroutine = StartCoroutine(RunCoroutineAsync(FadeAllEnviromentSoundsCoroutine(duration), tcs));

        fadeTasks.Add(SafeIgnoreErrors(tcs.Task));

        await Task.WhenAll(fadeTasks); // ждём, пока все погаснут

        //await tcs.Task;
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
        // Загружаем все AudioClip из указанной папки
        AudioClip[] clipsFight = Resources.LoadAll<AudioClip>(path);

        // Добавляем в словарь по имени
        foreach (AudioClip clip in clipsFight)
        {
            if (clip != null)
            {
                // clip.name — это имя файла без расширения
                dict[clip.name] = clip;
                Debug.Log($"Загружен трек: {clip.name}");
            }
        }
    }

    private AudioSource AttachToObjectAndCashAudioSource(GameObject obj, TYPE_SOUND typeSound, TYPE_AUDIO_SOURCE typeAudioSource, float maxVolume)
    {
        AudioSource audioSourceTarget = obj.AddComponent<AudioSource>();
        audioSourceTarget.volume = GameManager.Instance.currentSettings.volumeEffects;

        dictionaryObjectsAndTheirAudioSourcesByTypes[obj][typeSound] = new AudioSourceExtended(audioSourceTarget, maxVolume);

        switch (typeAudioSource)
        {
            case TYPE_AUDIO_SOURCE._3DStandard:
                audioSourceTarget.spatialBlend = 1;
                audioSourceTarget.rolloffMode = AudioRolloffMode.Linear;
                audioSourceTarget.minDistance = 4f;
                audioSourceTarget.maxDistance = 21f;
                break;
            case TYPE_AUDIO_SOURCE._2DStandard:
                break; // по умолчанию тот компонент, который создаётся, нас устраивает
        }

        return audioSourceTarget;
    }

    private IEnumerator FadePreviousMusicAndStartBeginning()
    {
        while (audioMusicComponent.volume > 0.03f)
        {
            Debug.Log("И 1");
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
                Debug.Log("ПАПКА С МУЗЫКОЙ !!!!!!!!!!!!!!!!!!!!! ПУСТАААААААААААААААААААААААААААААААААААААААААААААААА");
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
                Debug.Log("ПАПКА С МУЗЫКОЙ !!!!!!!!!!!!!!!!!!!!! ПУСТАААААААААААААААААААААААААААААААААААААААААААААААА");
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
        audioMusicComponent.volume = GameManager.Instance.currentSettings.VolumeMusic; // на данный момент у нас после затухания предыдущей музыки музыка перехода начинается в полную силу. Может, нужно сделать плавное нарастание
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
        audioMusicComponent.volume = GameManager.Instance.currentSettings.VolumeMusic; // на данный момент у нас после затухания предыдущей музыки музыка перехода начинается в полную силу. Может, нужно сделать плавное нарастание
        if (_transitionMusic != null) // по идее у нас музыка перехода подгружается только при UpdateLevelSet, который вызывается из LevelBuilder.Instance. На сценах без него у нас её...
                                      // в целом нету. Может потом добавлю. И музыка без перехода одна в другую переливается
        {
            if (audioMusicComponent.clip != _transitionMusic)
            {
                Debug.Log("Что-то странное");
                audioMusicComponent.clip = _transitionMusic;
                audioMusicComponent.Play();
            }
            StartCoroutine(WaitForTransitionBetweenCertainMusicEnd(targetMusic, nameMusic)); // здесь должно быть transition
        }
        else
        {
            Debug.Log("Музыка перехода отсутствует! Пропускаем логику с ней и переходим к следующей музыке!");

            _musicWasEndedByItself = true;

            StartCertainMusicInLoop(nameMusic);
        }
    }

    private IEnumerator WaitForTransitionBetweenFightOrAmbientMusicEnd(AudioClip targetMusic, bool? isFightMusic = null)
    {
        //Debug.Log($"Вызов из: {System.Environment.StackTrace}");
        if (_transitionMusic != null) // хотя если это у нас боевая или музыка покоя, музыка перехода у нас всегда будет, но да ладно, оставлю проверку
        {
            yield return new WaitForSecondsRealtime(_transitionMusic.length);

            Debug.Log("Музыка перехода закончилась!");

            _musicWasEndedByItself = true;

            if (isFightMusic != null) // вообще, дичь это. Нужно поменять на какой-нибудь enum. На данный момент у нас может быть лишь 3 состояния: true (боевая), false (мирная) и null - начальная
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
            Debug.Log("Музыка перехода отсутствует! Пропускаем логику с ней и переходим к следующей музыке!");

            _musicWasEndedByItself = true;

            if (isFightMusic != null) // вообще, дичь это. Нужно поменять на какой-нибудь enum. На данный момент у нас может быть лишь 3 состояния: true (боевая), false (мирная) и null - начальная
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

        Debug.Log("Музыка перехода закончилась!");

        _musicWasEndedByItself = true;

        StartCertainMusicInLoop(nameMusic);
    }

    private IEnumerator WaitForAmbientOrFightMusicEnd(AudioClip targetMusic, bool isFightMusic)

    {
        yield return new WaitForSecondsRealtime(targetMusic.length);

        Debug.Log("Закончившаяся мелодия: " + targetMusic.name);
        Debug.Log("Длилась " + targetMusic.length + " секунд");
        Debug.Log("Начинаем музыкальный цикл заново!");

        _musicWasEndedByItself = true;

        PlayFightOrAmbientMusic(isFightMusic);
    }
    private IEnumerator WaitForBeginningMusicEnd()
    {
        yield return new WaitForSecondsRealtime(_beginningMusic.length);

        _musicWasEndedByItself = true;

        Debug.Log("Переходим из начальной музыки в эмбиент");
        PlayFightOrAmbientMusic(false);
    }
    private IEnumerator WaitForMusicEndByItself(AudioClip music, string nameMusic)
    {
        yield return new WaitForSecondsRealtime(music.length);

        _musicWasEndedByItself = true;

        Debug.Log("Закончившаяся мелодия в главном меню: " + music.name);

        StartCertainMusicInLoop(nameMusic);
    }
    private IEnumerator RunCoroutineAsync(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
    {
        yield return coroutine;
        _fadeEnvironmentSoundsCoroutine = null;
        tcs.SetResult(true);

    }

    // -------------------------------------------- КОНЕЦ НОВОГО УПРАВЛЯТОРА --------------------------------------------- // 

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




    // --- Интеграция новой системы управления звуковыми эффектами --- //

    public void StartSoundEffectAtSpecifiedEmitter(string nameEffect,
                                                   AudioEmitter audioEmitter,
                                                   TYPE_SOUND typeSound,
                                                   TYPE_AUDIO_SOURCE typeAudioSource,
                                                   List<TYPE_SOUND> typeSoundsToStop = null,
                                                   float maxVolume = 1,
                                                   bool asAudioSource = false,
                                                   bool playInLoop = true)
    {
        if (string.IsNullOrEmpty(nameEffect) || !_sourcesSounds.ContainsKey(nameEffect) || _amountSoundEffectsInNearTime >= _maxSoundEffectsInNearTime)
        {
            return;
        }

        if (typeSound == TYPE_SOUND.AttackPeak || typeSound == TYPE_SOUND.Death ||  typeSound == TYPE_SOUND.GetDamage)
        {
            _amountSoundEffectsInNearTime++;

            if (_resetAmountSoundsEffectsCorotune == null)
            {
                StartCoroutine(ResetAmountSoundsEffects());
            }
        }

        if (typeSoundsToStop != null)
        {
            foreach (TYPE_SOUND typeSoundToStop in typeSoundsToStop)
                audioEmitter.Stop(typeSoundToStop);
        }

        audioEmitter.Play(typeSound, typeAudioSource, _sourcesSounds[nameEffect],  maxVolume, asAudioSource, playInLoop);
    }

    public void RegisterEmitter(AudioEmitter emitter)
    {
        if (!emitters.Contains(emitter))
            emitters.Add(emitter);
    }

    public void UnregisterEmitter(AudioEmitter emitter)
    {
        emitters.Remove(emitter);
    }

    public void StopSomeTypeSoundOnEmitter(TYPE_SOUND typeSound, AudioEmitter audioEmitter)
    {
        audioEmitter.Stop(typeSound);
    }

    private IEnumerator ResetAmountSoundsEffects()
    {
        yield return new WaitForSecondsRealtime(_nearTime);
        _amountSoundEffectsInNearTime = 0;
        _resetAmountSoundsEffectsCorotune = null;
    }


    // --- Конец интеграции --- //

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (_fadeEnvironmentSoundsCoroutine != null)
            StopCoroutine(_fadeEnvironmentSoundsCoroutine);
    }
}
