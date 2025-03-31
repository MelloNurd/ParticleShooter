using NaughtyAttributes;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    [Scene] public string sceneName;

    public void OnClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

}
