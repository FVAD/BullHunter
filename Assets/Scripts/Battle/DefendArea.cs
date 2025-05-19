using Bingyan;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DefendArea : MonoBehaviour
{
    private enum Owner { Player, Enemy }
    [SerializeField, Title("拥有者")] private Owner owner;
    [SerializeField, Title("倍率")] private float factor = 1;

    private void Start() => tag = owner.ToString();

    public event Action<float> OnAttacked;
    public void ReceiveDamage(float damage) => OnAttacked?.Invoke(damage * factor);
}
