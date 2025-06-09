using TMPro;
using UnityEngine;

public class BullUIState : MonoBehaviour
{
    public GameObject text;
    private TextMeshProUGUI textMeshPro;
    void Start() => textMeshPro = text.GetComponent<TextMeshProUGUI>();

    void Update() =>
        textMeshPro.text = BattleManager.Instance.Enemy switch
        {
            Bull1 b => b.GetState(),
            SuperBull s => s.GetState(),
            _ => throw new System.Exception()
        };
}
