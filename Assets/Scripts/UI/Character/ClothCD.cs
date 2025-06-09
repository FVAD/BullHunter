using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static ClothWeapon;

public class ClothCD : MonoBehaviour
{
    public float RedCD;
    public float GreenCD;
    [Header("冷却界面")]
    [SerializeField, NotNull] private Image RedMask;  // 添加NotNull特性
    [SerializeField, NotNull] private Image GreenMask;
    private bool flag;
    // Start is called before the first frame update
    void Start()
    {
        flag = false;
        var stats = BattleManager.Instance.Player.Stats;
        stats.OnCurrentWeaponChange += ClothInit;

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void ClothInit(Player.PlayerStats.Weapon value)
    {
        if (!flag && value == Player.PlayerStats.Weapon.Cloth)
        {
            var cloth = BattleManager.Instance.Player.Stats.Cloth;  // 增加空值检查
            if (cloth == null || cloth.Dict == null) return;

            cloth.Dict[Colour.Red].OnCDChange += ClothCDCount;
            cloth.Dict[Colour.Green].OnCDChange += ClothCDCount;
            flag = true;
        }
    }

    private void ClothCDCount(ClothWeapon.Colour col, float value)
    {
        if (RedMask == null || GreenMask == null) return; // 添加空值检查

        switch (col)
        {
            case ClothWeapon.Colour.Red when RedMask != null:
                RedMask.fillAmount = value / 120f;
                break;
            case ClothWeapon.Colour.Green when GreenMask != null:
                GreenMask.fillAmount = value / 120f;
                break;
        }
    }
}
