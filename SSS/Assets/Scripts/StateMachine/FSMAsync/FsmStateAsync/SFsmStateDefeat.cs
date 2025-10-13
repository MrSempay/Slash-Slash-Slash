using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SFsmStateDefeat : AsyncStateBase
{
    private enum Step { IntroDialogue, SpawnFirstEnemy, AfterKillDelay, TeleportToSchool, CameraMove, End }
    private Step _step;

    public SFsmStateDefeat(FsmAsync fsm, GameObject GameObject) : base(fsm, GameObject)      
    {
    }

    public override void Enter(Dictionary<string, object> args)
    {
        if (args != null && args.TryGetValue("startStep", out var s) && s is Step st) _step = st;
        else _step = Step.IntroDialogue;
    }

    public override async Task RunAsync(CancellationToken ct)
    {
        while (_step != Step.End && !ct.IsCancellationRequested)
        {
            switch (_step)
            {
                case Step.IntroDialogue:
                    //await DialogueSystem.Instance.StartDialogueAsync("Dialogue1_1", ct);
                    _step = Step.SpawnFirstEnemy;
                    break;

                case Step.SpawnFirstEnemy:
                    //await SomeHelper.SpawnFirstEnemyAndWaitKillAsync(owner, ct);
                    //await Task.Delay(TimeSpan.FromSeconds(2), ct);
                    _step = Step.TeleportToSchool;
                    break;

                case Step.TeleportToSchool:
                    // синхронные действия на main thread
                    //CameraManager.Instance.DelinkCameraPlayer();
                    //Player.instance.transform.position = ...;
                    _step = Step.CameraMove;
                    break;

                case Step.CameraMove:
                    //await CameraManager.Instance.MoveCameraAsync(..., ct);
                    _step = Step.End;
                    break;
            }
        }
    }
}
