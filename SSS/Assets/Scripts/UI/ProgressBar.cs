using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] public Image _imageFilling;

    private float _maxValue;
    private float _minValue;
    private float _currentValue;

    public float CurrentValue {  
        get { return _currentValue; }
        set {

            _currentValue = value;
            ChangeProgressBar((value - _minValue) / (_maxValue - _minValue));
        }
    }

    public void Initialize(float minimum, float maximum)
    {
        _minValue = minimum;
        _maxValue = maximum;
        CurrentValue = CurrentValue;
    }

    private void ChangeProgressBar(float valueAsPercantage)
    {
        _imageFilling.fillAmount = valueAsPercantage;
    }
}
