using Bingyan;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/Player")]
public class PlayerConfig : ScriptableObject
{
    [Header("移动")]
    [SerializeField, Title("基础速度")] private float baseSpeed = 5;
    [SerializeField, Title("旋转速度")] private float rotateSpeed = 2;

    public float BaseSpeed => baseSpeed;
    public float RotationSpeed => rotateSpeed;
}
