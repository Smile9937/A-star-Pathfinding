using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid : MonoBehaviour
{
    [SerializeField] private PathfinderManager pathfinderManager;
    [SerializeField] private Grid gridBase;
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    [SerializeField] private Vector2Int scanStart = new Vector2Int(-20, 20), scanFinish = new Vector2Int(-20, 20);

    private List<Node> unsortedNodes = new List<Node>();
    
    private Node[,] nodes;
    private Vector2Int gridBound = new Vector2Int(0, 0);
    private void OnEnable()
    {
        PathfinderManager.onGenerateGrid += CreateGrid;
    }
    private void OnDisable()
    {
        PathfinderManager.onGenerateGrid -= CreateGrid;
    }

    private void Start()
    {
        pathfinderManager = PathfinderManager.instance;
    }

    public List<Node> GetNeighbors(int x, int y, int width, int height)
    {
        List<Node> myNeighbors = new List<Node>();

        if (x > 0 && x < width - 1)
        {
            if (y > 0 && y < height - 1)
            {
                #region Cardial
                if (nodes[x + 1, y] != null)
                {
                    Node worldTile1 = nodes[x + 1, y];
                    if (worldTile1 != null) myNeighbors.Add(worldTile1);
                }

                if (nodes[x - 1, y] != null)
                {
                    Node worldTile2 = nodes[x - 1, y];
                    if (worldTile2 != null) myNeighbors.Add(worldTile2);
                }

                if (nodes[x, y + 1] != null)
                {
                    Node worldTile3 = nodes[x, y + 1];
                    if (worldTile3 != null) myNeighbors.Add(worldTile3);
                }

                if (nodes[x, y - 1] != null)
                {
                    Node worldTile4 = nodes[x, y - 1];
                    if (worldTile4 != null) myNeighbors.Add(worldTile4);
                }
                #endregion
            }
            else if (y == 0)
            {
                #region Cardinal
                if (nodes[x + 1, y] != null)
                {
                    Node worldTile1 = nodes[x + 1, y];
                    if (worldTile1 != null) myNeighbors.Add(worldTile1);
                }

                if (nodes[x - 1, y] != null)
                {
                    Node worldTile2 = nodes[x - 1, y];
                    if (worldTile2 != null) myNeighbors.Add(worldTile2);
                }

                if (nodes[x, y + 1] == null)
                {
                    Node worldTile3 = nodes[x, y + 1];
                    if (worldTile3 != null) myNeighbors.Add(worldTile3);
                }
                #endregion
            }
            else if (y == height - 1)
            {
                #region Cardinal
                if (nodes[x, y - 1] != null)
                {
                    Node worldTile4 = nodes[x, y - 1];
                    if (worldTile4 != null) myNeighbors.Add(worldTile4);
                }
                if (nodes[x + 1, y] != null)
                {
                    Node worldTile1 = nodes[x + 1, y];
                    if (worldTile1 != null) myNeighbors.Add(worldTile1);
                }

                if (nodes[x - 1, y] != null)
                {
                    Node worldTile2 = nodes[x - 1, y];
                    if (worldTile2 != null) myNeighbors.Add(worldTile2);
                }
                #endregion
            }
        }
        else if (x == 0)
        {
            if (y > 0 && y < height - 1)
            {
                #region Cardinal
                if (nodes[x + 1, y] != null)
                {
                    Node worldTile1 = nodes[x + 1, y];
                    if (worldTile1 != null) myNeighbors.Add(worldTile1);
                }

                if (nodes[x, y - 1] != null)
                {
                    Node worldTile4 = nodes[x, y - 1];
                    if (worldTile4 != null) myNeighbors.Add(worldTile4);
                }

                if (nodes[x, y + 1] != null)
                {
                    Node worldTile3 = nodes[x, y + 1];
                    if (worldTile3 != null) myNeighbors.Add(worldTile3);
                }
                #endregion
            }
            else if (y == 0)
            {
                #region Cardnial
                if (nodes[x + 1, y] != null)
                {
                    Node worldTile1 = nodes[x + 1, y];
                    if (worldTile1 != null) myNeighbors.Add(worldTile1);
                }

                if (nodes[x, y + 1] != null)
                {
                    Node worldTile3 = nodes[x, y + 1];
                    if (worldTile3 != null) myNeighbors.Add(worldTile3);
                }
                #endregion
            }
            else if (y == height - 1)
            {
                #region Cardinal
                if (nodes[x + 1, y] != null)
                {
                    Node worldTile1 = nodes[x + 1, y];
                    if (worldTile1 != null) myNeighbors.Add(worldTile1);
                }

                if (nodes[x, y - 1] != null)
                {
                    Node worldTile4 = nodes[x, y - 1];
                    if (worldTile4 != null) myNeighbors.Add(worldTile4);
                }
                #endregion
            }
        }
        else if (x == width - 1)
        {
            if (y > 0 && y < height - 1)
            {
                #region Cardinal
                if (nodes[x - 1, y] != null)
                {
                    Node worldTile4 = nodes[x - 1, y];
                    if (worldTile4 != null) myNeighbors.Add(worldTile4);
                }

                if (nodes[x, y + 1] != null)
                {
                    Node worldTile3 = nodes[x, y + 1];
                    if (worldTile3 != null) myNeighbors.Add(worldTile3);
                }

                if (nodes[x, y - 1] != null)
                {
                    Node worldTile4 = nodes[x, y - 1];
                    if (worldTile4 != null) myNeighbors.Add(worldTile4);
                }
                #endregion
            }
            else if (y == 0)
            {
                #region Cardinal
                if (nodes[x - 1, y] != null)
                {
                    Node worldTile2 = nodes[x - 1, y];
                    if (worldTile2 != null) myNeighbors.Add(worldTile2);
                }
                if (nodes[x, y + 1] != null)
                {
                    Node worldTile3 = nodes[x, y + 1];
                    if (worldTile3 != null) myNeighbors.Add(worldTile3);
                }
                #endregion
            }
            else if (y == height - 1)
            {
                #region Cardinal
                if (nodes[x - 1, y] != null)
                {
                    Node worldTile2 = nodes[x - 1, y];
                    if (worldTile2 != null) myNeighbors.Add(worldTile2);
                }

                if (nodes[x, y - 1] != null)
                {
                    Node worldTile4 = nodes[x, y - 1];
                    if (worldTile4 != null) myNeighbors.Add(worldTile4);
                }
                #endregion
            }
        }

        return myNeighbors;
    }

    [ContextMenu("Create Grid")]
    public void CreateGrid()
    {
        unsortedNodes.Clear();

        Vector2Int grid = new Vector2Int(0, 0);

        for (int x = scanStart.x; x < scanFinish.x; x++)
        {
            for (int y = scanStart.y; y < scanFinish.y; y++)
            {
                TileBase tileBase = floorTilemap.GetTile(new Vector3Int(x, y, 0));
                if (tileBase != null)
                {
                    bool walkableTile = true;
                    TileBase obstacleTileBase = obstacleTilemap.GetTile(new Vector3Int(x, y, 0));
                    if (obstacleTileBase != null) walkableTile = false;

                    Vector3 worldPosition = new Vector3(x + 0.5f + gridBase.transform.position.x, y + 0.5f + gridBase.transform.position.y, 0);
                    Vector3Int cellPosition = floorTilemap.WorldToCell(worldPosition);

                    Node node = new Node(new Vector2Int(grid.x, grid.y), walkableTile, worldPosition, new Vector2Int(cellPosition.x, cellPosition.y));

                    unsortedNodes.Add(node);

                    grid.y++;
                    if (grid.y > gridBound.y)
                        gridBound.y = grid.y;
                }
            }

            grid.y = 0;

            grid.x++;
            if (grid.x > gridBound.x)
                gridBound.x = grid.x;
        }

        nodes = new Node[gridBound.x + 1, gridBound.y + 1];

        foreach (Node node in unsortedNodes)
        {
            nodes[node.position.x, node.position.y] = node;
        }

        for (int x = 0; x < gridBound.x; x++)
        {
            for (int y = 0; y < gridBound.y; y++)
            {
                if (nodes[x, y] != null)
                {
                    Node node = nodes[x, y];
                    node.neighbors = GetNeighbors(x, y, gridBound.x, gridBound.y);
                }
            }
        }
    }

    public Node GetNodeByCellPosition(Vector2 worldPosition)
    {
        Vector3Int cellPosition = floorTilemap.WorldToCell(worldPosition);
        Node node = null;
        for (int x = 0; x < gridBound.x; x++)
        {
            for (int y = 0; y < gridBound.y; y++)
            {
                if (nodes[x, y] != null)
                {
                    Node _node = nodes[x, y];

                    if (_node.walkable && _node.cell.x == cellPosition.x && _node.cell.y == cellPosition.y)
                    {
                        node = _node;
                        break;
                    }
                }
            }
        }
        return node;
    }

    private void OnDrawGizmos()
    {
        if (pathfinderManager == null)
            pathfinderManager = PathfinderManager.instance;

        if (!pathfinderManager.DrawGrid || nodes == null) return;
        foreach(Node node in nodes)
        {
            if(node == null || !node.walkable) continue;
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(new Vector3(node.worldPosition.x, node.worldPosition.y, 0), Vector3.one * 0.5f);
        }
    }
}
