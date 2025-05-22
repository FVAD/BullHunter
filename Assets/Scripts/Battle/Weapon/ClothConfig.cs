using Bingyan;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Close")]
public class ClothConfig : ScriptableObject
{
    [SerializeField, Title("种类")] private ClothWeapon.Colour colour;
    [SerializeField, Title("使用前摇")] private float startup = 2;
    [SerializeField, Title("冷却时间")] private float cooldown = 120;
    [SerializeField, Title("材质")] private Material mat;

    public ClothWeapon.Colour Colour => colour;
    public float Startup => startup;
    public float Cooldown => cooldown;
    public Material Mat => mat;

    public bool Ready => timer >= Cooldown;
    private float timer;

    public ClothConfig Init()
    {
        timer = Cooldown;
        return this;
    }
    public void Tick(float delta)
    {
        if (!Ready) timer += delta;
    }
    public void Refresh() => timer = 0;
}
