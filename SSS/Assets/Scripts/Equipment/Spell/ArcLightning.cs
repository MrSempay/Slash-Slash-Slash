using UnityEngine;

public class ArcLightning : Spell
{


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

            

        }
    }
    public override void Deactivate(Unit whoCastedSpell)
    {
        base.Activate(whoCastedSpell); // хотя я вот думаю, что логику базового метода можно было бы вывести просто в отдельную функцию и вызывать её при надобности

        if (isActivated)
        {
            StartCallDown();

            isActivated = false;
        }
    }

    public override void UnitCastAnimationFinishedForThisEquipment()
    {
        Deactivate(ownerUnit);
    }


}
