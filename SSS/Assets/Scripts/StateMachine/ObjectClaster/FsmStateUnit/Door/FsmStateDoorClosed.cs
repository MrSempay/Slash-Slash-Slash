using System.Collections.Generic;
using UnityEngine;

public class FsmStateDoorClosed : FsmStateDoor
{
    public FsmStateDoorClosed(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        //Debug.Log("Closed state [ENTER]");

        door.selfCollider.enabled = true;
        door.selfSprite.sprite = door.spriteDoorClosed;

        if (initialConditionsEntering == null) // игнорируем звук при спавне двери
        {
            AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(C.MusicSounds.DoorClosing,
                                                                    door.audioEmitter,
                                                                    AudioManager.TYPE_SOUND.Default,
                                                                    AudioManager.TYPE_AUDIO_SOURCE._3DStandard,
                                                                    new List<AudioManager.TYPE_SOUND> { AudioManager.TYPE_SOUND.Default });
        }

    }

    public override void Exit()
    {
        //Debug.Log("Closed state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
    }

}
