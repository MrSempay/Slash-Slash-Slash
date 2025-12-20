using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class FsmStateCastUnit : FsmStateUnit // пока что сделали так, что токмо после ПОЛНОГО ЗАВЕРШЕНИЯ АНИМАЦИИ КАСТА ПЕРСОНАЖА мы применяем какое-либо действие в плане активности

                                                 // переделали, теперь хоть сразу при начале анимации (её может даже не быть) применяем активность, хоть после её окончания.
                                                 // по сути данное состояние нужно лишь для контроля передачи управления логикой самому снаряжению. Также детектим внешние раздражители,
                                                 // которые могут прервать начальный каст (или всеобщий каст, если одна анимация должна работать на протяжении всей активности снаряжения)
{
    public Equipment equipmentWhatWasPressed;
    public bool wasCastAnimationFinished = false;

    public FsmStateCastUnit(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        //Debug.Log("Player cast state [ENTER]");

        if (initialConditionsEntering != null)
        {
            StaticClassForAdditionalFunctions.AssignParametersAndProperties(initialConditionsEntering, this);

            equipmentWhatWasPressed = (Equipment) initialConditionsEntering["equipmentWhatWasPressed"]; // если ошибка тут, то либо передали не Equipment, либо вообще такого ключа нет в словаре
                                                                                                        // а сделано это потому, что кастомный класс не реализует IConvertable

            unit.OnCastAnimationFinished += CastAnimationFinished;

            //player.OnSetStateIdle += SetStateIdleCallback; // А как?... У нас же нет у Unit OnSetStateIdle

            //AdjustEquipmentParameters.CallActionFunctionByName(equipmentWhatWasPressed, equipmentWhatWasPressed.amountUpCombo, equipmentWhatWasPressed.player);

            if (equipmentWhatWasPressed.shouldBeCastedAtStartUnitAnimation)
            {
                AdjustEquipmentParameters.CallActionFunctionByLink(equipmentWhatWasPressed, equipmentWhatWasPressed.amountUpCombo, equipmentWhatWasPressed.ownerUnit, equipmentWhatWasPressed.Cast);
            }

            string nameAnimationCast = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker)? 
                equipmentWhatWasPressed.equipmentName + C.Prefixes.Cast + C.StatesAdditional.Berserker :
                equipmentWhatWasPressed.equipmentName + C.Prefixes.Cast;

            StartAnimationOrSetIdle(nameAnimationCast);

            //AudioManager.Instance.StartSoundEffectAtSpecifiedObject(equipmentWhatWasPressed.equipmentName + C.Prefixes.Cast,
            //                                                        equipmentWhatWasPressed.gameObject,
            //                                                        AudioManager.TYPE_SOUND.Default,
            //                                                        AudioManager.TYPE_AUDIO_SOURCE._2DStandard);
            AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(equipmentWhatWasPressed.equipmentName + C.Prefixes.Cast,
                                                                    unit.audioEmitter,
                                                                    AudioManager.TYPE_SOUND.Default,
                                                                    AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
        }
        else
        {
            //Debug.LogError("Вошли в состояние каста без ПАРАМЕТРОВ!!!");
        }

    }

    public override void Exit()
    {
        //Debug.Log("Player cast state [EXIT]");

        unit.OnCastAnimationFinished -= CastAnimationFinished;

        if (!wasCastAnimationFinished && equipmentWhatWasPressed.shouldBeCastedAtStartUnitAnimation) // если каст был прерван, при этом какая-то логика была запущена (снаряжение было isActivated)
        {
            ExitBySomethingWrong();
        }

        wasCastAnimationFinished = false;
    }
    public void ExitBySomethingWrong()
    {
        AdjustEquipmentParameters.CallActionFunctionByLink(equipmentWhatWasPressed, 0, equipmentWhatWasPressed.ownerUnit, equipmentWhatWasPressed.Deactivate);
    }

    private void CastAnimationFinished(string nameCastAnimation) // по идее, пока мы в этом состоянии, может закончиться анимация каста только текущего спела/предмета
    {
        if (!equipmentWhatWasPressed.shouldBeCastedAtStartUnitAnimation) // если анимация каста персонажа завершилась и активность должна примениться именно после этой анимации
        {
            AdjustEquipmentParameters.CallActionFunctionByLink(equipmentWhatWasPressed, equipmentWhatWasPressed.amountUpCombo, equipmentWhatWasPressed.ownerUnit, equipmentWhatWasPressed.Cast);
        }

        wasCastAnimationFinished = true;
        unit._fsm.SetStateIdle(equipmentWhatWasPressed.ownerUnit);
    }

    private void StartAnimationOrSetIdle(string nameAnimationCast)
    {
        if (StaticClassForAdditionalFunctions.AnimationExists(nameAnimationCast, unit.animator))
        {
            ////Debug.Log(equipmentWhatWasPressed.equipmentName + C.Prefixes.Cast);
            unit.animator.Play(nameAnimationCast);
            unit.rb.linearVelocityX = 0;
        }
        else // кастуем мгновенно и уходим в состояние покоя
        {
            ////Debug.Log(2);
            wasCastAnimationFinished = true;
            unit._fsm.SetStateIdle(equipmentWhatWasPressed.ownerUnit);
        }

    }
}
