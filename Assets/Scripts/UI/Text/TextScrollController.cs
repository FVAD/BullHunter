using UnityEngine;
using UnityEngine.UI;

public class TextScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;

    void Start()
    {
        // ��ʼ������λ�õ�����
        scrollRect.verticalNormalizedPosition = 1;
    }

    // ���趯̬�����ı�
    public void SetText(string content)
    {
        Text textComponent = scrollRect.content.GetComponentInChildren<Text>();
        textComponent.text = content;
    }
}