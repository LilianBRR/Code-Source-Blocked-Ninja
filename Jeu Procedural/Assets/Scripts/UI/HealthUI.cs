using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] Health _health;
    [SerializeField] Slider _healthBar;
    [SerializeField] GameObject _fill;
    void Start()
    {
        _health.OnDamageTaken += OnDamage;
        _health.OnHeal += OnDamage;
        _healthBar.value = _healthBar.maxValue;
        _fill.SetActive(true);
    }

    private void OnDestroy()
    {
        _health.OnDamageTaken -= OnDamage;
        _health.OnHeal -= OnDamage;
    }


    void OnDamage(float maxHealth, float currentHealth)
    {
        float healthRatio = currentHealth / maxHealth;
        _healthBar.value = healthRatio; 

        if(_healthBar.value <= 0)
        {
            _fill.SetActive(false);
        }
    }
}
