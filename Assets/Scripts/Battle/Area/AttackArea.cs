using Bingyan;
using System;
using UnityEngine;

/// <summary>
/// 攻击区域
/// <br/>监听<see cref="OnAttacking"/>事件处理攻击
/// </summary>
[RequireComponent(typeof(Collider))]
public class AttackArea : MonoBehaviour
{
    private enum Owner { Player, Enemy }
    [SerializeField, Title("拥有者")] private Owner owner;

    private string target;
    /// <summary>
    /// 攻击事件，使用例：
    /// <code>
    /// area.OnAttacking += (atk, def) => def.ReceiveDamage(114514);
    /// </code>
    /// </summary>
    public event Action<AttackArea, DefendArea> OnAttacking = (atk, def) => Debug.Log($"{atk.name}对{def.name}发起攻击");
    private Collider col;

    private RaycastHit[] hits;
    private const int MAX_HIT = 19;
    public Vector3 HitPoint { get; private set; }

    private bool active;
    public bool Active
    {
        get => active;
        set => col.enabled = active = value;
    }

    private void Start()
    {
        (col = GetComponent<Collider>()).isTrigger = true;
        Active = false;
        tag = owner.ToString();
        target = owner switch { Owner.Player => "Enemy", Owner.Enemy => "Player", _ => "Default" };
        hits = new RaycastHit[MAX_HIT];
    }

    private void OnTriggerStay(Collider collider)
    {
        if (!Active || !collider.CompareTag(target) ||
            !collider.TryGetComponent(out DefendArea other))
            return;

        var dir = (owner switch
        {
            Owner.Player => BattleManager.Instance.Enemy.transform,
            Owner.Enemy => BattleManager.Instance.Player.transform,
            _ => throw new Exception()
        }).position - transform.position;
        var cnt = Physics.RaycastNonAlloc(transform.position, dir, hits);
        for (int i = 0; i < cnt; i++)
        {
            if (!hits[i].collider.CompareTag(target)) continue;
            HitPoint = hits[i].point;
            break;
        }

        OnAttacking?.Invoke(this, other);
    }
}
