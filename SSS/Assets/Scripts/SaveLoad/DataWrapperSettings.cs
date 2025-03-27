using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StaticClassForAdditionalFunctions;


[System.Serializable]
public class DataWrapperSettings
{
    public List<DataWrapperToggle> allTogglesData;

    public List<DataWrapperSlider> allSlidersData;

    public List<DataWrapperChoseList> allChoseListsData;

    [System.Serializable]

    // ИМЕНА ДОЛЖНЫ ТОЧНО СООТВЕТСТВОВАТЬ ПОЛЯМ/СВОЙСТВАМ серриализуемого класса!
    public class DataWrapperToggle : Wrapper
    {
        public bool IsToggled;
        public ENUM currentLanguage;

        // вроде пока что через конструктор ничего не определяем, используем рефлексию
        public DataWrapperToggle(string nameToggleOrHisParentParam = null, bool isToggledParam = false)
        {
            selfName = nameToggleOrHisParentParam;
            IsToggled = isToggledParam;
        }

    }

    [System.Serializable]
    public class DataWrapperSlider : Wrapper
    {
        public float CurrentValue;
    }
    [System.Serializable]
    public class DataWrapperChoseList : Wrapper
    {
        public string CurrentTextValue;
        public int _indexCurrentString;
    }

    // проверяем, является ли объект девственным (чтоб, если чего вдруг, не пытались мы загружать настройки из такового
    public bool IsPristine()
    {
        return (allTogglesData == null || allTogglesData.Count == 0) && 
               (allSlidersData == null || allSlidersData.Count == 0) && 
               (allChoseListsData == null || allChoseListsData.Count == 0);
    }
}
