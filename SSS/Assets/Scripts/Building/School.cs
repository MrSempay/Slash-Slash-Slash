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
        onUpdateAssortment?.Invoke(null, this); // ïîäïèñûâàåìñÿ íà ñîáûòèå â ScenarioScript, ÷òîá çíàòü, êîãäà îòïèñûâàòüñÿ îò ïğîñëóøèâàíèÿ ñîáûòèÿ ïğîøëîé ïàğòèè àññîğòèìåíòà, ïîêà
                                                // òà åù¸ íå áûëà óäàëåíà
        foreach (Equipment equipment in equipmentInBuilding)
        {
            if (equipment) Destroy(equipment.gameObject);
        }

        equipmentInBuilding.Clear();
        foreach (RectTransform placeForEquipment in rectTransformEquipmentPlaces)
        {
            // ÎÒÊËŞ×ÀÅÌ ÑÊĞÈÏÒ Equipment ÏĞÈ ÑÏÀÂÍÅ ÑÍÀĞßÆÅÍÈß, ×ÒÎÁ Awake ÑĞÀÇÓ ÍÅ ÎÒĞÀÁÀÒÛÂÀË!

            //MonoBehaviour scriptToDisable = prefubOfEquipment.GetComponent<Equipment>();
            //scriptToDisable.enabled = false;

            // ÑÎÇÄÀ¨Ì ÎÁÚÅÊÒ ÑÍÀĞßÆÅÍÈß, ÏÎËÓ×ÀÅÌ ÅÃÎ ÈÌß, RectTransform, ÑÏÀÂÍÈÌ Ó ÇÀÄÀÍÍÎÃÎ ĞÎÄÈÒÅËß (ÌÅÑÒÀ ÑÍÀĞßÆÅÍÈß)

            GameObject newEquipment = Instantiate(prefubOfEquipment, Vector3.zero, Quaternion.identity);

            RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
            string randomEquipmentName = AdjustEquipmentParameters.GetRandomSpellName();
            newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - ÷òîáû íå ñîõğàíÿòü ìèğîâûå êîîğäèíàòû (ïîçèöèş, ìàñøòàá, ïîâîğîò)

            // ÍÀÑÒĞÀÈÂÀÅÌ ÊÎÌÏÎÍÅÍÒ RectTransform Ó İÊÇÅÌÏËßĞÀ ÑÍÀĞßÆÅÍÈß
            newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchoredPosition = Vector2.zero; // Óñòàíàâëèâàåì ñìåùåíèå îòíîñèòåëüíî ÿêîğåé â (0, 0)
            newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
            newEquipmentRectTransform.name = randomEquipmentName;

            // ÍÀÑÒĞÀÈÂÀÅÌ ÊÎÌÏÎÍÅÍÒ SpriteRenderer Ó İÊÇÅÌÏËßĞÀ ÑÍÀĞßÆÅÍÈß
            SpriteRenderer spriteRenderer = newEquipment.GetComponent<SpriteRenderer>();
            string fullPath = folderImagesOfEquipment + randomEquipmentName;
            Sprite spellSprite = Resources.Load<Sprite>(fullPath);



            // ÓÄÀËßÅÌ ÎÁÙÈÉ ÑÊĞÈÏÒ ÑÍÀĞßÆÅÍÈß Ó ÇÀÑÏÀÂÍÅÍÍÎÃÎ ÑÍÀĞßÆÅÍÈß. ÄÎÁÀÂËßÅÌ ÊÀÑÒÎÌÍÛÉ ÑÊĞÈÏÒ, ÅÑËÈ ÒÀÊÎÂÎÉ ÎÏĞÅÄÅË¨Í ÄËß ÒÅÊÓÙÅÃÎ ĞÀÍÄÎÌÍÎÃÎ ÈÌÅÍÈ randomEquipmentName

            Spell scriptOfEquipment;

            if (customScriptsEquipment.ContainsKey(randomEquipmentName))
            {            
                //Debug.Log("shit");

                scriptOfEquipment = (Spell) newEquipment.AddComponent(customScriptsEquipment[randomEquipmentName]);

                //Debug.Log(scriptOfEquipment.GetInstanceID());
            }
            else
            {
                scriptOfEquipment = (Spell) newEquipment.AddComponent(typeof(Spell));
            }
            //scriptOfEquipment.enabled = true;

            // ÍÀÑÒĞÀÈÂÀÅÌ ÊÎÌÏÎÍÅÍÒ Equipment (ÑÎÁÑÍÀ ÅÃÎ ÑÊĞÈÏÒ) Ó İÊÇÅÌÏËßĞÀ ÑÍÀĞßÆÅÍÈß  

            scriptOfEquipment.equipmentName = randomEquipmentName;
            if (spellSprite) scriptOfEquipment.sprite = spellSprite;
            if (spellSprite) spriteRenderer.sprite = spellSprite;
            scriptOfEquipment.isEquipmentASpell = true; // ïîêà ÷òî äëÿ ñïåëîâ òîëüêî
            scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
            scriptOfEquipment.BuildingWhereEquipmentIs = this;
            scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = rectTransformTargetEquipmentPanelPlayer;
            scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;

            // ÈÇÌÅÍßÅÌ ÏÀĞÀÌÅÒĞÛ ÇÄÀÍÈß ÏĞÈ ÄÎÁÀÂËÅÍÈÈ Â ÍÅÃÎ ÍÎÂÎÃÎ ÑÍÀĞßÆÅÍÈß
            equipmentInBuilding.Add(scriptOfEquipment);
            PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
            scriptOfPlace.Equipment = scriptOfEquipment;
            scriptOfPlace.isBuildingPlace = true;

            //scriptOfEquipment.Awake();
            //scriptOfEquipment.Start();
        }
        onUpdateAssortment?.Invoke(equipmentInBuilding, this); // ïîäïèñûâàåìñÿ íà ñîáûòèå â ScenarioScript, ÷òîá çíàòü, êîãäà áûë îáíîâë¸í àññîğòèìåíò â çäàíèè
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
                    Debug.LogError("Çàêëèíàíèå ñ òàêèì íàçâàíèåì íå áûëî íàéäåíî!");
                    return "";
                }
                string nameSpellLocal = FindSpell();

                GameObject newEquipment = Instantiate(GameManager.Instance.prefubSpell, Vector3.zero, Quaternion.identity);

                RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
                newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - ÷òîáû íå ñîõğàíÿòü ìèğîâûå êîîğäèíàòû (ïîçèöèş, ìàñøòàá, ïîâîğîò)

                // ÍÀÑÒĞÀÈÂÀÅÌ ÊÎÌÏÎÍÅÍÒ RectTransform Ó İÊÇÅÌÏËßĞÀ ÑÍÀĞßÆÅÍÈß
                newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                newEquipmentRectTransform.anchoredPosition = Vector2.zero; // Óñòàíàâëèâàåì ñìåùåíèå îòíîñèòåëüíî ÿêîğåé â (0, 0)
                newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
                newEquipmentRectTransform.name = nameSpellLocal;

                // ÍÀÑÒĞÀÈÂÀÅÌ ÊÎÌÏÎÍÅÍÒ SpriteRenderer Ó İÊÇÅÌÏËßĞÀ ÑÍÀĞßÆÅÍÈß
                SpriteRenderer spriteRenderer = newEquipment.GetComponent<SpriteRenderer>();
                string fullPath = C.Paths.FolderImagesForSpells + nameSpellLocal; 
                Sprite spellSprite = Resources.Load<Sprite>(fullPath);

                // ÓÄÀËßÅÌ ÎÁÙÈÉ ÑÊĞÈÏÒ ÑÍÀĞßÆÅÍÈß Ó ÇÀÑÏÀÂÍÅÍÍÎÃÎ ÑÍÀĞßÆÅÍÈß. ÄÎÁÀÂËßÅÌ ÊÀÑÒÎÌÍÛÉ ÑÊĞÈÏÒ, ÅÑËÈ ÒÀÊÎÂÎÉ ÎÏĞÅÄÅË¨Í ÄËß ÒÅÊÓÙÅÃÎ ĞÀÍÄÎÌÍÎÃÎ ÈÌÅÍÈ randomEquipmentName

                Spell scriptOfEquipment;
                Dictionary<string, Type> customScriptsEquipment = (Dictionary<string, Type>)AdjustBuildingParameters.buildingParameters[C.DK.School][C.DK.customScriptsEquipment];

                if (customScriptsEquipment.ContainsKey(nameSpellLocal))
                {
                    //Debug.Log("shit");

                    scriptOfEquipment = (Spell)newEquipment.AddComponent(customScriptsEquipment[nameSpellLocal]);

                    //Debug.Log(scriptOfEquipment.GetInstanceID());
                }
                else
                {
                    scriptOfEquipment = (Spell)newEquipment.AddComponent(typeof(Spell));
                }
                //scriptOfEquipment.enabled = true;

                // ÍÀÑÒĞÀÈÂÀÅÌ ÊÎÌÏÎÍÅÍÒ Equipment (ÑÎÁÑÍÀ ÅÃÎ ÑÊĞÈÏÒ) Ó İÊÇÅÌÏËßĞÀ ÑÍÀĞßÆÅÍÈß  

                scriptOfEquipment.equipmentName = nameSpellLocal;
                if (spellSprite) scriptOfEquipment.sprite = spellSprite; 
                scriptOfEquipment.ownerUnit = inventory.UnitSelf;
                scriptOfEquipment.isEquipmentASpell = true; // ïîêà ÷òî äëÿ ñïåëîâ òîëüêî
                scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
                scriptOfEquipment.BuildingWhereEquipmentIs = buildingWhereEquipmentIs;
                scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = GameObject.Find((string)AdjustBuildingParameters.buildingParameters[C.DK.School][C.DK.NameTargetEquipmentPanelPlayer]).GetComponent<RectTransform>();
                scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;

                PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
                scriptOfPlace.Equipment = scriptOfEquipment;

                // ÈÇÌÅÍßÅÌ ÏÀĞÀÌÅÒĞÛ ÇÄÀÍÈß ÏĞÈ ÄÎÁÀÂËÅÍÈÈ Â ÍÅÃÎ ÍÎÂÎÃÎ ÑÍÀĞßÆÅÍÈß
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
            buildingWhereEquipmentIs.onUpdateAssortment?.Invoke(buildingWhereEquipmentIs.equipmentInBuilding, buildingWhereEquipmentIs); // ïîäïèñûâàåìñÿ íà ñîáûòèå â ScenarioScript, ÷òîá çíàòü, êîãäà áûë îáíîâë¸í àññîğòèìåíò â çäàíèè
        }
    }
}
