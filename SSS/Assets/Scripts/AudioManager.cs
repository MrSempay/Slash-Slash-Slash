using UnityEngine;
using static GameManager;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{

    private static AudioManager _instance;
    private float _timeTickOfChangingVolumeBetweenMusic = 0.03f;
    private Dictionary<string, AudioClip> _sourcesSounds = new();
    private AudioClip _currentMusic; // —сылка на аудиофайл дл€ взрыва

    public AudioSource _audioMusicComponent; // —сылка на AudioSource дл€ музыки
    public AudioSource _audioEffectsComponent; // —сылка на AudioSource дл€ звуковых эффектов
    public string _pathToMusicFile = "Music/Musics/";
    public string _pathToSoundsEffect = "Music/Effects/";



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

        _audioMusicComponent = gameObject.AddComponent<AudioSource>();
        _audioMusicComponent.loop = true;

        _audioEffectsComponent = gameObject.AddComponent<AudioSource>();
    }

    public void StartMusic(string nameMusic)
    {
        //Debug.Log(nameMusic);
        StopAllCoroutines();
        if (_sourcesSounds.ContainsKey(nameMusic))
        {
            if (_sourcesSounds[nameMusic] != _audioMusicComponent.clip)
            {
                //Debug.Log(1);
                FadeCurrentMusicAndAfterRise(_sourcesSounds[nameMusic]);
            }
            else
            {
                //Debug.Log(2);
                //RiseCurrentMusic(_sourcesSounds[nameMusic]);
            }
        }
        else
        {
            _currentMusic = Resources.Load<AudioClip>(_pathToMusicFile + nameMusic);
            _sourcesSounds[nameMusic] = _currentMusic;

            if ( _sourcesSounds.Count == 1)
            {
                //Debug.Log(3);
                _audioMusicComponent.volume = 0;
                RiseCurrentMusic(_currentMusic);
            }
            else
            {
                //Debug.Log(4);
                FadeCurrentMusicAndAfterRise(_currentMusic);
            }

        }
    }

    public void StartSoundEffect(string nameEffect)
    {
        if (_sourcesSounds.ContainsKey(nameEffect))
        {
            _audioEffectsComponent.PlayOneShot(_sourcesSounds[nameEffect]);

        }
        else
        {
            AudioClip _currentEffect = Resources.Load<AudioClip>(_pathToSoundsEffect + nameEffect);
            _sourcesSounds[nameEffect] = _currentEffect;
            _audioEffectsComponent.PlayOneShot(_currentEffect);
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
        while (_audioMusicComponent.volume > 0.03f)
        {
            _audioMusicComponent.volume -= 0.015f;
            yield return new WaitForSeconds(_timeTickOfChangingVolumeBetweenMusic);
        }
        StartCoroutine(RiseCurrentMusicTick(explosionSound));
    }
     IEnumerator FadeCurrentMusicTick(AudioClip explosionSound)
    {
        while (_audioMusicComponent.volume > 0.03f)
        {
            _audioMusicComponent.volume -= 0.015f;
            yield return new WaitForSeconds(_timeTickOfChangingVolumeBetweenMusic);
        }
    }

    IEnumerator RiseCurrentMusicTick(AudioClip explosionSound)
    {
        yield return null;

        _audioMusicComponent.clip = explosionSound;
        _audioMusicComponent.Play();
        while (_audioMusicComponent.volume < GameManager.Instance.currentSettings.VolumeMusic)
        {
            //Debug.Log("???");
            _audioMusicComponent.volume += 0.015f;
            yield return new WaitForSeconds(_timeTickOfChangingVolumeBetweenMusic);
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
