using Bingyan;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AttackArea : MonoBehaviour
{
    private enum Owner { Player, Enemy }
    [SerializeField, Title("拥有者")] private Owner owner;

    private string target;
    public event Func<DefendArea, float> OnAttacking;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        tag = owner.ToString();
        target = owner switch { Owner.Player => "Enemy", Owner.Enemy => "Player", _ => "Default" };
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag(target) && 
            collider.TryGetComponent(out DefendArea other)) 
            other.ReceiveDamage(OnAttacking(other));
    }
}
