using UnityEngine;
using Bingyan;
using System;
using UnityEngine.InputSystem;

public class Player : FSM
{
    [SerializeField, Title("配置")] private PlayerConfig config;

    private PlayerStats stats;
    private class PlayerStats
    {
        public float Stamina { get; set; }
    }

    private Rigidbody rb;

    public override void Init()
    {
        base.Init();

        stats = new PlayerStats
        {
            Stamina = config.MaxStamina
        };

        rb = GetComponent<Rigidbody>();
    }

    protected override void DefineStates()
    {
        AddState(new MoveState(this));
        AddState(new RunState(this));
        AddState(new DodgeState(this));
        AddState(new AttackState(this));
        AddState(new DeathState(this));
    }
    protected override Type GetDefaultState() => typeof(MoveState);

    private abstract class PlayerState : FSMState
    {
        protected new Player Host;
        public PlayerState(Player host) : base(host) => Host = host;

        protected PlayerConfig Config => Host.config;
        protected PlayerStats Stats => Host.stats;

        protected Rigidbody Rb => Host.rb;
        protected Transform Trans => Host.transform;
    }

    private class MoveState : PlayerState
    {
        private Vector3 Target;
        protected Vector2 Input { get; private set; }

        protected virtual float Speed => Config.BaseSpeed;

        public MoveState(Player host) : base(host) { }

        public override void OnEnter() => InputManager.Instance.Actions.InGame.Sprint.started += Accelerate;
        public override void OnExit() => InputManager.Instance.Actions.InGame.Sprint.started -= Accelerate;
        private void Accelerate(InputAction.CallbackContext ctx) => Host.ChangeState<RunState>();

        public override void OnUpdate(float delta)
        {
            Input = InputManager.Instance.Actions.InGame.Move.ReadValue<Vector2>();
            Target = CameraManager.Instance.Forward * Input.y + CameraManager.Instance.Right * Input.x;
        }

        public override void OnFixedUpdate(float delta)
        {
            ProcessPhysics(delta);
            Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);
        }

        protected void ProcessPhysics(float delta)
        {
            Rb.MovePosition(Speed * delta * Target + Trans.position);

            switch (CameraManager.Instance.CurrentTarget)
            {
                case CameraManager.Target.Player:
                    if (Input != Vector2.zero)
                        Rb.MoveRotation(Quaternion.RotateTowards(Trans.rotation,
                            Quaternion.LookRotation(Target, Vector3.up), Config.RotationSpeed));
                    break;
                case CameraManager.Target.Enemy:
                    Rb.MoveRotation(Quaternion.RotateTowards(Trans.rotation,
                        Quaternion.LookRotation(CameraManager.Instance.Forward, Vector3.up), Config.RotationSpeed));
                    break;
            }
        }
    }

    private class RunState : MoveState
    {
        protected override float Speed => Config.SprintSpeed;

        public RunState(Player host) : base(host) { }

        public override void OnEnter() => InputManager.Instance.Actions.InGame.Sprint.canceled += Decelerate;
        public override void OnExit() => InputManager.Instance.Actions.InGame.Sprint.canceled -= Decelerate;
        private void Decelerate(InputAction.CallbackContext ctx) => Host.ChangeState<MoveState>();

        public override void OnFixedUpdate(float delta)
        {
            ProcessPhysics(delta);
            if (Input == Vector2.zero) Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);
            else
            {
                Stats.Stamina = Mathf.Max(Stats.Stamina - Config.SprintStamina * delta, 0);
                if (Stats.Stamina == 0) Host.ChangeState<MoveState>();
            }
        }
    }

    private class DodgeState : PlayerState
    {
        public DodgeState(Player host) : base(host) { }
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
