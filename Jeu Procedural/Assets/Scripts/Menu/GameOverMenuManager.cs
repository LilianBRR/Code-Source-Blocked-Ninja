using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class GameOverMenuManager : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private VictoryConditions _victoryConditions;
    public void Restart()
    {
        Time.timeScale = 1f;
        _victoryConditions.ResetPercentToKill();
        SceneManager.LoadScene("Game");
    }
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        _victoryConditions.ResetPercentToKill();
        SceneManager.LoadScene("MainMenu");
    }

}
