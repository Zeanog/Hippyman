using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerProxy : MonoBehaviour
{
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    public void LoadSceneAsync(string name)
    {
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
    }
}