using System;
using UnityEngine;

public abstract class ManagerBase : MonoBehaviour
{
    public abstract void Init();
}

public abstract class ManagerBase<T> : ManagerBase where T : ManagerBase
{
    private static T instance;
    public static T Instance => instance ? instance :
        throw new NullReferenceException($"找不到{typeof(T).Name}，请检查初始化顺序!");

    public override void Init() => instance = this as T;
    protected virtual void OnDestroy() => instance = null;
}
