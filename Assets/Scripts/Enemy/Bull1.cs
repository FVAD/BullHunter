using System;
using System.Collections;
using System.Collections.Generic;
using Bingyan;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public bool moveAbleFlag { get; set; }  // 移动标志，触发Bull技能时这个标志应当变为False
        public bool dashFlag { get; set; } // 冲刺正在进行标志
        public bool bigCircleFlag { get; set; } // 大回旋正在进行标志
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

            // 旋转Bull1，只用水平分量
            Vector3 horizontal = new Vector3(value.x, 0, value.z);
            if (horizontal.sqrMagnitude > 0.01f)
            {
                _targetRotation = Quaternion.LookRotation(horizontal.normalized, Vector3.up);
            }
            rb.velocity = value; // 设置新的速度
        }
    }

    private Quaternion _targetRotation;

    private RaycastHit frontHitInfo, backHitInfo;
    private RaycastResult frontRaycastResult, backRaycastResult;

    private void DetectFrontAndBack()
    {
        // 检测前方和后方是否会有空洞，用于判断前方和后方是否是有效的地面（地图边缘确定）
        Ray frontRay = new Ray(transform.position, transform.forward);
        Ray backRay = new Ray(transform.position, -transform.forward);

        if (Physics.Raycast(frontRay, out frontHitInfo, 1f))
        {
            frontRaycastResult = new RaycastResult
            {
                gameObject = frontHitInfo.collider.gameObject,
                distance = frontHitInfo.distance
            };
        }
        else
        {
            frontRaycastResult = default;
        }

        if (Physics.Raycast(backRay, out backHitInfo, 1f))
        {
            backRaycastResult = new RaycastResult
            {
                gameObject = backHitInfo.collider.gameObject,
                distance = backHitInfo.distance
            };
        }
        else
        {
            backRaycastResult = default;
        }
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
            Health = config.HealthBull1,
            takeDamageRate = config.TakeDamageRateBull1,
            Speed = config.SpeedBull1,
            AttackPower = config.AttackPowerBull1,
            swordAttackedCount = 0,
            lanceAttackedCount = 0,
            invulnerableTimeCounter = 0f, // 初始化无敌时间计数器
            dashFlag = false,
            bigCircleFlag = false,
            moveAbleFlag = true,
            passionateFlag = false,
            hesitateFlag = false,
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

        AudioMap.Bull.Roar.Play();
    }

    protected override void Update()
    {
        base.Update();
        RotateToTarget(_targetRotation); // 旋转到目标方向

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
        AddState(new AngryState(this));
        AddState(new VeryAngryState(this));
        AddState(new TiredState(this));
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

        

        protected Coroutine dashCoroutine; // 冲刺协程
        protected bool bigCircleFlag = false; // 大回旋攻击标志
        protected Coroutine bigCircleCoroutine; // 大回旋攻击协程

        // Bull1 大回旋攻击逻辑
        protected void BigCircleAttack(Action onComplete = null)
        {
            // 这里可以实现大回旋攻击的逻辑
            // 比如检测玩家位置，计算攻击范围等
            // 然后触发动画和伤害逻辑
            // Anim.SetTrigger("BigCircleAttack");

            if (Stats.bigCircleFlag)
            {
                // 如果已经在大回旋攻击中，则不再触发新的大回旋攻击
                return;
            }
            Debug.Log("大回旋攻击触发");
            Stats.bigCircleFlag = true; // 设置大回旋攻击标志
            Stats.moveAbleFlag = false; // 禁止移动
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
            yield return new WaitForSeconds(Config.BigCircleBeforeDelayBull1); // 前摇时间

            // 开始旋转
            float rotateSpeed = Config.BigCircleSpeedBull1 * Mathf.Deg2Rad; // 转换为弧度

            // 旋转360度
            float totalRotation = 0f;
            while (totalRotation < 360f)
            {
                float deltaRotation = rotateSpeed * Time.deltaTime; // 每帧旋转的角度
                totalRotation += deltaRotation * Mathf.Rad2Deg; // 累计旋转角度
                Host.transform.Rotate(Vector3.up, deltaRotation * Mathf.Rad2Deg); // 旋转Bull1
                yield return null; // 等待下一帧
            }
            Stats.bigCircleFlag = false;
            Stats.moveAbleFlag = true; // 恢复移动能力
            onComplete?.Invoke();
        }

        protected void DashAttack(Action onComplete = null)
        {
            // 这里可以实现冲刺攻击的逻辑
            // 比如检测玩家位置，计算冲刺方向和速度等
            // 然后触发动画和伤害逻辑
            // Anim.SetTrigger("DashAttack");
            
            if (Stats.dashFlag)
            {
                // 如果已经在冲刺中，则不再触发新的冲刺
                // Debug.Log("Bull1 正在冲刺中，无法再次触发冲刺攻击");
                return;
            }

            Debug.Log("冲刺攻击触发");
            Stats.dashFlag = true; // 设置冲刺标志
            Stats.moveAbleFlag = false; // 禁止移动
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
            AudioMap.Bull.Warning.Play();

            Debug.Log("开始冲刺攻击");
            yield return new WaitForSeconds(Config.DashBeforeDelayBull1); // 前摇时间
            Vector3 direction = (targetPosition - Host.transform.position).normalized;
            direction.y = 0; // 保持水平运动
            Vector3 startPosition = Host.transform.position; // 记录运动距离

            // 进入冲刺
            Host.Velocity = direction * Config.DashSpeedBull1; // 设置冲刺速度
            while (Vector3.Distance(Host.transform.position, targetPosition) > 0.1f)
            {
                // Rb.MovePosition(Host.transform.position + direction * Config.DashSpeedBull1 * Time.deltaTime);
                yield return null; // 等待下一帧
            }

            // 冲刺结束，进入后摇
            Debug.Log("冲刺攻击结束");
            yield return new WaitForSeconds(Config.DashAfterDelayBull1); // 后摇时间
            Stats.dashFlag = false;
            Stats.moveAbleFlag = true; // 恢复移动能力
            onComplete?.Invoke(); // 处理后续事件
        }
    }

    private class IdleState : BullState
    {
        private float idleDirFlag; // 用于控制空闲状态下的运动方向
        private Coroutine getDirFlagCoroutine;
        private float idleDirAdjustTime;
        private float dirChangeLockTimer; // 用于锁定方向变化的计时器
        private float angryTransitionTimer = 0f; // 愤怒状态转换计时器

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

            dirChangeLockTimer = 0f;
            idleDirFlag = 0f;
            angryTransitionTimer = Config.IdleAngryConvertTimeBull1; // 设置愤怒状态转换计时器

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

            if (distance < Config.IdleTriggerBigCircleDistanceBull1)
            {
                // 如果距离小于触发大回旋攻击的距离，则触发大回旋攻击
                BigCircleAttack(() => TransToAngryState()); // 同时愤怒
            }
            else if (Stats.moveAbleFlag)
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

            // 1. 攻击次数检测
            if (Stats.swordAttackedCount >= 1 || Stats.lanceAttackedCount > 3)
            {
                // 如果被剑攻击次数大于等于1次，或者被枪攻击次数大于3次，则转换到愤怒状态
                Debug.Log("Bull1 攻击次数条件达成，转换到愤怒状态");
                return true;
            }

            // 2. 愤怒转换计时器检测
            if (angryTransitionTimer <= 0f)
            {
                // 如果愤怒转换计时器小于等于0，则转换到愤怒状态
                Debug.Log("Bull1 愤怒转换计时器到达，转换到愤怒状态");
                return true;
            }

            // 3. 场地边缘检测（目前未定义场地边缘，暂不实现）
            if (false) // 这里可以添加具体的场地边缘检测逻辑
            {
                // 如果Bull1接近场地边缘，则转换到愤怒状态
                Debug.Log("Bull1 接近场地边缘，转换到愤怒状态");
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

        private void MaintainDistanceFromPlayer(float distance)
        {
            Vector3 direction;
            // 维持Bull1与玩家距离
            if (distance > Config.IdleMaintainTargetDistanceBull1 + Config.IdleMaintainTargetDistanceOffsetBull1)
            {
                // 如果距离小于维持距离，则Bull1尝试远离玩家
                direction = (PlayerRef.transform.position - Host.transform.position).normalized;
                dirChangeLockTimer = Config.IdleDirChangeLockTime[0]; // 锁定方向变化的时间
            }
            else if (distance < Config.IdleMaintainTargetDistanceBull1 - Config.IdleMaintainTargetDistanceOffsetBull1)
            {
                // 如果距离大于维持距离，则Bull1尝试靠近玩家
                direction = (Host.transform.position - PlayerRef.transform.position).normalized;
                dirChangeLockTimer = Config.IdleDirChangeLockTime[0]; // 锁定方向变化的时间
            }
            else
            {
                // 如果距离在这个区间内，则Bull1尝试围绕玩家运动并保持距离
                // 拿到一个圆周运动方向，圆周方向随机
                direction = (Vector3.Cross(PlayerRef.transform.position - Host.transform.position, Vector3.up) * (idleDirFlag < 0 ? -1f : 1f)).normalized;
                dirChangeLockTimer = Config.IdleDirChangeLockTime[1]; // 锁定方向变化的时间
            }
            direction.y = 0; // 保持水平运动
            Host.Velocity = direction * Stats.Speed; // 设置Bull1的速度
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

            AudioMap.Bull.Roar.Play();
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

                Host.Velocity = direction * Stats.Speed; // 设置Bull1的速度
            }
            else
            {
                if (!Stats.dashFlag)
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
