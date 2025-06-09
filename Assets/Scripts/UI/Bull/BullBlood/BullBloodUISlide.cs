using UnityEngine;
using UnityEngine.UI;

public class BullBloodUISlide : MonoBehaviour
{
    [SerializeField] private Slider bloodSlider;
    [Range(0, Mathf.Infinity)] private float bloodValue;
    private float bloodcur;
    private float maxHealth;
    private Bull1 bull1;
    private SuperBull super;
    // Start is called before the first frame update
    void Start()
    {
        bull1 = null;
        super = null;
        var enemy = BattleManager.Instance.Enemy;
        if (enemy is Bull1 b)
        {
            bull1 = b;
            var stats = bull1.Stats;
            maxHealth = stats.Health;
            bloodcur = stats.Health;
        }
        else if (enemy is SuperBull s)
        {
            super = s;
            var stats = super.Stats;
            maxHealth = stats.Health;
            bloodcur = stats.Health;
        }
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
        if (bull1 && bull1.Stats.Health != bloodcur)
        {
            UpdateHealth(bull1.Stats.Health);
        }
        else if (super && super.Stats.Health != bloodcur)
        {
            UpdateHealth(super.Stats.Health);
        }
    }
}
