using System.Collections.Generic;
using UnityEngine;

public class FsmStatePlayersEquipment : FsmStateEquipment
{
    public Player player;

    public FsmStatePlayersEquipment(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        // рср бегде хяонкэгсел Player.instance хан дюммне янярнъмхе лнфер ашрэ бшгбюмн рнкэйн дкъ Player !!!

        player = Player.instance;
    }
}
