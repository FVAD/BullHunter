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
    private bool flag;//�Ƿ��и�����ʾ
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

    private void HighLight(Player.PlayerStats.Weapon value)//������ʾ��ǰ������ͼ�꣬Ȼ����һ�������ĸ���ȡ��
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
