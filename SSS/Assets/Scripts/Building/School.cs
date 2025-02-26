using UnityEngine;

public class School : Building
{
    protected override void Awake()
    {
        rectTransformTargetEquipmentPanelPlayer = GameObject.Find("SpellPanel").GetComponent<RectTransform>();
        base.Awake();
    }


}
