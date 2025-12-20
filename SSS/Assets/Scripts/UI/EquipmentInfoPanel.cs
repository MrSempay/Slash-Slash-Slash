using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentInfoPanel : MonoBehaviour
{
    private static FieldInfo _fieldInfoPrefub;

    [SerializeField] private Image _iconEquipment; 
    [SerializeField] private TextEdit _textDescription;
    [SerializeField] private TextEdit _textRarity;
    [SerializeField] private TextEdit _textNameEquipment;
    [SerializeField] private RectTransform _rectTransformParameterFieldsPlace;

    private List<string> _listEquipmentParametersForVisualization = new List<string> { C.DK.cost, C.DK.timeCallDown, C.DK.amountBlockingAttackMax, C.DK.damage, C.DK.durationActiveState,
                                                                                        C.DK.healthHealAmount, C.DK.CurrentIncreasingStamina, C.DK.damage, C.DK.healthMax, C.DK.CurrentIncreasingStamina,
                                                                                    C.DK.jumpForce, C.DK.stuneChanceByStandardAttackPercentage, C.DK.DamageReductionPercentage, C.DK.evasionPercentage};
    // Короче, у нас в параметре C.DK.increasingUnitParametersByAmmunitionAbsolute могут быть параметры, которые увеличиваются хоть и на абсолютное значение, но это ПРОЦЕНТЫ. Поэтому чтобы
    // детектить их создаём этот список. Для параметрво в этом списке мы добавляем "%" в конце визуализационного поля
    // Вообще, чём это всё бурдовый подход. Нужно сделать класс-раширение для всех этих параметров и в них назначать доп. параметры для вывода
    private List<string> _listPercentageParameters = new List<string> { C.DK.CurrentIncreasingStamina, C.DK.stuneChanceByStandardAttackPercentage, C.DK.DamageReductionPercentage, 
                                                                        C.DK.evasionPercentage }; 



    void Awake()
    {
        if (_fieldInfoPrefub == null)
        {
            _fieldInfoPrefub = Resources.Load<FieldInfo>(C.Paths.PrefubFieldEquipmentInfo);
        }
    }

    public EquipmentInfoPanel FillInfoForm(Equipment equipment)
    {
        _textNameEquipment.Text = equipment.equipmentName;
        _textDescription.Text = equipment.equipmentName + C.Prefixes.Description;
        _iconEquipment.sprite = equipment.sprite;


        if (equipment.isEquipmentASpell)
        {
            Spell spellScript = (Spell)equipment;

            foreach (var parameter in AdjustEquipmentParameters.spellParameters[spellScript.equipmentName])
            {
                if (_listEquipmentParametersForVisualization.Contains(parameter.Key))
                {
                    FieldInfo fieldInfo = Instantiate(_fieldInfoPrefub, _rectTransformParameterFieldsPlace, false);
                    fieldInfo.textNameInfo.Text = parameter.Key;
                    if (parameter.Key == C.DK.durationActiveState)
                    {
                        if ((float)parameter.Value != -1)
                        {
                            fieldInfo.textValueInfo.SetNotLocalizableText(parameter.Value.ToString());
                        }
                        else
                        {
                            fieldInfo.textValueInfo.Text = C.Just.Infinite;
                        }
                        continue;
                    }
                    fieldInfo.textValueInfo.SetNotLocalizableText(parameter.Value.ToString());
                }
                if (parameter.Key == C.DK.increasingUnitParametersByAmmunitionPercentageByCast)
                {
                    Dictionary<string, float> parametersPercentage = (Dictionary<string, float>)AdjustEquipmentParameters.spellParameters[spellScript.equipmentName][C.DK.increasingUnitParametersByAmmunitionPercentageByCast];
                    foreach (var increasunParameter in parametersPercentage)
                    {
                        FieldInfo fieldInfo = Instantiate(_fieldInfoPrefub, _rectTransformParameterFieldsPlace, false);
                        fieldInfo.textNameInfo.Text = increasunParameter.Key;
                        fieldInfo.textValueInfo.SetNotLocalizableText("+" + increasunParameter.Value.ToString() + "%");
                    }
                }
            }
        }
        else
        {
            Ammunition ammunitionScript = (Ammunition)equipment;

            string equipmentRarityType = ammunitionScript.categoryAndRarityTypesOfEquipment.equipmentRarityType;
            string equipmentCategory = ammunitionScript.categoryAndRarityTypesOfEquipment.equipmentCategory;

            _textRarity.Text = equipmentRarityType;

            foreach (var parameter in AdjustEquipmentParameters.ammunitionParameters[equipmentCategory][equipmentRarityType][ammunitionScript.equipmentName])
            {
                if (parameter.Key == C.DK.increasingUnitParametersByAmmunitionPercentage)
                {
                    Dictionary<string, float> parametersPercentage = (Dictionary<string, float>) AdjustEquipmentParameters.ammunitionParameters[equipmentCategory][equipmentRarityType][ammunitionScript.equipmentName][C.DK.increasingUnitParametersByAmmunitionPercentage];
                    foreach (var parameterPercentage in parametersPercentage)
                    {
                        if (_listEquipmentParametersForVisualization.Contains(parameterPercentage.Key))
                        {
                            FieldInfo fieldInfo = Instantiate(_fieldInfoPrefub, _rectTransformParameterFieldsPlace, false); 
                            fieldInfo.textNameInfo.Text = parameterPercentage.Key;
                            if (parameterPercentage.Value >= 0)
                            {
                                fieldInfo.textValueInfo.SetNotLocalizableText("+" + parameterPercentage.Value.ToString() + "%");
                            }
                            else
                            {
                                fieldInfo.textValueInfo.SetNotLocalizableText(parameterPercentage.Value.ToString() + "%"); // тут уже будет минус бо parameterPercentage.Value отрицательно
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Параметр, который не указан к выводу на панель!");
                        }
                    }
                    continue;
                }
                if (parameter.Key ==  C.DK.increasingUnitParametersByAmmunitionAbsolute)
                {
                    Dictionary<string, float> parametersAbsolute = (Dictionary<string, float>) AdjustEquipmentParameters.ammunitionParameters[equipmentCategory][equipmentRarityType][ammunitionScript.equipmentName][C.DK.increasingUnitParametersByAmmunitionAbsolute];
                    foreach (var parameterAbsolute in parametersAbsolute)
                    {
                        if (_listEquipmentParametersForVisualization.Contains(parameterAbsolute.Key))
                        {
                            FieldInfo fieldInfo = Instantiate(_fieldInfoPrefub, _rectTransformParameterFieldsPlace, false);
                            fieldInfo.textNameInfo.Text = parameterAbsolute.Key;
                            if (parameterAbsolute.Value >= 0)
                            {
                                if (_listPercentageParameters.Contains(parameterAbsolute.Key))
                                {
                                    fieldInfo.textValueInfo.SetNotLocalizableText("+" + parameterAbsolute.Value.ToString() + "%");
                                    continue;
                                }
                                fieldInfo.textValueInfo.SetNotLocalizableText("+" + parameterAbsolute.Value.ToString());
                            }
                            else
                            {
                                if (_listPercentageParameters.Contains(parameterAbsolute.Key))
                                {
                                    fieldInfo.textValueInfo.SetNotLocalizableText(parameterAbsolute.Value.ToString() + "%");
                                    continue;
                                }
                                fieldInfo.textValueInfo.SetNotLocalizableText(parameterAbsolute.Value.ToString());
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Параметр, который не указан к выводу на панель!");
                        }
                    }
                    continue;
                }
                if (parameter.Key ==  C.DK.increasingUnitParametersByAmmunitionPercentageByCast)
                {
                    Dictionary<string, float> parameters = (Dictionary<string, float>) AdjustEquipmentParameters.ammunitionParameters[equipmentCategory][equipmentRarityType][ammunitionScript.equipmentName][C.DK.increasingUnitParametersByAmmunitionPercentageByCast];
                    foreach (var parameterIncrease in parameters)
                    {
                        if (_listEquipmentParametersForVisualization.Contains(parameterIncrease.Key))
                        {
                            FieldInfo fieldInfo = Instantiate(_fieldInfoPrefub, _rectTransformParameterFieldsPlace, false);
                            fieldInfo.textNameInfo.Text = parameterIncrease.Key;
                            if (parameterIncrease.Value >= 0)
                            {
                                fieldInfo.textValueInfo.SetNotLocalizableText("+" + parameterIncrease.Value.ToString() + "%");
                            }
                            else
                            {
                                fieldInfo.textValueInfo.SetNotLocalizableText(parameterIncrease.Value.ToString() + "%");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Параметр, который не указан к выводу на панель!");
                        }
                    }
                    continue;
                }

                if (true) // вот просто чтоб одинаковое имя для fieldInfo не багало вышенаписанные объявления 
                {
                    FieldInfo fieldInfo = Instantiate(_fieldInfoPrefub, _rectTransformParameterFieldsPlace, false); // это черевато багами, если мы таки не найдём параметр для вывода на панель
                    fieldInfo.textNameInfo.Text = parameter.Key;
                    if (parameter.Key == C.DK.durationActiveState)
                    {
                        float value = Convert.ToSingle(parameter.Value);
                        if (value != -1f)
                        {
                            fieldInfo.textValueInfo.SetNotLocalizableText(value.ToString());
                        }
                        else
                        {
                            fieldInfo.textValueInfo.Text = C.Just.Infinite;
                        }
                        continue;
                    }
                    fieldInfo.textValueInfo.SetNotLocalizableText(parameter.Value.ToString()); 
                }
            }
        }

        return this;
    }

    private void Update()
    {
        //Canvas.ForceUpdateCanvases();
        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private void OnEnable()
    {
        CoroutineManager.Instance.StartManagedCoroutine(gameObject, RebuildNextFrame((RectTransform)transform));
    }


    private IEnumerator RebuildNextFrame(RectTransform rectTransformPIP)
    {
        yield return null; // ждём конца кадра
        yield return null; // ждём конца кадра
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransformPIP);
    }

}
