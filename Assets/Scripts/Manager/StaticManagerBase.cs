using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaticManagerBase<T> where T : new()
{
    private static T instance;
    public static T Instance => instance ??= new T();
}
