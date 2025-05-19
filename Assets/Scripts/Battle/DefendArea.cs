using Bingyan;
using System;
using UnityEngine;

/// <summary>
/// 受击区域
/// <br/>攻击者调用<see cref="ReceiveDamage(float)"/>方法造成伤害
/// <br/>然后受击者监听<see cref="OnAttacked"/>事件承受伤害
/// </summary>
[RequireComponent(typeof(Collider))]
public class DefendArea : MonoBehaviour
{
    private enum Owner { Player, Enemy }
    [SerializeField, Title("拥有者")] private Owner owner;
    [SerializeField, Title("倍率")] private float factor = 1;

    private void Start() => tag = owner.ToString();

    /// <summary>
    /// 受击事件，使用例：
    /// <code>
    /// area.OnAttacked += f => hp -= f;
    /// </code>
    /// </summary>
    public event Action<float> OnAttacked = f => Debug.Log($"造成了{f}点伤害，效果拔群！");
    public void ReceiveDamage(float damage) => OnAttacked?.Invoke(damage * factor);
}
