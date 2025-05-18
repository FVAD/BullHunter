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
        public float CurrentSpeed { get; set; }
    }

    private Rigidbody rb;

    public override void Init()
    {
        base.Init();

        stats = new PlayerStats
        {
            Stamina = config.MaxStamina,
            CurrentSpeed = 0
        };

        rb = GetComponent<Rigidbody>();
    }

    protected override void DefineStates()
    {
        AddState(new IdleState(this));
        AddState(new WalkState(this));
        AddState(new RunState(this));
        AddState(new DodgeState(this));
        AddState(new AttackState(this));
        AddState(new DeathState(this));
    }
    protected override Type GetDefaultState() => typeof(IdleState);

    private abstract class PlayerState : FSMState
    {
        protected new Player Host;
        public PlayerState(Player host) : base(host) => Host = host;

        protected InputActions GetInput => InputManager.Instance.Actions;

        protected PlayerConfig Config => Host.config;
        protected PlayerStats Stats => Host.stats;

        protected Rigidbody Rb => Host.rb;
        protected Transform Trans => Host.transform;
    }

    private abstract class MoveState : PlayerState
    {
        protected Vector2 Input { get; private set; }
        protected Vector3 Target { get; private set; }

        protected abstract float MaxSpeed { get; }
        protected abstract float Delay { get; }

        private TweenHandle handle;

        public MoveState(Player host) : base(host) { }

        public override void OnEnter()
        {
            var speed = Stats.CurrentSpeed;
            handle = Tween.Linear(Delay).Once().Process(t => Stats.CurrentSpeed = Mathf.Lerp(speed, MaxSpeed, t)).Build().Play();
        }
        public override void OnExit()
        {
            handle.Stop();
        }

        public override void OnUpdate(float delta)
        {
            Debug.Log(Stats.CurrentSpeed);
            if ((Input = GetInput.InGame.Move.ReadValue<Vector2>()) == Vector2.zero)
            {
                Host.ChangeState<IdleState>();
                return;
            }
            Target = CameraManager.Instance.Forward * Input.y + CameraManager.Instance.Right * Input.x;
        }

        public override void OnFixedUpdate(float delta)
        {
            Rb.MovePosition(Stats.CurrentSpeed * delta * Target + Trans.position);

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

    private class IdleState : MoveState
    {
        protected override float MaxSpeed => 0;
        protected override float Delay => Config.IdleDelay;

        public IdleState(Player host) : base(host) { }

        public override void OnUpdate(float delta)
        {
            Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);

            base.OnUpdate(delta);
            if (Input == Vector2.zero) return;

            if (GetInput.InGame.Sprint.IsPressed()) Host.ChangeState<RunState>();
            else Host.ChangeState<WalkState>();
        }
    }

    private class WalkState : MoveState
    {
        protected override float MaxSpeed => Config.WalkSpeed;
        protected override float Delay => Config.WalkDelay;

        public WalkState(Player host) : base(host) { }

        public override void OnEnter()
        {
            base.OnEnter();
            GetInput.InGame.Sprint.started += Accelerate;
        }
        public override void OnExit()
        {
            base.OnExit();
            GetInput.InGame.Sprint.started -= Accelerate;
        }
        private void Accelerate(InputAction.CallbackContext ctx) => Host.ChangeState<RunState>();

        public override void OnFixedUpdate(float delta)
        {
            base.OnFixedUpdate(delta);
            Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);
        }
    }

    private class RunState : MoveState
    {
        protected override float MaxSpeed => Config.RunSpeed;
        protected override float Delay => Config.RunDelay;

        public RunState(Player host) : base(host) { }

        public override void OnEnter()
        {
            base.OnEnter();
            GetInput.InGame.Sprint.canceled += Decelerate;
        }
        public override void OnExit()
        {
            base.OnExit();
            GetInput.InGame.Sprint.canceled -= Decelerate;
        }

        private void Decelerate(InputAction.CallbackContext ctx) => Host.ChangeState<WalkState>();

        public override void OnFixedUpdate(float delta)
        {
            base.OnFixedUpdate(delta);
            if (Input == Vector2.zero) Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);
            else
            {
                Stats.Stamina = Mathf.Max(Stats.Stamina - Config.RunStamina * delta, 0);
                if (Stats.Stamina == 0) Host.ChangeState<WalkState>();
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
