using Unity.VisualScripting;
using UnityEngine;

public class EnemyShooterBehavior : MonoBehaviour
{
    [Header("Depandences")]

    [SerializeField] private Health _health;

    void Start()
    {
        _health.OnDamageTaken += OnDamage;
        //world align uv triplanar
    }

    private void OnDestroy()
    {
        _health.OnDamageTaken -= OnDamage;
    }
    void OnDamage(float maxHealth, float currentHealth)
    {
        if (currentHealth <= 0)
        {
            Variables.Object(gameObject).Set("_isDead", true);
        }
    }

    public void SetAttackRange(bool isInRange)
    {
        Variables.Object(gameObject).Set("_playerInAttackRange", isInRange);
    }
}
