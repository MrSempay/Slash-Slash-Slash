using System.Collections.Generic;
using System;
using UnityEngine;
using static AdjustEquipmentParameters;

public class School : Building, IMainTarget
{

    public new event Action<List<Equipment>, Building> onUpdateAssortment;


# region IMainTarget

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

# endregion

    protected override void Awake()
    {
        selfName = "School";
        base.Awake();
    }

    protected override void Start()
    {
        SetLikeAMainTarget();
        base.Start();
    }

    public override void UpdateAssortmentInBuilding(RectTransform rectTransformEquipmentPlaces)
    {
        onUpdateAssortment?.Invoke(null, this); // подписываемс€ на событие в ScenarioScript, чтоб знать, когда отписыватьс€ от прослушивани€ событи€ прошлой партии ассортимента, пока
                                                // та ещЄ не была удалена
        foreach (Equipment equipment in equipmentInBuilding)
        {
            if (equipment) Destroy(equipment.gameObject);
        }

        equipmentInBuilding.Clear();
        foreach (RectTransform placeForEquipment in rectTransformEquipmentPlaces)
        {
            // ќ“ Ћё„ј≈ћ — –»ѕ“ Equipment ѕ–» —ѕј¬Ќ≈ —Ќј–я∆≈Ќ»я, „“ќЅ Awake —–ј«” Ќ≈ ќ“–јЅј“џ¬јЋ!

            //MonoBehaviour scriptToDisable = prefubOfEquipment.GetComponent<Equipment>();
            //scriptToDisable.enabled = false;

            // —ќ«ƒј®ћ ќЅЏ≈ “ —Ќј–я∆≈Ќ»я, ѕќЋ”„ј≈ћ ≈√ќ »ћя, RectTransform, —ѕј¬Ќ»ћ ” «јƒјЌЌќ√ќ –ќƒ»“≈Ћя (ћ≈—“ј —Ќј–я∆≈Ќ»я)

            GameObject newEquipment = Instantiate(prefubOfEquipment, Vector3.zero, Quaternion.identity);

            RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
            string randomEquipmentName = AdjustEquipmentParameters.GetRandomSpellName();
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
            Sprite spellSprite = Resources.Load<Sprite>(fullPath);



            // ”ƒјЋя≈ћ ќЅў»… — –»ѕ“ —Ќј–я∆≈Ќ»я ” «ј—ѕј¬Ќ≈ЌЌќ√ќ —Ќј–я∆≈Ќ»я. ƒќЅј¬Ћя≈ћ  ј—“ќћЌџ… — –»ѕ“, ≈—Ћ» “ј ќ¬ќ… ќѕ–≈ƒ≈Ћ®Ќ ƒЋя “≈ ”ў≈√ќ –јЌƒќћЌќ√ќ »ћ≈Ќ» randomEquipmentName

            Spell scriptOfEquipment;

            if (customScriptsEquipment.ContainsKey(randomEquipmentName))
            {
                scriptOfEquipment = (Spell) newEquipment.AddComponent(customScriptsEquipment[randomEquipmentName]);
            }
            else
            {
                scriptOfEquipment = (Spell) newEquipment.AddComponent(typeof(Spell));
            }
            //scriptOfEquipment.enabled = true;

            // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ Equipment (—ќЅ—Ќј ≈√ќ — –»ѕ“) ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я  

            scriptOfEquipment.equipmentName = randomEquipmentName;
            if (spellSprite) scriptOfEquipment.sprite = spellSprite;
            if (spellSprite) spriteRenderer.sprite = spellSprite;
            scriptOfEquipment.isEquipmentASpell = true; // пока что дл€ спелов только
            scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
            scriptOfEquipment.BuildingWhereEquipmentIs = this;
            scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = rectTransformTargetEquipmentPanelPlayer;
            scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;

            // »«ћ≈Ќя≈ћ ѕј–јћ≈“–џ «ƒјЌ»я ѕ–» ƒќЅј¬Ћ≈Ќ»» ¬ Ќ≈√ќ Ќќ¬ќ√ќ —Ќј–я∆≈Ќ»я
            equipmentInBuilding.Add(scriptOfEquipment);
            PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
            scriptOfPlace.Equipment = scriptOfEquipment;
            scriptOfPlace.isBuildingPlace = true;

            //scriptOfEquipment.Awake();
            //scriptOfEquipment.Start();
        }
        onUpdateAssortment?.Invoke(equipmentInBuilding, this); // подписываемс€ на событие в ScenarioScript, чтоб знать, когда был обновлЄн ассортимент в здании
    }


