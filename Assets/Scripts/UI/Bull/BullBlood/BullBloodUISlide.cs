using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AudioMap;
public class BullBloodUISlide : MonoBehaviour
{
    [SerializeField] private Slider bloodSlider;
    [Range(0, Mathf.Infinity)] private float bloodValue;
    private float bloodcur;
    private float maxHealth;
    private Bull1 bull1;
    // Start is called before the first frame update
    void Start()
    {
        bull1 = BattleManager.Instance.Enemy;
        var stats = bull1.Stats;
        maxHealth = stats.Health;
        bloodcur = stats.Health;
    }
    
    private void UpdateHealth(float value)
    {
        if (bloodValue > maxHealth) bloodValue = maxHealth;
        if (bloodValue <= 0) bloodValue = 0;
        bloodSlider.value = value / maxHealth;
    }
    // Update is called once per frame
    void Update()
    {
        if (bull1.Stats.Health != bloodcur)
        {
            UpdateHealth(bull1.Stats.Health);
        }
    }
}
