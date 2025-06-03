using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AgreementController : MonoBehaviour
{
    public Toggle agreementToggle; // 拖入AgreementToggle对象
    public Button enterButton;     // 拖入EnterButton对象

    void Start()
    {
        // 初始状态同步
        enterButton.interactable = agreementToggle.isOn;

        // 绑定Toggle事件
        agreementToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    // Toggle状态变化时的回调
    private void OnToggleChanged(bool isChecked)
    {
        enterButton.interactable = isChecked;
    }
    // 新增场景切换方法
    private void OnEnterGame()
    {
        SceneManager.LoadScene("Start"); // 场景名称必须与Build Settings一致
    }
}
