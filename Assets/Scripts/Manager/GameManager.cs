using Bingyan;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField, Title("背景音乐")] private AudioRef bgm;
    private void Awake()
    {
        GetComponentsInChildren<ManagerBase>().ForEach(m => m.Init());

        AudioMap.BGM.Title.Stop();
        AudioMap.BGM.Battle.Stop();
        if(bgm) bgm.PlaySingleton();
    }
}
