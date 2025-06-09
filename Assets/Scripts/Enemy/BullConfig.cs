using Bingyan;
using UnityEngine;

[CreateAssetMenu(fileName = "BullConfig", menuName = "Config/Bull")]
public class BullConfig : ScriptableObject
{
    [Header("通用")]
    [SerializeField, Title("受击无敌时间")] private float invulnerableTime = 1f;
    [SerializeField, Title("检测地图边缘距离")] private float checkMapEdgeDistance = 2f;
    [SerializeField, Header("IDLE状态随机调整速度时间（靠近远离， 环绕）")] private float[] idleDirChangeLockTime = new float[2] { 1f, 2f };
    [Header("Bull1基础")]
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

    [Header("Bull1AI")]
    [SerializeField, Title("bull1IDLE保持目标距离")] private float idleMaintainTargetDistance_bull1 = 2f;
    [SerializeField, Title("bull1IDLE保持目标距离偏移值")] private float idleMaintainTargetDistanceOffset_bull1 = 0.3f;
    [SerializeField, Title("bull1IDLE触发大回旋距离")] private float idleTriggerBigCircleDistance_bull1 = 5f;
    [SerializeField, Title("bull1IDLE承伤系数")] private float takeDamageRateIdle_bull1 = 0.85f;
    [SerializeField, Title("bull1IDLE移动方向调整最小时间")] private float idleMoveDirectionAdjustMinTime_bull1 = 5f;
    [SerializeField, Title("bull1IDLE愤怒转换时间")] private float idleAngryConvertTime_bull1 = 30f;
    [SerializeField, Title("bull1IDLE疲劳转换生命值丢失阈值")] private float idleToTiredHealthDeRate_bull1 = 0.1f;
    [SerializeField, Title("bull1Angry调整距离最大时间")] private float angryAdjustMaxTime_bull1 = 10f;
    [SerializeField, Title("bull1VeryAngry承伤系数")] private float takeDamageRateVeryAngry_bull1 = 1.25f;
    [SerializeField, Title("bull1VeryAngry速度加成系数")] private float veryAngrySpeedRate_bull1 = 1.5f;
    [SerializeField, Title("bull1Tired承伤系数")] private float takeDamageRateTired_bull1 = 1.5f;
    [SerializeField, Title("bull1Tired持续时长（s）")] private float tiredDuration_bull1 = 3f;
    [SerializeField, Title("bull1Tired持续时生命值丢失阈值")] private float tiredHealthDeRate_bull1 = 0.2f;
    [SerializeField, Title("bull1犹疑冲刺前摇比率（>100%）")] private float hesitateDashBeforeDelayRate_bull1 = 1.5f;

    [Header("SuperBull基础")]
    [SerializeField, Title("superBull生命值")] private float health_superBull = 100f;
    [SerializeField, Title("superBull受伤倍率")] private float takeDamageRate_superBull = 1f;
    [SerializeField, Title("superBull移动速度")] private float speed_superBull = 5f;
    [SerializeField, Title("superBull攻击力")] private float attackPower_superBull = 10f;
    [SerializeField, Title("superBull冲刺速度")] private float dashSpeed_superBull = 30f;
    [SerializeField, Title("superBull冲刺前摇时间")] private float dashBeforeDelay_superBull = 1f;
    [SerializeField, Title("superBull冲刺后摇时间")] private float dashAfterDelay_superBull = 2f;
    [SerializeField, Title("superBull大回旋旋转速度（°）")] private float bigCircleSpeed_superBull = 180f;
    [SerializeField, Title("superBull大回旋前摇时间")] private float bigCircleBeforeDelay_superBull = 1f;
    [SerializeField, Title("superBull大回旋后摇时间")] private float bigCircleAfterDelay_superBull = 2f;
    [SerializeField, Title("superBull跳跃震击上升时间")] private float jumpAttackRisingDuration_superBull = 2f;
    [SerializeField, Title("superBull跳跃震击下落时间")] private float jumpAttackFallingDuration_superBull = 2f;
    [SerializeField, Title("superBull跳跃震击后摇时间")] private float jumpAttackAfterDelay_superBull = 2f;
    [SerializeField, Title("superBull跳跃震击攻击范围时间")] private float jumpAttackAttackRange_superBull = 5f;