    public static void SpawnParticularSpell(string nameSpell, IInventory inventory, School buildingWhereEquipmentIs = null)
    {
        foreach (RectTransform placeForEquipment in inventory.Inventory.rectTransformSpellPanel)
        {
            if (placeForEquipment.childCount == 2)
            {
                //Debug.Log("B<JJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ");
                string FindSpell()
                {
                    foreach (var spell in AdjustEquipmentParameters.spellParameters)
                    {
                        if (nameSpell == spell.Key)
                        {
                            return spell.Key;
                        }
                    }
                    Debug.LogError("«аклинание с таким названием не было найдено!");
                    return "";
                }
                string nameSpellLocal = FindSpell();

                GameObject newEquipment = Instantiate(GameManager.Instance.prefubSpell, Vector3.zero, Quaternion.identity);

                RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
                newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - чтобы не сохран€ть мировые координаты (позицию, масштаб, поворот)

                // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ RectTransform ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
                newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchoredPosition = Vector2.zero; // ”станавливаем смещение относительно €корей в (0, 0)
                newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
                newEquipmentRectTransform.name = nameSpellLocal;

                // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ SpriteRenderer ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
                SpriteRenderer spriteRenderer = newEquipment.GetComponent<SpriteRenderer>();
                string fullPath = C.Paths.FolderImagesForSpells + nameSpellLocal; 
                Sprite spellSprite = Resources.Load<Sprite>(fullPath);

                // ”ƒјЋя≈ћ ќЅў»… — –»ѕ“ —Ќј–я∆≈Ќ»я ” «ј—ѕј¬Ќ≈ЌЌќ√ќ —Ќј–я∆≈Ќ»я. ƒќЅј¬Ћя≈ћ  ј—“ќћЌџ… — –»ѕ“, ≈—Ћ» “ј ќ¬ќ… ќѕ–≈ƒ≈Ћ®Ќ ƒЋя “≈ ”ў≈√ќ –јЌƒќћЌќ√ќ »ћ≈Ќ» randomEquipmentName

                Spell scriptOfEquipment;
                Dictionary<string, Type> customScriptsEquipment = (Dictionary<string, Type>)AdjustBuildingParameters.buildingParameters[C.DK.School][C.DK.customScriptsEquipment];

                if (customScriptsEquipment.ContainsKey(nameSpellLocal))
                {
                    scriptOfEquipment = (Spell)newEquipment.AddComponent(customScriptsEquipment[nameSpellLocal]);
                }
                else
                {
                    scriptOfEquipment = (Spell)newEquipment.AddComponent(typeof(Spell));
                }
                //scriptOfEquipment.enabled = true;

                // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ Equipment (—ќЅ—Ќј ≈√ќ — –»ѕ“) ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я  

                scriptOfEquipment.equipmentName = nameSpellLocal;
                if (spellSprite) scriptOfEquipment.sprite = spellSprite; 
                scriptOfEquipment.ownerUnit = inventory.UnitSelf;
                scriptOfEquipment.isEquipmentASpell = true; // пока что дл€ спелов только
                scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
                scriptOfEquipment.BuildingWhereEquipmentIs = buildingWhereEquipmentIs;
                scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = GameObject.Find((string)AdjustBuildingParameters.buildingParameters[C.DK.School][C.DK.NameTargetEquipmentPanelPlayer]).GetComponent<RectTransform>();
                scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;

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

    public override void Sell(Player targetForBuy, Equipment equipment)
    {
        targetForBuy.CountAccessToUpInSchool--;
        targetForBuy.CurrentMoney -= equipment.cost;

        //AudioManager.Instance.StartSoundEffectAtSpecifiedObject(C.MusicSounds.Teach, gameObject, AudioManager.TYPE_SOUND.Default, AudioManager.TYPE_AUDIO_SOURCE._2DStandard);
        // по идее дл€ UI-звуков не можно и не использовать такое разделение, а идти через StartSoundEffect
        AudioManager.Instance.StartSoundEffect(C.MusicSounds.Teach);
    }
}
