using System.Collections.Generic;
using UnityEngine;

public class CorridorSegment
{
    public Vector2 start;
    public Vector2 end;

    public CorridorSegment(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}