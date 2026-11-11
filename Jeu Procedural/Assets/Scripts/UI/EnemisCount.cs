using TMPro;
using UnityEngine;

public class EnemisCount : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private VictoryConditions _victoryConditions;
    [SerializeField] private TextMeshProUGUI _count;

    private void Start()
    {
        _count.text = _victoryConditions.ENEMY_KILLED + " / " + _victoryConditions.ENEMY_TO_KILLED;
    }

    private void Update()
    {
        _count.text = _victoryConditions.ENEMY_KILLED + " / " + _victoryConditions.ENEMY_TO_KILLED;
    }
}
