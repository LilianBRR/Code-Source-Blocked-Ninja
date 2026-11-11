using System;
using TMPro;
using UnityEngine;

public class Chrono : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private TextMeshProUGUI _chrono;
    float _currentTime;
    float _bestTime;
    bool _isRunning;
    
    public float CURRENT_TIME => _currentTime;
    public float BEST_TIME => _bestTime;
    void Start()
    {
        ResetChrono();
        StartChrono();
        if (PlayerPrefs.HasKey("bestTimer"))
        {
            _bestTime = PlayerPrefs.GetFloat("bestTimer");
        }
        else
        {
            _bestTime = 0;
        }
    }

    void Update()
    {
        if(_isRunning)
        {
            _currentTime += Time.deltaTime;
            UpdateChrono(_currentTime);
        }
    }

    void UpdateChrono(float currentTime)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(_currentTime);
        _chrono.text = string.Format("{0:00}:{1:00}.{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
    }

    void StartChrono()
    {
        _isRunning = true;
    }

    public void StopChrono()
    {
        _isRunning = false;
        if (_currentTime >= _bestTime)
        {
            _bestTime = _currentTime;
            SaveChrono();
        }
        
    }

    void ResetChrono()
    {
        _currentTime = 0;
    }
    
    public void SaveChrono()
    {
        PlayerPrefs.SetFloat("bestTimer", _bestTime);
    }
}
