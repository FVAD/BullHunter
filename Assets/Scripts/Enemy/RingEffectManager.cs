using System.Collections.Generic;
using UnityEngine;

public class RingEffectManager : MonoBehaviour
{
    public static RingEffectManager Instance;

    // 圆环预制体配置
    [System.Serializable]
    public class RingEffectConfig
    {
        public GameObject ringPrefab; // 圆环预制体

        public ParticleSystem ringParticlePrefab;
        public ParticleSystem impactParticlePrefab;
    }

    [SerializeField] private RingEffectConfig config;

    // 存储所有活动中的圆环
    private List<ActiveRing> activeRings = new List<ActiveRing>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnRing(Vector3 position, float radius, float duration, Color? color = null)
    {
        GameObject ringContainer = new GameObject("RingEffect");
        ringContainer.transform.position = position;

        SpriteRenderer ringRenderer = CreateRingMesh(ringContainer, radius, color ?? Color.cyan);

        ParticleSystem ringParticles = Instantiate(config.ringParticlePrefab, position, Quaternion.Euler(90f, 0f, 0f), ringContainer.transform);
        ParticleSystem impactParticles = Instantiate(config.impactParticlePrefab, position, Quaternion.identity, ringContainer.transform);
        ConfigureParticles(ringParticles, radius, color ?? Color.cyan);
        ConfigureParticles(impactParticles, radius, color ?? Color.cyan);

        ActiveRing newRing = new ActiveRing(ringContainer, ringRenderer, ringParticles, impactParticles, duration);
        activeRings.Add(newRing);
        newRing.Start();
    }

    // 计算旋转覆盖的最大圆半径
    public static float CalculateMaxCoverRadius(BoxCollider targetCollider)
    {
        if (targetCollider == null)
        {
            Debug.LogError("Target collider is not assigned!");
            return 0f;
        }

        // 获取BoxCollider的局部坐标点
        Vector3 center = targetCollider.center;
        Vector3 size = targetCollider.size;

        // 计算BoxCollider的8个顶点（局部坐标）
        Vector3[] vertices = new Vector3[8];

        // 底部四个顶点
        vertices[0] = center + new Vector3(-size.x / 2, -size.y / 2, -size.z / 2);
        vertices[1] = center + new Vector3(-size.x / 2, -size.y / 2, size.z / 2);
        vertices[2] = center + new Vector3(size.x / 2, -size.y / 2, -size.z / 2);
        vertices[3] = center + new Vector3(size.x / 2, -size.y / 2, size.z / 2);

        // 顶部四个顶点
        vertices[4] = center + new Vector3(-size.x / 2, size.y / 2, -size.z / 2);
        vertices[5] = center + new Vector3(-size.x / 2, size.y / 2, size.z / 2);
        vertices[6] = center + new Vector3(size.x / 2, size.y / 2, -size.z / 2);
        vertices[7] = center + new Vector3(size.x / 2, size.y / 2, size.z / 2);

        float maxRadius = 0f;

        // 计算每个顶点到Y轴的距离（忽略Y坐标）
        foreach (Vector3 vertex in vertices)
        {
            // 计算在X-Z平面上的距离
            float radius = Mathf.Sqrt(vertex.x * vertex.x + vertex.z * vertex.z);

            if (radius > maxRadius)
            {
                maxRadius = radius;
            }
        }

        return maxRadius;
    }

    private SpriteRenderer CreateRingMesh(GameObject parent, float radius, Color color)
    {
        GameObject ringObject = Instantiate(config.ringPrefab, parent.transform);
        ringObject.name = "RingMesh";

        ringObject.transform.localPosition = Vector3.zero;
        // ringObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        ringObject.transform.localScale = new Vector3(radius, 1f, radius);

        // 设置材质
        SpriteRenderer renderer = ringObject.GetComponentInChildren<SpriteRenderer>();
        renderer.material.color = color;

        return renderer;
    }

    private void ConfigureParticles(ParticleSystem ps, float radius, Color color)
    {
        var main = ps.main;
        var shape = ps.shape;

        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;

        main.startColor = color;

        main.startSize = new ParticleSystem.MinMaxCurve(radius * 0.1f, radius * 0.2f);
    }

    // 移除所有圆环
    public void RemoveAllRings()
    {
        for (int i = activeRings.Count - 1; i >= 0; i--)
        {
            activeRings[i].Stop();
        }
        activeRings.Clear();
    }

    private void Update()
    {
        // 更新所有活动中的圆环
        for (int i = activeRings.Count - 1; i >= 0; i--)
        {
            if (!activeRings[i].Update(Time.deltaTime))
            {
                activeRings.RemoveAt(i);
            }
        }
    }

    // 活动圆环的内部类
    private class ActiveRing
    {
        public GameObject container;
        public SpriteRenderer ringRenderer;
        public ParticleSystem ringParticles;
        public ParticleSystem impactParticles;
        public float duration;
        public float elapsedTime;
        public bool isActive;

        public ActiveRing(GameObject container, SpriteRenderer ringRenderer,
                         ParticleSystem ringParticles, ParticleSystem impactParticles,
                         float duration)
        {
            this.container = container;
            this.ringRenderer = ringRenderer;
            this.ringParticles = ringParticles;
            this.impactParticles = impactParticles;
            this.duration = duration;
            elapsedTime = 0f;
            isActive = false;
        }

        public void Start()
        {
            ringParticles.Play();
            impactParticles.Play();
            isActive = true;
        }

        public void Stop()
        {
            if (isActive)
            {
                ringParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                impactParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                isActive = false;
            }
        }

        // 返回false表示可以销毁
        public bool Update(float deltaTime)
        {
            if (!isActive)
            {
                // 检查粒子是否完全停止
                if (!ringParticles.IsAlive() && !impactParticles.IsAlive())
                {
                    Destroy(container);
                    return false;
                }
                return true;
            }

            elapsedTime += deltaTime;

            // 更新透明度
            float alpha = Mathf.Clamp01(1 - (elapsedTime / duration));
            Color color = ringRenderer.material.color;
            color.a = alpha;
            ringRenderer.material.color = color;

            // 检查是否结束
            if (elapsedTime >= duration)
            {
                Stop();
            }

            return true;
        }


    }
}