using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public Slider soundSlider;
    public Slider musicSlider;
    public GameObject optionsPanel;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
                ResumeGame();
            else
                OpenOptions();
        }
    }
    public void ResumeGame()
    {
        optionsPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void QuitGame()
    {
        Application.Quit(); 
        Debug.Log("Quit Game");
    }
}