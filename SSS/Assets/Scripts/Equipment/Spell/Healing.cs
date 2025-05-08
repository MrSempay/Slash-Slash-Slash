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

    public override void EnteredIntoInventory(Unit ownerInventory)
    {
        base.EnteredIntoInventory(ownerInventory);
    }
    public override void ExitedFromInventory(Unit ownerInventory)
    {
        base.ExitedFromInventory(ownerInventory);
    }

    public override void Cast(Unit whoCastedSpell)
    {
        Activate(whoCastedSpell);
    }

    public override void Activate(Unit whoCastedSpell)
    {
        base.Activate(whoCastedSpell);

        if (!isActivated)
        {
            isActivated = true;

            StartTimerActiveState(whoCastedSpell); // контролим через событие Peak анимации

        }
    }
    public override void Deactivate(Unit whoCastedSpell)
    {
        base.Activate(whoCastedSpell);

        if (isActivated)
        {
            StartCallDown();

            isActivated = false;
        }
    }

    public override void UnitCastAnimationPeackedForThisEquipment()
    {
        //Debug.Log("Hilim");
        player.Heal(healthHealAmount); // Срочно!!! нужно менять!!! на owner !!!, я не могу более на это смотреть...
    }

    public override void UnitCastAnimationFinishedForThisEquipment()
    {
        //Debug.Log("konchaem");
        Deactivate(player); // AAAAAAAAA
    }

}
