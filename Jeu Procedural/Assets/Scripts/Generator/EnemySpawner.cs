
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] Transform _enemyParent;

    [Header("Spawn Settings")]
    [SerializeField] float _wallMargin = 1f;
    [SerializeField] int _maxAttemptsPerEnemy = 30;
    [SerializeField] float _spawnHeight = 2f;

    List<Vector3> _spawnedPositions = new List<Vector3>();

    // Generates enemies inside every room.
    public void GenerateEnemies(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            if (room == null || room._roomStyle == null)
                continue;

            SpawnEnemiesInRoom(room);
        }
    }

    // Return the spawned positions of the enemies
    public List<Vector3> GetSpawnedPos()
    {
        return _spawnedPositions;
    }

    // Clears enemies inside every room.
    public void ClearEnemies()
    {
        List<GameObject> childTransforms = new List<GameObject>();

        foreach (Transform t in _enemyParent)
        {
            childTransforms.Add(t.gameObject);
        }

        foreach (var child in childTransforms)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    // Generates a random number of enemies inside one room.
    void SpawnEnemiesInRoom(Room room)
    {
        var style = room._roomStyle;

        if (style._enemyPrefabs == null || style._enemyPrefabs.Count == 0)
            return;

        int min = Mathf.Max(0, style._minEnemies);
        int max = Mathf.Max(min, style._maxEnemies);

        int enemyCount = Random.Range(min, max + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            if (TryGetSpawnPosition(room._rect, style._minEnemySpacing, _spawnedPositions, out Vector3 position))
            {
                GameObject prefab = style._enemyPrefabs[Random.Range(0, style._enemyPrefabs.Count)];

                if (prefab == null)
                    continue;

                Vector3 eulerAngles = new Vector3(0f, Random.Range(0, 360), 0f);
                Quaternion rotation = Quaternion.Euler(eulerAngles);

                GameObject enemy = Instantiate(prefab, position, rotation, _enemyParent);

                _spawnedPositions.Add(position);
            }
        }
    }

    // Finds a valid random position inside the room.
    bool TryGetSpawnPosition(Rect roomRect, float minSpacing, List<Vector3> existingPositions, out Vector3 position)
    {
        position = Vector3.zero;

        float minX = roomRect.xMin + _wallMargin;
        float maxX = roomRect.xMax - _wallMargin;
        float minZ = roomRect.yMin + _wallMargin;
        float maxZ = roomRect.yMax - _wallMargin;

        if (minX >= maxX || minZ >= maxZ)
            return false;

        for (int attempt = 0; attempt < _maxAttemptsPerEnemy; attempt++)
        {
            Vector3 candidate = new Vector3(Random.Range(minX, maxX), _spawnHeight, Random.Range(minZ, maxZ));

            bool valid = true;

            foreach (Vector3 otherPosition in existingPositions)
            {
                float distance = Vector2.Distance(new Vector2(candidate.x, candidate.z), new Vector2(otherPosition.x, otherPosition.z));

                if (distance < minSpacing)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            position = candidate;
            return true;
        }

        return false;
    }
}