using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour

{
    [SerializeField] private Image _healthBarFilling;
    [SerializeField] DamageReceiver target;

    private void Awake()
    {
        target.HealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        target.HealthChanged -= OnHealthChanged;

    }

    private void OnHealthChanged(float valueAsPercantage)
    {
        _healthBarFilling.fillAmount = valueAsPercantage;
    }

}
