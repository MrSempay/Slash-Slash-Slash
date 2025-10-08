using System.Collections.Generic;
using UnityEngine;

public class Berserker : Spell
{
    private static Dictionary<Unit, Berserker> _dictionaryUnitAndLastCastedBerserker = new();

    private readonly Vector3 _biasPositionBerserkerEyesFromOwner = new Vector3(0f, 0f, 0f);
    private readonly int _sortOrderProtectionFieldSprite = 13;

    private Transform _transformParentBerserkerEyes;
    //private AppearingSprite _scriptBerserkerEyesSprite;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void EnteredIntoUnitInventory(Unit ownerInventory)
    {
        if (_transformParentBerserkerEyes == null)
        {
            _transformParentBerserkerEyes = StaticClassForAdditionalFunctions.InstanceEmptyObjectAndGetTransform(ownerInventory.transform,
                                                                                                                   "BerserkerEyesPosition",
                                                                                                                   _biasPositionBerserkerEyesFromOwner);
        }
    }
    public override void ExitedFromUnitInventory(Unit ownerInventory)
    {
        base.ExitedFromUnitInventory(ownerInventory);

        if (_transformParentBerserkerEyes != null)
        {
            Destroy(_transformParentBerserkerEyes.gameObject);

            _transformParentBerserkerEyes = null;
        }
    }

    public override void Cast(Unit whoCastedSpell)
    {
        Activate(whoCastedSpell);
    }

    public override void Activate(Unit whoCastedSpell)
    {
        if (!isActivated)
        {
            if (_dictionaryUnitAndLastCastedBerserker.ContainsKey(whoCastedSpell))
            {
                Berserker scriptLastBerserker = _dictionaryUnitAndLastCastedBerserker[whoCastedSpell];
                scriptLastBerserker.Deactivate(whoCastedSpell);
                scriptLastBerserker.StopCoroutine(scriptLastBerserker.DurationActive(whoCastedSpell));
            }
            _dictionaryUnitAndLastCastedBerserker[whoCastedSpell] = this;

            StartTimerActiveState(whoCastedSpell);

            isActivated = true;

            whoCastedSpell.ChangeUnitParametersByPercentage(increasingUnitParametersByAmmunitionPercentageByCast, true);

            whoCastedSpell.AddUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker);
        }
    }
    public override void Deactivate(Unit whoCastedSpell)
    {
        if (isActivated)
        {
            //Debug.Log("Here");
            StartCallDown();

            isActivated = false;

            whoCastedSpell.ChangeUnitParametersByPercentage(increasingUnitParametersByAmmunitionPercentageByCast, false);

            whoCastedSpell.RemoveUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker);
            
            whoCastedSpell.BerserkerStateDeactivated();
            
            _dictionaryUnitAndLastCastedBerserker.Remove(whoCastedSpell);
        }
    }
}
