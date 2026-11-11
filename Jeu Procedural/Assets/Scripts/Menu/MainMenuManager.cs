using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    void Start()
    {
        PlayerPrefs.DeleteAll();
    }
    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteKey("currentLevel");
        PlayerPrefs.DeleteKey("percentToKill");
        PlayerPrefs.Save();
    }
    
}
