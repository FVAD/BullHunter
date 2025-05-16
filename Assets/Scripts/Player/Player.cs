using UnityEngine;
using Bingyan;
using System;

public class Player : FSM
{
    [SerializeField, Title("配置")] private PlayerConfig config;

    private Rigidbody rb;

    public override void Init()
    {
        base.Init();
        rb = GetComponent<Rigidbody>();
    }

    protected override void DefineStates()
    {
        AddState(new MoveState(this));
        AddState(new AttackState(this));
        AddState(new DeathState(this));
    }
    protected override Type GetDefaultState() => typeof(MoveState);

    private abstract class PlayerState : FSMState
    {
        protected new Player Host;
        public PlayerState(Player host) : base(host) => Host = host;

        protected PlayerConfig Config => Host.config;
        protected Rigidbody Rb => Host.rb;
        protected Transform Trans => Host.transform;
    }

    private class MoveState : PlayerState
    {
        Vector3 Forward => CameraManager.Instance.Vir.transform.forward.WithY(0).normalized;
        Vector3 Right => Vector3.Cross(Vector3.up, Forward);

        private Vector3 target;
        private Vector2 input;

        public MoveState(Player host) : base(host) { }

        public override void OnUpdate(float delta)
        {
            input = InputManager.Instance.Actions.InGame.Move.ReadValue<Vector2>();
            target = Forward * input.y + Right * input.x;
        }

        public override void OnFixedUpdate(float delta)
        {
            if (input == Vector2.zero) return;
            Rb.MovePosition(Config.BaseSpeed * delta * target + Trans.position);
            Rb.MoveRotation(Quaternion.RotateTowards(Trans.rotation,
                Quaternion.LookRotation(target, Vector3.up), Config.RotationSpeed));
        }
            
    }

    private class AttackState : PlayerState
    {
        public AttackState(Player host) : base(host) { }
    }

    private class DeathState : PlayerState
    {
        public DeathState(Player host) : base(host) { }
    }
}
