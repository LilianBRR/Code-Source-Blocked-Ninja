using NaughtyAttributes;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GeneratorBSP : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] MapDataSO _mapData;

    [Header("Dependencies")]
    [SerializeField] Transform _transformParent;

    [Header("Player")]
    [SerializeField] GameObject _player = null;

    [Header("Enemy Generation")]
    [SerializeField] EnemySpawner _enemySpawner;

    [Header("Props Generation")]
    [SerializeField] PropsSpawner _propsSpawner;

    [Header("End Room")]
    [SerializeField] GameObject _endRoomDoorPrefab;

    [Header("Rooms")]
    [SerializeField] RoomStyleSO _spawnRoomStyle;
    [SerializeField] RoomStyleSO _endRoomStyle;
    [SerializeField] List<RoomStyleSO> _listRoomsTypes;

    private readonly Dictionary<Node, Room> _roomByNode = new();
    private readonly Dictionary<Room, List<Room>> _roomConnections = new();

    private readonly HashSet<GameObject> _listTiles = new();

    private List<CorridorSegment> _corridors = new();
    private readonly List<Rect> _wallRects = new();
    private readonly List<CorridorSegment> _originalCorridors = new();
    private readonly List<Room> _listCurrentRooms = new();
    private readonly List<RoomOpening> _roomOpenings = new();

    private Node _root;
    private Node _spawnNode;
    private Node _endNode;

    private GameObject _endRoomDoor;

    private int _nbRooms = 0;

    private void Awake()
    {
        if(!_player) _player = GameObject.FindWithTag("Player");

        Generate();
    }

    public GameObject GetEndRoomDoor()
    {
        return _endRoomDoor;
    }

    [Button("Generate")]
    // Generates the entire dungeon, from the BSP layout to the floors and walls.
    public void Generate()
    {
        ClearRooms();
        InitRandomSeed();

        InitRoot();
        SplitNode();

        GenerateRooms();
        GenerateCorridors();

        ChooseEndRoomByCorridorCount();

        SpawnEndRoomDoor();

        GenerateWalls();

        DrawFloor();
        DrawCorridors();
        DrawCorridorWalls();
        DrawWalls();

        SpawnPlayer();

        _enemySpawner.GenerateEnemies(_listCurrentRooms);
        _propsSpawner.GenerateProps(_listCurrentRooms, _enemySpawner.GetSpawnedPos());
    }

    // Spawns the player at the center of the spawn room.
    private void SpawnPlayer()
    {
        if (_player == null || _spawnNode == null)
            return;

        if (!_roomByNode.TryGetValue(_spawnNode, out Room spawnRoom))
            return;

        Vector2 center = spawnRoom._rect.center;
        Vector3 spawnPosition = new Vector3(center.x, 2f, center.y);

        Rigidbody rb = _player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnPosition;
        }
        else
            _player.transform.position = spawnPosition;
    }

    // Generates walls along both sides of each corridor segment.
    private void DrawCorridorWalls()
    {
        if (_mapData._wallPrefab == null)
            return;

        float corridorWidth = _mapData._corridorSize;
        float wallWidth = _mapData._wallWidth;
        float wallHeight = _mapData._wallHeight;
        float overlap = corridorWidth * 0.6f;

        foreach (CorridorSegment segment in _corridors)
        {
            Vector2 direction = segment.end - segment.start;
            float length = direction.magnitude;

            if (length <= 0.001f)
                continue;

            direction.Normalize();

            // Vérifie si ce segment touche une extrémité du corridor original.
            bool isStart = false;
            bool isEnd = false;

            foreach (CorridorSegment original in _originalCorridors)
            {
                if (Vector2.Distance(segment.start, original.start) < 0.01f && Vector2.Distance(segment.end, original.end) < 0.01f)
                {
                    isStart = true;
                    isEnd = true;
                    break;
                }

                if (Vector2.Distance(segment.start, original.start) < 0.01f)
                    isStart = true;

                if (Vector2.Distance(segment.end, original.end) < 0.01f)
                    isEnd = true;
            }

            // Retire le débordement uniquement aux extrémités.
            Vector2 wallStart = segment.start;
            Vector2 wallEnd = segment.end;

            if (isStart)
                wallStart += direction * overlap;

            if (isEnd)
                wallEnd -= direction * overlap;

            float wallLength = Vector2.Distance(wallStart, wallEnd);

            //if (isEnd || isStart)
            //    wallLength -= _mapData._wallWidth;

            if (wallLength <= 0.001f)
                continue;

            Vector2 center = (wallStart + wallEnd) * 0.5f;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, new Vector3(direction.x, 0f, direction.y));

            float offset = corridorWidth * 0.5f + wallWidth * 0.5f;

            CreateCorridorWall(center + perpendicular * offset, wallLength, wallWidth, wallHeight, rotation);

            CreateCorridorWall(center - perpendicular * offset, wallLength, wallWidth, wallHeight, rotation);
        }
    }

    // Instantiates a corridor wall with the specified position, dimensions, and rotation.
    private void CreateCorridorWall(Vector2 position, float length, float wallWidth, float wallHeight, Quaternion rotation)
    {
        GameObject wall = Instantiate(_mapData._wallPrefab, _transformParent);

        wall.transform.position = new Vector3(position.x, wallHeight * 0.5f, position.y);

        wall.transform.rotation = rotation;

        wall.transform.localScale = new Vector3(length, wallHeight, wallWidth);
    }

    // Draws the walls around the rooms, splitting them into smaller segments.
    private void DrawWalls()
    {
        if (_mapData._wallPrefab == null)
            return;

        foreach (Rect rect in _wallRects)
        {
            if (rect.width <= 0f || rect.height <= 0f)
                continue;

            List<Rect> newWalls = new List<Rect>();

            if (rect.width > rect.height)
                newWalls = RectHelp.Split(rect, new Vector2(_mapData._maxWallSize, _mapData._wallWidth));
            else
                newWalls = RectHelp.Split(rect, new Vector2(_mapData._wallWidth, _mapData._maxWallSize));

            foreach (Rect r in newWalls)
            {
                GameObject wall = Instantiate(_mapData._wallPrefab, _transformParent);

                wall.transform.position = new Vector3(r.center.x, _mapData._wallHeight * 0.5f, r.center.y);

                wall.transform.localScale = new Vector3(r.width, _mapData._wallHeight, r.height);
            }
        }
    }

    // Generates wall segments while leaving openings for connected corridors.
    private void AddWallWithOpenings(Room room, WallSide side, Rect wallRect, float start, float end, float wallThickness)
    {
        List<RoomOpening> openings = _roomOpenings.FindAll(o => o.room == room && o.side == side);

        // Les coordonnées des ouvertures sont celles de l'axe du mur.
        openings.Sort((a, b) => a.center.CompareTo(b.center));

        float current = start;

        foreach (RoomOpening opening in openings)
        {
            float openingStart = Mathf.Clamp(opening.center - opening.width * 0.5f, start, end);

            float openingEnd = Mathf.Clamp(opening.center + opening.width * 0.5f, start, end);

            if (openingStart > current)
                AddWallSegment(side, wallRect, current, openingStart, wallThickness);

            current = Mathf.Max(current, openingEnd);
        }

        if (current < end)
            AddWallSegment(side, wallRect, current, end, wallThickness);
    }

    // Creates a wall segment between two positions along a room wall.
    private void AddWallSegment(WallSide side, Rect wallRect, float start, float end, float wallThickness)
    {
        if (end <= start)
            return;

        Rect segment;

        if (side == WallSide.Left || side == WallSide.Right)
        {
            segment = new Rect(wallRect.xMin, start, wallThickness, end - start);
        }
        else
        {
            segment = new Rect(start, wallRect.yMin, end - start, wallThickness);
        }

        _wallRects.Add(segment);
    }

    // Calculates the wall positions around every room and accounts for corridor openings.
    private void GenerateWalls()
    {
        float t = _mapData._wallWidth;

        foreach (Room room in _listCurrentRooms)
        {
            Rect r = room._rect;

            // Murs verticaux
            Rect left = new Rect(r.xMin - t, r.yMin, t, r.height);
            Rect right = new Rect(r.xMax, r.yMin, t, r.height);

            AddWallWithOpenings(room, WallSide.Left, left, r.yMin, r.yMax, t);

            AddWallWithOpenings(room, WallSide.Right, right, r.yMin, r.yMax, t);

            // Murs horizontaux
            Rect bottom = new Rect(r.xMin - t, r.yMin - t, r.width + 4f * t, t);

            Rect top = new Rect(r.xMin - t, r.yMax, r.width + 4f * t, t);

            AddWallWithOpenings(room, WallSide.Bottom, bottom, r.xMin, r.xMax, t);

            AddWallWithOpenings(room, WallSide.Top, top, r.xMin, r.xMax, t);
        }
    }

    // Instantiates the floor segments used to create the corridors.
    private void DrawCorridors()
    {
        if (_mapData._corridorFloorPrefab == null)
            return;

        float width = _mapData._corridorSize;

        foreach (CorridorSegment corridor in _corridors)
        {
            Vector2 direction = corridor.end - corridor.start;
            float length = direction.magnitude;

            if (length <= 0.001f)
                continue;

            Vector2 center = (corridor.start + corridor.end) * 0.5f;

            GameObject floor = Instantiate(_mapData._corridorFloorPrefab, _transformParent);

            floor.transform.position = new Vector3(center.x, -0.15f, center.y);

            floor.transform.rotation = Quaternion.FromToRotation(Vector3.right, new Vector3(direction.x, 0f, direction.y));

            floor.transform.localScale = new Vector3(length, 1f, width);

            _listTiles.Add(floor);
        }
    }

    // Connects rooms through the BSP tree and subdivides long corridors into smaller segments.
    private void GenerateCorridors()
    {
        _corridors.Clear();
        _originalCorridors.Clear();
        _roomOpenings.Clear();

        if (_root != null)
            ConnectNode(_root);

        _originalCorridors.AddRange(_corridors);

        SubDivideCorridors();
    }

    // Recursively connects rooms belonging to different branches of the BSP tree.
    private List<Room> ConnectNode(Node node)
    {
        if (node.IsLeaf())
        {
            if (_roomByNode.TryGetValue(node, out Room room))
                return new List<Room> { room };

            return new List<Room>();
        }

        List<Room> roomsA = new List<Room>();
        List<Room> roomsB = new List<Room>();

        foreach (Node child in node._children)
        {
            List<Room> childRooms = ConnectNode(child);

            if (roomsA.Count == 0)
                roomsA = childRooms;
            else
                roomsB.AddRange(childRooms);
        }

        if (roomsA.Count > 0 && roomsB.Count > 0)
        {
            Room roomA = null;
            Room roomB = null;
            float minDistance = float.MaxValue;

            foreach (Room a in roomsA)
            {
                foreach (Room b in roomsB)
                {
                    float distance = Vector2.Distance(a._rect.center, b._rect.center);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        roomA = a;
                        roomB = b;
                    }
                }
            }

            if (roomA != null && roomB != null)
                CreateCorridor(roomA, roomB);
        }

        roomsA.AddRange(roomsB);
        return roomsA;
    }

    // Creates a corridor between two rooms and registers their connection.
    private void CreateCorridor(Room roomA, Room roomB)
    {
        Rect a = roomA._rect;
        Rect b = roomB._rect;

        float width = _mapData._corridorSize;

        Vector2 direction = (b.center - a.center).normalized;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // Récupère les points d'intersection et les vrais côtés des murs.
        Vector2 start = GetRoomBoundaryPoint(a, direction, out WallSide sideA);
        Vector2 end = GetRoomBoundaryPoint(b, -direction, out WallSide sideB);

        if (Vector2.Distance(start, end) < 0.01f)
            return;

        // Enregistre les ouvertures exactement sur les points d'entrée.
        _roomOpenings.Add(new RoomOpening(roomA, sideA, sideA == WallSide.Left || sideA == WallSide.Right ? start.y : start.x, width));

        _roomOpenings.Add(new RoomOpening(roomB, sideB, sideB == WallSide.Left || sideB == WallSide.Right ? end.y : end.x, width));

        SpawnOpening(start, direction, sideA);
        SpawnOpening(end, direction, sideB);

        // Prolonge le sol dans les salles sans déplacer les ouvertures.
        float overlap = width * 0.3f;
        start -= direction * overlap;
        end += direction * overlap;

        _corridors.Add(new CorridorSegment(start, end));

        RegisterConnection(roomA, roomB);
    }

    // Finds the point where a direction intersects a room boundary and identifies the wall side.
    private Vector2 GetRoomBoundaryPoint(Rect room, Vector2 direction, out WallSide side)
    {
        Vector2 center = room.center;

        float halfWidth = room.width * 0.5f;
        float halfHeight = room.height * 0.5f;

        float tx = Mathf.Abs(direction.x) > 0.0001f ? halfWidth / Mathf.Abs(direction.x) : float.MaxValue;

        float ty = Mathf.Abs(direction.y) > 0.0001f ? halfHeight / Mathf.Abs(direction.y) : float.MaxValue;

        // Le premier bord rencontré détermine le côté.
        if (tx <= ty)
        {
            side = direction.x >= 0f ? WallSide.Right : WallSide.Left;
        }
        else
        {
            side = direction.y >= 0f ? WallSide.Top : WallSide.Bottom;
        }

        float distance = Mathf.Min(tx, ty);
        Vector2 point = center + direction * distance;

        return point;
    }

    // Splits long corridors into smaller segments to limit their length.
    private void SubDivideCorridors()
    {
        float maxLength = _mapData._maxCorridorLenght;

        if (maxLength <= 0f)
            return;

        List<CorridorSegment> subdivided = new List<CorridorSegment>();

        foreach (CorridorSegment corridor in _corridors)
        {
            Vector2 direction = corridor.end - corridor.start;
            float length = direction.magnitude;

            if (length <= 0.001f)
                continue;

            int segmentCount = Mathf.CeilToInt(length / maxLength);

            for (int i = 0; i < segmentCount; i++)
            {
                float tStart = (float)i / segmentCount;
                float tEnd = (float)(i + 1) / segmentCount;

                Vector2 start = Vector2.Lerp(corridor.start, corridor.end, tStart);

                Vector2 end = Vector2.Lerp(corridor.start, corridor.end, tEnd);

                subdivided.Add(new CorridorSegment(start, end));
            }
        }

        _corridors = subdivided;
    }

    // Instantiates and positions a visual object at a corridor entrance.
    private void SpawnOpening(Vector2 position, Vector2 direction, WallSide side)
    {
        if (_mapData._openingPrefab == null)
            return;

        GameObject opening = Instantiate(_mapData._openingPrefab, _transformParent);

        Vector2 openingPos = position;

        // Rotation selon le côté du mur
        switch (side)
        {
            case WallSide.Left:
                openingPos.x -= 1.5f;
                break;
            case WallSide.Right:
                openingPos.x += 1.5f;
                break;
            case WallSide.Top:
                openingPos.y += 1.5f;
                break;
            case WallSide.Bottom:
                openingPos.y -= 1.5f;
                break;
        }

        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        opening.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        opening.transform.position = new Vector3(openingPos.x, 0.5f, openingPos.y);
    }

    // Creates rooms inside BSP leaves and assigns their room styles.
    private void GenerateRooms()
    {
        List<Node> leaves = new List<Node>();
        GetLeafNodes(_root, leaves);

        foreach (Node leaf in leaves)
        {
            if (_nbRooms >= _mapData._maxRooms)
                break;

            int nb = Random.Range(0, _listRoomsTypes.Count);

            RoomStyleSO roomStyle = _listRoomsTypes[nb];

            if(leaf == _spawnNode)
                roomStyle = _spawnRoomStyle;
            else if (leaf == _endNode)
                roomStyle = _endRoomStyle;

            Room room = new Room();
            room._roomStyle = roomStyle;

            int padding = _mapData._padding;

            float availableWidth = leaf._rect.width - padding;
            float availableHeight = leaf._rect.height - padding;

            if (availableWidth <= 0 || availableHeight <= 0)
                continue;

            float minWidth = Mathf.Min(roomStyle._minHeight, availableWidth);
            float maxWidth = Mathf.Min(roomStyle._maxHeight, availableWidth);

            float minHeight = Mathf.Min(roomStyle._minWidth, availableHeight);
            float maxHeight = Mathf.Min(roomStyle._maxWidth, availableHeight);

            float width = Random.Range(minWidth, maxWidth);
            float height = Random.Range(minHeight, maxHeight);

            room._rect = new Rect(leaf._rect.center.x - width / 2f, leaf._rect.center.y - height / 2f, width, height);

            _listCurrentRooms.Add(room);
            _roomByNode[leaf] = room;
            _nbRooms++;
        }
    }

    // Registers a bidirectional connection between two rooms.
    private void RegisterConnection(Room roomA, Room roomB)
    {
        if (!_roomConnections.ContainsKey(roomA))
            _roomConnections[roomA] = new List<Room>();

        if (!_roomConnections.ContainsKey(roomB))
            _roomConnections[roomB] = new List<Room>();

        if (!_roomConnections[roomA].Contains(roomB))
            _roomConnections[roomA].Add(roomB);

        if (!_roomConnections[roomB].Contains(roomA))
            _roomConnections[roomB].Add(roomA);
    }

    // Selects the spawn node as the leaf farthest from the dungeon center.
    private void ChooseSpawnNodes()
    {
        List<Node> leaves = new List<Node>();
        GetLeafNodes(_root, leaves);

        _spawnNode = null;
        _endNode = null;

        if (leaves.Count == 0)
            return;

        Vector2 center = _root._rect.center;
        float greatestDistance = -1f;

        foreach (Node node in leaves)
        {
            float distance = Vector2.Distance(node._rect.center, center);

            if (distance > greatestDistance)
            {
                greatestDistance = distance;
                _spawnNode = node;
            }
        }
    }

    // Finds the room farthest from the spawn in terms of corridor connections and assigns the end room style.
    private void ChooseEndRoomByCorridorCount()
    {
        if (_spawnNode == null || !_roomByNode.TryGetValue(_spawnNode, out Room spawnRoom))
            return;

        Dictionary<Room, int> distances = new Dictionary<Room, int>();
        Queue<Room> queue = new Queue<Room>();

        distances[spawnRoom] = 0;
        queue.Enqueue(spawnRoom);

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();

            if (!_roomConnections.ContainsKey(current))
                continue;

            foreach (Room neighbor in _roomConnections[current])
            {
                if (distances.ContainsKey(neighbor))
                    continue;

                distances[neighbor] = distances[current] + 1;
                queue.Enqueue(neighbor);
            }
        }

        Room endRoom = null;
        int greatestDistance = -1;

        foreach (Room room in _listCurrentRooms)
        {
            if (room == spawnRoom || !distances.ContainsKey(room))
                continue;

            if (distances[room] > greatestDistance)
            {
                greatestDistance = distances[room];
                endRoom = room;
            }
        }

        if (endRoom == null)
            return;

        endRoom._roomStyle = _endRoomStyle;

        foreach (KeyValuePair<Node, Room> pair in _roomByNode)
        {
            if (pair.Value == endRoom)
            {
                _endNode = pair.Key;
                break;
            }
        }
    }

    // Splits the root node into smaller regions and selects the spawn node.
    private void SplitNode()
    {
        SplitNodeRecursive(_root, 0);

        ChooseSpawnNodes();
    }

    // Recursively splits a node into two smaller regions until the maximum depth or minimum size is reached.
    private void SplitNodeRecursive(Node node, int depth)
    {
        if (depth >= _mapData._maxDepth)
            return;

        float width = node._rect.width;
        float height = node._rect.height;

        Vector2 minNodeSize = _mapData._minNodeSize;

        bool canSplitX = width >= minNodeSize.x * 2f;
        bool canSplitY = height >= minNodeSize.y * 2f;

        if (!canSplitX && !canSplitY)
            return;

        bool splitVertically;

        if (!canSplitX)
            splitVertically = false;
        else if (!canSplitY)
            splitVertically = true;
        else
            splitVertically = Random.value < 0.5f;

        Rect rectA;
        Rect rectB;

        bool stop = Random.Range(0, 100) <= 5;

        if (stop && depth >= 3)
            return;

        if (splitVertically)
        {
            float splitX = Random.Range(node._rect.xMin + minNodeSize.x, node._rect.xMax - minNodeSize.x);

            rectA = Rect.MinMaxRect(node._rect.xMin, node._rect.yMin, splitX, node._rect.yMax);

            rectB = Rect.MinMaxRect(splitX, node._rect.yMin, node._rect.xMax, node._rect.yMax);
        }
        else
        {
            float splitY = Random.Range(node._rect.yMin + minNodeSize.y, node._rect.yMax - minNodeSize.y);

            rectA = Rect.MinMaxRect(node._rect.xMin, node._rect.yMin, node._rect.xMax, splitY);

            rectB = Rect.MinMaxRect(node._rect.xMin, splitY, node._rect.xMax, node._rect.yMax);
        }

        Node childA = new Node(rectA, node, _root);
        Node childB = new Node(rectB, node, _root);

        node._children.Add(childA);
        node._children.Add(childB);

        SplitNodeRecursive(childA, depth + 1);
        SplitNodeRecursive(childB, depth + 1);
    }

    // Creates the root node that defines the dungeon's initial boundaries.
    private void InitRoot()
    {
        int padding = _mapData._padding;

        float width = Random.Range(_mapData._minLength, _mapData._maxLength) + padding;

        float height = Random.Range(_mapData._minWidth, _mapData._maxWidth) + padding;

        Vector2 center = new Vector2(transform.position.x, transform.position.z);

        Rect rect = new Rect(center.x - width / 2f, center.y - height / 2f, width, height);

        _root = new Node(rect);
    }

    // Collects all leaf nodes from the BSP tree.
    private void GetLeafNodes(Node node, List<Node> leaves)
    {
        if (node.IsLeaf())
        {
            leaves.Add(node);
            return;
        }

        foreach (Node child in node._children)
        {
            GetLeafNodes(child, leaves);
        }
    }

    // Draws the floor tiles inside every generated room.
    void DrawFloor()
    {
        foreach (var validRoom in _listCurrentRooms)
        {
            validRoom._rectList = RectHelp.Split(validRoom._rect, new Vector2(_mapData._maxTileSize, _mapData._maxTileSize));

            foreach(Rect rect in validRoom._rectList)
            {
                GameObject tile = Instantiate(validRoom._roomStyle._roomFloorPrefab, _transformParent);

                tile.transform.position = new Vector3(rect.center.x, 0f, rect.center.y);

                tile.transform.localScale = new Vector3(rect.width, 1f, rect.height);

                _listTiles.Add(tile);
            }
        }
    }

    // Spawns the door at the entrance of the end room.
    private void SpawnEndRoomDoor()
    {
        if (_endNode == null || !_roomByNode.TryGetValue(_endNode, out Room endRoom))
            return;

        if (!_roomConnections.TryGetValue(endRoom, out List<Room> neighbors) || neighbors.Count == 0)
            return;

        Room connectedRoom = neighbors[0];

        Vector2 direction = (endRoom._rect.center - connectedRoom._rect.center).normalized;
        Vector2 doorPosition = GetRoomBoundaryPoint(endRoom._rect, -direction, out WallSide side);
        Vector3 position = new Vector3(doorPosition.x, 0.5f, doorPosition.y);

        float angle = side 
            switch
            {
                WallSide.Top => 90f,
                WallSide.Bottom => 90f,
                WallSide.Left => 0f,
                WallSide.Right => 0f,
                _ => 0f
            };

        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

        _endRoomDoor = Instantiate(_endRoomDoorPrefab, position, rotation, _transformParent);
    }

    [Button("ClearRooms")]
    // Destroys generated objects and clears the data used by the previous generation.
    void ClearRooms()
    {
        List<GameObject> childTransforms = new List<GameObject>();

        foreach (Transform t in _transformParent)
        {
            childTransforms.Add(t.gameObject);
        }

        foreach (var child in childTransforms)
        {
            DestroyImmediate(child.gameObject);
        }

        _enemySpawner.ClearEnemies();
        _propsSpawner.ClearProps();

        childTransforms.Clear();
        _listTiles.Clear();
        _listCurrentRooms.Clear();
        _roomByNode.Clear();
        _corridors.Clear();
        _originalCorridors.Clear();
        _wallRects.Clear();
        _roomOpenings.Clear();

        _nbRooms = 0;
    }

    // Initializes the random number generator using the configured seed.
    void InitRandomSeed()
    {
        if (_mapData._randomSeed == true)
        {
            _mapData._seed = Random.Range(int.MinValue, int.MaxValue);
        }

        Random.InitState(_mapData._seed);
    }
}