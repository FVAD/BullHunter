using System;
using System.Collections;
using System.Collections.Generic;
using Bingyan;
using UnityEngine;

public class Bull1 : FSM
{
    [SerializeField, Title("配置")] private BullConfig config;
    public BullStats Stats { get; private set; }
    public class BullStats
    {
        public float Health { get; set; }
        public float takeDamageRate { get; set; }
        public float Speed { get; set; }
        public float AttackPower { get; set; }
        public int swordAttackedCount { get; set; } // 用于记录剑攻击次数
        public int lanceAttackedCount { get; set; } // 用于记录枪攻击次数
        public float invulnerableTimeCounter { get; set; } // 用于记录无敌时间计数器
        public bool passionateFlag { get; set; } = false; // 激昂状态Flag
        public bool hesitateFlag { get; set; } = false; // 犹疑状态Flag
    }

    private Rigidbody rb;
    private Animator anim;

    [SerializeField, Title("玩家引用")] private Player player;

    public override void Init()
    {
        base.Init();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        Stats = new BullStats
        {
            Health = config.HealthBull1,
            takeDamageRate = config.TakeDamageRateBull1,
            Speed = config.SpeedBull1,
            AttackPower = config.AttackPowerBull1,
            swordAttackedCount = 0,
            lanceAttackedCount = 0,
            invulnerableTimeCounter = 0f // 初始化无敌时间计数器
        };

        GetComponentInChildren<DefendArea>().OnAttacked += (atk, def, f) =>
        {
            // 处理受击逻辑
            if (Stats.invulnerableTimeCounter > 0f) return;
            Stats.invulnerableTimeCounter = config.InvulnerableTime; // 设置无敌时间
            Stats.Health -= f * Stats.takeDamageRate;
            Debug.Log($"Bull1 受到了 {f} * {Stats.takeDamageRate} = {f * Stats.takeDamageRate}点伤害，当前生命值：{Stats.Health}");
            if (atk.GetComponent<LanceWeapon>() != null)
            {
                Stats.lanceAttackedCount++;
                Debug.Log($"Bull1 被枪攻击次数：{Stats.lanceAttackedCount}");
            }
            else
            {
                Stats.swordAttackedCount++;
                Debug.Log($"Bull1 被剑攻击次数：{Stats.swordAttackedCount}");
            }

            if (Stats.Health <= 0)
            {
                // anim.SetTrigger("Die");
                // 处理死亡逻辑
                Debug.Log("Bull1 死亡");
            }
        };

    }

    protected override void Update()
    {
        base.Update();

        if (Stats.invulnerableTimeCounter > 0f)
        {
            // 如果无敌时间计数器大于0，则减少计数器
            Stats.invulnerableTimeCounter -= Time.deltaTime;
            if (Stats.invulnerableTimeCounter < 0f)
            {
                Stats.invulnerableTimeCounter = 0f; // 确保计数器不小于0
            }
        }
    }

    protected override void DefineStates()
    {
        AddState(new IdleState(this));
    }

    protected override Type GetDefaultState() => typeof(IdleState);

    private class BullState : FSMState
    {
        protected new Bull1 Host;

        /// <summary>
        /// 构造函数，传入主机对象
        /// </summary>
        /// <param name="host"></param>
        public BullState(Bull1 host) : base(host) => Host = host;
        protected BullStats Stats => Host.Stats;
        protected BullConfig Config => Host.config;

        protected Rigidbody Rb => Host.rb;
        protected Animator Anim => Host.anim;
        protected Transform Trans => Host.transform;
        protected Player PlayerRef => Host.player;

        protected bool dashFlag = false; // 冲刺标志
        protected Coroutine dashCoroutine; // 冲刺协程
        protected bool bigCircleFlag = false; // 大回旋攻击标志

        // Bull1 大回旋攻击逻辑
        protected void BigCircleAttack()
        {
            // 这里可以实现大回旋攻击的逻辑
            // 比如检测玩家位置，计算攻击范围等
            // 然后触发动画和伤害逻辑
            // Anim.SetTrigger("BigCircleAttack");
            // Debug.Log("大回旋攻击触发");
        }

