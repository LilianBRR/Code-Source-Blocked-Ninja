using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PauseMenuManager : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] InputActionAsset _inputActionsAsset;
    public void Resume()
    {
        Time.timeScale = 1f;
        InputActionMap playerMap = _inputActionsAsset.FindActionMap("Player");
        playerMap?.Enable();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
