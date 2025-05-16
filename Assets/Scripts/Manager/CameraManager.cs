using Bingyan;
using Cinemachine;
using System;
using UnityEngine;

public class CameraManager : ManagerBase<CameraManager>
{
    public enum Target
    {
        Player,
        Enemy,
    }
    public Transform Player { get; set; }
    public Transform Enemy { get; set; }

    [Header("相机")]
    [SerializeField, Title("主相机")] private Camera cam;
    [SerializeField, Title("虚拟相机")] private CinemachineVirtualCamera vir;
    [Header("目标")]
    [SerializeField, Title("玩家")] private Transform player;
    [SerializeField, Title("敌人")] private Transform enemy;
    [Header("视角")]
    [SerializeField, Title("Y轴限制")] private FloatRange pitchBound;
    [SerializeField, Title("灵敏度")] private Vector2 accuracy = new Vector2(1, 0.01f);

    private Target currentTarget;
    public Target CurrentTarget
    {
        get => currentTarget;
        set
        {
            if (currentTarget == value) return;
            OnTargetChanged?.Invoke(currentTarget = value);
        }
    }
    public event Action<Target> OnTargetChanged;

    private CinemachineOrbitalTransposer body;
    private float boomLength, currentPitch;

    public override void Init()
    {
        base.Init();

        Player = player;
        Enemy = enemy;

        body = vir.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        boomLength = body.m_FollowOffset.magnitude;
        currentPitch = Mathf.Atan(-body.m_FollowOffset.y / body.m_FollowOffset.z);
        body.m_XAxis.m_MaxSpeed = accuracy.x;

        OnTargetChanged += t => vir.LookAt = t switch
        {
            Target.Player => Player,
            Target.Enemy => Enemy,
            _ => Player
        };

        InputManager.Instance.Actions.InGame.Look.performed += ctx =>
        {
            currentPitch = Mathf.Clamp(currentPitch - ctx.ReadValue<Vector2>().y * accuracy.y, pitchBound.Min * Mathf.Deg2Rad, pitchBound.Max * Mathf.Deg2Rad);
            body.m_FollowOffset = new Vector3(0, Mathf.Sin(currentPitch), -Mathf.Cos(currentPitch)) * boomLength;
        };
    }
}