        protected void DashAttack(Action onComplete = null)
        {
            // 这里可以实现冲刺攻击的逻辑
            // 比如检测玩家位置，计算冲刺方向和速度等
            // 然后触发动画和伤害逻辑
            // Anim.SetTrigger("DashAttack");
            Debug.Log("冲刺攻击触发");
            if (dashFlag)
            {
                // 如果已经在冲刺中，则不再触发新的冲刺
                Debug.Log("Bull1 正在冲刺中，无法再次触发冲刺攻击");
                return;
            }
            dashFlag = true; // 设置冲刺标志
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
            Debug.Log("开始冲刺攻击");
            yield return new WaitForSeconds(Config.DashBeforeDelayBull1); // 前摇时间
            Vector3 direction = (targetPosition - Host.transform.position).normalized;
            direction.y = 0; // 保持水平运动
            Vector3 startPosition = Host.transform.position; // 记录运动距离

            // 进入冲刺
            while (Vector3.Distance(Host.transform.position, targetPosition) > 0.1f)
            {
                Rb.MovePosition(Host.transform.position + direction * Config.DashSpeedBull1 * Time.deltaTime);
                yield return null; // 等待下一帧
            }

            // 冲刺结束，进入后摇
            Debug.Log("冲刺攻击结束");
            yield return new WaitForSeconds(Config.DashAfterDelayBull1); // 后摇时间
            dashFlag = false;
            onComplete?.Invoke(); // 处理后续事件
        }
    }

    private class IdleState : BullState
    {
        private float idleDirFlag = 0f; // 用于控制空闲状态下的运动方向
        private Coroutine getDirFlagCoroutine;
        private float idleDirAdjustTime;

        private IEnumerator getDirFlag()
        {
            while (true)
            {
                idleDirFlag = UnityEngine.Random.Range(-1f, 1f);
                yield return new WaitForSeconds(idleDirAdjustTime); // 更新一次
            }
        }
        public IdleState(Bull1 host) : base(host)
        {


        }

        public override void OnEnter()
        {
            base.OnEnter();
            // Anim.SetTrigger("Idle");
            // IDLE状态下承伤系数为85%
            Stats.takeDamageRate = Config.TakeDamageRateIdleBull1;
            idleDirAdjustTime = Config.IdleDirAdjustTimeBull1;
            // 启动获取方向的协程
            getDirFlagCoroutine = Host.StartCoroutine(getDirFlag());

        }

        public override void OnExit()
        {
            base.OnExit();
            // 退出IDLE状态时恢复承伤系数
            Stats.takeDamageRate = Config.TakeDamageRateBull1;
            // 停止获取方向的协程
            if (getDirFlagCoroutine != null)
            {
                Host.StopCoroutine(getDirFlagCoroutine);
                getDirFlagCoroutine = null;
            }
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            // 处理空闲状态逻辑
            DetectPlayerDistance();
        }

        private void DetectPlayerDistance()
        {
            // 检测玩家距离
            float distance = Vector3.Distance(PlayerRef.transform.position, Host.transform.position);

            if (distance < Config.IdleTriggerBigCircleDistanceBull1)
            {
                // 如果距离小于触发大回旋攻击的距离，则触发大回旋攻击
                BigCircleAttack();
            }
            else
            {
                // 否则，维持当前状态
                MaintainDistanceFromPlayer(distance);
            }

            if (DetectTransToAngryState())
            {
                // 检测是否转换到愤怒状态
                TransToAngryState();
            }
        }

