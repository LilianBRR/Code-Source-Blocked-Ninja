
using System.Collections.Generic;
using UnityEngine;

public static class RectHelp
{
    public static List<Rect> Split(this Rect source, Vector2 maxSize)
    {
        List<Rect> result = new List<Rect>();

        if (maxSize.x <= 0f || maxSize.y <= 0f)
        {
            result.Add(source);
            return result;
        }

        bool splitX = source.width > maxSize.x;
        bool splitY = source.height > maxSize.y;

        if (!splitX && !splitY)
        {
            result.Add(source);
            return result;
        }

        if (splitX)
        {
            int count = Mathf.CeilToInt(source.width / maxSize.x);
            float segmentWidth = source.width / count;

            for (int i = 0; i < count; i++)
            {
                Rect segment = source;
                segment.x = source.x + i * segmentWidth;
                segment.width = segmentWidth;

                result.AddRange(Split(segment, maxSize));
            }
        }
        else
        {
            int count = Mathf.CeilToInt(source.height / maxSize.y);
            float segmentHeight = source.height / count;

            for (int i = 0; i < count; i++)
            {
                Rect segment = source;
                segment.y = source.y + i * segmentHeight;
                segment.height = segmentHeight;

                result.AddRange(Split(segment, maxSize));
            }
        }

        return result;
    }
}