    [Header("SuperBullAI")]
    [SerializeField, Title("superBullIDLE保持目标距离")] private float idleMaintainTargetDistance_superBull = 2f;
    [SerializeField, Title("superBullIDLE保持目标距离偏移值")] private float idleMaintainTargetDistanceOffset_superBull = 0.3f;
    [SerializeField, Title("superBullIDLE触发大回旋距离")] private float idleTriggerBigCircleDistance_superBull = 5f;
    [SerializeField, Title("superBullIDLE承伤系数")] private float takeDamageRateIdle_superBull = 0.85f;
    [SerializeField, Title("superBullIDLE移动方向调整最小时间")] private float idleMoveDirectionAdjustMinTime_superBull = 5f;
    [SerializeField, Title("superBullIDLE愤怒转换时间")] private float idleAngryConvertTime_superBull = 30f;
    [SerializeField, Title("superBullIDLE疲劳转换生命值丢失阈值")] private float idleToTiredHealthDeRate_superBull = 0.1f;
    [SerializeField, Title("superBullAngry尝试冲刺检测距离最小值")] private float angryTryDashDistance_superBull = 6f;
    [SerializeField, Title("superBullAngry调整距离最大时间")] private float angryAdjustMaxTime_superBull = 10f;
    [SerializeField, Title("superBullVeryAngry承伤系数")] private float takeDamageRateVeryAngry_superBull = 1.25f;
    [SerializeField, Title("superBullVeryAngry速度加成系数")] private float veryAngrySpeedRate_superBull = 1.5f;
    [SerializeField, Title("superBullTired承伤系数")] private float takeDamageRateTired_superBull = 1.5f;
    [SerializeField, Title("superBullTired持续时长（s）")] private float tiredDuration_superBull = 3f;
    [SerializeField, Title("superBullTired持续时生命值丢失阈值")] private float tiredHealthDeRate_superBull = 0.2f;
    [SerializeField, Title("superBull犹疑冲刺前摇比率（>100%）")] private float hesitateDashBeforeDelayRate_superBull = 1.5f;
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
    public float CheckMapEdgeDistance => checkMapEdgeDistance;

    public float[] IdleDirChangeLockTime => idleDirChangeLockTime;
    public float IdleAngryConvertTimeBull1 => idleAngryConvertTime_bull1;
    public float IdleToTiredHealthDeRateBull1 => idleToTiredHealthDeRate_bull1;
    public float AngryAdjustMaxTimeBull1 => angryAdjustMaxTime_bull1;
    public float HesitateDashBeforeDelayRateBull1 => hesitateDashBeforeDelayRate_bull1;

    public float HealthSuperBull => health_superBull;
    public float TakeDamageRateSuperBull => takeDamageRate_superBull;
    public float SpeedSuperBull => speed_superBull;
    public float AttackPowerSuperBull => attackPower_superBull;
    public float IdleMaintainTargetDistanceSuperBull => idleMaintainTargetDistance_superBull;
    public float IdleMaintainTargetDistanceOffsetSuperBull => idleMaintainTargetDistanceOffset_superBull;
    public float IdleTriggerBigCircleDistanceSuperBull => idleTriggerBigCircleDistance_superBull;
    public float TakeDamageRateIdleSuperBull => takeDamageRateIdle_superBull;
    public float IdleDirAdjustTimeSuperBull => idleMoveDirectionAdjustMinTime_superBull;
    public float DashSpeedSuperBull => dashSpeed_superBull;
    public float DashBeforeDelaySuperBull => dashBeforeDelay_superBull;
    public float DashAfterDelaySuperBull => dashAfterDelay_superBull;
    public float BigCircleSpeedSuperBull => bigCircleSpeed_superBull;
    public float BigCircleBeforeDelaySuperBull => bigCircleBeforeDelay_superBull;
    public float BigCircleAfterDelaySuperBull => bigCircleAfterDelay_superBull;
    public float JumpAttackRisingDurationSuperBull => jumpAttackRisingDuration_superBull;
    public float JumpAttackFallingDurationSuperBull => jumpAttackFallingDuration_superBull;
    public float JumpAttackAfterDelaySuperBull => jumpAttackAfterDelay_superBull;
    public float JumpAttackAttackRangeSuperBull => jumpAttackAttackRange_superBull;
    public float TakeDamageRateVeryAngrySuperBull => takeDamageRateVeryAngry_superBull;
    public float VeryAngrySpeedRateSuperBull => veryAngrySpeedRate_superBull;
    public float TakeDamageRateTiredSuperBull => takeDamageRateTired_superBull;
    public float TiredDurationSuperBull => tiredDuration_superBull;
    public float AngryDashTryDistanceSuperBull => angryTryDashDistance_superBull;
    public float TiredHealthDeRateSuperBull => tiredHealthDeRate_superBull;
    public float IdleAngryConvertTimeSuperBull => idleAngryConvertTime_superBull;
    public float IdleToTiredHealthDeRateSuperBull => idleToTiredHealthDeRate_superBull;
    public float AngryAdjustMaxTimeSuperBull => angryAdjustMaxTime_superBull;
    public float HesitateDashBeforeDelayRateSuperBull => hesitateDashBeforeDelayRate_superBull;
}