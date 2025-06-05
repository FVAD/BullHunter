using Bingyan;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    private void OnEnable() =>
        Flow.Create()
            .Delay(1)
            .Then(() => GetComponentsInChildren<AttackArea>().ForEach(a =>
            {
                a.Active = true;
                a.OnAttacking += (atk, def) =>
                {
                    def.ReceiveDamage(atk, def, 0);
                    Debug.Log($"{atk.name}对{def.name}发起攻击，伤害为0");
                };
            }))
            .Run(); // 这玩意也真是无敌了
}
