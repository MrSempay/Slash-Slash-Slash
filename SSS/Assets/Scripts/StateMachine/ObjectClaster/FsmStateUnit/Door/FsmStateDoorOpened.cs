using System.Collections.Generic;
using UnityEngine;

public class FsmStateDoorOpened : FsmStateDoor
{
    public FsmStateDoorOpened(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        //Debug.Log("Opened door state [ENTER]");

        door.selfCollider.enabled = false;
        door.selfSprite.sprite = door.spriteDoorOpened;

        if (initialConditionsEntering == null)
        {
            AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(C.MusicSounds.DoorOpening,
                                                                    door.audioEmitter,
                                                                    AudioManager.TYPE_SOUND.Default,
                                                                    AudioManager.TYPE_AUDIO_SOURCE._3DStandard,
                                                                    new List<AudioManager.TYPE_SOUND> { AudioManager.TYPE_SOUND.Default });
        }


    }

    public override void Exit()
    {
        //Debug.Log("Opened door state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
    }

}
