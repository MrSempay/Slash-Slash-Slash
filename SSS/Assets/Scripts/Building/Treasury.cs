using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AdjustEquipmentParameters;

public class Treasury : Building, IMainTarget
{

#region IMainTarget

    private bool _wasDestroyed;

    [SerializeField] private bool _isMainTarget = true;

    public bool WasDestroyed { get { return _wasDestroyed; } set { _wasDestroyed = value; } }
    public bool IsMainTarget { get { return _isMainTarget; } set { _isMainTarget = value; } }
    public Transform targetTransform { get { return transform; } }

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
        selfName = "Treasury";
        base.Awake();
    }

    protected override void Start()
    {
        SetLikeAMainTarget();
        base.Start();
        //Treasury.SpawnParticularAmmunition(C.DK.PlateArmor, )
    }

    protected override void UpdateAssortmentInBuilding(RectTransform rectTransformEquipmentPlaces)
    {
        List<EquipmentChance> randomCategoryAndRarityTypesOfEquipment = GenerateItems(rectTransformEquipmentPlaces.childCount); 
        int i = 0;
        onUpdateAssortment?.Invoke(null, this);
        foreach (Equipment equipment in equipmentInBuilding)
        {
            if (equipment) Destroy(equipment.gameObject);
        }
        equipmentInBuilding.Clear();
        foreach (RectTransform placeForEquipment in rectTransformEquipmentPlaces)
        {
            string randomEquipmentName = GetRandomAmmunitionName(randomCategoryAndRarityTypesOfEquipment[i]);
            if (randomEquipmentName == null) // это значит, что в заданной категории и редкости нет ни одного предмета!!! Функция GetRandomAmmunitionName не нашла ни одного имени там!!!
            {
                continue;
            }

            // СОЗДАЁМ ОБЪЕКТ СНАРЯЖЕНИЯ, ПОЛУЧАЕМ ЕГО ИМЯ, RectTransform, СПАВНИМ У ЗАДАННОГО РОДИТЕЛЯ (МЕСТА СНАРЯЖЕНИЯ)
            GameObject newEquipment = Instantiate(GameManager.Instance.prefubAmmunition, Vector3.zero, Quaternion.identity);
            RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
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
            Sprite equipmentSprite = Resources.Load<Sprite>(fullPath);

            Ammunition scriptOfEquipment;

            if (customScriptsEquipment.ContainsKey(randomEquipmentName))
            {
                //Debug.Log("shit");

                scriptOfEquipment = (Ammunition)newEquipment.AddComponent(customScriptsEquipment[randomEquipmentName]);

                //Debug.Log(scriptOfEquipment.GetInstanceID());
            }
            else
            {
                scriptOfEquipment = (Ammunition)newEquipment.AddComponent(typeof(Ammunition));
            }
            scriptOfEquipment.Awake();
            scriptOfEquipment.equipmentName = randomEquipmentName;
            if (equipmentSprite) scriptOfEquipment.sprite = equipmentSprite;
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

            //scriptOfEquipment.Awake();
            //scriptOfEquipment.Start();

            i++;


        }
        onUpdateAssortment?.Invoke(equipmentInBuilding, this); // подписываемся на событие в ScenarioScript, чтоб знать, когда был обновлён ассортимент в здании
    }


    // Стоит отметить, что тут мы получаем префаб снаряжения не из базового класса Building, а из GameManager, где его получаем в начале игры и сохраняем ссылку. В отличии от функции выше.
    // Пытаемся изменить функции выше по этому же подобию.
    public static void SpawnParticularAmmunition(string nameAmmunition, IInventory inventory, Treasury buildingWhereEquipmentIs = null)
    {
        foreach (RectTransform placeForEquipment in inventory.Inventory.rectTransformAmmunitionPanel)
        {
                
            if (placeForEquipment.childCount == 2) // если 2 дочерних элемента, то это текстовые поля для цены и названия снаряжения. То есть поле пустое, ибо иначе дочерних > 2
            {
                //Debug.Log("B<JJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ");
                EquipmentChance FindEquipment()
                {
                    foreach (var category in AdjustEquipmentParameters.ammunitionParameters)
                    {
                        foreach (var rarityType in category.Value)
                        {
                            foreach (var ammunition in rarityType.Value)
                            {
                                //Debug.Log(ammunition.Key);
                                if (nameAmmunition == ammunition.Key)
                                {
                                    return new EquipmentChance() 
                                    {
                                        equipmentCategory = category.Key,
                                        equipmentRarityType = rarityType.Key
                                    };
                                }
                            }
                        }
                    }
                    Debug.LogError("Аммуниции с таким названием не было найдено!");
                    return default;
                }

                EquipmentChance equipmentChance = FindEquipment();

                

                // СОЗДАЁМ ОБЪЕКТ СНАРЯЖЕНИЯ, ПОЛУЧАЕМ ЕГО ИМЯ, RectTransform, СПАВНИМ У ЗАДАННОГО РОДИТЕЛЯ (МЕСТА СНАРЯЖЕНИЯ)
                GameObject newEquipment = Instantiate(GameManager.Instance.prefubAmmunition, Vector3.zero, Quaternion.identity);
                RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
                newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - чтобы не сохранять мировые координаты (позицию, масштаб, поворот)

                // НАСТРАИВАЕМ КОМПОНЕНТ RectTransform У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
                newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchoredPosition = Vector2.zero; // Устанавливаем смещение относительно якорей в (0, 0) 
                newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
                newEquipmentRectTransform.name = nameAmmunition;

                // НАСТРАИВАЕМ КОМПОНЕНТ SpriteRenderer У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
                SpriteRenderer spriteRenderer = newEquipment.GetComponent<SpriteRenderer>();
                string fullPath = C.Paths.FolderImagesForAmmunition + nameAmmunition;
                Sprite equipmentSprite = Resources.Load<Sprite>(fullPath);

                Ammunition scriptOfEquipment;
                Dictionary<string, Type> customScriptsEquipment = (Dictionary<string, Type>)AdjustBuildingParameters.buildingParameters[C.DK.Treasury][C.DK.customScriptsEquipment];
                if (customScriptsEquipment.ContainsKey(nameAmmunition))
                {
                    //Debug.Log("shit");

                    scriptOfEquipment = (Ammunition)newEquipment.AddComponent(customScriptsEquipment[nameAmmunition]);

                    //Debug.Log(scriptOfEquipment.GetInstanceID());
                }
                else
                {
                    scriptOfEquipment = (Ammunition)newEquipment.AddComponent(typeof(Ammunition));
                }
                scriptOfEquipment.Awake();
                scriptOfEquipment.equipmentName = nameAmmunition;
                scriptOfEquipment.ownerUnit = inventory.UnitSelf;
                if (equipmentSprite) scriptOfEquipment.sprite = equipmentSprite; // если вдруг тут спрайт не найдём по имени, то в Awake для снаряжения по умолчанию проставился текущий из инспектора
                scriptOfEquipment.isEquipmentASpell = false; // пока что для спелов только
                scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
                scriptOfEquipment.BuildingWhereEquipmentIs = buildingWhereEquipmentIs;
                scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = GameObject.Find((string)AdjustBuildingParameters.buildingParameters[C.DK.Treasury][C.DK.NameTargetEquipmentPanelPlayer]).GetComponent<RectTransform>();
                scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;
                scriptOfEquipment.categoryAndRarityTypesOfEquipment = equipmentChance;

                PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
                scriptOfPlace.Equipment = scriptOfEquipment;

                // ИЗМЕНЯЕМ ПАРАМЕТРЫ ЗДАНИЯ ПРИ ДОБАВЛЕНИИ В НЕГО НОВОГО СНАРЯЖЕНИЯ
                if (buildingWhereEquipmentIs)
                {
                    buildingWhereEquipmentIs.equipmentInBuilding.Add(scriptOfEquipment);
                    scriptOfPlace.isBuildingPlace = true;
                }

                //scriptOfEquipment.Awake();
                //scriptOfEquipment.Start();

                return;
            }
        }
        if (buildingWhereEquipmentIs)
        {
            buildingWhereEquipmentIs.onUpdateAssortment?.Invoke(buildingWhereEquipmentIs.equipmentInBuilding, buildingWhereEquipmentIs); // подписываемся на событие в ScenarioScript, чтоб знать, когда был обновлён ассортимент в здании
        }
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
