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

    [Header("���")]
    [SerializeField, Title("�����")] private Camera cam;
    [SerializeField, Title("�������")] private CinemachineVirtualCamera vir;
    [Header("��ʼ����")]
    [SerializeField, Title("���")] private Transform player;
    [SerializeField, Title("����")] private Transform enemy;

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
