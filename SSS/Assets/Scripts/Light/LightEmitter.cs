using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightEmitter : MonoBehaviour
{

    [SerializeField] private float _maxBrightness = -1f;
    private Light2D _lightSource;
    private bool _isFading;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (_lightSource == null)
            _lightSource = GetComponent<Light2D>();
        if (_maxBrightness == -1f)
        {
            _maxBrightness = _lightSource.intensity;
        }
        LightManager.Instance.AddLightEmitter(this);
    }

    public async Task FadeOutAsync(float duration)
    {
        if (_lightSource == null || duration <= 0f)
        {
            if (_lightSource != null)
                _lightSource.intensity = 0f;
            return;
        }

        if (_isFading)
            return; // уже гаснет

        _isFading = true;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new();

        float startIntensity = _lightSource.intensity;
        float elapsed = 0f;

        IEnumerator FadeCoroutine()
        {
            while (elapsed < duration)
            {
                if (_lightSource == null)
                    break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _lightSource.intensity = Mathf.Lerp(startIntensity, 0f, t);
                yield return null; // аналог yield return null
            }

            yield break;
        }

        CoroutineExtensions.CoroutineHandle coroutineHandle = FadeCoroutine().AsTask(this, _cts.Token);
        try
        {
            await coroutineHandle.Task;
            if (_lightSource != null)
                _lightSource.intensity = 0f;
        }
        finally
        {
            _isFading = false;
        }

    }

    private void OnDestroy()
    {
        GameManager.UnityContext.Post(_ =>
        {
            try
            {
                _cts?.Cancel();

                /*  Хз, правда это или нет: 
                 *  
                 * Когда StopCoroutine() отрабатывает, Unity фактически прекращает выполнение IEnumerator не прямо в тот момент вызова,
                 а помечает корутину “на остановку”, и прекращает вызывать MoveNext() только в начале следующего кадра.

                 Если да, то могут быть проблемы в рамках корутины по типу NRE. Ну и бредятина

                */

            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Some exception while canceling light fading: {ex}");
            }
            finally
            {
                _cts?.Dispose();
            }
        }, null);

        if (LightManager.isExisting)
            LightManager.Instance.RemoveLightEmitter(this);
    }
}

