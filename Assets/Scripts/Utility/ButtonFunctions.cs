using NaughtyAttributes;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    [Scene] public string sceneName;

    public void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

}
