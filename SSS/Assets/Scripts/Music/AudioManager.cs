using UnityEngine;
using static StaticClassForAdditionalFunctions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AudioManager : MonoBehaviour
{


    //DefaultAS - по сути тот же смысл, что и у Default, но он для AudioSource, у которых установлен AudioSource и проигрывается он как "музыка", зачастую в loop
    public enum TYPE_SOUND { Walk, AttackPeak, GetDamage, Default, DefaultAS, Death, Destroy}; 
    public enum TYPE_AUDIO_SOURCE { _2DStandard, _3DStandard }; 
    public Dictionary<GameObject, Dictionary<TYPE_SOUND, AudioSourceExtended>> dictionaryObjectsAndTheirAudioSourcesByTypes = new(); // возможно скоро будет Legacy, переходим на emitters
    public List<AudioEmitter> emitters = new();
    public AudioSource audioMusicComponent; // Ссылка на AudioSource для музыки
    public AudioSource audioEffectsUIComponent; // Ссылка на AudioSource для UI-звуковых эффектов
    public bool LockVolumeEnviromentSounds { get; set; }

    private static AudioManager _instance;
    private float _timeTickOfChangingVolumeBetweenMusic = 0.03f;
    private float _timeCleanUpSoundSourcesDictionary = 60f;
    private bool _musicWasEndedByItself = true;
    private Dictionary<string, AudioClip> _sourcesSounds = new();
    private Dictionary<string, AudioClip> _dictionaryFightMusic = new();
    private Dictionary<string, AudioClip> _dictionaryAmbientMusic = new();
    private Dictionary<string, AudioClip> _dictionaryOtherMusic = new();
    private List<string> _listCurrentEffects = new();
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
    private int _amountSoundEffectsInNearTime = 0; // Legacy
    private Coroutine _resetAmountSoundsEffectsCorotune;

    private readonly int _maxSoundEffectsInNearTime = 3;
    private readonly float _nearTime = 0.2f;

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



    #region Music controlling

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


    #endregion Music controlling


    #region Sound effects controlling


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

    public void StartSoundEffectAtSpecifiedEmitter(string nameEffect,
                                                   AudioEmitter audioEmitter,
                                                   TYPE_SOUND typeSound,
                                                   TYPE_AUDIO_SOURCE typeAudioSource,
                                                   List<TYPE_SOUND> typeSoundsToStop = null,
                                                   float maxVolume = 1,
                                                   bool asAudioSource = false,
                                                   bool playInLoop = true)
    {
        if (string.IsNullOrEmpty(nameEffect) || !_sourcesSounds.ContainsKey(nameEffect) || _listCurrentEffects.Contains(nameEffect)) // _amountSoundEffectsInNearTime >= _maxSoundEffectsInNearTime
        {
            return;
        }

        if (typeSound == TYPE_SOUND.AttackPeak || typeSound == TYPE_SOUND.Death || typeSound == TYPE_SOUND.GetDamage)
        {
            _amountSoundEffectsInNearTime++; // Legacy

            if (!_listCurrentEffects.Contains(nameEffect)) // Логика следующая: при запуске вышеуказанных в условии звуков мы добавляем НАЗВАНИЕ звука в массив. Если при последующем 
                // вызове этой функции в данном массиве уже будет звук для текущего вызова, то мы ничего не вызываем и выходим из функции. Очищаем массив раз в _nearTime.
                _listCurrentEffects.Add(nameEffect);

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

        audioEmitter.Play(typeSound, typeAudioSource, _sourcesSounds[nameEffect], maxVolume, asAudioSource, playInLoop);
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

    private IEnumerator RunCoroutineAsync(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
    {
        yield return coroutine;
        _fadeEnvironmentSoundsCoroutine = null;
        tcs.SetResult(true);

    }

    private IEnumerator ResetAmountSoundsEffects()
    {
        yield return new WaitForSecondsRealtime(_nearTime);
        _amountSoundEffectsInNearTime = 0; // Legacy
        _listCurrentEffects.Clear();
        _resetAmountSoundsEffectsCorotune = null;
    }

    #endregion Sound effects controlling


    #region General sound conrtrolling

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


    #endregion

    // -------------------------------------------- КОНЕЦ НОВОГО УПРАВЛЯТОРА --------------------------------------------- // 

    private IEnumerator ControlSoundSourcesDictionary() // поддержка Legacy
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

    private void OnDestroy()
    {
        if (_fadeEnvironmentSoundsCoroutine != null)
            StopCoroutine(_fadeEnvironmentSoundsCoroutine);
    }
}
