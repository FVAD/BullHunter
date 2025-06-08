using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalFlagController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Player playerInstance;
    [SerializeField] private List<Bull1> bull1Instances;

    public void SetBullPassionateFlag(bool flag)
    {
        if (bull1Instances == null || bull1Instances.Count == 0)
        {
            Debug.LogWarning("No Bull1 instances found to set passionate flag.");
            return;
        }

        foreach (var bull in bull1Instances)
        {
            if (bull != null)
            {
                bull.SetPassionateFlag(flag);
            }
        }
    }

    public void SetBullHesitateFlag(bool flag)
    {
        if (bull1Instances == null || bull1Instances.Count == 0)
        {
            Debug.LogWarning("No Bull1 instances found to set hesitate flag.");
            return;
        }

        foreach (var bull in bull1Instances)
        {
            if (bull != null)
            {
                bull.SetHesitateFlag(flag);
            }
        }
    }
}
