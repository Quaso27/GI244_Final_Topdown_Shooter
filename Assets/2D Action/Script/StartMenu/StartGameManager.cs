using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour
{
    public string sceneToLoad = "Game";

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LoadGameScene();
        }
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}