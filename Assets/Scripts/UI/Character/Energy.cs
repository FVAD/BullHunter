using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    [SerializeField]private Slider energySlider; 
    private float maxEnergy = 90f;
    private void UpdateEnergy(float value)
    {
        energySlider.value = value/maxEnergy;
    }
    public void SetMaxEnergy(float value)
    {
        maxEnergy = value;
    }
}
