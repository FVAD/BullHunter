// SplashScreenController.cs
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SplashScreenController : MonoBehaviour
{
    // 新增手柄相关变量
    [Header("Gamepad")]
    public float scrollSensitivity = 0.5f;
    public float analogThreshold = 0.2f;
    private PlayerInput playerInput;
    [Header("References")]
    public ScrollRect scrollRect;
    public Toggle agreementToggle;
    public Button confirmButton;
    public float gamepadScrollSpeed = 0.5f;
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        ConfigureNavigation();
        confirmButton.interactable = false;
        agreementToggle.onValueChanged.AddListener(OnToggleChanged);
        scrollRect.onValueChanged.AddListener(OnScroll);
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
    }
    private void ConfigureNavigation()
    {
        // 设置默认选中元素
        EventSystem.current.SetSelectedGameObject(scrollRect.gameObject);

        // 配置导航路径
        var trigger = scrollRect.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        entry.callback.AddListener((data) => {
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition + 0.1f
            );
        });
        trigger.triggers.Add(entry);
    }
    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 input = context.ReadValue<Vector2>();
        if (Mathf.Abs(input.y) > analogThreshold)
        {
            float scrollValue = input.y * scrollSensitivity * Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition - scrollValue
            );
        }
    }
    private void Update()
    {
        if (Gamepad.current != null)
        {
            // 通过手柄方向键控制滚动
            var input = Gamepad.current.leftStick.ReadValue();
            if (Mathf.Abs(input.y) > 0.1f)
            {
                scrollRect.verticalNormalizedPosition += input.y * gamepadScrollSpeed * Time.deltaTime;
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
            }
        }
    }
    // 提交按钮处理
    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed && confirmButton.interactable)
        {
            confirmButton.onClick.Invoke();
        }
    }
    private void OnScroll(Vector2 position)
    {
        // 当滚动到底部时激活协议勾选
        if (position.y <= 0.05f)
        {
            agreementToggle.interactable = true;
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        confirmButton.interactable = isOn;
        confirmButton.onClick.AddListener(() =>
        {
            // 加载下一场景的逻辑
            SceneManager.LoadScene("MainGame");
        });
    }

    // 初始化文本内容（在Editor中赋值）
    public void SetContent(TMP_Text title, TMP_Text content)
    {
        title.text = "斗牛须知";
        content.text = @"一、欢迎声明
尊敬的斗士，欢迎踏入《牛之猎人》的狂野世界！
为确保您与牛牛的搏斗公平且安全，请务必仔细阅读以下规则。若您已熟知内容，可直接跳转至文末签署协议。未阅读须知导致的斗牛失败，后果需自行承担。


---

二、战斗道具说明
1. 剑（数字键1/方向键左）
- 定位：高风险高回报的近战利器。
- 机制：
  - 前摇：长蓄力动作，期间无法移动。
  - 范围：极短，需贴身作战。
  - 伤害：基础伤害高，且每支插在牛身上的标枪会额外提升伤害（1支标枪+20%伤害）。
  - 代价：后摇长，使用后精力耗尽且无法即时恢复。
- 策略：精准把握时机，配合标枪易伤效果爆发输出。

2. 标枪（数字键2/方向键上）
- 定位：中程辅助武器，叠加易伤与流血。
- 机制：
  - 前摇：可移动投掷，前摇适中。
  - 范围：中距离，但禁止正面攻击（会被牛角弹开）。
  - 效果：造成极低伤害，附加持续流血，并为后续攻击提供易伤增益（后续伤害+15%）。
- 策略：绕后投掷，削弱牛牛防御，为剑技铺路。

3. 斗牛布（数字键3/方向键右）
- 切换方式：鼠标滚轮/LT和RT循环切换（红→绿→混沌→红…）。
- 类型与效果：
  - 【激昂斗牛布（红）】
    - 触发：持红布2秒激活，冷却120秒（红绿共用）。
    - 效果：
      - 牛牛：1.5分钟内加速狂暴（更快愤怒/冲锋/疲劳），结束后强制疲劳。
      - 玩家：30秒内精力消耗减半、动作加速，结束后60秒精力消耗翻倍。
    - 适用场景：速攻战术，高风险爆发。

  - 【怪异斗牛布（绿）】
    - 触发：持绿布2秒激活，冷却120秒（红绿共用）。
    - 效果：
      - 牛牛：1分钟内强制游弋，冲锋前摇延长，停止时间减少，且无法红温。
    - 适用场景：拖延喘息，控制节奏。

  - 【间色斗牛布（混沌）】
    - 触发：持混沌布10秒激活，无冷却。
    - 效果：
      - 清除牛牛当前状态，红/绿布冷却减少30秒。
    - 适用场景：紧急救场，重置战局。


---

三、操作指南
- 移动：W/A/S/D
- 闪避：移动方向+空格（消耗精力）
- 疾跑：按住Shift（持续消耗精力）
- 道具切换：
  - 数字键1/左：剑
  - 数字键2/上：标枪
  - 数字键3/右：斗牛布
  - 斗牛布切换：鼠标滚轮/LT和RT
- 攻击/生效：鼠标左键（使用当前道具）


---

四、核心规则
1. 精力机制：疾跑、闪避、攻击均消耗精力，静止时自动恢复。
2. 状态叠加：牛牛受标枪易伤、流血可叠加，但激昂/迟疑状态互斥。
3. 冷却策略：红/绿布共享冷却，混沌布灵活救场。
4. 致命风险：剑技前后摇期间无保护，误判时机将遭致命反击！


---

五、免责协议
我已充分理解并接受：
- 斗牛为高风险运动，游戏内伤亡属模拟行为。
- 道具使用不当或操作失误导致的失败，责任自负。";
    }
}
