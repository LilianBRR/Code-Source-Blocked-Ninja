using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerBrain : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] GameObject _player;
    [SerializeField] Animator _playerAnimator;
    [SerializeField] InputActionAsset _inputActionsAsset;
    [SerializeField] InputActionReference _moveInput;
    [SerializeField] InputActionReference _sprintInput;
    [SerializeField] InputActionReference _attackInput;
    [SerializeField] InputActionReference _dashInput;
    [SerializeField] InputActionReference _pauseInput;

    [Space(10)]
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] MeleeWeapon _meleeWeapon;
    [SerializeField] GameObject _pauseMenu;
    [SerializeField] GameObject _gameOverMenu;
    [SerializeField] GameObject _UI;
    [SerializeField] GameObject _explosionParticle;
    [SerializeField] GameObject _healParticle;
    [SerializeField] GameObject _renderer;
    [SerializeField] Collider _col;

    [Space(10)]
    [SerializeField] Health _health;


    void Start()
    {
        InputActionMap playerMap = _inputActionsAsset.FindActionMap("Player");
        playerMap?.Enable();
        _playerMovement.enabled = true;
        _moveInput.action.started += OnMove;
        _moveInput.action.performed += OnMove;
        _moveInput.action.canceled += OnMoveCancel;

        _sprintInput.action.performed += OnSprint;
        _sprintInput.action.canceled += OnSprintCancel;

        _attackInput.action.performed += OnAttack;

        _dashInput.action.performed += OnDash;

        _pauseInput.action.performed += OnPause;

        _health.OnDamageTaken += OnDamage;
        _health.OnHeal += OnHeal;
    }

    void OnDestroy()
    {
        _moveInput.action.started -= OnMove;
        _moveInput.action.performed -= OnMove;
        _moveInput.action.canceled -= OnMoveCancel;

        _sprintInput.action.performed -= OnSprint;
        _sprintInput.action.canceled -= OnSprintCancel;

        _attackInput.action.performed -= OnAttack;

        _dashInput.action.performed -= OnDash;

        _pauseInput.action.performed -= OnPause;

        _health.OnDamageTaken -= OnDamage;
        _health.OnHeal -= OnHeal;
    }

    void OnMove(InputAction.CallbackContext obj)
    {
        Vector2 inputDir = obj.ReadValue<Vector2>();
        _playerMovement.SetMoveInput(inputDir);
        _playerAnimator.SetBool("isWalking", true);
    }
    void OnMoveCancel(InputAction.CallbackContext obj)
    {
        _playerMovement.SetMoveInput(new Vector2(0, 0));
        _playerAnimator.SetBool("isWalking", false);
    }

    void OnSprint(InputAction.CallbackContext obj)
    {
        _playerMovement.SetIsSprinting(true);
        _playerAnimator.SetBool("isRunning", true);
    }

    void OnSprintCancel(InputAction.CallbackContext obj)
    {
        _playerMovement.SetIsSprinting(false);
        _playerAnimator.SetBool("isRunning", false);
    }

    void OnAttack(InputAction.CallbackContext obj)
    {
        _meleeWeapon.EnableWeapon();
    }
    
    void OnDash(InputAction.CallbackContext obj)
    {
        _playerMovement.StartDash();
    }

    void OnPause(InputAction.CallbackContext obj)
    {
        InputActionMap playerMap = _inputActionsAsset.FindActionMap("Player");
        if (_pauseMenu.activeSelf)
        {
            Time.timeScale = 1f;
            _pauseMenu.SetActive(false);
            _UI.SetActive(true);
            playerMap?.Enable();
        }
        else
        {
            Time.timeScale = 0f;
            _pauseMenu.SetActive(true);
            _UI.SetActive(false);
            playerMap?.Disable();
        }
        
    }

    void OnDamage(float maxHealth, float currentHealth)
    {
        if(currentHealth <= 0)
        {
            Death();
        }
    }

    void OnHeal(float maxHealth, float currentHealth)
    {
        StartCoroutine(PlayParticles());
    }

    private IEnumerator PlayParticles()
    {
        _healParticle.SetActive(true);

        yield return new WaitForSecondsRealtime(1.1f);

        _healParticle.SetActive(false);
    }
    void Death()
    {
        StartCoroutine(PlayDeath());
    }
    private IEnumerator PlayDeath()
    {
        _playerMovement.enabled = false;
        _playerAnimator.SetTrigger("Death");
        _col.enabled = false;
        _player.GetComponent<Rigidbody>().useGravity = false;

        yield return new WaitForSecondsRealtime(2.5f);

        _explosionParticle.SetActive(true);
        _renderer.SetActive(false);
        
        yield return new WaitForSecondsRealtime(1.2f);

        _UI.SetActive(false);
        _gameOverMenu.SetActive(true);
        Time.timeScale = 0f; 
        Destroy(_player);
    }
}
