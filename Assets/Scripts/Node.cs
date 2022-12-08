using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int gCost;
    public int hCost;
    public Vector2Int position;
    public Vector2 worldPosition;
    public Vector2Int cell;
    public bool walkable = true;
    public List<Node> neighbors;
    public Node parent;

    public Node(Vector2Int position, bool walkable, Vector2 worldPosition, Vector2Int cell)
    {
        this.walkable = walkable;
        this.position = position;
        this.worldPosition = worldPosition;
        this.cell = cell;
    }

    public int fCost => gCost + hCost;
}