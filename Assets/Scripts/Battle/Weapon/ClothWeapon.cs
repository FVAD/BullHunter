using Bingyan;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ClothWeapon : MonoBehaviour
{
    [SerializeField, Title("配置")] private ClothConfig[] configs;

    public Dictionary<Colour, ClothConfig> Dict { get; private set; }
    public List<Colour> Colours { get; private set; }
    private int idx;

    private Renderer rend;

    private void Start()
    {
        Dict = configs.ToDictionary(config => config.Init().Colour);
        Colours = new List<Colour>()
        {
            Colour.Red,
            Colour.Green,
            Colour.Chaos,
        };
        (rend = GetComponent<Renderer>()).material = Dict[CurrentColour].Mat;
        SetVisible(false);
    }

    private float timer;
    public void Refresh() => timer = 0;
    public void Tick(float delta)
    {
        ClothConfig config = Dict[CurrentColour];
        if (!config.Ready)
        {
            timer = 0;
            return;
        }
        if ((timer += delta) >= config.Startup) Use();
    }

    public void SetVisible(bool visible) => GetComponent<Renderer>().enabled = visible;

    public Colour CurrentColour => Colours[idx];
    public enum Colour
    {
        Red,
        Green,
        Chaos,
    }

    private void Change(int delta)
    {
        var target = (idx + delta + Colours.Count) % Colours.Count;
        if (CurrentColour == Colours[target]) return;
        idx = target;
        rend.material = Dict[CurrentColour].Mat;
        timer = 0;

        AudioMap.Cloth.Change.Play();
    }
    public void Next() => Change(1);
    public void Prev() => Change(-1);

    private void Use()
    {
        ClothConfig config = Dict[CurrentColour];
        if (!config.Ready) return;

        config.Refresh();
        timer = 0;

        switch (CurrentColour)
        {
            case Colour.Red:
                BattleManager.Instance.Flag.SetBullPassionateFlag(true);
                break;
            case Colour.Green:
                BattleManager.Instance.Flag.SetBullHesitateFlag(true);
                break;
            case Colour.Chaos:
                configs.Where(c => c.Colour != Colour.Chaos).ForEach(c => c.Tick(30));
                break;
        }
        Debug.Log($"使用了{CurrentColour}斗牛布");

        AudioMap.Cloth.Use.Play();
        Instantiate(config.Eff, transform).GetComponent<ParticleSystem>().Play();
    }
}
