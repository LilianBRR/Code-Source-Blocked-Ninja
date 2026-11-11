using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomStyle", menuName = "Dungeon/Room Style")]
public class RoomStyleSO : ScriptableObject
{
    [Header("Identity")]
    public string _styleId = "Normal";
    public Color _gizmoColor = Color.white;

    [Header("Size")]
    public int _minWidth = 6;
    public int _maxWidth = 12;
    public int _minHeight = 6;
    public int _maxHeight = 12;

    [Header("Prefabs")]
    public GameObject _roomFloorPrefab;
    public GameObject _wallPrefab;

    [Header("Enemy Generation")]
    public int _minEnemies = 1;
    public int _maxEnemies = 4;
    public float _minEnemySpacing = 3f;
    public List<GameObject> _enemyPrefabs = new List<GameObject>();

    [Header("Props Generation")]
    public int _minProps = 1;
    public int _maxProps = 4;
    public float _minPropsSpacing = 3f;
    public List<GameObject> _propsPrefabs = new List<GameObject>();

    [Header("Generation")]
    [Range (1, 10)]
    public int _weight = 1;

    [Header("Rendering")]
    public Vector3 _tileScale = Vector3.one;
    public Vector2 _floorPrefabBaseSize = Vector2.one;
    public Vector2 _corridorFloorPrefabBaseSize = Vector2.one;
}