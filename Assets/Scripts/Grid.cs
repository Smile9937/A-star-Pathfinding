using Packages.Rider.Editor.UnitTesting;
using System.Collections.Generic;
using Unity.Mathematics;
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

    public Tilemap GetObstacleTilemap() => obstacleTilemap;
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

        void AddNeighbor(int neighborX, int neighborY)
        {
            if (nodes[neighborX, neighborY] != null && nodes[neighborX, neighborY].walkable)
            {
                Node worldTile = nodes[neighborX, neighborY];
                if (worldTile != null) myNeighbors.Add(worldTile);
            }
        }
        
        void AddDiagonalNeighbor(int neighborX, int neighborY)
        {
            if (nodes[neighborX, neighborY] == null || !nodes[neighborX, neighborY].walkable) return;

            if(neighborX == x + 1 && neighborY == y + 1)
            {
                PositionCheck(x + 1, y, x, y + 1);
                return;
            }

            if(neighborX == x - 1 && neighborY == y + 1)
            {
                PositionCheck(x - 1, y, x, y + 1);
                return;
            }

            if (neighborX == x - 1 && neighborY == y - 1)
            {
                PositionCheck(x - 1, y, x, y - 1);
                return;
            }

            if(neighborX == x + 1 && neighborY == y - 1)
            {
                PositionCheck(x + 1, y, x, y - 1);
                return;
            }


            void PositionCheck(int x1, int y1, int x2, int y2)
            {
                Node worldTile = nodes[neighborX, neighborY];

                if (nodes[x1, y1] != null && nodes[x1, y1].walkable)
                {
                    myNeighbors.Add(worldTile);
                    return;
                }

                if (nodes[x2, y2] != null && nodes[x2, y2].walkable)
                {
                    myNeighbors.Add(worldTile);
                    return;
                }
            }
        }

        if (x > 0 && x < width - 1)
        {
            if (y > 0 && y < height - 1)
            {
                #region Cardial
                AddNeighbor(x + 1, y);
                AddNeighbor(x - 1, y);
                AddNeighbor(x, y + 1);
                AddNeighbor(x, y - 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x + 1, y + 1);
                AddDiagonalNeighbor(x + 1, y - 1);
                AddDiagonalNeighbor(x - 1, y + 1);
                AddDiagonalNeighbor(x - 1, y - 1);
                #endregion

            }
            else if (y == 0)
            {
                #region Cardinal
                AddNeighbor(x + 1, y);
                AddNeighbor(x - 1, y);
                AddNeighbor(x, y + 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x + 1, y + 1);
                AddDiagonalNeighbor(x - 1, y + 1);
                #endregion
            }
            else if (y == height - 1)
            {
                #region Cardinal
                AddNeighbor(x, y - 1);
                AddNeighbor(x + 1, y);
                AddNeighbor(x - 1, y);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x + 1, y - 1);
                AddDiagonalNeighbor(x - 1, y - 1);
                #endregion
            }
        }
        else if (x == 0)
        {
            if (y > 0 && y < height - 1)
            {
                #region Cardinal
                AddNeighbor(x + 1, y);
                AddNeighbor(x, y - 1);
                AddNeighbor(x, y + 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x + 1, y + 1);
                AddDiagonalNeighbor(x + 1, y - 1);
                #endregion
            }
            else if (y == 0)
            {
                #region Cardnial
                AddNeighbor(x + 1, y);
                AddNeighbor(x, y + 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x + 1, y + 1);
                #endregion
            }
            else if (y == height - 1)
            {
                #region Cardinal
                AddNeighbor(x + 1, y);
                AddNeighbor(x, y - 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x + 1, y - 1);
                #endregion
            }
        }
        else if (x == width - 1)
        {
            if (y > 0 && y < height - 1)
            {
                #region Cardinal
                AddNeighbor(x - 1, y);
                AddNeighbor(x, y + 1);
                AddNeighbor(x, y - 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x - 1, y + 1);
                AddDiagonalNeighbor(x - 1, y - 1);
                #endregion
            }
            else if (y == 0)
            {
                #region Cardinal
                AddNeighbor(x - 1, y);
                AddNeighbor(x, y + 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x - 1, y + 1);
                #endregion
            }
            else if (y == height - 1)
            {
                #region Cardinal
                AddNeighbor(x - 1, y);
                AddNeighbor(x, y - 1);
                #endregion

                #region Diagonal
                AddDiagonalNeighbor(x - 1, y - 1);
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

    public Node GetNodeByWorldPosition(Vector2 worldPosition)
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
