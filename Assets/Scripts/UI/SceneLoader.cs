using Bingyan;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
        AudioMap.UI.Popup.Play();
    }

    public void QuitE()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public static void To(string name, float time) =>
        Flow.Create()
            .Delay(time)
            .Then(() => SceneManager.LoadScene(name))
            .Run();

}
