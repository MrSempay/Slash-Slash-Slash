using System;
using System.Collections.Generic;
using UnityEngine;
using static StaticClassForAdditionalFunctions;
using static UnityEngine.Rendering.DebugUI;

// класс просто нужен для того, чтоб из SettubgsMenu задетектить, что очередная прожатая кнопка (односторонняя) находится в группке. Вся логика там.
public class TogglesGroup : MonoBehaviour, IControlLifeCicleFunctions
{
    private ToggleFixedOneWay _toggleFixedOneWayLast;
    private string _nameInvokingFunction;

    public bool AwakeWasCalledAlready { get; set; }
    public bool StartWasCalledAlready { get; set; }

    public void Awake()
    {
        if (!AwakeWasCalledAlready)
        { 
            _nameInvokingFunction = "Trigger" + gameObject.name;
            AwakeWasCalledAlready = true;   
            //AssignParametersAndProperties(AdjustSettingsParameters.settingsParameters[name], this);
        }
    }

    public void ControllOnlyOneToggledToggle(ToggleFixedOneWay toggleFixedOneWay)
    {
        if (_toggleFixedOneWayLast != toggleFixedOneWay)
        {
            if (_toggleFixedOneWayLast != null)
            {
                _toggleFixedOneWayLast.IsToggled = false;
            }
            _toggleFixedOneWayLast = toggleFixedOneWay;
        }
    }

    // у нас будут также функции для группы тумблеров, чтоб при любом вжатии тумблера в рамках данной группы эмулировалась одна функция, но передавались бы разные
    // параметры уже в зависимости от того, какой именно тумблер был вжат
    public void InvokeGroupFunction(ToggleFixed scriptToggle)
    {
        LANGUAGE foundLanguage;
        string nameToggle = scriptToggle.selfName;
        object[] parameters = null;

        if (Enum.TryParse(nameToggle, out foundLanguage))
        {
            parameters = new object[] { foundLanguage, (RectTransform)transform };
        }
        CallFunctionByName(_nameInvokingFunction, EventBus.Instance, parameters);
    }
}
