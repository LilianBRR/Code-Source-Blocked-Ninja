using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Dungeon/MapData")]
public class MapDataSO : ScriptableObject
{
    [Header("Identity")]
    public string _styleId = "Map";
    public Color _gizmoColor = Color.white;

    [Header("Size")]
    public int _minWidth = 100;
    public int _maxWidth = 300;
    public int _minLength = 100;
    public int _maxLength = 300;

    [Header("Rooms")]
    public int _maxRooms = 40;
    public int _minRooms = 10;  // not used for now
    public int _padding = 2;
    public int _maxTileSize = 10;

    [Header("Corridor")]
    public GameObject _corridorFloorPrefab;
    public GameObject _openingPrefab;
    public int _corridorSize = 4;
    public int _maxCorridorLenght = 10;

    [Header("Wall")]
    public GameObject _wallPrefab;
    public int _wallWidth = 4;
    public int _wallHeight = 6;
    public int _maxWallSize = 10;

    [Header("Generation")]
    public int _seed = 5;
    public bool _randomSeed = false;

    [Header("Position")]
    public Vector3 _position = Vector3.zero; // not used for now

    [Header("GeneratorBSP")]
    public Vector2 _minNodeSize = new Vector2(8f, 8f);
    public int _maxDepth = 4;
}