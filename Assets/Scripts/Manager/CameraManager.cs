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

    public Camera Cam => cam;
    public CinemachineVirtualCamera Vir => vir;

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

    public CinemachineOrbitalTransposer Body { get;private set; }
    private float boomLength, currentPitch, targetPitch;

    public override void Init()
    {
        base.Init();

        Player = player;
        Enemy = enemy;

        Body = vir.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        boomLength = Body.m_FollowOffset.magnitude;
        currentPitch = targetPitch = Mathf.Atan(-Body.m_FollowOffset.y / Body.m_FollowOffset.z);

        OnTargetChanged += t => vir.LookAt = t switch
        {
            Target.Player => Player,
            Target.Enemy => Enemy,
            _ => Player
        };
    }

    private void Update()
    {
        var deltaPitch = InputManager.Instance.Actions.InGame.Look.ReadValue<Vector2>().y;
        if (deltaPitch != 0) targetPitch = Mathf.Clamp(currentPitch - deltaPitch, pitchBound.Min * Mathf.Deg2Rad, pitchBound.Max * Mathf.Deg2Rad);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, 0.1f);
        Body.m_FollowOffset = new Vector3(0, Mathf.Sin(currentPitch), -Mathf.Cos(currentPitch)) * boomLength;
    }
}
