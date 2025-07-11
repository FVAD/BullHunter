﻿using Bingyan;
using UnityEngine;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(AttackArea))]
public class LanceWeapon : MonoBehaviour
{
    [SerializeField, Title("飞行速度")] private float velocity = 10;
    [SerializeField, Title("飞行时间")] private float time = 0.5f;
    [SerializeField, Title("配置文件")] private PlayerConfig config;

    private float timer = 0;
    private bool move = false;
    private Vector3 orient;

    private AttackArea area;

    public void Activate() => area.Active = true;
    public void Move(Vector3 orient)
    {
        transform.parent = null;
        move = true;
        this.orient = orient.normalized;
    }

    private void Start()
    {
        area = GetComponent<AttackArea>();
        area.OnAttacking += (atk, def) =>
        {
            area.Active = false;
            transform.parent = def.transform;
            // 触发受击事件
            def.ReceiveDamage(atk, def, config.LanceDamage);

            AudioMap.Lance.Hit.Play();
            Addressables.LoadAssetAsync<GameObject>(PathHelper.EFF_BLOOD_LANCE).Completed += handle =>
                Instantiate(handle.Result, atk.HitPoint, Quaternion.identity).GetComponent<ParticleSystem>().Play();
        };
    }

    private void FixedUpdate()
    {
        if (!move || !area.Active) return;
        transform.position += Time.fixedDeltaTime * velocity * orient;
        if ((timer += Time.fixedDeltaTime) <= time) return;
        area.Active = move = false;
        Destroy(gameObject);
    }
}
