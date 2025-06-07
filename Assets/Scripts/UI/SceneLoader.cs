using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string Startname;
    [SerializeField] private string FirsLevelname;
    [SerializeField] private string LastLevelname;
    public void LoadStartScene(string Startname)
    {
        SceneManager.LoadScene(Startname);
    }
    public void LoadFirstScene(string FirsLevelname)
    {
        SceneManager.LoadScene(FirsLevelname);
    }
    public void LoadLastScene(string LastLevelname)
    {
        SceneManager.LoadScene(LastLevelname);
    }
    public void QuitE()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


}
