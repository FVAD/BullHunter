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
                a.OnAttacking += (atk, def) => def.ReceiveDamage(atk, def, 0);
            }))
            .Run();
}
