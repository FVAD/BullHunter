using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    [SerializeField] private Slider energySlider;
    [Range(0, Mathf.Infinity)] private float energyValue;
    private float maxEnergy;
    private Player player;

    
private void UpdateEnergy(float value)
    {
        if (energyValue > maxEnergy) energyValue = maxEnergy;
        if (energyValue <= 0) energyValue = 0;
        energySlider.value = value / maxEnergy;
    }
    public void SetMaxEnergy(float value)
    {
        maxEnergy = value;
    }

    private void Start()
    {
        var stats = BattleManager.Instance.Player.Stats;
        stats.OnStaminaChange += UpdateEnergy;
        maxEnergy = stats.Stamina;
    }
}
