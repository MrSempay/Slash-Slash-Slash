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

    public bool WasDestroyed
    {
        get { return _wasDestroyed; }
        set
        {
            if (value != _wasDestroyed)
            {
                _wasDestroyed = value;

                if (value) ScenarioScript.instance.RemoveMainTarget(this);
                else ScenarioScript.instance.AddMainTargetNotPlayer(this);

            }
        }
    }
    public bool IsMainTarget { get { return _isMainTarget; } set { _isMainTarget = value; } }
    public Transform targetTransform { get { return transform; } }

    public void SetLikeAMainTarget()
    {
        if (IsMainTarget)
        {
            LevelBuilder.instance.listMainTargets.Add(this);
            ScenarioScript.instance.AddMainTargetNotPlayer(this);
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

    public override void UpdateAssortmentInBuilding(RectTransform rectTransformEquipmentPlaces)
    {
        //List<EquipmentChance> randomCategoryAndRarityTypesOfEquipment = GenerateItems(rectTransformEquipmentPlaces.childCount); 
        int i = 0;
        onUpdateAssortment?.Invoke(null, this);
        foreach (Equipment equipment in equipmentInBuilding)
        {
            if (equipment) Destroy(equipment.gameObject);
        }
        equipmentInBuilding.Clear();
        foreach (RectTransform placeForEquipment in rectTransformEquipmentPlaces)
        {

            string randomEquipmentName = null;
            EquipmentChance randomCategoryAndRarityTypesOfEquipment = default;
            while (randomEquipmentName == null)
            {
                randomCategoryAndRarityTypesOfEquipment = GenerateItems(1)[0];
                randomEquipmentName = GetRandomAmmunitionName(randomCategoryAndRarityTypesOfEquipment);
            }

            // —ќ«ƒј®ћ ќЅЏ≈ “ —Ќј–я∆≈Ќ»я, ѕќЋ”„ј≈ћ ≈√ќ »ћя, RectTransform, —ѕј¬Ќ»ћ ” «јƒјЌЌќ√ќ –ќƒ»“≈Ћя (ћ≈—“ј —Ќј–я∆≈Ќ»я)
            GameObject newEquipment = Instantiate(GameManager.Instance.prefubAmmunition, Vector3.zero, Quaternion.identity);
            RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
            newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - чтобы не сохран€ть мировые координаты (позицию, масштаб, поворот)

            // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ RectTransform ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
            newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchoredPosition = Vector2.zero; // ”станавливаем смещение относительно €корей в (0, 0) 
            newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
            newEquipmentRectTransform.name = randomEquipmentName;

            // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ SpriteRenderer ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
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
            scriptOfEquipment.isEquipmentASpell = false; // пока что дл€ спелов только
            scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
            scriptOfEquipment.BuildingWhereEquipmentIs = this;
            scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = rectTransformTargetEquipmentPanelPlayer;
            scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;
            scriptOfEquipment.categoryAndRarityTypesOfEquipment = randomCategoryAndRarityTypesOfEquipment;


            // »«ћ≈Ќя≈ћ ѕј–јћ≈“–џ «ƒјЌ»я ѕ–» ƒќЅј¬Ћ≈Ќ»» ¬ Ќ≈√ќ Ќќ¬ќ√ќ —Ќј–я∆≈Ќ»я
            equipmentInBuilding.Add(scriptOfEquipment);
            PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
            scriptOfPlace.Equipment = scriptOfEquipment;
            scriptOfPlace.isBuildingPlace = true;

            //scriptOfEquipment.Awake();
            //scriptOfEquipment.Start();

            i++;


        }
        onUpdateAssortment?.Invoke(equipmentInBuilding, this); // подписываемс€ на событие в ScenarioScript, чтоб знать, когда был обновлЄн ассортимент в здании
    }


    // —тоит отметить, что тут мы получаем префаб снар€жени€ не из базового класса Building, а из GameManager, где его получаем в начале игры и сохран€ем ссылку. ¬ отличии от функции выше.
    // ѕытаемс€ изменить функции выше по этому же подобию.
    public static void SpawnParticularAmmunition(string nameAmmunition, IInventory inventory, Treasury buildingWhereEquipmentIs = null)
    {
        foreach (RectTransform placeForEquipment in inventory.Inventory.rectTransformAmmunitionPanel)
        {
                
            if (placeForEquipment.childCount == 2) // если 2 дочерних элемента, то это текстовые пол€ дл€ цены и названи€ снар€жени€. “о есть поле пустое, ибо иначе дочерних > 2
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
                    Debug.LogError("јммуниции с таким названием не было найдено!");
                    return default;
                }

                EquipmentChance equipmentChance = FindEquipment();

                

                // —ќ«ƒј®ћ ќЅЏ≈ “ —Ќј–я∆≈Ќ»я, ѕќЋ”„ј≈ћ ≈√ќ »ћя, RectTransform, —ѕј¬Ќ»ћ ” «јƒјЌЌќ√ќ –ќƒ»“≈Ћя (ћ≈—“ј —Ќј–я∆≈Ќ»я)
                GameObject newEquipment = Instantiate(GameManager.Instance.prefubAmmunition, Vector3.zero, Quaternion.identity);
                RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
                newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - чтобы не сохран€ть мировые координаты (позицию, масштаб, поворот)

                // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ RectTransform ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
                newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchoredPosition = Vector2.zero; // ”станавливаем смещение относительно €корей в (0, 0) 
                newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
                newEquipmentRectTransform.name = nameAmmunition;

                // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ SpriteRenderer ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
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
                if (equipmentSprite) scriptOfEquipment.sprite = equipmentSprite; // если вдруг тут спрайт не найдЄм по имени, то в Awake дл€ снар€жени€ по умолчанию проставилс€ текущий из инспектора
                scriptOfEquipment.isEquipmentASpell = false; // пока что дл€ спелов только
                scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
                scriptOfEquipment.BuildingWhereEquipmentIs = buildingWhereEquipmentIs;
                scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = GameObject.Find((string)AdjustBuildingParameters.buildingParameters[C.DK.Treasury][C.DK.NameTargetEquipmentPanelPlayer]).GetComponent<RectTransform>();
                scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;
                scriptOfEquipment.categoryAndRarityTypesOfEquipment = equipmentChance;

                PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
                scriptOfPlace.Equipment = scriptOfEquipment;

                // »«ћ≈Ќя≈ћ ѕј–јћ≈“–џ «ƒјЌ»я ѕ–» ƒќЅј¬Ћ≈Ќ»» ¬ Ќ≈√ќ Ќќ¬ќ√ќ —Ќј–я∆≈Ќ»я
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
            buildingWhereEquipmentIs.onUpdateAssortment?.Invoke(buildingWhereEquipmentIs.equipmentInBuilding, buildingWhereEquipmentIs); // подписываемс€ на событие в ScenarioScript, чтоб знать, когда был обновлЄн ассортимент в здании
        }
    }




}
