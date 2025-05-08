using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class FsmStateCastPlayer : FsmStatePlayer // пока что сделали так, что токмо после ПОЛНОГО ЗАВЕРШЕНИЯ АНИМАЦИИ КАСТА ПЕРСОНАЖА мы применяем какое-либо действие в плане активности

                                                 // переделали, теперь хоть сразу при начале анимации (её может даже не быть) применяем активность, хоть после её окончания.
                                                 // по сути данное состояние нужно лишь для контроля передачи управления логикой самому снаряжению. Также детектим внешние раздражители,
                                                 // которые могут прервать начальный каст (или всеобщий каст, если одна анимация должна работать на протяжении всей активности снаряжения)
{
    public Equipment equipmentWhatWasPressed;
    public bool wasCastAnimationFinished = false;

    public FsmStateCastPlayer(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Player cast state [ENTER]");

        if (initialConditionsEntering != null)
        {
            StaticClassForAdditionalFunctions.AssignParametersAndProperties(initialConditionsEntering, this);

            equipmentWhatWasPressed = (Equipment) initialConditionsEntering["equipmentWhatWasPressed"]; // если ошибка тут, то либо передали не Equipment, либо вообще такого ключа нет в словаре
                                                                                                        // а сделано это потому, что кастомный класс не реализует IConvertable

            player.OnCastAnimationFinished += CastAnimationFinished;

            //AdjustEquipmentParameters.CallActionFunctionByName(equipmentWhatWasPressed, equipmentWhatWasPressed.amountUpCombo, equipmentWhatWasPressed.player);
            
            if (equipmentWhatWasPressed.shouldBeCastedAtStartUnitAnimation)
            {
                AdjustEquipmentParameters.CallActionFunctionByLink(equipmentWhatWasPressed, equipmentWhatWasPressed.amountUpCombo, equipmentWhatWasPressed.player, equipmentWhatWasPressed.Cast);
            }

            if (StaticClassForAdditionalFunctions.AnimationExists(equipmentWhatWasPressed.equipmentName + C.Prefixes.Cast, player.animator))
            {
                player.animator.Play(equipmentWhatWasPressed.equipmentName + C.Prefixes.Cast);
                player.rb.linearVelocityX = 0;
            }
            else // кастуем мгновенно и уходим в состояние покоя
            {
                wasCastAnimationFinished = true;
                player._fsm.SetState<FsmStateIdle>();
            }
        }
        else
        {
            Debug.LogError("Вошли в состояние каста без ПАРАМЕТРОВ!!!");
        }

    }

    public override void Exit()
    {
        Debug.Log("Player cast state [EXIT]");

        player.OnCastAnimationFinished -= CastAnimationFinished;

        if (!wasCastAnimationFinished && equipmentWhatWasPressed.shouldBeCastedAtStartUnitAnimation) // если каст был прерван, при этом какая-то логика была запущена (снаряжение было isActivated)
        {
            ExitBySomethingWrong();
        }

        wasCastAnimationFinished = false;
    }
    public void ExitBySomethingWrong()
    {
        AdjustEquipmentParameters.CallActionFunctionByLink(equipmentWhatWasPressed, 0, equipmentWhatWasPressed.player, equipmentWhatWasPressed.Deactivate);
    }

    private void CastAnimationFinished(string nameCastAnimation) // по идее, пока мы в этом состоянии, может закончиться анимация каста только текущего спела/предмета
    {
        if (!equipmentWhatWasPressed.shouldBeCastedAtStartUnitAnimation) // если анимация каста персонажа завершилась и активность должна примениться именно после этой анимации
        {
            AdjustEquipmentParameters.CallActionFunctionByLink(equipmentWhatWasPressed, equipmentWhatWasPressed.amountUpCombo, equipmentWhatWasPressed.player, equipmentWhatWasPressed.Cast);
        }

        wasCastAnimationFinished = true;
        player._fsm.SetState<FsmStateIdle>();
    }
}
