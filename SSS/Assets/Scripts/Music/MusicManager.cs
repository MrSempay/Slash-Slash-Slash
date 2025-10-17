using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// MusicManager — асинхронный контроллер фоновой музыки без корутин.
/// Поддерживает плавные переходы через переходную композицию и управляет циклами.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private static AudioManager _instance;

    [Header("Audio Sources")]
    [SerializeField] public AudioSource musicSource;

    [Header("Timings")]
    [SerializeField] private float fadeDuration = 1.5f; // сек
    [SerializeField] private float transitionFadeDuration = 1.5f;

    [Header("Music Clips")]
    [SerializeField] private AudioClip beginningMusic;
    [SerializeField] private AudioClip transitionMusic;
    [SerializeField] private List<AudioClip> ambientMusics;
    [SerializeField] private List<AudioClip> fightMusics;
    [SerializeField] private List<AudioClip> otherMusics;

    private AudioClip _currentClip;
    private CancellationTokenSource _cts;
    private System.Random _rnd = new();

    private void Awake()
    {
        Instance = this;
    }

    // =======================================================================
    // 🔹 Публичные API
    // =======================================================================
    public void UpdateMusicLevelSet(AudioClip beginning = null,
                                    AudioClip transition = null,
                                    List<AudioClip> ambient = null,
                                    List<AudioClip> fight = null,
                                    List<AudioClip> other = null)
    {
        // Обновляем только если передан новый клип и он отличается от текущего
        if (beginning != null && beginning != beginningMusic)
            beginningMusic = beginning;

        if (transition != null && transition != transitionMusic)
            transitionMusic = transition;

        ambientMusics = ambient ?? ambientMusics ?? new List<AudioClip>();
        fightMusics = fight ?? fightMusics ?? new List<AudioClip>();
        otherMusics = other ?? otherMusics ?? new List<AudioClip>();
    }

    public void UploadOtherMusic(List<AudioClip> otherMusics)
    {
        Debug.Log(otherMusics);
        this.otherMusics = otherMusics;
    }

    public async void PlayBeginningMusic() =>
        await SafePlayAsync(async ct => await PlayMusicFlowAsync(beginningMusic, MusicType.Beginning, ct));

    public async void PlayAmbientMusic() =>
        await SafePlayAsync(async ct => await PlayMusicFlowAsync(GetRandom(ambientMusics), MusicType.Ambient, ct));

    public async void PlayFightMusic() =>
        await SafePlayAsync(async ct => await PlayMusicFlowAsync(GetRandom(fightMusics), MusicType.Fight, ct));

    public async void PlayCertainMusic(string name) =>
        await SafePlayAsync(async ct => await PlayMusicFlowAsync(FindClipByName(name), MusicType.Certain, ct));

    // =======================================================================
    // 🔹 Внутренняя логика
    // =======================================================================

    private async Task SafePlayAsync(Func<CancellationToken, Task> playFunc)
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            await playFunc(_cts.Token);
        }
        catch (OperationCanceledException) { /* переход отменён — это норма */ }
        catch (Exception ex)
        {
            Debug.LogError($"[MusicManager] Ошибка проигрывания музыки: {ex}");
        }
    }

    private enum MusicType { Beginning, Ambient, Fight, Certain }

    private async Task PlayMusicFlowAsync(AudioClip targetClip, MusicType type, CancellationToken ct)
    {
        if (targetClip == null)
        {
            Debug.LogError($"[MusicManager] Целевая музыка отсутствует для {type}!");
            return;
        }

        Debug.Log($"🎵 Переход к {type} → {targetClip.name}");

        // 1️⃣ Затухаем текущую музыку (если есть)
        if (musicSource.isPlaying)
            await FadeOutAsync(musicSource, fadeDuration, ct);

        // 2️⃣ Играем переходную музыку, если есть
        if (transitionMusic != null)
        {
            await PlayWithTransitionAsync(targetClip, type, ct);
        }
        else
        {
            // Без перехода — просто включаем целевую с fade-in
            await FadeInAndLoopAsync(targetClip, ct);
        }
    }

    private async Task PlayWithTransitionAsync(AudioClip targetClip, MusicType type, CancellationToken ct)
    {
        // Воспроизводим музыку перехода
        musicSource.clip = transitionMusic;
        musicSource.volume = 0f;
        musicSource.Play();

        await FadeInAsync(musicSource, transitionFadeDuration, ct);

        // Ждём окончания перехода или его отмены
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(transitionMusic.length - transitionFadeDuration), ct);
        }
        catch (OperationCanceledException) { musicSource.Stop(); throw; }

        // Затухаем переход
        await FadeOutAsync(musicSource, transitionFadeDuration, ct);
        musicSource.Stop();

        // Включаем целевую музыку
        await FadeInAndLoopAsync(targetClip, ct);

        // Ждём конца композиции (если не луп)
        if (!musicSource.loop)
        {
            await Task.Delay(TimeSpan.FromSeconds(targetClip.length), ct);
            Debug.Log($"🎵 Музыка {type} завершилась, перезапускаем...");
            await PlayMusicFlowAsync(targetClip, type, ct); // повтор цикла
        }
    }

    private async Task FadeInAndLoopAsync(AudioClip clip, CancellationToken ct)
    {
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.loop = false;
        musicSource.Play();

        await FadeInAsync(musicSource, fadeDuration, ct);

        _currentClip = clip;
        _musicWasEndedByItself = true;
    }

    // =======================================================================
    // 🔹 Вспомогательные методы
    // =======================================================================

    private async Task FadeInAsync(AudioSource source, float duration, CancellationToken ct)
    {
        float start = 0f;
        float end = GameManager.Instance.currentSettings.VolumeMusic;
        float time = 0f;
        while (time < duration)
        {
            ct.ThrowIfCancellationRequested();
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, end, time / duration);
            await Task.Yield();
        }
        source.volume = end;
    }

    private async Task FadeOutAsync(AudioSource source, float duration, CancellationToken ct)
    {
        float start = source.volume;
        float time = 0f;
        while (time < duration)
        {
            ct.ThrowIfCancellationRequested();
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, 0f, time / duration);
            await Task.Yield();
        }
        source.volume = 0f;
    }

    private AudioClip GetRandom(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return null;
        return clips[_rnd.Next(clips.Count)];
    }

    private AudioClip FindClipByName(string name)
    {
        IEnumerable<AudioClip> allClips = Enumerable.Empty<AudioClip>();
        Debug.Log(otherMusics);
        if (ambientMusics != null)
            allClips = allClips.Concat(ambientMusics);
        if (fightMusics != null)
            allClips = allClips.Concat(fightMusics);
        if (otherMusics != null)
            allClips = allClips.Concat(otherMusics);

        return allClips.FirstOrDefault(c => c != null && c.name == name);
    }

    // Для флага (вместо старого поля)
    private bool _musicWasEndedByItself = true;


    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

}
