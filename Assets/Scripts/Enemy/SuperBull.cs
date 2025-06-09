using System;
using System.Collections;
using System.Collections.Generic;
using Bingyan;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SuperBull : FSM
{
    [SerializeField, Title("配置")] private BullConfig config;
    [SerializeField, Title("地图中心定位（半径为Render中提到的Length）")] private Transform mapCenter;
    private float mapRadius;
    public BullStats Stats { get; private set; }
    [SerializeField, Title("当前状态")] private string curState;
    public class BullStats
    {
        public float Health { get; set; }
        public float TakeDamageRate { get; set; }
        public float Speed { get; set; }
        public float AttackPower { get; set; }
        public int SwordAttackedCount { get; set; } // 用于记录剑攻击次数
        public int LanceAttackedCount { get; set; } // 用于记录枪攻击次数
        public float InvulnerableTimeCounter { get; set; } // 用于记录无敌时间计数器
        public bool PassionateFlag { get; set; } = false; // 激昂状态Flag
        public float PassionateTimeCounter { get; set; } // 激昂状态时间计数器
        public bool HesitateFlag { get; set; } = false; // 犹疑状态Flag
        public float HesitateTimeCounter { get; set; } // 犹疑状态时间计数器
        public bool MoveAbleFlag { get; set; }  // 移动标志，触发Bull技能时这个标志应当变为False
        public bool DashFlag { get; set; } // 冲刺正在进行标志
        public bool BigCircleFlag { get; set; } // 大回旋正在进行标志
        public bool JumpAttackFlag { get; set; } // 大回旋正在进行标志

    }
    public string GetState()
    {
        return curState;
    }
    private Rigidbody rb;
    private Animator anim;

    public Vector3 Velocity
    {
        get => rb.velocity;
        private set
        {
            // 只设置水平速度，保留y分量
            Vector3 v = value;
            v.y = rb.velocity.y; // 保持当前y速度（受重力影响）
            rb.velocity = v;

            // 旋转SuperBull，只用水平分量
            Vector3 horizontal = new Vector3(value.x, 0, value.z);
            if (horizontal.sqrMagnitude > 0.01f)
            {
                _targetRotation = Quaternion.LookRotation(horizontal.normalized, Vector3.up);
            }
            rb.velocity = value; // 设置新的速度
        }
    }

    private Quaternion _targetRotation;

    public void AddSpeedByRate(float rate, float duration)
    {
        StartCoroutine(AddSpeedByRateInTimeProgress(rate, duration));
    }

    private IEnumerator AddSpeedByRateInTimeProgress(float rate, float duration)
    {
        Stats.Speed *= 1f + rate;
        yield return new WaitForSeconds(duration);
        Stats.Speed /= 1f + rate;
    }

    public void SetPassionateFlag(bool flag = true)
    {
        // 设置SuperBull的激昂状态标志
        Stats.PassionateFlag = flag;
        if (flag)
        {
            Stats.PassionateTimeCounter = 90f; // 重置激昂时间计数器（90s）
            Debug.Log("SuperBull 进入激昂状态");
        }
        else
        {
            Stats.PassionateTimeCounter = 0f; // 清除激昂时间计数器
            Debug.Log("SuperBull 离开激昂状态");
            // 激昂状态一旦结束，SuperBull会进入PrepareState状态
            ChangeState(typeof(PrepareState)); // 进入PrepareState状态
        }
    }

    public void SetHesitateFlag(bool flag = true)
    {
        // 设置SuperBull的犹疑状态标志
        Stats.HesitateFlag = flag;
        if (flag)
        {
            Stats.HesitateTimeCounter = 60f; // 重置犹疑时间计数器（60s）
            Debug.Log("SuperBull 进入犹疑状态");
            // 该状态一进入就会立刻进入Idle
            ChangeState(typeof(IdleState)); // 进入Idle状态
        }
        else
        {
            Stats.HesitateTimeCounter = 0f; // 清除犹疑时间计数器
            Debug.Log("SuperBull 离开犹疑状态");
        }
    }

    protected bool DetectMapEdge()
    {
        // 检测SuperBull是否接近地图边缘，地图定义为mapCenter为中心，scale.y为半径
        // 为了防止SuperBull走出地图边缘，当superBull的移动方向上的一定距离的点超出了范围，则返回True
        Vector3 position = transform.position;
        Vector3 direction = Velocity.normalized; // 获取当前移动方向
        if ((position + direction * config.CheckMapEdgeDistance - mapCenter.position).magnitude > mapRadius)
        {
            // 如果SuperBull接近地图边缘，则返回True，并且调整速度至面向玩家
            Velocity = (player.transform.position - position).normalized * config.SpeedSuperBull; // 面向玩家
            return true;
        }

        return false; // 没有接近地图边缘
    }

    private void RotateToTarget(Quaternion targetRotation)
    {
        // 旋转到目标方向
        if (Quaternion.Angle(rb.rotation, targetRotation) > 0.1f)
        {
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f));
        }
    }

    [SerializeField, Title("玩家引用")] private Player player;

    public override void Init()
    {
        base.Init();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        Stats = new BullStats
        {
            Health = config.HealthSuperBull,
            TakeDamageRate = config.TakeDamageRateSuperBull,
            Speed = config.SpeedSuperBull,
            AttackPower = config.AttackPowerSuperBull,
            SwordAttackedCount = 0,
            LanceAttackedCount = 0,
            InvulnerableTimeCounter = 0f, // 初始化无敌时间计数器
            DashFlag = false,
            BigCircleFlag = false,
            MoveAbleFlag = true,
            PassionateFlag = false,
            HesitateFlag = false,
            PassionateTimeCounter = 0f,
            HesitateTimeCounter = 0f,
        };


        // 获取地图半径
        MapRadiusRender mapRadiusRender = mapCenter.GetComponent<MapRadiusRender>();
        if (mapRadiusRender == null) Debug.LogError("mapCenter对象应当挂载以一个MapRadiusRender脚本，这个脚本用于获取地图半径。");
        mapRadius = mapRadiusRender.LineLength; // 获取地图半径

        GetComponentInChildren<DefendArea>().OnAttacked += (atk, def, f) =>
        {
            // 处理受击逻辑
            if (Stats.InvulnerableTimeCounter > 0f) return;
            Stats.InvulnerableTimeCounter = config.InvulnerableTime; // 设置无敌时间

            var damage = f * Stats.TakeDamageRate;

            if (atk.GetComponent<LanceWeapon>() != null)
            {
                Stats.LanceAttackedCount++;
                Debug.Log($"Bull1 被枪攻击次数：{Stats.LanceAttackedCount}");
            }
            else
            {
                Stats.SwordAttackedCount++;
                damage *= 1.0f + Stats.LanceAttackedCount * 0.2f;
                Debug.Log($"Bull1 被剑攻击次数：{Stats.SwordAttackedCount}");
            }

            Stats.Health -= damage;
            Debug.Log($"Bull1 受到了 {damage} 点伤害，当前生命值：{Stats.Health}");

            if (Stats.Health <= 0)
            {
                AudioMap.Misc.Otto.Play();
                Debug.Log("SuperBull 死亡");
                SceneLoader.To("Start", 5);
                enabled = false;
            }
        };

        Flow.Create()
            .Delay(1)
            .Then(() => GetComponentsInChildren<AttackArea>().ForEach(a =>
            {
                a.Active = false; // 攻击区域初始关闭
                a.OnAttacking += (atk, def) =>
                {
                    def.ReceiveDamage(atk, def, 0);
                    Debug.Log($"{atk.name}对{def.name}发起攻击，伤害为0");
                };
            }))
            .Run();

        AudioMap.Bull.Roar.Play();
    }
    protected void SetAttackAreaIsActive(bool flag)
    {
        GetComponentsInChildren<AttackArea>().ForEach(a => a.Active = flag);
    }

    protected override void Update()
    {
        base.Update();
        RotateToTarget(_targetRotation); // 旋转到目标方向

        if (Stats.InvulnerableTimeCounter > 0f)
        {
            // 如果无敌时间计数器大于0，则减少计数器
            Stats.InvulnerableTimeCounter -= Time.deltaTime;
            if (Stats.InvulnerableTimeCounter < 0f)
            {
                Stats.InvulnerableTimeCounter = 0f; // 确保计数器不小于0
            }
        }

        // 激昂和犹疑计时器变化
        if (Stats.PassionateFlag)
        {
            // 如果SuperBull处于激昂状态，则减少激昂时间计数器
            Stats.PassionateTimeCounter -= Time.deltaTime;
            if (Stats.PassionateTimeCounter <= 0f)
            {
                // 激昂时间结束，退出激昂状态
                SetPassionateFlag(false);
            }
        }
        if (Stats.HesitateFlag)
        {
            // 如果SuperBull处于犹疑状态，则减少犹疑时间计数器
            Stats.HesitateTimeCounter -= Time.deltaTime;
            if (Stats.HesitateTimeCounter <= 0f)
            {
                // 犹疑时间结束，退出犹疑状态
                SetHesitateFlag(false);
            }
        }
    }

    protected override void DefineStates()
    {
        AddState(new IdleState(this));
        AddState(new AngryState(this));
        AddState(new VeryAngryState(this));
        AddState(new TiredState(this));
        AddState(new PrepareState(this));
    }

    protected override Type GetDefaultState() => typeof(IdleState);

    private class BullState : FSMState
    {
        protected new SuperBull Host;

        /// <summary>
        /// 构造函数，传入主机对象
        /// </summary>
        /// <param name="host"></param>
        public BullState(SuperBull host) : base(host) => Host = host;
        protected BullStats Stats => Host.Stats;
        protected BullConfig Config => Host.config;

        protected Rigidbody Rb => Host.rb;
        protected Animator Anim => Host.anim;
        protected Transform Trans => Host.transform;
        protected Player PlayerRef => Host.player;
        protected bool healthFirstLowerThan20p = false; // 用于标记是否第一次生命值低于20%
        protected int angryStateCounter = 0; // 愤怒状态计数器，记录一共愤怒次数，用于检测是否进入红温状态



        protected Coroutine dashCoroutine; // 冲刺协程

        protected Coroutine bigCircleCoroutine; // 大回旋攻击协程
        protected Coroutine jumpAttackCoroutine; // 跳跃震击协程

        // SuperBull 跳跃震击攻击逻辑
        protected void JumpAttack(Action onComplete = null)
        {
            if (Stats.JumpAttackFlag)
            {
                // 如果已经在跳跃震击中，则不再触发新的跳跃震击
                return;
            }
            Debug.Log("跳跃震击触发");

            Stats.JumpAttackFlag = true;
            Stats.MoveAbleFlag = false; // 禁止移动
            Host.Velocity = Vector3.zero; // 停止当前速度

            if (jumpAttackCoroutine != null)
            {
                Host.StopCoroutine(jumpAttackCoroutine); // 如果已有跳跃震击协程在运行，则停止它
                jumpAttackCoroutine = null; // 清空跳跃震击协程引用
            }
            jumpAttackCoroutine = Host.StartCoroutine(JumpAttackCoroutine(onComplete)); // 启动跳跃震击协程
        }
        protected IEnumerator JumpAttackCoroutine(Action onComplete = null)
        {
            Vector3 startPoint = Host.transform.position;

            RingEffectManager.Instance.SpawnRing(startPoint, Config.JumpAttackAttackRangeSuperBull, Config.JumpAttackRisingDurationSuperBull + Config.JumpAttackFallingDurationSuperBull, Color.red);
            // 高高跃起
            // 下面的内容设重力加速度9.8
            float g = 9.8f;
            // 上升时间决定初速
            float riseInitSpeed = g * Config.JumpAttackRisingDurationSuperBull;
            // x = 1 / 2 g * t^2
            float timer = 0f;
            float x = 0f;
            while (timer < Config.JumpAttackRisingDurationSuperBull)
            {
                x = 0.5f * g * timer * timer;
                timer += Time.deltaTime;
                Host.transform.position = startPoint + Vector3.up * x;
                yield return null;
            }

            // 归位
            Host.transform.position = startPoint + 0.5f * g * Mathf.Pow(Config.JumpAttackRisingDurationSuperBull, 2f) * Vector3.up;

            // 进入下落阶段
            timer = 0f;
            Vector3 halfPoint = Host.transform.position;
            while (timer < Config.JumpAttackFallingDurationSuperBull)
            {
                x = 0.5f * g * timer * timer;
                timer += Time.deltaTime;
                Host.transform.position = halfPoint - Vector3.up * x;
                yield return null;
            }

            // 归位
            Host.transform.position = startPoint;

            // 造成伤害，伤害半径见配置
            Vector3 toPlayer = PlayerRef.transform.position - Host.transform.position;
            toPlayer.y = 0f;
            if (toPlayer.magnitude < Config.JumpAttackAttackRangeSuperBull)
            {
                // 造成伤害
                PlayerRef.GetComponentInChildren<DefendArea>().ReceiveDamage(Host.GetComponentInChildren<AttackArea>(), PlayerRef.GetComponentInChildren<DefendArea>(), 0f);
            }

            // 后摇
            yield return new WaitForSeconds(Config.JumpAttackAfterDelaySuperBull);

            Stats.JumpAttackFlag = false;
            Stats.MoveAbleFlag = true; // 恢复移动能力

            onComplete?.Invoke();
        }

        // SuperBull 大回旋攻击逻辑
        protected void BigCircleAttack(Action onComplete = null)
        {
            // 这里可以实现大回旋攻击的逻辑
            // 比如检测玩家位置，计算攻击范围等
            // 然后触发动画和伤害逻辑
            // Anim.SetTrigger("BigCircleAttack");

            if (Stats.BigCircleFlag)
            {
                // 如果已经在大回旋攻击中，则不再触发新的大回旋攻击
                return;
            }
            Debug.Log("大回旋攻击触发");
            Stats.BigCircleFlag = true; // 设置大回旋攻击标志
            Stats.MoveAbleFlag = false; // 禁止移动
            Host.Velocity = Vector3.zero; // 停止当前速度

            if (bigCircleCoroutine != null)
            {
                Host.StopCoroutine(bigCircleCoroutine); // 如果已有大回旋攻击协程在运行，则停止它
                bigCircleCoroutine = null; // 清空大回旋攻击协程引用
            }
            bigCircleCoroutine = Host.StartCoroutine(BigCircleAttackCoroutine(onComplete)); // 启动大回旋攻击协程
        }

        protected IEnumerator BigCircleAttackCoroutine(Action onComplete = null)
        {
            // 大回旋攻击协程
            Debug.Log("开始大回旋攻击");
            RingEffectManager.Instance.SpawnRing(Host.transform.position, RingEffectManager.CalculateMaxCoverRadius(Host.GetComponentInChildren<AttackArea>().GetComponent<BoxCollider>()), Config.BigCircleBeforeDelaySuperBull, Color.blue);
            yield return new WaitForSeconds(Config.BigCircleBeforeDelaySuperBull); // 前摇时间

            // 攻击区域激活
            Host.SetAttackAreaIsActive(true);


            // 开始旋转
            float rotateSpeed = Config.BigCircleSpeedSuperBull * Mathf.Deg2Rad; // 转换为弧度

            // 旋转360度
            float totalRotation = 0f;
            while (totalRotation < 360f)
            {
                float deltaRotation = rotateSpeed * Time.deltaTime; // 每帧旋转的角度
                totalRotation += deltaRotation * Mathf.Rad2Deg; // 累计旋转角度
                Host.transform.Rotate(Vector3.up, deltaRotation * Mathf.Rad2Deg); // 旋转SuperBull
                yield return null; // 等待下一帧
            }

            // 攻击区域关闭
            Host.SetAttackAreaIsActive(false);
            Stats.BigCircleFlag = false;
            Stats.MoveAbleFlag = true; // 恢复移动能力
            onComplete?.Invoke();
        }

        protected void DashAttack(Action onComplete = null)
        {
            // 这里可以实现冲刺攻击的逻辑
            // 比如检测玩家位置，计算冲刺方向和速度等
            // 然后触发动画和伤害逻辑
            // Anim.SetTrigger("DashAttack");

            if (Stats.DashFlag)
            {
                // 如果已经在冲刺中，则不再触发新的冲刺
                // Debug.Log("SuperBull 正在冲刺中，无法再次触发冲刺攻击");
                return;
            }

            Debug.Log("冲刺攻击触发");
            Stats.DashFlag = true; // 设置冲刺标志
            Stats.MoveAbleFlag = false; // 禁止移动
            Host.Velocity = Vector3.zero; // 停止当前速度
            if (dashCoroutine != null) // 不一定需要
            {
                Host.StopCoroutine(dashCoroutine); // 如果已有冲刺协程在运行，则停止它
                dashCoroutine = null; // 清空冲刺协程引用
            }
            Vector3 targetPosition = PlayerRef.transform.position; // 获取玩家位置作为目标位置
            dashCoroutine = Host.StartCoroutine(DashAttackCoroutine(targetPosition, onComplete)); // 启动冲刺攻击协程
        }

        protected IEnumerator DashAttackCoroutine(Vector3 targetPosition, Action onComplete = null)
        {
            // 这里可以实现冲刺攻击的协程逻辑
            // 比如计算冲刺方向和速度，处理前摇和后摇等
            // Anim.SetTrigger("DashAttack");

            // 小巧思
            Host.Velocity = PlayerRef.transform.position - Host.transform.position;
            Host.Velocity = Vector3.zero;

            AudioMap.Bull.Warning.Play();

            Debug.Log("开始冲刺攻击");
            if (Stats.HesitateFlag)
            {
                // 如果处于犹疑状态，则前摇时间增加
                yield return new WaitForSeconds(Config.DashBeforeDelaySuperBull * Config.HesitateDashBeforeDelayRateSuperBull);
            }
            else
            {
                yield return new WaitForSeconds(Config.DashBeforeDelaySuperBull); // 前摇时间
            }

            // 攻击区域激活
            Host.SetAttackAreaIsActive(true);

            Vector3 direction = (targetPosition - Host.transform.position).normalized;
            direction.y = 0; // 保持水平运动
            Vector3 startPosition = Host.transform.position; // 记录运动距离

            // 进入冲刺
            Host.Velocity = direction * Config.DashSpeedSuperBull; // 设置冲刺速度
            float targetDistance = new Vector3(targetPosition.x - startPosition.x, 0, targetPosition.z - startPosition.z).magnitude; // 计算目标距离

            Vector3 finishedDistance = Host.transform.position - startPosition; // 计算当前位置与起始位置的距离
            while (targetDistance - finishedDistance.magnitude > 0.1f)
            {
                // Rb.MovePosition(Host.transform.position + direction * Config.DashSpeedSuperBull * Time.deltaTime);
                finishedDistance = Host.transform.position - startPosition;
                yield return null; // 等待下一帧
            }

            // 减小速度至0,时间固定为DashStopTime
            float timer = 0f;
            float newSpeed;
            while (timer < Config.DashStopTime)
            {
                timer += Time.deltaTime;
                if (timer > Config.DashStopTime) timer = Config.DashStopTime; // 确保timer
                newSpeed = Mathf.SmoothStep(Config.DashSpeedBull1, 0f, timer / Config.DashStopTime);
                Host.Velocity = direction * newSpeed;
                yield return null; // 等待下一帧
            }

            Host.Velocity = Vector3.zero; // 停止冲刺
            // 冲刺结束，进入后摇
            Debug.Log("冲刺攻击结束");

            // 攻击区域关闭
            Host.SetAttackAreaIsActive(false);

            if (Stats.PassionateFlag)
            {
                yield return new WaitForSeconds(Config.DashAfterDelaySuperBull / 2f); // 后摇时间，激昂时减半
            }
            else if (Stats.HesitateFlag)
            {
                // 如果处于犹疑状态，则后摇时间略微缩短
                yield return new WaitForSeconds(Config.DashAfterDelaySuperBull * 3f / 4f);
            }
            else
            {
                yield return new WaitForSeconds(Config.DashAfterDelaySuperBull); // 后摇时间
            }

            Stats.DashFlag = false;
            Stats.MoveAbleFlag = true; // 恢复移动能力
            onComplete?.Invoke(); // 处理后续事件
        }
    }

    private class PrepareState : BullState
    {
        private float prepareTimer; // 整备状态计时器
        private float realPrepareTimer;
        private float dirChangeLockTimer; // 用于锁定方向变化的计时器


        public PrepareState(SuperBull host) : base(host)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();
            Host.curState = "准备";
            prepareTimer = 30f; // 初始会有最大时间30s，这个时间同时还会收到玩家攻击造成伤害的影响
            realPrepareTimer = 0f;

            Stats.Speed = Stats.Speed * 0.8f; // 速度减小20%

            Host.GetComponentInChildren<DefendArea>().OnAttacked += PrepareSpecialOnAttackedEvent;
        }

        private void PrepareSpecialOnAttackedEvent(AttackArea area1, DefendArea area2, float arg3)
        {
            if (prepareTimer > 0f)
            {
                // 这里设定的是 每次受击都会让计时器减去当次实际受伤的2%数值的秒
                prepareTimer -= arg3 * Stats.TakeDamageRate * 0.02f;
                if (prepareTimer < 0f) prepareTimer = 0f;
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log($"SuperBull退出了整备阶段，这一阶段内停留了{realPrepareTimer}秒，之后的1分钟内移速提升{5f * realPrepareTimer}%");
            Host.AddSpeedByRate(1f + 0.05f * realPrepareTimer, 60f); // 速度增加，应该能叠速度
            Host.GetComponentInChildren<DefendArea>().OnAttacked -= PrepareSpecialOnAttackedEvent;
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);

            realPrepareTimer += delta;
            prepareTimer -= delta;

            if (prepareTimer == 0f && realPrepareTimer >= 5f)
            {
                // 至少经过5s切换到愤怒状态
                Host.ChangeState(typeof(AngryState));
            }

            if (!Host.DetectMapEdge())
            {
                MoveAwayFromPlayer();
            }
            else
            {
                Vector3 direction = -(Host.transform.position - PlayerRef.transform.position).normalized;
                Host.Velocity = direction * Stats.Speed;
                // 进入愤怒状态
                Host.ChangeState(typeof(AngryState));
            }
        }

        private void MoveAwayFromPlayer()
        {
            // 尽可能远离Player
            // 方向需要在计数器归零的时候改变
            if (dirChangeLockTimer > 0f)
            {
                dirChangeLockTimer -= Time.deltaTime;
                return;
            }
            dirChangeLockTimer = Config.IdleDirChangeLockTime[1]; // 重置方向变化锁定时间
            Vector3 direction = (Host.transform.position - PlayerRef.transform.position).normalized;
            Host.Velocity = direction * Stats.Speed;
        }
    }

    private class IdleState : BullState
    {
        private float idleDirFlag; // 用于控制空闲状态下的运动方向
        private Coroutine getDirFlagCoroutine;
        private float idleDirAdjustTime;
        private float dirChangeLockTimer; // 用于锁定方向变化的计时器
        private float angryTransitionTimer = 0f; // 愤怒状态转换计时器
        private float lastDamage; // 上次受到的伤害，用于判断是否需要转换到红温状态
        private float passionateTransToAngryTimer = 0f; // 激昂状态转换到愤怒状态的计时器

        private IEnumerator getDirFlag()
        {
            while (true)
            {
                idleDirFlag = UnityEngine.Random.Range(-1f, 1f);
                yield return new WaitForSeconds(idleDirAdjustTime); // 更新一次
            }
        }
        public IdleState(SuperBull host) : base(host)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();
            Host.curState = "空闲";
            // Anim.SetTrigger("Idle");
            // IDLE状态下承伤系数为85%
            Stats.TakeDamageRate = Config.TakeDamageRateIdleSuperBull;
            idleDirAdjustTime = Config.IdleDirAdjustTimeSuperBull;

            dirChangeLockTimer = 0f;
            idleDirFlag = 0f;
            angryTransitionTimer = Config.IdleAngryConvertTimeSuperBull; // 设置愤怒状态转换计时器

            // 添加额外的受伤操作
            Host.GetComponentInChildren<DefendArea>().OnAttacked += IdleSpecialOnAttackEvent;

            // 设置激昂状态下的愤怒计时器
            passionateTransToAngryTimer = 5f;
            // 启动获取方向的协程
            getDirFlagCoroutine = Host.StartCoroutine(getDirFlag());


        }

        private void IdleSpecialOnAttackEvent(AttackArea area1, DefendArea area2, float arg3)
        {
            if (Stats.InvulnerableTimeCounter > 0f) return;
            Stats.InvulnerableTimeCounter = Config.InvulnerableTime; // 设置无敌时间
            angryTransitionTimer = Config.IdleAngryConvertTimeSuperBull; // 重置愤怒状态转换计时器

            lastDamage = arg3 * Stats.TakeDamageRate; // 记录上次受到的伤害
        }

        public override void OnExit()
        {
            base.OnExit();
            // 退出IDLE状态时恢复承伤系数
            Stats.TakeDamageRate = Config.TakeDamageRateSuperBull;
            // 停止获取方向的协程
            if (getDirFlagCoroutine != null)
            {
                Host.StopCoroutine(getDirFlagCoroutine);
                getDirFlagCoroutine = null;
            }

            Host.GetComponentInChildren<DefendArea>().OnAttacked -= IdleSpecialOnAttackEvent;
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            // 处理空闲状态逻辑

            if (dirChangeLockTimer > 0f)
            {
                // 如果方向变化被锁定，则减少锁定计时器
                dirChangeLockTimer -= delta;
                if (dirChangeLockTimer < 0f)
                {
                    dirChangeLockTimer = 0f; // 确保计时器不小于0
                }
            }
            else
            {
                DetectPlayerDistance();
            }

            if (angryTransitionTimer > 0) angryTransitionTimer -= delta;
            else
            {
                angryTransitionTimer = 0f; // 确保计时器不小于0
            }


        }

        private void DetectPlayerDistance()
        {
            // 检测玩家距离
            float distance = Vector3.Distance(PlayerRef.transform.position, Host.transform.position);

            if (distance < Config.IdleTriggerBigCircleDistanceSuperBull)
            {
                // 如果距离小于触发大回旋攻击的距离，则触发大回旋攻击
                BigCircleAttack(() => TransToAngryState()); // 同时愤怒
            }
            else if (Stats.MoveAbleFlag)
            {
                // 否则，维持当前状态
                MaintainDistanceFromPlayer(distance);
            }

            if (DetectTransToAngryState())
            {
                // 检测是否转换到愤怒状态
                TransToAngryState();
            }

            if (DetectTransToVeryAngryState())
            {
                // 检测是否转换到红温状态
                TransToVeryAngryState();
            }

            if (DetectTransToTiredState())
            {
                // 检测是否转换到疲劳状态
                TransToTiredState();
            }
        }

        #region Idle状态转换检测
        private bool DetectTransToAngryState()
        {
            // 检测是否转换到愤怒状态
            // 这里可以根据具体逻辑判断是否需要转换状态
            // 比如检测玩家位置、血量等

            // 1. 攻击次数检测
            if (Stats.SwordAttackedCount >= 1 || Stats.LanceAttackedCount > 3)
            {
                // 如果被剑攻击次数大于等于1次，或者被枪攻击次数大于3次，则转换到愤怒状态
                Debug.Log("SuperBull 攻击次数条件达成，转换到愤怒状态");
                Stats.SwordAttackedCount = 0;
                Stats.LanceAttackedCount = 0;
                return true;
            }

            // 2. 愤怒转换计时器检测
            if (angryTransitionTimer <= 0f)
            {
                // 如果愤怒转换计时器小于等于0，则转换到愤怒状态
                Debug.Log("SuperBull 愤怒转换计时器到达，转换到愤怒状态");
                return true;
            }

            // 2.5 激昂状态的计时器
            if (Stats.PassionateFlag)
            {
                // 如果SuperBull处于激昂状态，则减少激昂时间计数器
                passionateTransToAngryTimer -= Time.deltaTime;
                if (passionateTransToAngryTimer <= 0f)
                {
                    // 激昂时间结束，转换到愤怒状态
                    Debug.Log("SuperBull 激昂状态愤怒转换计时器到达，转换到愤怒状态");
                    return true;
                }
            }

            // 3. 场地边缘检测（目前未定义场地边缘，暂不实现）
            if (Host.DetectMapEdge()) // 这里可以添加具体的场地边缘检测逻辑
            {
                // 如果SuperBull接近场地边缘，则转换到愤怒状态
                Debug.Log("SuperBull 接近场地边缘，转换到愤怒状态");
                return true;
            }

            // 4. 玩家位置检测

            return false;
        }

        private void TransToAngryState()
        {
            // 转换到愤怒状态
            // 这里可以实现转换状态的逻辑
            // 比如触发动画、改变状态机等
            Debug.Log("转换到愤怒状态");
            Host.ChangeState(typeof(AngryState));
        }

        private bool DetectTransToVeryAngryState()
        {
            // 检测是否转换到红温状态
            // 这里可以根据具体逻辑判断是否需要转换状态
            // 比如检测玩家位置、血量等
            // 0. 首先不能是犹疑状态
            if (Stats.HesitateFlag)
            {
                // 如果处于犹疑状态，则不转换到红温状态
                return false;
            }

            // 1. 伤害检测
            if (lastDamage >= (Host.Stats.Health + lastDamage) / 2f)
            {
                // 如果单次受到伤害超过半数生命值，则转换到红温状态
                Debug.Log("SuperBull 单次受到伤害超过半数生命值，转换到红温状态");
                return true;
            }

            return false;
        }

        private void TransToVeryAngryState()
        {
            // 转换到红温状态
            // 这里可以实现转换状态的逻辑
            // 比如触发动画、改变状态机等
            Debug.Log("转换到红温状态");
            Host.ChangeState(typeof(VeryAngryState));
        }

        private bool DetectTransToTiredState()
        {
            // 检测是否转换到疲劳状态
            // 这里可以根据具体逻辑判断是否需要转换状态
            // 比如检测玩家位置、血量等

            // 1. 生命值检测
            if (Stats.Health <= Config.HealthSuperBull * Config.IdleToTiredHealthDeRateSuperBull)
            {
                // 如果生命值低于一定比例，则转换到疲劳状态
                Debug.Log($"SuperBull 生命值低于{Config.IdleToTiredHealthDeRateSuperBull}阈值，转换到疲劳状态");
                return true;
            }

            return false;
        }

        private void TransToTiredState()
        {
            // 转换到疲劳状态
            // 这里可以实现转换状态的逻辑
            // 比如触发动画、改变状态机等
            Debug.Log("转换到疲劳状态");
            Host.ChangeState(typeof(TiredState));
        }

        #endregion

        private void MaintainDistanceFromPlayer(float distance)
        {
            Vector3 direction;
            // 维持SuperBull与玩家距离
            if (distance > Config.IdleMaintainTargetDistanceSuperBull + Config.IdleMaintainTargetDistanceOffsetSuperBull)
            {
                // 如果距离小于维持距离，则SuperBull尝试远离玩家
                direction = (PlayerRef.transform.position - Host.transform.position).normalized;
                dirChangeLockTimer = Config.IdleDirChangeLockTime[0]; // 锁定方向变化的时间
            }
            else if (distance < Config.IdleMaintainTargetDistanceSuperBull - Config.IdleMaintainTargetDistanceOffsetSuperBull)
            {
                // 如果距离大于维持距离，则SuperBull尝试靠近玩家
                direction = (Host.transform.position - PlayerRef.transform.position).normalized;
                dirChangeLockTimer = Config.IdleDirChangeLockTime[0]; // 锁定方向变化的时间
            }
            else
            {
                // 如果距离在这个区间内，则SuperBull尝试围绕玩家运动并保持距离
                // 拿到一个圆周运动方向，圆周方向随机
                direction = (Vector3.Cross(PlayerRef.transform.position - Host.transform.position, Vector3.up) * (idleDirFlag < 0 ? -1f : 1f)).normalized;
                dirChangeLockTimer = Config.IdleDirChangeLockTime[1]; // 锁定方向变化的时间
            }
            direction.y = 0; // 保持水平运动
            Host.Velocity = direction * Stats.Speed; // 设置SuperBull的速度
        }
    }
    private class AngryState : BullState
    {

        private bool adjustDistanceFlag = false; // 是否需要调整距离的标志
        private bool prepareToDashFlag; // 是否准备冲刺，如果是否则表示尝试进行跳跃震击
        private float adjustDistanceTimer = 0f; // 调整距离的计时器
        private int maxCounter = 3;
        public AngryState(SuperBull host) : base(host)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            // 计数器+1
            angryStateCounter++;
            Host.curState = "愤怒";

            // Anim.SetTrigger("Angry");
            // 愤怒状态下承伤系数为100%
            Stats.TakeDamageRate = Config.TakeDamageRateSuperBull;
            // 这里可以实现愤怒状态的逻辑，比如增加攻击力、改变行为等
            Debug.Log("进入愤怒状态");
            adjustDistanceFlag = true;
            adjustDistanceTimer = Config.AngryAdjustMaxTimeSuperBull; // 设置调整距离的最大时间
            SetPrepareToDashFlag();

            // 如果愤怒状态计数器大于3，且不犹疑，则转换到红温状态
            if (!Stats.HesitateFlag && angryStateCounter > maxCounter)
            {
                Debug.Log("SuperBull 愤怒状态计数器超过3，转换到红温状态");
                Host.ChangeState(typeof(VeryAngryState));
                angryStateCounter = 0; // 重置愤怒状态计数器
                return; // 直接转换状态，不再执行后续逻辑
            }

            AudioMap.Bull.Roar.Play();
        }

        private bool SetPrepareToDashFlag()
        {
            // 检测玩家距离
            Vector3 bullToPlayer = Host.transform.position - PlayerRef.transform.position;
            bullToPlayer.y = 0;
            float distanceToPlayer = bullToPlayer.magnitude;
            // 如果状态发生了切换，那么重置计时器时间
            if (!(distanceToPlayer < Config.AngryDashTryDistanceSuperBull && !prepareToDashFlag)) adjustDistanceTimer = Config.AngryAdjustMaxTimeSuperBull;
            if (distanceToPlayer < Config.AngryDashTryDistanceSuperBull) prepareToDashFlag = false;
            else prepareToDashFlag = true;
            return prepareToDashFlag;
        }

        private bool DetectPlayerInAttackRange()
        {
            // 检测玩家是否在跳跃攻击的攻击范围
            Vector3 bullToPlayer = Host.transform.position - PlayerRef.transform.position;
            bullToPlayer.y = 0f;
            if (bullToPlayer.magnitude <= Config.JumpAttackAttackRangeSuperBull) return true;
            return false;
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            // 处理愤怒状态逻辑
            // 比如检测玩家位置，触发冲刺攻击等
            if (angryStateCounter > maxCounter) return;
            if (!healthFirstLowerThan20p)
            {
                // 如果生命值第一次低于20%且不犹疑，则转换到红温状态
                if (Stats.Health <= Config.HealthSuperBull * 0.2f)
                {
                    healthFirstLowerThan20p = true;
                    if (!Stats.HesitateFlag)
                    {
                        Host.ChangeState(typeof(VeryAngryState));
                    }
                    return; // 直接转换状态，不再执行后续逻辑
                }
            }

            if (adjustDistanceFlag && prepareToDashFlag && Host.DetectMapEdge())
            {
                // 如果接近地图边缘，则停止调整距离
                adjustDistanceFlag = false; // 停止调整距离
                Debug.Log("SuperBull 接近地图边缘，停止调整距离");
            }

            if (adjustDistanceFlag && !prepareToDashFlag && DetectPlayerInAttackRange())
            {
                // 如果Player位于跳跃攻击范围之内，则停止调整距离
                adjustDistanceFlag = false; // 停止调整距离
                Debug.Log("SuperBull 进入跳跃攻击范围，停止调整距离");
            }

            if (adjustDistanceFlag)
            {
                if (prepareToDashFlag)
                {
                    // 尝试去远离玩家，这里之后需要调整使得能够修改flag，目前的考虑是给一个协程每隔一定时间更新一个位置，比较前后位置的距离差
                    Vector3 direction = (Host.transform.position - PlayerRef.transform.position).normalized;
                    direction.y = 0f;

                    Host.Velocity = direction * Stats.Speed; // 设置SuperBull的速度
                    adjustDistanceTimer -= delta; // 减少调整距离的计时器
                    if (adjustDistanceTimer <= 0f)
                    {
                        // 如果调整距离的计时器小于等于0，则停止调整距离
                        Debug.Log("SuperBull 达到调整最大时间，停止调整距离");
                        adjustDistanceFlag = false;
                        adjustDistanceTimer = 0f; // 确保计时器不小于0
                    }
                }
                else
                {
                    // 尝试靠近玩家
                    Vector3 direction = (PlayerRef.transform.position - Host.transform.position).normalized;
                    direction.y = 0f;

                    Host.Velocity = direction * Stats.Speed; // 设置SuperBull的速度
                    adjustDistanceTimer -= delta; // 减少调整距离的计时器
                    if (adjustDistanceTimer <= 0f)
                    {
                        // 如果调整距离的计时器小于等于0，则停止调整距离
                        Debug.Log("SuperBull 达到调整最大时间，停止调整距离");
                        adjustDistanceFlag = false;
                        adjustDistanceTimer = 0f; // 确保计时器不小于0
                    }
                }

            }
            else
            {
                if (prepareToDashFlag)
                {
                    if (!Stats.DashFlag)
                    {
                        DashAttack(() =>
                        {
                            Debug.Log("冲刺执行完毕，转为IDLE状态");
                            Host.ChangeState(typeof(IdleState));
                        });
                    }
                }
                else
                {
                    // 发动跳跃
                    if (!Stats.JumpAttackFlag)
                    {
                        JumpAttack(() =>
                        {
                            Debug.Log("跳跃攻击执行完毕，重新开始判断距离");
                            adjustDistanceTimer = Config.AngryAdjustMaxTimeSuperBull;
                            adjustDistanceFlag = true; // 重新开始
                            SetPrepareToDashFlag();
                        });

                    }
                }
            }
        }
    }

    private class VeryAngryState : BullState
    {
        public VeryAngryState(SuperBull host) : base(host)
        {
        }
        public override void OnEnter()
        {
            base.OnEnter();
            Host.curState = "红温";
            // Anim.SetTrigger("Angry");
            // 红温状态下承伤系数为125%
            Stats.TakeDamageRate = Config.TakeDamageRateVeryAngrySuperBull;
            Stats.Speed = Stats.Speed * Config.VeryAngrySpeedRateSuperBull;
            // 这里可以实现红温状态的逻辑，比如增加攻击力、改变行为等
            Debug.Log("进入红温状态");

            DashAttack(() => DashAttack(() => DashAttack(() => Host.ChangeState(typeof(TiredState)))));
        }

        public override void OnExit()
        {
            base.OnExit();
            // 参数还原
            Stats.TakeDamageRate = Config.TakeDamageRateSuperBull;
            Stats.Speed = Stats.Speed / Config.VeryAngrySpeedRateSuperBull;
            Debug.Log("离开红温状态");
        }
    }

    private class TiredState : BullState
    {
        private float initHealth;
        private float healthRate
        {
            // 生命值百分比
            get => Mathf.Abs(initHealth - Stats.Health) / Config.HealthSuperBull;
        }
        public TiredState(SuperBull host) : base(host)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();
            Host.curState = "疲劳";
            Debug.Log("进入疲劳状态");
            initHealth = Stats.Health;
            Stats.TakeDamageRate = Config.TakeDamageRateTiredSuperBull;
            Flow.Create()
            .Delay(Config.TiredDurationSuperBull) // 激昂状态下这个时间不减半，与Bull1不同
            .Then(() =>
            {
                if (healthRate <= Config.TiredHealthDeRateSuperBull)
                {
                    Host.ChangeState(typeof(IdleState));
                }
                else
                {
                    Host.ChangeState(typeof(AngryState));
                }
            }).Run();
        }
    }
}
