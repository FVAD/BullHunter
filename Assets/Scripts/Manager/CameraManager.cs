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
    [Header("初始设置")]
    [SerializeField, Title("玩家")] private Transform player;
    [SerializeField, Title("敌人")] private Transform enemy;

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

    public override void Init()
    {
        base.Init();

        Player = player;
        Enemy = enemy;

        OnTargetChanged += t => vir.LookAt = t switch
        {
            Target.Player => Player,
            Target.Enemy => Enemy,
            _ => Player
        };
    }
}
