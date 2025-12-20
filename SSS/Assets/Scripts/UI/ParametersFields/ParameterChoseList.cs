using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using static StaticClassForAdditionalFunctions;

public class ParameterChoseList : ParameterFieldSettings, IControlLifeCicleFunctions
{
    private string _currentTextValue;
    private LANGUAGE _currentValue;
    private string _nameInvokingFunction;

    [SerializeField] private TextEdit _textButton;
    
    [NonSerialized] public int _indexCurrentString = 0;

    public string _baseStringOfText;
    public List<LANGUAGE> listChosing;
    public string selfName;

    public bool AwakeWasCalledAlready { get; set; }
    public bool StartWasCalledAlready { get; set; }

    public string CurrentTextValue
    {
        get { return _currentTextValue; }
        set
        {
            _currentTextValue = value;
            _textButton.Text = value; // типа чтоб какую-нибудь приставку могли писать, а изменять только смысловое слово, по типу:
                                      // "ENUM: " + "Horizontal"
        }
    }

    public LANGUAGE CurrentValue
    {
        get { return _currentValue; }
        set
        {
            _currentValue = value;
            object[] parameters = new object[] { value, (RectTransform)transform };
            CallFunctionByName(_nameInvokingFunction, EventBus.Instance, parameters);
        }
    }

    public void Awake()
    {
        //Debug.Log("3");
        if (!AwakeWasCalledAlready)
        {
            _textButton.Awake();
            //Debug.Log("1");
            selfName = gameObject.name;
            StaticClassForAdditionalFunctions.AssignParametersAndProperties(AdjustSettingsParameters.settingsParameters[selfName], this);
            _nameInvokingFunction = C.Prefixes.PrefixTrigger + selfName;
            SetValue();
            AwakeWasCalledAlready = true;
        }

    }

    public void OnParameterButtonClick()
    {
        SetValue();
    }

    private bool _lock;
    public void SetValue()
    {
        if (!_lock) // чтоб из этого метода этот же не вызывать, если мы из этого класса его вызываем через CurrentValue
        {
            _lock = true;
            //Debug.Log("2");
            CurrentTextValue = listChosing[_indexCurrentString].ToString();
            CurrentValue = listChosing[_indexCurrentString]; // при вызове этого метода из currentSettings этот вызов не пройдёт, так как у нас стоит проверка на то же значение
            _indexCurrentString++;
            if (_indexCurrentString == listChosing.Count) _indexCurrentString = 0;
            _lock = false;


        }
    }
}