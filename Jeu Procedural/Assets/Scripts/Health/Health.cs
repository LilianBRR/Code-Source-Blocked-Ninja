using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Health : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    public event Action<float, float> OnDamageTaken;
    public event Action<float, float> OnHeal;

    private void Start()
    {
        _currentHealth = _maxHealth;
        
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }
        OnDamageTaken?.Invoke(_maxHealth, _currentHealth);
        Debug.Log(gameObject.name + " take " + damage + " damage.");
    }

    public void Heal(int health)
    {
        _currentHealth += health;
        if(_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
        OnHeal?.Invoke(_maxHealth, _currentHealth);
    }
}
