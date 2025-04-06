using System;
using System.Collections.Generic;
using UnityEngine;
using static AdjustEquipmentParameters;

public class Treasury : Building, IMainTarget
{

#region IMainTarget

    private bool _wasDestroyed;

    [SerializeField] private bool _isMainTarget;

    public bool WasDestroyed { get { return _wasDestroyed; } set { _wasDestroyed = value; } }
    public bool IsMainTarget { get { return _isMainTarget; } set { _isMainTarget = value; } }

    public void SetLikeAMainTarget()
    {
        if (IsMainTarget)
        {
            LevelBuilder.instance.listMainTargets.Add(this);
        }
    }

#endregion

    public new event Action<List<Equipment>, Building> onUpdateAssortment;

    protected override void Awake()
    {
        nameOfObject = "Treasury";
        base.Awake();
    }

    protected override void Start()
    {
        SetLikeAMainTarget();
        base.Start();
    }

    protected override void UpdateAssortmentInBuilding(RectTransform rectTransformEquipmentPlaces)
    {
        List<EquipmentChance> randomCategoryAndRarityTypesOfEquipment = GenerateItems(rectTransformEquipmentPlaces.childCount);
        onUpdateAssortment?.Invoke(null, this);
        int i = 0;
        foreach (Equipment equipment in equipmentInBuilding)
        {
            if (equipment) Destroy(equipment.gameObject);
        }
        equipmentInBuilding.Clear();
        foreach (RectTransform placeForEquipment in rectTransformEquipmentPlaces)
        {
            // СОЗДАЁМ ОБЪЕКТ СНАРЯЖЕНИЯ, ПОЛУЧАЕМ ЕГО ИМЯ, RectTransform, СПАВНИМ У ЗАДАННОГО РОДИТЕЛЯ (МЕСТА СНАРЯЖЕНИЯ)
            GameObject newEquipment = Instantiate(prefubOfEquipment, Vector3.zero, Quaternion.identity);
            RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
            string randomEquipmentName = GetRandomAmmunitionName(randomCategoryAndRarityTypesOfEquipment[i]);
            newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - чтобы не сохранять мировые координаты (позицию, масштаб, поворот)

            // НАСТРАИВАЕМ КОМПОНЕНТ RectTransform У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
            newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchoredPosition = Vector2.zero; // Устанавливаем смещение относительно якорей в (0, 0) 
            newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
            newEquipmentRectTransform.name = randomEquipmentName;

            // НАСТРАИВАЕМ КОМПОНЕНТ SpriteRenderer У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
            SpriteRenderer spriteRenderer = newEquipment.GetComponent<SpriteRenderer>();
            string fullPath = folderImagesOfEquipment + randomEquipmentName;
            Sprite spellSprite = Resources.Load<Sprite>(fullPath);
            //spriteRenderer.sprite = spellSprite;

            // НАСТРАИВАЕМ КОМПОНЕНТ Equipment (СОБСНА ЕГО СКРИПТ) У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
            Ammunition scriptOfEquipment = newEquipment.GetComponent<Ammunition>();
            scriptOfEquipment.Awake();
            scriptOfEquipment.equipmentName = randomEquipmentName;
            scriptOfEquipment.sprite = spellSprite;
            scriptOfEquipment.isEquipmentASpell = false; // пока что для спелов только
            scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
            scriptOfEquipment.BuildingWhereEquipmentIs = this;
            scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = rectTransformTargetEquipmentPanelPlayer;
            scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;
            scriptOfEquipment.categoryAndRarityTypesOfEquipment = randomCategoryAndRarityTypesOfEquipment[i];


            // ИЗМЕНЯЕМ ПАРАМЕТРЫ ЗДАНИЯ ПРИ ДОБАВЛЕНИИ В НЕГО НОВОГО СНАРЯЖЕНИЯ
            equipmentInBuilding.Add(scriptOfEquipment);
            PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
            scriptOfPlace.Equipment = scriptOfEquipment;
            scriptOfPlace.isBuildingPlace = true;

            i++;

        }
        onUpdateAssortment?.Invoke(equipmentInBuilding, this); // подписываемся на событие в ScenarioScript, чтоб знать, когда был обновлён ассортимент в здании
    }


    public List<EquipmentChance> possibleItems =  new List<EquipmentChance>(allEquipmentTypesAndCategoriesChance);

    public List<EquipmentChance> GenerateItems(int numberOfItems)
    {
        List<EquipmentChance> generatedItems = new List<EquipmentChance>();

        // 1. Нормализуем вероятности (если они не нормализованы)
        float totalChance = 0;
        foreach (var item in possibleItems)
        {
            totalChance += item.chance;
        }

        if (Math.Abs(totalChance - 100) > 0.01f) // Проверяем, что сумма близка к 100
        {
            Debug.LogWarning("Сумма вероятностей не равна 100%. Нормализуем...");
            // Нормализуем
            float normalizationFactor = 100f / totalChance;
            for (int i = 0; i < possibleItems.Count; i++)
            {
                EquipmentChance item = possibleItems[i];
                item.chance *= normalizationFactor;
                possibleItems[i] = item;
            }
        }

        // 2. Генерируем предметы
        for (int i = 0; i < numberOfItems; i++)
        {
            float randomValue = UnityEngine.Random.Range(0f, 100f);
            float cumulativeChance = 0f;
            EquipmentChance selectedItem = default;

            foreach (var item in possibleItems)
            {
                //Debug.Log(item.chance);
                cumulativeChance += item.chance;
                if (randomValue <= cumulativeChance)
                {
                    selectedItem = item;
                    break; // Выбираем первый подходящий предмет
                }
            }

            if (selectedItem.equipmentCategory != null) // Проверяем, что предмет был выбран (selectedItem не остался default)
            {
                generatedItems.Add(selectedItem);
            }
            else
            {
                Debug.LogError("Не удалось выбрать предмет! Проверьте вероятности.");
            }
        }

        return generatedItems;
    }


}
