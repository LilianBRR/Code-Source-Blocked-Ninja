using TMPro;
using UnityEngine;

public class LevelCount : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private TextMeshProUGUI _count;
    void Start()
    {
        _count.text = "Level " + PlayerPrefs.GetInt("currentLevel");
    }
}
