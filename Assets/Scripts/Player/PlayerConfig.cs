using Bingyan;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/Player")]
public class PlayerConfig : ScriptableObject
{
    [Header("基础")]
    [SerializeField, Title("精力上限")] private float maxStamina = 90;
    [SerializeField, Title("精力恢复速度")] private float staminaRetrive = 10;
    [Header("移动")]
    [SerializeField, Title("基础速度")] private float baseSpeed = 5;
    [SerializeField, Title("旋转速度")] private float rotateSpeed = 2;
    [Header("冲刺")]
    [SerializeField, Title("冲刺速度")] private float sprintSpeed = 15;
    [SerializeField, Title("精力消耗")] private float sprintStamina = 10;
    [Header("闪避")]
    [SerializeField, Title("无敌时间")] private float invulnerableTime = 0.5f;
    [SerializeField, Title("闪避距离")] private float dodgeDistance = 2;
    [SerializeField, Title("精力消耗")] private float dodgeStamina = 20;
    [SerializeField, Title("冷却时间")] private float dodgeCooldown = 0.5f;

    public float MaxStamina => maxStamina;
    public float StaminaRetrive => staminaRetrive;

    public float BaseSpeed => baseSpeed;
    public float RotationSpeed => rotateSpeed;

    public float SprintSpeed => sprintSpeed;
    public float SprintStamina => sprintStamina;

    public float InvulnerableTime => invulnerableTime;
    public float DodgeDistance => dodgeDistance;
    public float DodgeStamina => dodgeStamina;
    public float DodgeCooldown => dodgeCooldown;
}
