using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using static UnityEngine.Rendering.DebugUI;

public class AudioEmitter : MonoBehaviour
{
    public Dictionary<TYPE_SOUND, AudioSourceExtended> sources = new(); // может быть сразу много звуковых источников у объекта (его AudioEmitter)

    private CancellationTokenSource _cts = new();
    private bool _isFading = false;

    public void Play(TYPE_SOUND type, TYPE_AUDIO_SOURCE typeAudioSource, AudioClip clip, float maxVolume = 1, bool asAudioSource = false, bool loop = false)
    {
        AudioSource audioSource = AddAudioSourceExtended(type, typeAudioSource, maxVolume);

        if (asAudioSource)
        {
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void Stop(TYPE_SOUND type)
    {
        if (sources.TryGetValue(type, out var src) && src != null)
            src.audioSource.Stop();
    }

    public void SetVolume(float globalVolume)
    {
        foreach (AudioSourceExtended sourceExtended in sources.Values)
            if (sourceExtended != null)
                sourceExtended.audioSource.volume = Mathf.Clamp01(sourceExtended.maxVolume * globalVolume);
    }
    public async Task FadeOutAsync(float duration)
    {
        if (duration <= 0f)
        {
            foreach (var audioSourceExtended in sources)
            {
                audioSourceExtended.Value.audioSource.volume = 0f;
            }
            return;
        }

        if (_isFading)
            return; // уже гаснет

        _isFading = true;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new();

        IEnumerator FadeCoroutine()
        {
            List<AudioSourceExtended> allAudioSources = new List<AudioSourceExtended>();

            foreach (AudioSourceExtended audioSourceExtended in sources.Values)
            {
                if (audioSourceExtended.audioSource != null)
                {
                    allAudioSources.Add(audioSourceExtended);
                }
            }

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

            foreach (var audioSourceExtended in allAudioSources)
            {
                if (audioSourceExtended.audioSource != null)
                    audioSourceExtended.audioSource.volume = 0f;
            }

            yield break;
        }

        CoroutineExtensions.CoroutineHandle coroutineHandle = FadeCoroutine().AsTask(this, _cts.Token);
        try
        {
            await coroutineHandle.Task;
            // Финально обнуляем все значения

        }
        finally
        {
            _isFading = false;
        }

    }

    private void RemoveEmiiterFromAudioManager()
    {
        if (AudioManager.Instance.emitters.Contains(this))
            AudioManager.Instance.emitters.Remove(this);
    }

    private AudioSource AddAudioSourceExtended(TYPE_SOUND type, TYPE_AUDIO_SOURCE typeAudioSource, float maxVolume)
    {
        if (!sources.TryGetValue(type, out var src) || src == null)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            src = new AudioSourceExtended(audioSource, maxVolume);
            sources[type] = src;
        }

        switch (typeAudioSource)
        {
            case TYPE_AUDIO_SOURCE._3DStandard:
                src.audioSource.spatialBlend = 1;
                src.audioSource.rolloffMode = AudioRolloffMode.Linear;
                src.audioSource.minDistance = 4f;
                src.audioSource.maxDistance = 21f;
                break;
            case TYPE_AUDIO_SOURCE._2DStandard:
                break; // по умолчанию тот компонент, который создаётся, нас устраивает
        }
        return src.audioSource;
    }

    private void OnDisable()
    {
        RemoveEmiiterFromAudioManager();
        _cts?.Cancel();
        if (_isFading)
        {
            foreach (var audioSourceExtended in sources)
            {
                if (audioSourceExtended.Value.audioSource != null)
                    audioSourceExtended.Value.audioSource.volume = 0f;
            }
        }
    }

    private void OnDestroy()
    {
        // удалить все ссылки при уничтожении
        sources.Clear();
        AudioManager.Instance?.UnregisterEmitter(this);
        _cts?.Cancel();
        _cts?.Dispose();

        // по идее нижестоящий пост не нужен, ибо мы и так OnDestroy вызываем из main thread. Он нужен, например, в методах Dispose и т.п
        GameManager.UnityContext.Post(_ =>
        {
            try
            {
                //_cts?.Cancel();

                /*  Хз, правда это или нет: 
                 *  
                 * Когда StopCoroutine() отрабатывает, Unity фактически прекращает выполнение IEnumerator не прямо в тот момент вызова,
                 а помечает корутину “на остановку”, и прекращает вызывать MoveNext() только в начале следующего кадра.

                 Если да, то могут быть проблемы в рамках корутины по типу NRE. Ну и бредятина

                */

            }
            catch (Exception ex)
            {
                //Debug.LogWarning($"Some exception while canceling light fading: {ex}");
            }
            finally
            {
                //_cts?.Dispose();
            }
        }, null);
    }

}