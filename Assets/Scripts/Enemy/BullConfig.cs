using Bingyan;
using UnityEngine;

[CreateAssetMenu(fileName = "BullConfig", menuName = "Config/Bull")]
public class BullConfig : ScriptableObject
{
    [Header("通用")]
    [SerializeField, Title("受击无敌时间")] private float invulnerableTime = 1f;
    [SerializeField, Header("IDLE状态随机调整速度时间（靠近远离， 环绕）")] private float[] idleDirChangeLockTime = new float[2] { 1f, 2f };
    [Header("基础")]
    [SerializeField, Title("bull1生命值")] private float health_bull1 = 100f;
    [SerializeField, Title("bull1受伤倍率")] private float takeDamageRate_bull1 = 1f;
    [SerializeField, Title("bull1移动速度")] private float speed_bull1 = 5f;
    [SerializeField, Title("bull1攻击力")] private float attackPower_bull1 = 10f;
    [SerializeField, Title("bull1冲刺速度")] private float dashSpeed_bull1 = 30f;
    [SerializeField, Title("bull1冲刺前摇时间")] private float dashBeforeDelay_bull1 = 1f;
    [SerializeField, Title("bull1冲刺后摇时间")] private float dashAfterDelay_bull1 = 2f;
    [SerializeField, Title("bull1大回旋旋转速度（°）")] private float bigCircleSpeed_bull1 = 180f;
    [SerializeField, Title("bull1大回旋前摇时间")] private float bigCircleBeforeDelay_bull1 = 1f;
    [SerializeField, Title("bull1大回旋后摇时间")] private float bigCircleAfterDelay_bull1 = 2f;

    [Header("AI")]
    [SerializeField, Title("bull1IDLE保持目标距离")] private float idleMaintainTargetDistance_bull1 = 2f;
    [SerializeField, Title("bull1IDLE保持目标距离偏移值")] private float idleMaintainTargetDistanceOffset_bull1 = 0.3f;
    [SerializeField, Title("bull1IDLE触发大回旋距离")] private float idleTriggerBigCircleDistance_bull1 = 5f;
    [SerializeField, Title("bull1IDLE承伤系数")] private float takeDamageRateIdle_bull1 = 0.85f;
    [SerializeField, Title("bull1IDLE移动方向调整最小时间")] private float idleMoveDirectionAdjustMinTime_bull1 = 5f;
    [SerializeField, Title("bull1IDLE愤怒转换时间")] private float idleAngryConvertTime_bull1 = 30f;
    [SerializeField, Title("bull1VeryAngry承伤系数")] private float takeDamageRateVeryAngry_bull1 = 1.25f;
    [SerializeField, Title("bull1VeryAngry速度加成系数")] private float veryAngrySpeedRate_bull1 = 1.5f;
    [SerializeField, Title("bull1Tired承伤系数")] private float takeDamageRateTired_bull1 = 1.5f;
    [SerializeField, Title("bull1Tired持续时长（s）")] private float tiredDuration_bull1 = 3f;
    [SerializeField, Title("bull1Tired持续时生命值丢失阈值")] private float tiredHealthDeRate_bull1 = 0.2f;

    public float HealthBull1 => health_bull1;
    public float TakeDamageRateBull1 => takeDamageRate_bull1;
    public float SpeedBull1 => speed_bull1;
    public float AttackPowerBull1 => attackPower_bull1;
    public float IdleMaintainTargetDistanceBull1 => idleMaintainTargetDistance_bull1;
    public float IdleMaintainTargetDistanceOffsetBull1 => idleMaintainTargetDistanceOffset_bull1;
    public float IdleTriggerBigCircleDistanceBull1 => idleTriggerBigCircleDistance_bull1;
    public float TakeDamageRateIdleBull1 => takeDamageRateIdle_bull1;
    public float IdleDirAdjustTimeBull1 => idleMoveDirectionAdjustMinTime_bull1;
    public float DashSpeedBull1 => dashSpeed_bull1;
    public float DashBeforeDelayBull1 => dashBeforeDelay_bull1;
    public float DashAfterDelayBull1 => dashAfterDelay_bull1;
    public float BigCircleSpeedBull1 => bigCircleSpeed_bull1;
    public float BigCircleBeforeDelayBull1 => bigCircleBeforeDelay_bull1;
    public float BigCircleAfterDelayBull1 => bigCircleAfterDelay_bull1;
    public float TakeDamageRateVeryAngryBull1 => takeDamageRateVeryAngry_bull1;
    public float VeryAngrySpeedRateBull1 => veryAngrySpeedRate_bull1;
    public float TakeDamageRateTiredBull1 => takeDamageRateTired_bull1;
    public float TiredDurationBull1 => tiredDuration_bull1;
    public float TiredHealthDeRateBull1 => tiredHealthDeRate_bull1;
    public float InvulnerableTime => invulnerableTime;

    public float[] IdleDirChangeLockTime => idleDirChangeLockTime;
    public float IdleAngryConvertTimeBull1 => idleAngryConvertTime_bull1;
}