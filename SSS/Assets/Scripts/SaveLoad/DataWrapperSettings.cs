using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DataWrapperSettings
{
    public List<DataWrapperToggle> allTogglesData;

    [System.Serializable]

    // ИМЕНА ДОЛЖНЫ ТОЧНО СООТВЕТСТВОВАТЬ ПОЛЯМ/СВОЙСТВАМ серриализуемого класса!
    public class DataWrapperToggle
    {
        public string nameToggle;
        public bool IsToggled;

        public DataWrapperToggle(string nameToggleOrHisParentParam = null, bool isToggledParam = false)
        {
            nameToggle = nameToggleOrHisParentParam;
            IsToggled = isToggledParam;
        }

    }

    // проверяем, является ли объект девственным (чтоб, если чего вдруг, не пытались мы загружать настройки из такового
    public bool IsPristine()
    {
        return allTogglesData == null || allTogglesData.Count == 0;
    }
}
