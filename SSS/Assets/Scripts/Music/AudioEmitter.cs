using System.Collections.Generic;
using UnityEngine;
using static AudioManager;

public class AudioEmitter : MonoBehaviour
{
    public Dictionary<TYPE_SOUND, AudioSourceExtended> sources = new();

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
                break; // по умолчанию тот компонент, который создаЄтс€, нас устраивает
        }
        return src.audioSource;
    }

    private void OnDestroy()
    {
        // удалить все ссылки при уничтожении
        sources.Clear();
        AudioManager.Instance?.UnregisterEmitter(this);
    }
}