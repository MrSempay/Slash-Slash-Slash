using System.Collections;
using UnityEngine;

public class Tragicomedy : Ammunition
{

    public override void Cast(Unit whoCastedSpell)
    {
        Activate(whoCastedSpell);
    }

    // по сути Activate и Deactivate - это состояние предмета, ещё один разрез его бытия. Оно может быть активировано или не активировано. Причём данные состояния не привязываются к КД и isReady
    public override void Activate(Unit whoCastedSpell)
    {
        if (!isActivated)
        {
            isActivated = true;
            whoCastedSpell.ChangeUnitParametersByPercentage(increasingUnitParametersByAmmunitionPercentageByCast, true);

            if (durationActiveState != -1f)
            {
                StartCoroutine(DurationActive(whoCastedSpell));
            }
        }
    }

    IEnumerator DurationActive(Unit whoCastedSpell)
    {
        yield return new WaitForSeconds(durationActiveState);

        Deactivate(whoCastedSpell);
    }

    public override void Deactivate(Unit whoCastedSpell)
    {
        if (isActivated)
        {
            StartCallDown();
            isActivated = false;
            whoCastedSpell.ChangeUnitParametersByPercentage(increasingUnitParametersByAmmunitionPercentageByCast, false);
        }
    }


}
