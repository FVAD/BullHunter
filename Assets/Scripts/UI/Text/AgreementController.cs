using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AgreementController : MonoBehaviour
{
    public Toggle agreementToggle; // ����AgreementToggle����
    public Button enterButton;     // ����EnterButton����

    void Start()
    {
        // ��ʼ״̬ͬ��
        enterButton.interactable = agreementToggle.isOn;

        // ��Toggle�¼�
        agreementToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    // Toggle״̬�仯ʱ�Ļص�
    private void OnToggleChanged(bool isChecked)
    {
        enterButton.interactable = isChecked;
    }
    // ���������л�����
    private void OnEnterGame()
    {
        SceneManager.LoadScene("Start"); // �������Ʊ�����Build Settingsһ��
    }
}
