using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static AdjustEquipmentParameters;
using static UnityEngine.Rendering.DebugUI;

public class InventoryPlayer : Inventory
{

    private RectTransform _rectTransformLastPanelChoose;
    private RectTransform _rectTransformLastInfoPanel;




    public override void Start()
    {
        base.Start();

        Player.instance.OnPlayerMove += CloseAllEquipmentUI;
    }




    public override bool SetEquipmentToInventory(Equipment equipment)
    {
        if (base.SetEquipmentToInventory(equipment))
        {
            equipment.OnEquipmentShouldBeActivate += Player.instance.SomeEquipmentShouldBeActivate; // используем Player.instance ибо данный инвентарий у нас может быть только для игрока
        }


        return true;
    }


    public override bool RemoveEquipmentFromInventory(Equipment equipment)
    {
        if (base.RemoveEquipmentFromInventory(equipment)) // 18.09, изменил с SetEquipmentToInventory на RemoveEquipmentFromInventory
        {
            equipment.OnEquipmentShouldBeActivate -= Player.instance.SomeEquipmentShouldBeActivate;
            CloseAllEquipmentUI();
        }

        return true;

    }


    // Update is called once per frame   
    void Update()
    {
        
    }

    public void ShowPanelChoose(RectTransform rectTransformPanelChoose)
    {
        if (_rectTransformLastPanelChoose != null)
        {
            HideLastPanelChoose();
        }

        _rectTransformLastPanelChoose = rectTransformPanelChoose;
        _rectTransformLastPanelChoose.gameObject.SetActive(true);
    }
    public void HideLastPanelChoose()
    {
        if (_rectTransformLastPanelChoose != null)
        {
            _rectTransformLastPanelChoose.gameObject.SetActive(false);
            _rectTransformLastPanelChoose = null;
        }
    }
    public void ShowInfoPanel(RectTransform rectTransformButtonChoose)
    {
        if (_rectTransformLastInfoPanel != null)
        {
            HideLastInfoPanel();
        }

        _rectTransformLastInfoPanel = rectTransformButtonChoose;
        _rectTransformLastInfoPanel.gameObject.SetActive(true);
    }
    public void HideLastInfoPanel()
    {
        if (_rectTransformLastInfoPanel != null)
        {
            _rectTransformLastInfoPanel.gameObject.SetActive(false);
            _rectTransformLastInfoPanel = null;
        }
    }
    private void CloseAllEquipmentUI()
    {
        HideLastPanelChoose();
        HideLastInfoPanel();
    }


    private void OnDestroy()
    {
        Player.instance.OnPlayerMove -= CloseAllEquipmentUI;
    }

}
