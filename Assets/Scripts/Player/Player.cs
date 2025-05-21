using UnityEngine;
using Bingyan;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player : FSM
{
    [SerializeField, Title("配置")] private PlayerConfig config;
    [SerializeField, Title("剑区域")] private AttackArea swordArea;
    [SerializeField, Title("枪区域")] private AttackArea lanceArea;
    [SerializeField, Title("布预制")] private GameObject closePrefab;
    [SerializeField, Title("布位置")] private Transform closeRoot;

    public PlayerStats Stats { get; private set; }
    public class PlayerStats
    {
        public float Stamina { get; set; }
        public float CurrentSpeed { get; set; }
        public bool Invulnerable { get; set; }
        public enum Item
        {
            Close,
            Lance,
            Sword,
        }
        public Item CurrentItem { get; set; }
        public Close Close { get; set; }
    }

    private Rigidbody rb;
    private Animator anim;

    public override void Init()
    {
        base.Init();

        Stats = new PlayerStats
        {
            Stamina = config.MaxStamina,
            CurrentSpeed = 0,
            Invulnerable = false,
            CurrentItem = PlayerStats.Item.Sword,
            Close = Instantiate(closePrefab, closeRoot).GetComponent<Close>(),
        };

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        InputManager.Instance.Actions.InGame.Close.started += _ => Stats.CurrentItem = PlayerStats.Item.Close;
        InputManager.Instance.Actions.InGame.Lance.started += _ => Stats.CurrentItem = PlayerStats.Item.Lance;
        InputManager.Instance.Actions.InGame.Sword.started += _ => Stats.CurrentItem = PlayerStats.Item.Sword;

        GetComponentsInChildren<DefendArea>().ForEach(a => a.OnAttacked += (atk, def, f) =>
        {
            if (Stats.Invulnerable) return;
            atk.Active = false;
            ChangeState<DeathState>();
        });
    }

    protected override void Update()
    {
        base.Update();
        Stats.Close.Dict.ForEach(kvp => kvp.Value.Tick(Time.deltaTime));
    }

    protected override void DefineStates()
    {
        AddState(new MoveState(this));
        AddState(new DodgeState(this));
        AddState(new LanceState(this));
        AddState(new CloseState(this));
        AddState(new SwordState(this));
        AddState(new DeathState(this));
    }
    protected override Type GetDefaultState() => typeof(MoveState);

    private abstract class PlayerState : FSMState
    {
        protected new Player Host;
        public PlayerState(Player host) : base(host) => Host = host;

        protected InputActions GetInput => InputManager.Instance.Actions;

        protected PlayerConfig Config => Host.config;
        protected PlayerStats Stats => Host.Stats;

        protected Rigidbody Rb => Host.rb;
        protected Animator Anim => Host.anim;
        protected Transform Trans => Host.transform;

        protected bool TryDodge()
        {
            if (!GetInput.InGame.Dodge.WasPressedThisFrame() ||
                !DodgeState.Ready ||
                Stats.Stamina < Config.DodgeStamina)
                return false;
            Stats.Stamina -= Config.DodgeStamina;
            Host.ChangeState<DodgeState>();
            return true;
        }
    }

    private class MoveState : PlayerState
    {
        private readonly Dictionary<Type, MoveSubstate> substates;
        private MoveSubstate current;

        private void ChangeSubstate<T>() where T : MoveSubstate
        {
            if (substates.TryGetValue(typeof(T), out MoveSubstate state))
            {
                current.OnExit();
                current = state;
                current.OnEnter();
            }
            else Debug.Log($"未定义{typeof(T).Name}");
        }

        public MoveState(Player host) : base(host)
        {
            substates = new Dictionary<Type, MoveSubstate>
            {
                { typeof(IdleState), new IdleState(Host, this) },
                { typeof(WalkState), new WalkState(Host, this) },
                { typeof(RunState), new RunState(Host, this) }
            };
            current = substates[typeof(IdleState)];
        }

        public override void OnEnter() => ChangeSubstate<IdleState>();
        public override void OnExit() => current.OnExit();

        public override void OnUpdate(float delta) => current.OnUpdate(delta);
        public override void OnFixedUpdate(float delta) => current.OnFixedUpdate(delta);

        private abstract class MoveSubstate : PlayerState
        {
            protected Vector2 Input { get; private set; }
            protected Vector3 Target { get; private set; }

            protected MoveState Subhost;
            public MoveSubstate(Player host, MoveState subhost) : base(host) => Subhost = subhost;

            protected abstract float MaxSpeed { get; }
            protected abstract float Delay { get; }

            private TweenHandle handle;

            public override void OnEnter()
            {
                var speed = Stats.CurrentSpeed;
                handle = Tween.Linear(Delay).Once().Process(t => Stats.CurrentSpeed = Mathf.Lerp(speed, MaxSpeed, t)).Build().Play();
            }
            public override void OnExit() => handle.Stop();

            public override void OnUpdate(float delta)
            {
                Input = GetInput.InGame.Move.ReadValue<Vector2>();
                Target = CameraManager.Instance.Forward * Input.y + CameraManager.Instance.Right * Input.x;

                if (TryDodge()) return;
                if (GetInput.InGame.Attack.WasPressedThisFrame())
                {
                    switch (Stats.CurrentItem)
                    {
                        case PlayerStats.Item.Sword:
                            if (Stats.Stamina >= Config.SwordStamina)
                            {
                                Stats.Stamina -= Config.SwordStamina;
                                Host.ChangeState<SwordState>();
                                return;
                            }
                            break;
                        case PlayerStats.Item.Lance:
                            if (Stats.Stamina >= Config.LanceStamina)
                            {
                                Stats.Stamina -= Config.LanceStamina;
                                Host.ChangeState<LanceState>();
                                return;
                            }
                            break;
                    }
                }
                if (Stats.CurrentItem == PlayerStats.Item.Close)
                {
                    Host.ChangeState<CloseState>();
                    return;
                }
                if (Input == Vector2.zero)
                {
                    Subhost.ChangeSubstate<IdleState>();
                    return;
                }
            }

            public override void OnFixedUpdate(float delta)
            {
                Rb.velocity = Stats.CurrentSpeed * Target;

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

        private class IdleState : MoveSubstate
        {
            protected override float MaxSpeed => 0;
            protected override float Delay => Config.IdleDelay;

            public IdleState(Player host, MoveState substate) : base(host, substate) { }

            public override void OnUpdate(float delta)
            {
                Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);

                base.OnUpdate(delta);
                if (Input == Vector2.zero) return;

                if (GetInput.InGame.Sprint.IsPressed()) Subhost.ChangeSubstate<RunState>();
                else Subhost.ChangeSubstate<WalkState>();
            }
        }

        private class WalkState : MoveSubstate
        {
            protected override float MaxSpeed => Config.WalkSpeed;
            protected override float Delay => Config.WalkDelay;

            public WalkState(Player host, MoveState subhost) : base(host, subhost) { }

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
            private void Accelerate(InputAction.CallbackContext ctx) => Subhost.ChangeSubstate<RunState>();

            public override void OnFixedUpdate(float delta)
            {
                base.OnFixedUpdate(delta);
                Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);
            }
        }

        private class RunState : MoveSubstate
        {
            protected override float MaxSpeed => Config.RunSpeed;
            protected override float Delay => Config.RunDelay;

            public RunState(Player host, MoveState subhost) : base(host, subhost) { }

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

            private void Decelerate(InputAction.CallbackContext ctx) => Subhost.ChangeSubstate<WalkState>();

            public override void OnFixedUpdate(float delta)
            {
                base.OnFixedUpdate(delta);
                if (Input == Vector2.zero) Stats.Stamina = Mathf.Min(Config.StaminaRetrive * delta + Stats.Stamina, Config.MaxStamina);
                else
                {
                    Stats.Stamina = Mathf.Max(Stats.Stamina - Config.RunStamina * delta, 0);
                    if (Stats.Stamina == 0) Subhost.ChangeSubstate<WalkState>();
                }
            }
        }
    }

    private class LanceState : MoveState
    {
        private enum Phase
        {
            Startup,
            Judge,
            Recovery,
        }

        private Phase phase;
        private float time, timer;

        public LanceState(Player host) : base(host) { }

        public override void OnEnter()
        {
            base.OnEnter();
            phase = Phase.Startup;
            time = Config.LanceStartup;
            timer = 0;
        }

        public override void OnExit()
        {
            base.OnExit();
            Host.lanceArea.Active = false;
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            switch (phase)
            {
                case Phase.Startup:
                    break;
                case Phase.Judge:
                    break;
                case Phase.Recovery:
                    break;
            }
            if ((timer += delta) <= time) return;
            timer = 0;
            switch (phase)
            {
                case Phase.Startup:
                    time = Config.LanceJudge;
                    phase = Phase.Judge;
                    Host.lanceArea.Active = true;
                    break;
                case Phase.Judge:
                    time = Config.LanceRecovery;
                    phase = Phase.Recovery;
                    Host.lanceArea.Active = false;
                    break;
                case Phase.Recovery:
                    Host.ChangeState<MoveState>();
                    break;
            }
        }
    }

    private class CloseState : MoveState
    {
        public CloseState(Player host) : base(host) { }

        public override void OnEnter()
        {
            base.OnEnter();
            Stats.Close.SetVisible(true);
        }
        public override void OnExit()
        {
            base.OnExit();
            Stats.Close.Tick(-114514);
            Stats.Close.SetVisible(false);
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            Stats.Close.Tick(delta);

            var input = GetInput.InGame.ChangeClose.ReadValue<float>();
            if (input > 0.5f) Stats.Close.Next();
            if (input < -0.5f) Stats.Close.Prev();

            if (Stats.CurrentItem != PlayerStats.Item.Close) Host.ChangeState<MoveState>();
        }
    }

    private class DodgeState : PlayerState
    {
        public static bool Ready { get; private set; } = true;

        private Vector3 orient;
        private float timer = 0;

        public DodgeState(Player host) : base(host) { }

        public override void OnEnter()
        {
            Ready = false;
            Stats.Invulnerable = true;
            timer = 0;

            var input = GetInput.InGame.Move.ReadValue<Vector2>();
            orient = (input == Vector2.zero ? -Trans.forward.WithY(0) :
                CameraManager.Instance.Forward * input.y + CameraManager.Instance.Right * input.x).normalized;

            Rb.velocity = Config.DodgeDistance * orient / Config.DodgeTime;
            Anim.SetTrigger("Dodge");
        }
        public override void OnExit()
        {
            Stats.Invulnerable = false;
            Flow.Create().Delay(Config.DodgeCooldown).Then(() => Ready = true).Run();
        }

        public override void OnFixedUpdate(float delta)
        {
            Rb.MoveRotation(Quaternion.RotateTowards(Trans.rotation,
                Quaternion.LookRotation(orient, Vector3.up), Config.DodgeRotate));
            if ((timer += delta) > Config.DodgeTime) Host.ChangeState<MoveState>();
        }
    }

    private class SwordState : PlayerState
    {
        private enum Phase
        {
            Startup,
            Judge,
            Recovery,
        }

        private Phase phase;
        private float time, timer;

        public SwordState(Player host) : base(host) { }

        public override void OnEnter()
        {
            phase = Phase.Startup;
            time = Config.SwordStartup;
            timer = 0;

            Rb.velocity = Vector2.zero;
        }

        public override void OnExit() => Host.swordArea.Active = false;

        public override void OnUpdate(float delta)
        {
            switch (phase)
            {
                case Phase.Startup:
                    if (TryDodge()) return;
                    break;
                case Phase.Judge:
                    break;
                case Phase.Recovery:
                    if (TryDodge()) return;
                    break;
            }
            if ((timer += delta) <= time) return;
            timer = 0;
            switch (phase)
            {
                case Phase.Startup:
                    time = Config.SwordJudge;
                    phase = Phase.Judge;
                    Host.swordArea.Active = true;
                    break;
                case Phase.Judge:
                    time = Config.SwordRecovery;
                    phase = Phase.Recovery;
                    Host.swordArea.Active = false;
                    break;
                case Phase.Recovery:
                    Host.ChangeState<MoveState>();
                    break;
            }
        }
    }

    private class DeathState : PlayerState
    {
        public DeathState(Player host) : base(host) { }

        public override void OnEnter() => Anim.SetTrigger("Die");
    }
}
