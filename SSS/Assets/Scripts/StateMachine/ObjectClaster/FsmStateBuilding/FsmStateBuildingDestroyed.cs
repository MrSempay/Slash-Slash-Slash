using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static FsmStatePlayer;

public class FsmStateBuildingDestroyed : FsmStateBuilding
{
    public FsmStateBuildingDestroyed(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        //Debug.Log("Building Destroyed state [ENTER]");
        //building.gameObject.SetActive(false);

        building.selfCollider.enabled = false;
        
        building.animator.Play(building.selfName + C.Prefixes.Destroyed);   

        building.buttonEnter.SetActive(false);

        building.rectTransformEquipmentPlaces.gameObject.SetActive(false);

        IMainTarget mainTarget = building as IMainTarget;
        if (mainTarget != null)
        {
            mainTarget.WasDestroyed = true;
        }

        //AudioManager.Instance.StartSoundEffectAtSpecifiedObject(C.MusicSounds.BuildingCrush, gameObject, AudioManager.TYPE_SOUND.Destroy, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
        AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(C.MusicSounds.BuildingCrush, building.audioEmitter, AudioManager.TYPE_SOUND.Destroy, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
    }

    public override void Exit()
    {
        //Debug.Log("Building Destroyed state [EXIT]");
    }
}
