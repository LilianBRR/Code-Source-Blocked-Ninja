using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private InputActionReference _restartInput;
    [SerializeField] private SceneLoader _sceneLoader;
    void Start()
    {
        _restartInput.action.started += OnRestart;
    }

    void OnDestroy()
    {
        _restartInput.action.started -= OnRestart;
    }
    void Update()
    {
        
    }

    void OnRestart(InputAction.CallbackContext obj)
    {
        _sceneLoader.ReloadCurrentScene();
    }
}
