using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // чтобы видеть Light2D

[RequireComponent(typeof(Light2D))]
public class TorchFlicker : MonoBehaviour
{
    [Header("Настройки мерцания")]
    [Tooltip("Базовая яркость факела (среднее значение).")]
    public float baseIntensity = 1f;

    [Tooltip("Амплитуда колебаний яркости.")]
    public float flickerAmplitude = 0.2f;

    [Tooltip("Частота изменения (чем меньше — тем быстрее).")]
    public float flickerSpeed = 0.05f;

    private Light2D _light2D;

    private void Awake()
    {
        _light2D = GetComponent<Light2D>();
    }

    private void OnEnable()
    {
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Берём случайное значение вокруг baseIntensity
            float target = baseIntensity + Random.Range(-flickerAmplitude, flickerAmplitude);

            // Меняем плавно к новому значению
            float t = 0f;
            float start = _light2D.intensity;
            while (t < 1f)
            {
                t += Time.deltaTime / flickerSpeed;
                _light2D.intensity = Mathf.Lerp(start, target, t);
                yield return null;
            }
        }
    }
}
