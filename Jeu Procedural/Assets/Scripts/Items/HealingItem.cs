using UnityEngine;

public class HealingItem : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private float _rotationSpeed;
    void Start()
    {
        
    }

    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        var finalSpeed = _rotationSpeed * Time.deltaTime;
        transform.Rotate(0, finalSpeed, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(other.TryGetComponent<Health>(out Health health))
            {
                health.Heal(25);
            }
            Destroy(gameObject);
        }
    }
}
