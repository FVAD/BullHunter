using UnityEngine;
using UnityEngine.UI;

public class TextScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;

    void Start()
    {
        // 初始化滚动位置到顶部
        scrollRect.verticalNormalizedPosition = 1;
    }

    // 若需动态加载文本
    public void SetText(string content)
    {
        Text textComponent = scrollRect.content.GetComponentInChildren<Text>();
        textComponent.text = content;
    }
}