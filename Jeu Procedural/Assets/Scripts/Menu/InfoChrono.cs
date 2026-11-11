using System;
using TMPro;
using UnityEngine;

public class InfoChrono : MonoBehaviour
{
    [SerializeField] Chrono _chrono;
    [SerializeField] TextMeshProUGUI _text;

    private void Update()
    {
        float currentTime = _chrono.CURRENT_TIME;
        float bestTime = _chrono.BEST_TIME;
        TimeSpan currentTimeSpan = TimeSpan.FromSeconds(currentTime);
        TimeSpan bestTimeSpan = TimeSpan.FromSeconds(bestTime);
        _text.text = "TIME : " + string.Format("{0:00}:{1:00}.{2:00}", currentTimeSpan.Minutes, currentTimeSpan.Seconds,currentTimeSpan.Milliseconds / 10)  
                               + " | BEST TIME : " + string.Format("{0:00}:{1:00}.{2:00}", bestTimeSpan.Minutes, bestTimeSpan.Seconds,bestTimeSpan.Milliseconds / 10);
    }
}
