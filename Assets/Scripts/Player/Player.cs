using UnityEngine;
using Bingyan;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class Player : FSM
{
    [SerializeField, Title("配置")] private PlayerConfig config;
    [Header("判定")]
    [SerializeField, Title("剑区域")] private AttackArea swordArea;
    [Header("槽位")]
    [SerializeField, Title("布位置")] private Transform clothSlot;
    [SerializeField, Title("剑槽位")] private Transform swordSlot;
    [SerializeField, Title("枪槽位")] private Transform lanceSlot;
    [SerializeField, Title("后槽位")] private Transform weaponSlot;
    [Header("预制")]
    [SerializeField, Title("布预制")] private GameObject clothPrefab;
    [SerializeField, Title("剑预制")] private GameObject swordPrefab;
    [SerializeField, Title("枪预制")] private GameObject lancePrefab;
    [Header("特效")]
    [SerializeField, Title("走")] private ParticleSystem walkEff;
    [SerializeField, Title("跑")] private ParticleSystem runEff;
    [SerializeField, Title("冲")] private ParticleSystem dodgeEff;

    public PlayerStats Stats { get; private set; }
    public class PlayerStats
    {
        private float stamina;
        public float Stamina
        {
            get => stamina;
            set
            {
                stamina = value;
                OnStaminaChange?.Invoke(stamina);
            }
        }
        public Action<float> OnStaminaChange;

        public float CurrentSpeed { get; set; }
        public bool Invulnerable { get; set; }
        public enum Weapon
        {
            Cloth,
            Lance,
            Sword,
        }
        private Weapon curWeapon;
        public Weapon CurrentWeapon
        {
            get => curWeapon;
            set
            {
                curWeapon = value;
                OnCurrentWeaponChange?.Invoke(curWeapon);
            }
        }

        public Action<Weapon> OnCurrentWeaponChange;
        public ClothWeapon Cloth { get; set; }
        public GameObject WeaponObject { get; set; }
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
            CurrentWeapon = PlayerStats.Weapon.Sword,
            Cloth = Instantiate(clothPrefab, clothSlot).GetComponent<ClothWeapon>(),
            WeaponObject = Instantiate(swordPrefab, weaponSlot),
        };

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        GetComponentsInChildren<DefendArea>().ForEach(a => a.OnAttacked += (atk, def, f) =>
        {
            if (Stats.Invulnerable) return;
            atk.Active = false;
            ChangeState<DeathState>();
        });

        swordArea.OnAttacking += (atk, def) =>
        {
            swordArea.Active = false;
            def.ReceiveDamage(atk, def, config.SwordDamage);

            AudioMap.Sword.Hit.Play();
            Addressables.LoadAssetAsync<GameObject>(PathHelper.EFF_BLOOD_SWORD).Completed += handle =>
                Instantiate(handle.Result, atk.HitPoint, Quaternion.identity).GetComponent<ParticleSystem>().Play();
        };
    }

    protected override void Update()
    {
        base.Update();
        Stats.Cloth.Dict.ForEach(kvp => kvp.Value.Tick(Time.deltaTime));
    }

    protected override void DefineStates()
    {
        AddState(new MoveState(this));
        AddState(new DodgeState(this));
        AddState(new LanceState(this));
        AddState(new ClothState(this));
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
        protected bool TryChange(bool show)
        {
            if (GetInput.InGame.Cloth.WasPressedThisFrame() && Stats.CurrentWeapon != PlayerStats.Weapon.Cloth)
            {
                Stats.CurrentWeapon = PlayerStats.Weapon.Cloth;
                HideWeapon();
                Host.ChangeState<ClothState>();

                AudioMap.Cloth.Change.Play();
                return true;
            }
            if (GetInput.InGame.Sword.WasPressedThisFrame() && Stats.CurrentWeapon != PlayerStats.Weapon.Sword)
            {
                Stats.CurrentWeapon = PlayerStats.Weapon.Sword;
                if (show) ShowWeapon();

                AudioMap.Weapon.Change.Play();
            }
            if (GetInput.InGame.Lance.WasPressedThisFrame() && Stats.CurrentWeapon != PlayerStats.Weapon.Lance)
            {
                Stats.CurrentWeapon = PlayerStats.Weapon.Lance;
                if (show) ShowWeapon();

                AudioMap.Weapon.Change.Play();
            }
            return false;
        }

        protected void ShowWeapon()
        {
            HideWeapon();
            switch (Stats.CurrentWeapon)
            {
                case PlayerStats.Weapon.Sword:
                    Stats.WeaponObject = Instantiate(Host.swordPrefab, Host.weaponSlot);
                    Stats.WeaponObject.GetComponent<WeaponVisualizer>().Show();
                    break;
                case PlayerStats.Weapon.Lance:
                    Stats.WeaponObject = Instantiate(Host.lancePrefab, Host.weaponSlot);
                    Stats.WeaponObject.GetComponent<WeaponVisualizer>().Show();
                    break;
            }
        }
        protected void HideWeapon()
        {
            if (Stats.WeaponObject) Stats.WeaponObject.GetComponent<WeaponVisualizer>().Hide(); //Destroy(Stats.WeaponObject);
            Stats.WeaponObject = null;
        }
    }

    private class MoveState : PlayerState
    {
        private readonly Dictionary<Type, MoveSubstate> substates;
        protected MoveSubstate Current { get; private set; }

        private void ChangeSubstate<T>() where T : MoveSubstate
        {
            if (substates.TryGetValue(typeof(T), out MoveSubstate state))
            {
                if (Current == state) return;
                Current?.OnExit();
                Current = state;
                Current.OnEnter();
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
        }

        protected Vector2 Input { get; private set; }
        protected Vector3 Target { get; private set; }

        public void UpdateMoveInput()
        {
            if ((Input = GetInput.InGame.Move.ReadValue<Vector2>()) == Vector2.zero) ChangeSubstate<IdleState>();
            Target = CameraManager.Instance.Forward * Input.y + CameraManager.Instance.Right * Input.x;
        }

        public override void OnEnter()
        {
            ChangeSubstate<IdleState>();
            ShowWeapon();
        }
        public override void OnExit()
        {
            Current.OnExit();
            HideWeapon();
        }

        public override void OnUpdate(float delta)
        {
            UpdateMoveInput();
            if (TryDodge() || TryChange(true)) return;
            if (Stats.CurrentWeapon == PlayerStats.Weapon.Cloth)
            {
                Host.ChangeState<ClothState>();
                return;
            }
            Current.OnUpdate(delta);
        }
        public override void OnFixedUpdate(float delta) => Current.OnFixedUpdate(delta);

        protected abstract class MoveSubstate : PlayerState
        {
            protected Vector2 Input => Subhost.Input;
            protected Vector3 Target => Subhost.Target;

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
                if (GetInput.InGame.Attack.WasPressedThisFrame())
                {
                    switch (Stats.CurrentWeapon)
                    {
                        case PlayerStats.Weapon.Sword:
                            if (Stats.Stamina >= Config.SwordStamina)
                            {
                                Host.ChangeState<SwordState>();
                                return;
                            }
                            break;
                        case PlayerStats.Weapon.Lance:
                            if (Stats.Stamina >= Config.LanceStamina)
                            {
                                Host.ChangeState<LanceState>();
                                return;
                            }
                            break;
                    }
                }
            }

            public override void OnFixedUpdate(float delta)
            {
                Rb.velocity = Stats.CurrentSpeed * Target;
                var animSpeed = Stats.CurrentSpeed / Config.RunSpeed;

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
                        if (Vector3.Dot(CameraManager.Instance.Forward, Target) < 0) animSpeed *= -1;
                        break;
                }

                Anim.SetFloat("Speed", animSpeed);
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

                AudioMap.Cat.Walk.Play();
                Host.walkEff.Play();
            }
            public override void OnExit()
            {
                base.OnExit();
                GetInput.InGame.Sprint.started -= Accelerate;

                AudioMap.Cat.Walk.Stop();
                Host.walkEff.Stop();
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

                AudioMap.Cat.Run.Play();
                Host.runEff.Play();
            }
            public override void OnExit()
            {
                base.OnExit();
                GetInput.InGame.Sprint.canceled -= Decelerate;

                AudioMap.Cat.Run.Stop();
                Host.runEff.Stop();
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

        private LanceWeapon lance;

        public LanceState(Player host) : base(host) { }

        public override void OnEnter()
        {
            base.OnEnter();
            phase = Phase.Startup;
            time = Config.LanceStartup;
            timer = 0;

            Anim.SetTrigger("Lance");
            HideWeapon();
            lance = Instantiate(Host.lancePrefab,
                                Host.lanceSlot.position,
                                Host.lanceSlot.rotation,
                                Host.transform).GetComponent<LanceWeapon>();
            lance.GetComponent<WeaponVisualizer>().Show();

            Stats.Stamina -= Config.LanceStamina;

            AudioMap.Lance.Use.Play();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (lance && lance.transform.parent == Host.transform) lance.GetComponent<WeaponVisualizer>().Hide();
        }

        public override void OnUpdate(float delta)
        {
            UpdateMoveInput();
            if (TryDodge()) return;
            Current.OnUpdate(delta);
            switch (phase)
            {
                case Phase.Startup:
                    lance.transform.position = Host.lanceSlot.position;
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
                    lance.Activate();
                    break;
                case Phase.Judge:
                    time = Config.LanceRecovery;
                    phase = Phase.Recovery;
                    if (lance.transform.parent == Host.transform) lance.Move(Host.transform.forward);
                    break;
                case Phase.Recovery:
                    Host.ChangeState<MoveState>();
                    break;
            }
        }
    }

    private class ClothState : MoveState
    {
        private bool changed;

        public ClothState(Player host) : base(host) { }

        public override void OnEnter()
        {
            base.OnEnter();
            Stats.Cloth.SetVisible(true);
            changed = false;
        }
        public override void OnExit()
        {
            base.OnExit();
            Stats.Cloth.Refresh();
            Stats.Cloth.SetVisible(false);
        }

        public override void OnUpdate(float delta)
        {
            UpdateMoveInput();
            if (TryDodge()) return;
            TryChange(true);
            Current.OnUpdate(delta);

            Stats.Cloth.Tick(delta);

            var input = GetInput.InGame.ChangeCloth.ReadValue<float>();
            var threshold = 0.5f;
            if (!changed && input > threshold) Stats.Cloth.Next();
            if (!changed && input < -threshold) Stats.Cloth.Prev();
            changed = Mathf.Abs(input) > threshold;

            if (Stats.CurrentWeapon != PlayerStats.Weapon.Cloth) Host.ChangeState<MoveState>();
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

            HideWeapon();

            AudioMap.Cat.Dodge.Play();
            Host.dodgeEff.Play();
        }
        public override void OnExit()
        {
            Stats.Invulnerable = false;
            Flow.Create().Delay(Config.DodgeCooldown).Then(() => Ready = true).Run();

            Host.dodgeEff.Stop();
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

        private WeaponVisualizer sword;

        public SwordState(Player host) : base(host) { }

        public override void OnEnter()
        {
            phase = Phase.Startup;
            time = Config.SwordStartup;
            timer = 0;

            Rb.velocity = Vector2.zero;
            Anim.SetTrigger("Sword");

            HideWeapon();
            sword = Instantiate(Host.swordPrefab, Host.swordSlot).GetComponent<WeaponVisualizer>().Show();

            Stats.Stamina -= Config.SwordStamina;

            AudioMap.Sword.Use.Play();
        }

        public override void OnExit()
        {
            Host.swordArea.Active = false;

            sword.Hide();
        }

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

        public override void OnEnter()
        {
            Anim.SetTrigger("Die");
            AudioMap.Cat.Die.Play();
            AudioMap.Misc.Laugh.Play();
        }
    }
}
