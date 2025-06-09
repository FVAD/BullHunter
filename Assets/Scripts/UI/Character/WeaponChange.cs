using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class WeaponChange : MonoBehaviour
{
    public Image swordUI;
    public Image spareUI;
    public Image clothUI;
    private Image forUIImage;
    private Player.PlayerStats.Weapon curWea;
    private Player.PlayerStats.Weapon forWea;
    private bool flag;//是否有高亮显示
    // Start is called before the first frame update
    void Start()
    {
        flag = false;
        var stats = BattleManager.Instance.Player.Stats;
        curWea = stats.CurrentWeapon;
        HighLight(curWea);
        stats.OnCurrentWeaponChange += HighLight;

    }
    // Update is called once per frame
    void Update()
    {

    }

    private void HighLight(Player.PlayerStats.Weapon value)//高亮显示当前的武器图标，然后将上一个武器的高亮取消
    {
        if (value == Player.PlayerStats.Weapon.Cloth)
        {
            clothUI.enabled = true;
            if (flag)
            {
                forUIImage.enabled = false;
            }
            forUIImage = clothUI;
        }
        if (value == Player.PlayerStats.Weapon.Sword)
        {
            swordUI.enabled = true;
            if (flag)
            {
                forUIImage.enabled = false;
            }
            forUIImage = swordUI;
        }
        if (value == Player.PlayerStats.Weapon.Lance)
        {
            spareUI.enabled = true;
            if (flag)
            {
                forUIImage.enabled = false;
            }
            forUIImage = spareUI;
        }
     
        flag = true;
    }

}
