using TMPro;
using UnityEngine;
using System;

public class SentenceResetSave : MonoBehaviour
{
    [Header("Sentence Reset")]
    [SerializeField] private TextMeshProUGUI _sentence;
    void Start()
    {
        if (PlayerPrefs.HasKey("currentLevel"))
        {
            _sentence.text = "You are level " + PlayerPrefs.GetInt("currentLevel") +  " & your best time is : ";
            if (PlayerPrefs.HasKey("bestTimer"))
            {
                float bestTime = PlayerPrefs.GetFloat("bestTimer");
                TimeSpan timeSpan = TimeSpan.FromSeconds(bestTime);
                _sentence.text += string.Format("{0:00}:{1:00}.{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
            }
            else
            {
                _sentence.text += "--:--.--";
            }
        }
        else
        {
            _sentence.text = "You are level 1 & your best time is : --:--.--";
        }
    }

    public void OnReset()
    {
        if (PlayerPrefs.HasKey("currentLevel"))
        {
            _sentence.text = "You are level " + PlayerPrefs.GetInt("currentLevel") +  " & your best time is : ";
            if (PlayerPrefs.HasKey("bestTimer"))
            {
                float bestTime = PlayerPrefs.GetFloat("bestTimer");
                TimeSpan timeSpan = TimeSpan.FromSeconds(bestTime);
                _sentence.text += string.Format("{0:00}:{1:00}.{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
            }
            else
            {
                _sentence.text += "--:--.--";
            }
        }
        else
        {
            _sentence.text = "You are level 1 & your best time is : ";
            if (PlayerPrefs.HasKey("bestTimer"))
            {
                float bestTime = PlayerPrefs.GetFloat("bestTimer");
                TimeSpan timeSpan = TimeSpan.FromSeconds(bestTime);
                _sentence.text += string.Format("{0:00}:{1:00}.{2:00}", timeSpan.Minutes, timeSpan.Seconds,timeSpan.Milliseconds / 10);
            }
            else
            {
                _sentence.text += "--:--.--";
            }
        }
    }
}
