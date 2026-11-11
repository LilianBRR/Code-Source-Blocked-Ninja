using System;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyFighterBehavior : MonoBehaviour
{

    [Header("Depandences")]

    [SerializeField] private Health _health;

    void Start()
    {
        _health.OnDamageTaken += OnDamage;
    }

    private void OnDestroy()
    {
        _health.OnDamageTaken -= OnDamage;
    }
    void OnDamage(float maxHealth, float currentHealth)
    {
        if(currentHealth <= 0)
        {
            Variables.Object(gameObject).Set("_isDead", true);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Rigidbody enemyRb = GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = true;
            }
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Rigidbody enemyRb = GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = false;
            }
        }
    }

    public void SetChaseRange(bool isInRange)
    {
        Variables.Object(gameObject).Set("_playerInChaseRange", isInRange);
    }
    public void SetAttackRange(bool isInRange)
    {
        Variables.Object(gameObject).Set("_playerInAttackRange", isInRange);
    }
}
