using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BullUIState : MonoBehaviour
{
    public GameObject text;
    private TextMeshProUGUI textMeshPro;
    void Start()
    {
        textMeshPro = text.GetComponent<TextMeshProUGUI>();
        var str = BattleManager.Instance.Enemy.GetState();
    }

    // Update is called once per frame
    void Update()
    {
        var str = BattleManager.Instance.Enemy.GetState();
        textMeshPro.text = str;
    }
}
