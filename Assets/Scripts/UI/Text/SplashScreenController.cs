// SplashScreenController.cs
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SplashScreenController : MonoBehaviour
{
    // �����ֱ���ر���
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
        // ����Ĭ��ѡ��Ԫ��
        EventSystem.current.SetSelectedGameObject(scrollRect.gameObject);

        // ���õ���·��
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
            // ͨ���ֱ���������ƹ���
            var input = Gamepad.current.leftStick.ReadValue();
            if (Mathf.Abs(input.y) > 0.1f)
            {
                scrollRect.verticalNormalizedPosition += input.y * gamepadScrollSpeed * Time.deltaTime;
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
            }
        }
    }
    // �ύ��ť����
    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed && confirmButton.interactable)
        {
            confirmButton.onClick.Invoke();
        }
    }
    private void OnScroll(Vector2 position)
    {
        // ���������ײ�ʱ����Э�鹴ѡ
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
            // ������һ�������߼�
            SceneManager.LoadScene("MainGame");
        });
    }

    // ��ʼ���ı����ݣ���Editor�и�ֵ��
    public void SetContent(TMP_Text title, TMP_Text content)
    {
        title.text = "��ţ��֪";
        content.text = @"һ����ӭ����
�𾴵Ķ�ʿ����ӭ̤�롶ţ֮���ˡ��Ŀ�Ұ���磡
Ϊȷ������ţţ�Ĳ�����ƽ�Ұ�ȫ���������ϸ�Ķ����¹�����������֪���ݣ���ֱ����ת����ĩǩ��Э�顣δ�Ķ���֪���µĶ�ţʧ�ܣ���������ге���


---

����ս������˵��
1. �������ּ�1/�������
- ��λ���߷��ո߻ر��Ľ�ս������
- ���ƣ�
  - ǰҡ���������������ڼ��޷��ƶ���
  - ��Χ�����̣���������ս��
  - �˺��������˺��ߣ���ÿ֧����ţ���ϵı�ǹ����������˺���1֧��ǹ+20%�˺�����
  - ���ۣ���ҡ����ʹ�ú����ľ����޷���ʱ�ָ���
- ���ԣ���׼����ʱ������ϱ�ǹ����Ч�����������

2. ��ǹ�����ּ�2/������ϣ�
- ��λ���г̸���������������������Ѫ��
- ���ƣ�
  - ǰҡ�����ƶ�Ͷ����ǰҡ���С�
  - ��Χ���о��룬����ֹ���湥�����ᱻţ�ǵ�������
  - Ч������ɼ����˺������ӳ�����Ѫ����Ϊ���������ṩ�������棨�����˺�+15%����
- ���ԣ��ƺ�Ͷ��������ţţ������Ϊ������·��

3. ��ţ�������ּ�3/������ң�
- �л���ʽ��������/LT��RTѭ���л�������̡�������졭����
- ������Ч����
  - ��������ţ�����죩��
    - �������ֺ첼2�뼤���ȴ120�루���̹��ã���
    - Ч����
      - ţţ��1.5�����ڼ��ٿ񱩣������ŭ/���/ƣ�ͣ���������ǿ��ƣ�͡�
      - ��ң�30���ھ������ļ��롢�������٣�������60�뾫�����ķ�����
    - ���ó������ٹ�ս�����߷��ձ�����

  - �����춷ţ�����̣���
    - ���������̲�2�뼤���ȴ120�루���̹��ã���
    - Ч����
      - ţţ��1������ǿ����߮�����ǰҡ�ӳ���ֹͣʱ����٣����޷����¡�
    - ���ó��������Ӵ�Ϣ�����ƽ��ࡣ

  - ����ɫ��ţ�������磩��
    - �������ֻ��粼10�뼤�����ȴ��
    - Ч����
      - ���ţţ��ǰ״̬����/�̲���ȴ����30�롣
    - ���ó����������ȳ�������ս�֡�


---

��������ָ��
- �ƶ���W/A/S/D
- ���ܣ��ƶ�����+�ո����ľ�����
- ���ܣ���סShift���������ľ�����
- �����л���
  - ���ּ�1/�󣺽�
  - ���ּ�2/�ϣ���ǹ
  - ���ּ�3/�ң���ţ��
  - ��ţ���л���������/LT��RT
- ����/��Ч����������ʹ�õ�ǰ���ߣ�


---

�ġ����Ĺ���
1. �������ƣ����ܡ����ܡ����������ľ�������ֹʱ�Զ��ָ���
2. ״̬���ӣ�ţţ�ܱ�ǹ���ˡ���Ѫ�ɵ��ӣ�������/����״̬���⡣
3. ��ȴ���ԣ���/�̲�������ȴ�����粼���ȳ���
4. �������գ�����ǰ��ҡ�ڼ��ޱ���������ʱ����������������


---

�塢����Э��
���ѳ����Ⲣ���ܣ�
- ��ţΪ�߷����˶�����Ϸ��������ģ����Ϊ��
- ����ʹ�ò��������ʧ���µ�ʧ�ܣ������Ը���";
    }
}
