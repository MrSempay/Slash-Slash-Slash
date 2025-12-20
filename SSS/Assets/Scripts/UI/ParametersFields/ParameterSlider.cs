using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParameterSlider : ParameterFieldSettings, IControlLifeCicleFunctions
{
    private string _nameInvokingFunction;
    private float _currentValue;
    private VerticalLayoutGroup _verticalGroup;

    [SerializeField] private Slider _slider;

    public string selfName;

    public bool AwakeWasCalledAlready { get; set; }
    public bool StartWasCalledAlready { get; set; }

    public float CurrentValue
    {
        get { return _currentValue; }
        set
        {
            _currentValue = value;
            _slider.value = value;
            object[] parameters = new object[] { value, (RectTransform)transform };
            StaticClassForAdditionalFunctions.CallFunctionByName(_nameInvokingFunction, EventBus.Instance, parameters);
        }
    }

    public void Awake()
    {
        if (!AwakeWasCalledAlready)
        {
            selfName = gameObject.name;
            _nameInvokingFunction = C.Prefixes.PrefixTrigger + selfName;
            AwakeWasCalledAlready = true;

            _verticalGroup = GetComponent<VerticalLayoutGroup>();

            ////Debug.Log(_slider);
            ValueOfSliderWasChanged();
        }
    }

    private void OnEnable()
    {
        ////Debug.Log("Ну и?");
        StaticClassForAdditionalFunctions.RefreshLayoutForGroups(this, _verticalGroup);
    }

    public void ValueOfSliderWasChanged()
    {
        ////Debug.Log(_slider);
        ////Debug.Log(_slider.value);
        ////Debug.Log(CurrentValue);
        CurrentValue = _slider.value;
    }
}
