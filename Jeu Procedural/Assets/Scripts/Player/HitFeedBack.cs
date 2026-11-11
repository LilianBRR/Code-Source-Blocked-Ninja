using UnityEngine;

public class HitFeedBack : MonoBehaviour
{
    [Header("Dependances")]

    [SerializeField] private Health _playerHealth;
    [SerializeField] private GameObject _ninjaRenderer;

    [Header("FeedBack Time")]
    [SerializeField] private float _feedBackDuration;
    [SerializeField] private float _feedBackTimer;

    bool _isHit;
    private void OnEnable()
    {
        Debug.Log("HitFeedBack Enabled");
        if (_playerHealth != null)
        {
            _playerHealth.OnDamageTaken += ActiveHit;
        }

        _isHit = false;
    }

    private void OnDisable()
    {
        Debug.Log("HitFeedBack Disabled");
        if (_playerHealth != null)
        {
            _playerHealth.OnDamageTaken -= ActiveHit;
        }
    }

    private void Update()
    {
        if(_isHit)
        {
            Debug.Log("Hit");  
            _ninjaRenderer.SetActive(true);
            _feedBackTimer += Time.deltaTime;
            if (_feedBackTimer >= _feedBackDuration)
            {
                Debug.Log("FeedBack End");  
                _ninjaRenderer.SetActive(false);
                _feedBackTimer = 0;
                _isHit = false;
            }
           
        }
    }
    void ActiveHit(float maxHealth, float currentHealth)
    {
        _isHit = true;
    }
        
    
}