        private bool DetectTransToAngryState()
        {
            // 检测是否转换到愤怒状态
            // 这里可以根据具体逻辑判断是否需要转换状态
            // 比如检测玩家位置、血量等
            // 目前简单返回false，表示不转换状态
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

        private void MaintainDistanceFromPlayer(float distance)
        {
            Vector3 direction;
            // 维持Bull1与玩家距离
            if (distance > Config.IdleMaintainTargetDistanceBull1 + Config.IdleMaintainTargetDistanceOffsetBull1)
            {
                // 如果距离小于维持距离，则Bull1尝试远离玩家
                direction = (PlayerRef.transform.position - Host.transform.position).normalized;
            }
            else if (distance < Config.IdleMaintainTargetDistanceBull1 - Config.IdleMaintainTargetDistanceOffsetBull1)
            {
                // 如果距离大于维持距离，则Bull1尝试靠近玩家
                direction = (Host.transform.position - PlayerRef.transform.position).normalized;
            }
            else
            {
                // 如果距离在这个区间内，则Bull1尝试围绕玩家运动并保持距离
                // 拿到一个圆周运动方向，圆周方向随机
                direction = (Vector3.Cross(PlayerRef.transform.position - Host.transform.position, Vector3.up) * (idleDirFlag < 0 ? -1f : 1f)).normalized;

            }
            direction.y = 0; // 保持水平运动

            Rb.MovePosition(Host.transform.position + direction * Stats.Speed * Time.deltaTime);
        }
    }
    private class AngryState : BullState
    {

        private bool adjustDistanceFlag = false; // 是否需要调整距离的标志
        public AngryState(Bull1 host) : base(host)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            // Anim.SetTrigger("Angry");
            // 愤怒状态下承伤系数为100%
            Stats.takeDamageRate = Config.TakeDamageRateBull1;
            // 这里可以实现愤怒状态的逻辑，比如增加攻击力、改变行为等
            Debug.Log("进入愤怒状态");
            adjustDistanceFlag = true;
        }

        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            // 处理愤怒状态逻辑
            // 比如检测玩家位置，触发冲刺攻击等
            if (adjustDistanceFlag)
            {
                // 尝试去远离玩家，这里之后需要调整使得能够修改flag，目前的考虑是给一个协程每隔一定时间更新一个位置，比较前后位置的距离差
                Vector3 direction = (PlayerRef.transform.position - Host.transform.position).normalized;
                direction.y = 0f;

                Rb.MovePosition(Host.transform.position + direction * Stats.Speed * Time.deltaTime);
            }
            else
            {
                if (!dashFlag)
                {
                    DashAttack(() =>
                    {
                        Debug.Log("冲刺执行完毕，转为IDLE状态");
                        Host.ChangeState(typeof(IdleState));
                    });
                }
            }
        }
    }

    private class VeryAngryState : BullState
    {
        public VeryAngryState(Bull1 host) : base(host)
        {
        }
        public override void OnEnter()
        {
            base.OnEnter();
            // Anim.SetTrigger("Angry");
            // 红温状态下承伤系数为125%
            Stats.takeDamageRate = Config.TakeDamageRateVeryAngryBull1;
            Stats.Speed = Config.SpeedBull1 * Config.VeryAngrySpeedRateBull1;
            // 这里可以实现红温状态的逻辑，比如增加攻击力、改变行为等
            Debug.Log("进入红温状态");
            DashAttack(() =>
            {
                DashAttack(() =>
                {
                    DashAttack(() =>
                    {
                        Host.ChangeState(typeof(TiredState));
                    }
                    );
                });
            });
        }

        public override void OnExit()
        {
            base.OnExit();
            // 参数还原
            Stats.takeDamageRate = Config.TakeDamageRateBull1;
            Stats.Speed = Config.SpeedBull1;
            Debug.Log("离开红温状态");
        }
    }

    private class TiredState : BullState
    {
        private float initHealth;
        private float healthRate
        {
            // 生命值百分比
            get => Mathf.Abs(initHealth - Stats.Health) / Config.HealthBull1;
        }
        public TiredState(Bull1 host) : base(host)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("进入疲劳状态");
            initHealth = Stats.Health;
            Stats.takeDamageRate = Config.TakeDamageRateTiredBull1;
            Flow.Create().Delay(Config.TiredDurationBull1).Then(() =>
            {
                if (healthRate <= Config.TiredHealthDeRateBull1)
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
