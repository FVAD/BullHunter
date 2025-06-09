using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeapoChange : MonoBehaviour
{
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = BattleManager.Instance.Player;
        var curWeapon = player.Stats.CurrentWeapon; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
