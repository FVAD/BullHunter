﻿using Bingyan;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/Player")]
public class PlayerConfig : ScriptableObject
{
    [Header("基础")]
    [SerializeField, Title("精力上限")] private float maxStamina = 90;
    [SerializeField, Title("精力恢复速度")] private float staminaRetrive = 10;
    [SerializeField, Title("移动延迟")] private float idleDelay = 0.1f;
    [SerializeField, Title("旋转速度")] private float rotateSpeed = 2;
    [Header("移动")]
    [SerializeField, Title("基础速度")] private float walkSpeed = 5;
    [SerializeField, Title("移动延迟")] private float walkDelay = 0.2f;
    [Header("冲刺")]
    [SerializeField, Title("冲刺速度")] private float runSpeed = 10;
    [SerializeField, Title("精力消耗")] private float runStamina = 10;
    [SerializeField, Title("移动延迟")] private float runDelay = 0.2f;
    [Header("闪避")]
    [SerializeField, Title("无敌时间")] private float dodgeTime = 0.5f;
    [SerializeField, Title("闪避距离")] private float dodgeDistance = 2;
    [SerializeField, Title("精力消耗")] private float dodgeStamina = 20;
    [SerializeField, Title("冷却时间")] private float dodgeCooldown = 0.5f;
    [SerializeField, Title("转向速度")] private float dodgeRotate = 10;
    [Header("剑")]
    [SerializeField, Title("前摇")] private float swordStartup = 0.5f;
    [SerializeField, Title("时长")] private float swordJudge = 0.5f;
    [SerializeField, Title("后摇")] private float swordRecovery = 0.5f;
    [SerializeField, Title("精力")] private float swordStamina = 30;
    [SerializeField, Title("伤害")] private float swordDamage = 514;
    [Header("枪")]
    [SerializeField, Title("前摇")] private float lanceStartup = 0.25f;
    [SerializeField, Title("时长")] private float lanceJudge = 0.5f;
    [SerializeField, Title("后摇")] private float lanceRecovery = 0.25f;
    [SerializeField, Title("精力")] private float lanceStamina = 30;
    [SerializeField, Title("伤害")] private float lanceDamage = 114;

    public float MaxStamina => maxStamina;
    public float StaminaRetrive => staminaRetrive;
    public float IdleDelay => idleDelay;
    public float RotationSpeed => rotateSpeed;

    public float WalkSpeed => walkSpeed;
    public float WalkDelay => walkDelay;

    public float RunSpeed => runSpeed;
    public float RunStamina => runStamina;
    public float RunDelay => runDelay;

    public float DodgeTime => dodgeTime;
    public float DodgeDistance => dodgeDistance;
    public float DodgeStamina => dodgeStamina;
    public float DodgeCooldown => dodgeCooldown;
    public float DodgeRotate => dodgeRotate;

    public float SwordStartup => swordStartup;
    public float SwordJudge => swordJudge;
    public float SwordRecovery => swordRecovery;
    public float SwordStamina => swordStamina;
    public float SwordDamage => swordDamage;

    public float LanceStartup => lanceStartup;
    public float LanceJudge => lanceJudge;
    public float LanceRecovery => lanceRecovery;
    public float LanceStamina => lanceStamina;
    public float LanceDamage => lanceDamage;
}
