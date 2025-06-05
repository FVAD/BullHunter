using Bingyan;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WeaponVisualizer : MonoBehaviour
{
    [SerializeField, Title("显示时长")] private float show = 0.5f;
    [SerializeField, Title("隐藏时长")] private float hide = 0.5f;

    private static readonly int id = Shader.PropertyToID("_Progress");
    private float progress;
    private TweenHandle handle;

    private Renderer rend;
    private Material mat;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material = Instantiate(rend.material);
        progress = 1;
        mat.SetFloat(id, 1);
    }

    public WeaponVisualizer Show()
    {
        handle = Tween.Linear(show)
                      .Process(f => mat?.SetFloat(id, progress = f))
                      .Build()
                      .Play();
        return this;
    }
    public void Hide()
    {
        handle.Stop();
        var current = progress;
        Tween.Linear(current * hide)
             .Process(f => mat?.SetFloat(id, progress = current * (1 - f)))
             .Finish(() => Destroy(gameObject))
             .Build()
             .Play();
    }
}
