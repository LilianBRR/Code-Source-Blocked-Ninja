using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] Camera _camera;
    [SerializeField] Rigidbody _rb;

    [Header("Variables")]
    [SerializeField] float _speed;
    [SerializeField] float _coefSprint;
    [SerializeField] private float _initalSpeed;
    
    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 25f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    
    private bool _isDashing;
    private bool _canDash = true;
    
    Vector3 _moveInputDirection;

    void Start()
    {
        _speed = _initalSpeed;
        _initalSpeed = _speed;
    }

    void FixedUpdate()
    {
        Vector3 f = _camera.transform.forward;
        f.y = 0f;
        var r = _camera.transform.right;
        var dir = (f * _moveInputDirection.z) + (r * _moveInputDirection.x);
        var finalspeed = _speed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + dir * finalspeed);

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
    }

    public void StartDash()
    {
        if (_canDash && !_isDashing)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        
        
        _canDash = false;
        _isDashing = true;

        bool originalGravity = _rb.useGravity;
        Vector3 originalVelocity = _rb.linearVelocity; 
        Vector3 dashDirection = transform.forward;
        _rb.useGravity = false;

        float elapsedTime = 0f;
        while (elapsedTime < _dashDuration)
        {
            _rb.linearVelocity = dashDirection * _dashSpeed;
            elapsedTime += Time.deltaTime;

            yield return null; 
        }

        _rb.useGravity = originalGravity;
        _rb.linearVelocity = originalVelocity;
        _isDashing = false;

        yield return new WaitForSeconds(_dashCooldown);
        _canDash = true;
    }

    public void SetMoveInput(Vector2 dir)
    {
        _moveInputDirection = new Vector3(dir.x, 0f, dir.y);
        
    }

    public void SetIsSprinting(bool value)
    {
        if (value)
        {
            _speed = _initalSpeed * _coefSprint;
        }
        else
        {
            _speed = _initalSpeed;
        }
    }
}
