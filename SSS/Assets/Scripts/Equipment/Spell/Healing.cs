using UnityEngine;

public class Healing : Spell
{
    public float healthHealAmount;



    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void EnteredIntoUnitInventory(Unit ownerInventory) // хотя это ближе не к ownerInventory, а просто к inventory
    {
        base.EnteredIntoUnitInventory(ownerInventory);
    }
    public override void ExitedFromUnitInventory(Unit ownerInventory)
    {
        base.ExitedFromUnitInventory(ownerInventory);
    }

    public override void Cast(Unit whoCastedSpell)
    {
        Activate(whoCastedSpell);
    }

    public override void Activate(Unit whoCastedSpell)
    {
        base.Activate(whoCastedSpell); // хотя я вот думаю, что логику базового метода можно было бы вывести просто в отдельную функцию и вызывать её при надобности 

        if (!isActivated)
        {
            isActivated = true;

            StartTimerActiveState(whoCastedSpell); // контролим через событие Peak анимации. 30.07.2025 - Нафиг тут эта строка вообще хз...

        }
    }
    public override void Deactivate(Unit whoCastedSpell)
    {
        // ТУТ БЫЛО:   base.Activate(whoCastedSpell);      !!!
        base.Deactivate(whoCastedSpell); // хотя я вот думаю, что логику базового метода можно было бы вывести просто в отдельную функцию и вызывать её при надобности

        if (isActivated)
        {
            StartCallDown();

            isActivated = false;
        }
    }

    public override void UnitCastAnimationPeackedForThisEquipment()
    {
        //Debug.Log("Hilim");
        ownerUnit.Heal(healthHealAmount); // Срочно!!! нужно менять!!! на owner !!!, я не могу более на это смотреть...
    }

    public override void UnitCastAnimationFinishedForThisEquipment()
    {
        //Debug.Log("konchaem");
        Deactivate(ownerUnit); // AAAAAAAAA
    }

}
