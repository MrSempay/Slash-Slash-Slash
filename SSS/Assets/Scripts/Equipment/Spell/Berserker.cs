using UnityEngine;

public class Berserker : Spell
{
    private readonly Vector3 _biasPositionBerserkerEyesFromOwner = new Vector3(0f, 0f, 0f);
    private readonly int _sortOrderProtectionFieldSprite = 13;

    private Transform _transformParentBerserkerEyes;
    private AppearingSprite _scriptBerserkerEyesSprite;

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
        if (_transformParentBerserkerEyes == null)
        {
            _transformParentBerserkerEyes = StaticClassForAdditionalFunctions.InstanceEmptyObjectAndGetTransform(ownerInventory.transform,
                                                                                                                   "BerserkerEyesPosition",
                                                                                                                   _biasPositionBerserkerEyesFromOwner);
        }
    }
    public override void ExitedFromInventory(Unit ownerInventory)
    {
        base.ExitedFromInventory(ownerInventory);

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
            isActivated = true;

            StartTimerActiveState(whoCastedSpell);

            whoCastedSpell.ChangeUnitParametersByPercentage(increasingUnitParametersByAmmunitionPercentageByCast, true);
            whoCastedSpell.OnDirectionViewWasChanged += BiasBerserkerEyes;

            _scriptBerserkerEyesSprite = GameManager.Instance.InvokeAppearingSprite(C.AppSprite.BerserkerEyes, _transformParentBerserkerEyes, -1f, true);
            _scriptBerserkerEyesSprite.selfSprite.sortingOrder = _sortOrderProtectionFieldSprite;
            _scriptBerserkerEyesSprite.selfSprite.flipX = !whoCastedSpell.lookingRight;
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
            whoCastedSpell.OnDirectionViewWasChanged -= BiasBerserkerEyes;

            Destroy(_scriptBerserkerEyesSprite.gameObject);
            _scriptBerserkerEyesSprite = null;
        }
    }

    public void BiasBerserkerEyes(bool lookingRight)
    {
        _scriptBerserkerEyesSprite.selfSprite.flipX = !lookingRight;
    }
}
