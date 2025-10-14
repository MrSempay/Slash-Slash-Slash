using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static GameManager;

public class LightManager : MonoBehaviour
{
    private static LightManager _instance;

    private Coroutine _fadeAllLightsCoroutine;

    public static LightManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("LightManager");
                _instance = obj.AddComponent<LightManager>();
                //DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

    }

    public async Task FadeAllLightsAsync(float fadeDuration) // по идее, эту штуку мне не надо отменять, поэтому CanelationToken сюда не передаём. Может потом изменим сигнатуру...
    {
        // Находим все Light2D на сцене
        Light2D[] lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);

        if (lights.Length == 0 || fadeDuration <= 0f)
        {
            foreach (var l in lights)
                l.intensity = 0f;
            return;
        }

        // Сохраняем исходные интенсивности
        float[] startIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
            startIntensities[i] = lights[i].intensity;

        float elapsed = 0f;

        // Функция, которая вернёт Task и работает как корутина
        Task fadeTask = new TaskCompletionSource<bool>().Task;

        // Лямбда для запуска корутины
        IEnumerator FadeCoroutine()
        {
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                for (int i = 0; i < lights.Length; i++)
                    lights[i].intensity = Mathf.Lerp(startIntensities[i], 0f, t);

                yield return null;
            }

            // Завершаем точно на 0
            for (int i = 0; i < lights.Length; i++)
                lights[i].intensity = 0f;

            yield break;
        }
        
        await FadeCoroutine().AwaitCoroutine(this); // ЧИТАЙ КЛАСС CoroutineExtensions !!!

        //var tcs = new TaskCompletionSource<bool>();

        //_fadeAllLightsCoroutine = StartCoroutine(RunCoroutineAsync(FadeCoroutine(), tcs));

        //await tcs.Task;

        //_fadeAllLightsCoroutine = null;
    }
    public async Task RiseAllLightsAsync(float riseDuration) // по идее, эту штуку мне не надо отменять, поэтому CanelationToken сюда не передаём. Может потом изменим сигнатуру...
    {

    }



    private IEnumerator RunCoroutineAsync(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
    {
        yield return coroutine;
        tcs.SetResult(true);
    }

    private void OnDestroy()
    {
        if (_fadeAllLightsCoroutine != null)
            StopCoroutine(_fadeAllLightsCoroutine);
    }
}
