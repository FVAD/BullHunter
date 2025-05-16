using Bingyan;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField, Title("背景音乐")] private AudioRef bgm;
    private void Awake()
    {
        GetComponentsInChildren<ManagerBase>().ForEach(m => m.Init());

        if(bgm) bgm.PlaySingleton();
    }
}
