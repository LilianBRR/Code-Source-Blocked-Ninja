using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Rect _rect;
    public Node _root;
    public Node _parentNode;
    public HashSet<Node> _children = new HashSet<Node>();

    public Node(Rect rect, Node parent = null, Node root = null)
    {
        _rect = rect;
        _parentNode = parent;
        _root = root ?? this;
    }

    public bool IsLeaf()
    {
        return _children.Count == 0;
    }
}