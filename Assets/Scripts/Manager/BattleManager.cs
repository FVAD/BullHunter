using Bingyan;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : ManagerBase<BattleManager>
{
    [Header("角色")]
    [SerializeField, Title("玩家")] private Player player;
    [SerializeField, Title("敌人")] private Bull1 enemy;
    [SerializeField, Title("控制器")] private GlobalFlagController flag;

    public Player Player => player;
    public Bull1 Enemy => enemy;
    public GlobalFlagController Flag => flag;
}
