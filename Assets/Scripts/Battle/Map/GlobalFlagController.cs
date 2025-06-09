using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalFlagController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Player playerInstance;
    [SerializeField] private List<Bull1> bull1Instances;
    [SerializeField] private List<SuperBull> superBullInstances;

    public void SetBullPassionateFlag(bool flag)
    {
        foreach (var bull in bull1Instances)
        {
            if (bull != null)
            {
                bull.SetPassionateFlag(flag);
            }
        }

        foreach (var superBull in superBullInstances)
        {
            if (superBull != null)
            {
                superBull.SetPassionateFlag(flag);
            }
        }
    }

    public void SetBullHesitateFlag(bool flag)
    {
        foreach (var bull in bull1Instances)
        {
            if (bull != null)
            {
                bull.SetHesitateFlag(flag);
            }
        }

        foreach (var superBull in superBullInstances)
        {
            if (superBull != null)
            {
                superBull.SetHesitateFlag(flag);
            }
        }
    }
}
