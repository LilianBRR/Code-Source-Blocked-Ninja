using System;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    [Header("Depandences")]

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _speed;
    [SerializeField] private int _projectileDamage;

    Vector3 _target;
    void Start()
    {
        Destroy(gameObject, 3f);

        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                return;
            }
        }
       
        _target = new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z);

        Vector3 direction = _playerTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation * Quaternion.Euler(90f, 0f, 0f);
        }

    }
    void Update()
    {
        if (_playerTransform == null) return;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.up * _speed;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            return;

        if(collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<Health>(out var health))
            {

                health.TakeDamage(_projectileDamage);
            }
        }
        Destroy(gameObject);
    }
}
