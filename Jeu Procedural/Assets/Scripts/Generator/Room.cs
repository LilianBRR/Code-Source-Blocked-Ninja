using System.Collections.Generic;
using UnityEngine;
using static Room;

public class Room
{
    public Rect _rect;
    public List<Rect> _rectList;
    public RoomStyleSO _roomStyle;
}

public enum WallSide
{
    Left,
    Right,
    Top,
    Bottom
}

public class RoomOpening
{
    public Room room;
    public WallSide side;
    public float center;
    public float width;

    public RoomOpening(Room room, WallSide side, float center, float width)
    {
        this.room = room;
        this.side = side;
        this.center = center;
        this.width = width;
    }
}