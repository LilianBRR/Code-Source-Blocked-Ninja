using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private Collider _weaponCollider;
    [SerializeField] private int _weaponDamage;
    [SerializeField] private GameObject _weaponTrail;

    [Header("Attack Time")]
    [SerializeField] private float _attackDuration;
    [SerializeField] private float _attackTimer;

    [Header("Gizmos Settings")]
    [SerializeField] private Color _activeGizmosColor = new Color(1f, 0f, 0f, 0.4f); 
    [SerializeField] private Color _inactiveGizmosColor = new Color(0f, 1f, 0f, 0.2f);
    [SerializeField] private bool _showGizmosOnlyWhenAttacking = false;

    private bool _isAttacking;

    private HashSet<GameObject> _hit = new HashSet<GameObject>();
    void Start()
    {
        _attackTimer = _attackDuration;
        _isAttacking = false;
        _weaponCollider.enabled = false;

        if(_weaponTrail.activeSelf)
        { 
            _weaponTrail.SetActive(false);
        }

    }
    void Update()
    {
        if(_isAttacking == true)
        {
            _attackTimer -= Time.deltaTime;
            if(_attackTimer <= 0)
            {
                DisableWeapon();
                _attackTimer = _attackDuration;
            }
        }
    }

   public void EnableWeapon()
    {
        _hit.Clear();
        _weaponCollider.enabled = true;
        _isAttacking = true;
        _weaponTrail.SetActive(true);
    }

    public void DisableWeapon()
    {
        _weaponCollider.enabled = false;
        _isAttacking = false;
        _weaponTrail.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayerAttackingEnemy = CompareTag("PlayerMeleeWeapon") && other.CompareTag("Enemy");
        bool isEnemyAttackingPlayer = CompareTag("Enemy") && other.CompareTag("Player");

        if (!isPlayerAttackingEnemy && !isEnemyAttackingPlayer) return;
        if (_hit.Contains(other.gameObject)) return;

        _hit.Add(other.gameObject);

        if (other.TryGetComponent<Health>(out var health))
        {
            health.TakeDamage(_weaponDamage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        bool isEnemyAttackingPlayer = CompareTag("Enemy") && other.CompareTag("Player");

        if (!isEnemyAttackingPlayer) return;
        
        if (_hit.Contains(other.gameObject)) return;

        _hit.Add(other.gameObject);

        if (other.TryGetComponent<Health>(out var health))
        {
            health.TakeDamage(_weaponDamage);
        }
    }

    private void OnDrawGizmos()
    {
        if (_weaponCollider == null) return;
        if (_showGizmosOnlyWhenAttacking && !_isAttacking) return;
        Gizmos.color = _isAttacking ? _activeGizmosColor : _inactiveGizmosColor;
        Gizmos.matrix = Matrix4x4.TRS(_weaponCollider.transform.position, _weaponCollider.transform.rotation, _weaponCollider.transform.lossyScale);
        if (_weaponCollider is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (_weaponCollider is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (_weaponCollider is CapsuleCollider capsule)
        {
            Gizmos.DrawSphere(capsule.center, capsule.radius);
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireSphere(capsule.center, capsule.radius);
        }
    }
}
