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

    public Vector3 Forward => CurrentTarget == Target.Player ?
        Vir.transform.forward.WithY(0).normalized : (Enemy.transform.position - Player.transform.position).WithY(0).normalized;
    public Vector3 Right => Vector3.Cross(Vector3.up, Forward);

    [Header("相机")]
    [SerializeField, Title("主相机")] private Camera cam;
    [SerializeField, Title("虚拟相机")] private CinemachineVirtualCamera vir;
    [Header("目标")]
    [SerializeField, Title("玩家")] private Transform player;
    [SerializeField, Title("敌人")] private Transform enemy;
    [Header("自由视角")]
    [SerializeField, Title("Y轴限制")] private FloatRange pitchBound;
    [Header("锁定视角")]
    [SerializeField, Title("敌人偏移")] private Vector2 enemyOffset;
    [SerializeField, Title("跟随速度")] private float followSpeed;

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

    public CinemachineOrbitalTransposer Body { get; private set; }
    private float boomLength, currentPitch, targetPitch;

    public CinemachineComposer Aim { get; private set; }
    private Vector2 playerOffset;

    private CinemachineInputProvider provider;

    public override void Init()
    {
        base.Init();

        Player = player;
        Enemy = enemy;

        Body = Vir.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        boomLength = Body.m_FollowOffset.magnitude;
        currentPitch = targetPitch = Mathf.Atan(-Body.m_FollowOffset.y / Body.m_FollowOffset.z);

        Aim = Vir.GetCinemachineComponent<CinemachineComposer>();
        playerOffset = Aim.m_TrackedObjectOffset;

        provider = Vir.GetComponent<CinemachineInputProvider>();

        OnTargetChanged += t =>
        {
            switch (t)
            {
                case Target.Player:
                    provider.enabled = true;
                    break;
                case Target.Enemy:
                    provider.enabled = false;
                    break;
            }
        };

        InputManager.Instance.Actions.InGame.Target.started += _ => CurrentTarget = (Target)(((int)currentTarget + 1) % Enum.GetValues(typeof(Target)).Length);
    }

    private void Update()
    {
        switch (currentTarget)
        {
            case Target.Player:
                var deltaPitch = InputManager.Instance.Actions.InGame.Look.ReadValue<Vector2>().y;
                if (deltaPitch != 0) targetPitch = Mathf.Clamp(currentPitch - deltaPitch, pitchBound.Min * Mathf.Deg2Rad, pitchBound.Max * Mathf.Deg2Rad);
                currentPitch = Mathf.Lerp(currentPitch, targetPitch, 0.1f);
                Body.m_FollowOffset = new Vector3(0, Mathf.Sin(currentPitch), -Mathf.Cos(currentPitch)) * boomLength;
                break;
            case Target.Enemy:
                var plane = (Player.transform.position - Enemy.transform.position).WithY(0);
                targetPitch = Mathf.Clamp(Mathf.Atan((playerOffset - enemyOffset).magnitude / plane.magnitude), pitchBound.Min * Mathf.Deg2Rad, pitchBound.Max * Mathf.Deg2Rad);
                currentPitch = Mathf.Lerp(currentPitch, targetPitch, 0.1f);
                Body.m_FollowOffset = new Vector3(0, Mathf.Sin(currentPitch), -Mathf.Cos(currentPitch)) * boomLength;

                var targetYaw = Quaternion.LookRotation(Forward, Vector3.up).eulerAngles.y;
                Body.m_XAxis.Value = Mathf.LerpAngle(Body.m_XAxis.Value, targetYaw, followSpeed);

                //var target = Player.transform.position +
                //    Mathf.Sin(pitch) * boomLength * Vector3.up +
                //    Mathf.Cos(pitch) * boomLength * plane.normalized;
                //var delta = target - Vir.transform.position;
                //Vir.transform.position += delta.magnitude > followSpeed ? delta.normalized * followSpeed : delta;
                break;
        }
    }
}
