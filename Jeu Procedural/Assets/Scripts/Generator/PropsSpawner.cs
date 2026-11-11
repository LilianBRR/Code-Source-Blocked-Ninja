
using System.Collections.Generic;
using UnityEngine;

public class PropsSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] Transform _propParent;
    [SerializeField] RoomStyleSO _endRoom;

    [Header("Spawn Settings")]
    [SerializeField] float _wallMargin = 1f;
    [SerializeField] int _maxAttemptsPerProp = 30;
    [SerializeField] float _spawnHeight = 2f;

    float _prevWallMargin = 1f;

    // Generates Props inside every room.
    public void GenerateProps(List<Room> rooms, List<Vector3> enemiesPositions)
    {
        _prevWallMargin = _wallMargin;

        foreach (Room room in rooms)
        {
            if (room == null || room._roomStyle == null)
                continue;

            if (room._roomStyle == _endRoom) _wallMargin = 0.7f;
            else _wallMargin = _prevWallMargin;

            SpawnPropsInRoom(room, enemiesPositions);
        }
    }

    // Clears Props inside every room.
    public void ClearProps()
    {
        List<GameObject> childTransforms = new List<GameObject>();

        foreach (Transform t in _propParent)
        {
            childTransforms.Add(t.gameObject);
        }

        foreach (var child in childTransforms)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    // Generates a random number of Props inside one room.
    void SpawnPropsInRoom(Room room, List<Vector3> enemiesPositions)
    {
        var style = room._roomStyle;

        if (style._propsPrefabs == null || style._propsPrefabs.Count == 0)
            return;

        int min = Mathf.Max(0, style._minProps);
        int max = Mathf.Max(min, style._maxProps);

        int propCount = Random.Range(min, max + 1);

        int healCount = 0;

        List<Vector3> spawnedPositions = new List<Vector3>();
        spawnedPositions.AddRange(enemiesPositions);

        for (int i = 0; i < propCount; i++)
        {
            if (TryGetSpawnPosition(room._rect, style._minPropsSpacing, spawnedPositions, out Vector3 position))
            {
                GameObject prefab = style._propsPrefabs[Random.Range(0, style._propsPrefabs.Count - healCount)];

                if (prefab == null)
                    continue;

                if (prefab.GetComponent<HealingItem>())
                {
                    position.y += 1f;
                    healCount++;
                }

                Vector3 eulerAngles = new Vector3(0f, Random.Range(0, 360), 0f);
                Quaternion rotation = Quaternion.Euler(eulerAngles);

                GameObject prop = Instantiate(prefab, position, rotation, _propParent);

                spawnedPositions.Add(position);
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

        for (int attempt = 0; attempt < _maxAttemptsPerProp; attempt++)
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