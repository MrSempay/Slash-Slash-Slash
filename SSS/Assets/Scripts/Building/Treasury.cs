using UnityEngine;

public class Treasury : Building
{
    protected override void Awake()
    {
        rectTransformTargetEquipmentPanelPlayer = GameObject.Find("AmmunitionPanel").GetComponent<RectTransform>();
        base.Awake();
    }


}
