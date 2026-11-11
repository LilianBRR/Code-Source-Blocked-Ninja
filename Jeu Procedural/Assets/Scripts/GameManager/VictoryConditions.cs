using System;
using UnityEngine;

public class VictoryConditions : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] Transform _enemyParent;
    [SerializeField] GeneratorBSP _generator;

    [Header("Config")]
    [SerializeField, Range(0, 1)] public float _killPercentageToWin;

    private GameObject _endRoomDoor;

    private int _enemyNb = 0;
    private int _enemyNbKilled = 0;
    private int _enemyNbToKill = 0;
    [SerializeField] private int _currentLevel = 1;
    private int _levelMax = 5;

    public int ENEMY_KILLED => _enemyNbKilled;
    public int ENEMY_TO_KILLED => _enemyNbToKill;


    private bool _endRoomUnlocked = false;

    void Awake()
    {
        if (!PlayerPrefs.HasKey("currentLevel"))
        {
            PlayerPrefs.SetInt("currentLevel", _currentLevel = 1);
        }
    }

    void Start()
    {
        if(PlayerPrefs.HasKey("percentToKill"))
        {
            _killPercentageToWin = PlayerPrefs.GetFloat("percentToKill");
        }
     
      

        _enemyParent = GameObject.FindWithTag("EnemiesParent").transform;
        _generator = GameObject.FindWithTag("Map").GetComponent<GeneratorBSP>();

        _enemyNb = _enemyParent.childCount;
        _enemyNbToKill = Mathf.CeilToInt(_enemyNb * _killPercentageToWin);
        _endRoomDoor = _generator.GetEndRoomDoor();

        _endRoomDoor.SetActive(true);
    }

    void Update()
    {        
        if(_enemyNb - _enemyNbKilled != _enemyParent.childCount)
            UpdateNbEnemies();
    }

    private void UpdateNbEnemies()
    {
        _enemyNbKilled++;

        print(_enemyNbKilled + " /// " + _enemyNbToKill);

        if (_enemyNbKilled >= _enemyNbToKill)
            UnlockEndRoom();
    }

    private void UnlockEndRoom()
    {
        if (_endRoomUnlocked)
            return;

        _endRoomUnlocked = true;

        foreach(Transform child in _endRoomDoor.transform)
        {
            child.gameObject.SetActive(false);
        }
        _endRoomDoor.GetComponent<BoxCollider>().isTrigger = true;
    }

    public void UpdateEnemyToKill(float add)
    {
        _killPercentageToWin += add;
        _currentLevel++;
        if (_killPercentageToWin >= 1)
        {
            _killPercentageToWin = 1;
            
        }
        if(_currentLevel >= _levelMax)
        {
            _currentLevel = _levelMax;
        }
        PlayerPrefs.SetFloat("percentToKill", _killPercentageToWin);
        PlayerPrefs.SetInt("currentLevel", _currentLevel);
        PlayerPrefs.Save();
    }
    public void ResetPercentToKill()
    {
        PlayerPrefs.DeleteKey("percentToKill");
        PlayerPrefs.DeleteKey("currentLevel");
        PlayerPrefs.Save();
    }
}