using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMenuManager : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private VictoryConditions _victoryConditions;
    [SerializeField] Chrono _chrono;

    public void StopGame()
    {
        _chrono.StopChrono();
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        _victoryConditions.UpdateEnemyToKill(0.2f);
        SceneManager.LoadScene("Game");
    }
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        _victoryConditions.UpdateEnemyToKill(0.2f);
        SceneManager.LoadScene("MainMenu");
    }
}
