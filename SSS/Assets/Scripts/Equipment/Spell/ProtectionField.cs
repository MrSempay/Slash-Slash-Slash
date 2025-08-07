using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProtectiveField : Spell
{
    private readonly Vector3 _biasPositionProtectionFieldFromOwner = new Vector3(0f, 0f, 0f);
    private readonly int _sortOrderProtectionFieldSprite = 14;

    private AppearingSprite _scriptProtectiveFieldSprite;
    private int _curremtAmountBlockingAttack;
    private Unit _ownerProtectiveField;
    private Transform _transformParentProtectiveField;
    private Transform _transformParentProtectiveFieldHits;

    [NonSerialized] public int amountBlockingAttackMax;

    public int CurremtAmountBlockingAttack
    {
        get { return _curremtAmountBlockingAttack; }
        set
        {
            _curremtAmountBlockingAttack = value;
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Cast(Unit whoCastedSpell)
    {
        Activate(whoCastedSpell);
    }

    public override void Activate(Unit whoCastedSpell)
    {
        if (!isActivated)
        {
            _ownerProtectiveField = whoCastedSpell;

            isActivated = true;


            _scriptProtectiveFieldSprite = GameManager.Instance.InvokeAppearingSprite(equipmentName + C.Prefixes.Appear, _transformParentProtectiveField, -1f, true);
            _scriptProtectiveFieldSprite.OnSomeAnimationWasFninished += SomeAnimationOfProtectiveFieldWasFinished;
            _scriptProtectiveFieldSprite.selfSprite.sortingOrder = _sortOrderProtectionFieldSprite;
            //Debug.Log("Ебануться нахуй");

        }
    }



    public override void Deactivate(Unit whoCastedSpell)
    {
        if (isActivated)
        {
            if (gameObject.activeSelf)  
            {
                StartCallDown();
            }



            _scriptProtectiveFieldSprite.OnSomeAnimationWasFninished -= SomeAnimationOfProtectiveFieldWasFinished;

            _ownerProtectiveField.OnThisUnitWasAttacked -= ProtectiveFieldWasHit;
            _ownerProtectiveField.isInvicible = false;

            Destroy(_scriptProtectiveFieldSprite.gameObject);
            _scriptProtectiveFieldSprite = null;

            CurremtAmountBlockingAttack = 0;
            isActivated = false;
        }
    }


    // whoMakeHitIntoField может быть null
    private void ProtectiveFieldWasHit(Unit ownerProtectiveField, Unit whoMakeHitIntoField)
    {
        //Debug.Log("Мы теперь тут");
        //Debug.Log(_ownerProtectiveField.isInvicible);
        AnimateHittingField();

        CurremtAmountBlockingAttack++;

        if (CurremtAmountBlockingAttack == amountBlockingAttackMax)
        {
            _scriptProtectiveFieldSprite.animator.Play(equipmentName + C.Prefixes.Disappear);
        }
        else if (CurremtAmountBlockingAttack == amountBlockingAttackMax + 1) // когда значение ударов достигает максимального, мы врубаем анимацию исчезания щита, после которой деактивируем
                                                                             // его. При этом если кто-то ещё раз ударит по щиту, то это уже будет max + 1 и мы тут уже сразу вырубаем щит.
                                                                             // Этот удар блокироваться не будет. Стоит отметить, что это не полная деактивация, а снятие эффекта неуязви
                                                                             // мости и отписка от детекции OnThisUnitWasAttacked
        {
            _ownerProtectiveField.OnThisUnitWasAttacked -= ProtectiveFieldWasHit;
            _ownerProtectiveField.isInvicible = false;
        }
    }


    private void AnimateHittingField()
    {
        //_scriptProtectiveFieldSprite.animator.Play("ProtectiveFieldHit");

        GameManager.Instance.InvokeAppearingSprite("ProtectiveFieldHit1", _transformParentProtectiveFieldHits, 0.2f, false, true);
        GameManager.Instance.InvokeAppearingSprite("ProtectiveFieldHit2", _transformParentProtectiveFieldHits, 0.2f, false, true);
        GameManager.Instance.InvokeAppearingSprite("ProtectiveFieldHit3", _transformParentProtectiveFieldHits, 0.2f, false, true);
        GameManager.Instance.InvokeAppearingSprite("ProtectiveFieldHit4", _transformParentProtectiveFieldHits, 0.2f, false, true);
    }

    public void SomeAnimationOfProtectiveFieldWasFinished(string nameFinishedAnimation)
    {
        switch (nameFinishedAnimation)
        {
            case "ProtectiveFieldDisappear":
                Deactivate(_ownerProtectiveField);
                break;
            case "ProtectiveFieldAppear": // очнеь важная штука для понимания! При преждевременной деактивации, спрайт удалится и сигнал о завершении анимации не придёт, эти эффекты в таком случае не применятся!

                _ownerProtectiveField.OnThisUnitWasAttacked += ProtectiveFieldWasHit;
                _ownerProtectiveField.isInvicible = true;
                _scriptProtectiveFieldSprite.animator.Play(equipmentName + C.Prefixes.Idle);

                StartTimerActiveState(_ownerProtectiveField);
                break;
        }
    }

    public override void EnteredIntoUnitInventory(Unit ownerInventory)
    {
        if (_transformParentProtectiveField == null)
        {
            _transformParentProtectiveField = StaticClassForAdditionalFunctions.InstanceEmptyObjectAndGetTransform(ownerInventory.transform,
                                                                                                                   "ProtectiveFieldPosition",
                                                                                                                   _biasPositionProtectionFieldFromOwner);

            _transformParentProtectiveFieldHits = StaticClassForAdditionalFunctions.InstanceEmptyObjectAndGetTransform(ownerInventory.transform,
                                                                                                                       "ProtectiveFieldHitsPosition",
                                                                                                                       _biasPositionProtectionFieldFromOwner);
        }
    }
    public override void ExitedFromUnitInventory(Unit ownerInventory)
    {
        base.ExitedFromUnitInventory(ownerInventory);

        if (_transformParentProtectiveField != null)
        {
            Destroy(_transformParentProtectiveField.gameObject);
            Destroy(_transformParentProtectiveFieldHits.gameObject);

            _transformParentProtectiveField = null;
            _transformParentProtectiveFieldHits = null;
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();

        if (_ownerProtectiveField != null)
        {
            Deactivate(_ownerProtectiveField);
            _ownerProtectiveField.OnThisUnitWasAttacked -= ProtectiveFieldWasHit;
        }
        if (_scriptProtectiveFieldSprite != null)
        {
            _scriptProtectiveFieldSprite.OnSomeAnimationWasFninished -= SomeAnimationOfProtectiveFieldWasFinished;
        }
    }

}
