using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ParameterChoseList : ParameterFieldSettings, IControlLifeCicleFunctions
{
    private string _currentTextValue;
    private string _nameInvokingFunction;

    [SerializeField] private TextEdit _textButton;
    
    [NonSerialized] public int _indexCurrentString = 0;

    public string _baseStringOfText;
    public List<object> listChosing;
    public string selfName;

    public bool awakeWasCalledAlready { get; set; }

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

    public void Awake()
    {
        //Debug.Log("3");
        if (!awakeWasCalledAlready)
        {
            _textButton.Awake();
            //Debug.Log("1");
            selfName = gameObject.name;
            StaticClassForAdditionalFunctions.AssignParametersAndProperties(AdjustSettingsParameters.settingsParameters[selfName], this);
            _nameInvokingFunction = C.Prefixes.PrefixTrigger + selfName;
            SetValue();
            awakeWasCalledAlready = true;
        }

    }

    public void OnParameterButtonClick()
    {
        SetValue();
    }

    public void SetValue()
    {
        //Debug.Log("2");
        CurrentTextValue = listChosing[_indexCurrentString].ToString();

        object[] parameters = new object[] { listChosing[_indexCurrentString], (RectTransform)transform };
        StaticClassForAdditionalFunctions.CallFunctionByName(_nameInvokingFunction, EventBus.Instance, parameters);
        _indexCurrentString++;
        if (_indexCurrentString == listChosing.Count) _indexCurrentString = 0;
    }
}