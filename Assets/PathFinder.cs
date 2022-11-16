using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private PathfinderManager pathfinderManager;
    private Vector3 lastDirection;
    private bool moveDone = false;
    private List<Node> nodePath = new List<Node>();
    private List<Node> reachedPathTiles = new List<Node>();
    private Grid grid;
    private Vector3 movePoint;
    private bool resetMovement;
    private Vector3 absoluteMousePosition;
    private Vector3 targetPosition;
    private Vector3 GetWorldGridPosition(Vector3 worldPosition) => new Vector3(Mathf.Floor(worldPosition.x) + 0.5f, Mathf.Floor(worldPosition.y) + 0.5f);
    private void Start()
    {
        pathfinderManager = PathfinderManager.instance;
        movePoint = targetPosition = transform.position;
        grid = FindObjectOfType<Grid>();
    }
    void Update()
    {
        MouseInput();
        MovementPerformed();
    }

    private void MouseInput()
    {
        if(Input.GetMouseButtonDown(0) && !resetMovement)
        {

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            absoluteMousePosition = GetWorldGridPosition(mousePos);

            if (grid.GetNodeByCellPosition(GetWorldGridPosition(transform.position)) == null || grid.GetNodeByCellPosition(absoluteMousePosition) == null)
                return;

            targetPosition = absoluteMousePosition;
            resetMovement = true;
            nodePath.Clear();
            reachedPathTiles.Clear();
            moveDone = false;
            lastDirection = Vector3.zero;
        }
    }

    void MovementPerformed()
    {
        if (resetMovement)
        {
            movePoint = GetWorldGridPosition(transform.position);

            if (Vector3.Distance(transform.position, movePoint) <= 0.001f)
            {
                resetMovement = false;
                FindPath(GetWorldGridPosition(transform.position), targetPosition);
            }
        }
        else
        {
            SetMovementVector();
        }

        transform.position = Vector3.MoveTowards(transform.position, movePoint, Time.deltaTime * pathfinderManager.GetPlayerSpeed());
    }

    private void FindPath(Vector3 startPosition, Vector3 endPosition)
    {
        Node startNode = grid.GetNodeByCellPosition(startPosition);
        Node targetNode = grid.GetNodeByCellPosition(endPosition);

        if (targetNode == null || startNode == null)
            return;

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        resetMovement = false;

        while (openSet.Count > 0)
        {
            if (resetMovement)
                return;

            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                nodePath = RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in currentNode.neighbors)
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);

                    }
                }
            }
        }
    }
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int distanceY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        int diagonalCost = 14;
        int cardinalCost = 10;

        if (distanceX > distanceY)
            return diagonalCost * distanceY + cardinalCost * (distanceX - distanceY);

        return diagonalCost * distanceX + cardinalCost * (distanceY - distanceX);
    }

    private List<Node> RetracePath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    void SetMovementVector()
    {
        if (nodePath != null)
        {
            if (nodePath.Count > 0)
            {
                if (!moveDone)
                {
                    for (int i = 0; i < nodePath.Count; i++)
                    {
                        if (reachedPathTiles.Contains(nodePath[i])) continue;
                        else reachedPathTiles.Add(nodePath[i]); break;
                    }
                    Node node = reachedPathTiles[reachedPathTiles.Count - 1];
                    lastDirection = new Vector3(Mathf.Ceil(node.worldPosition.x - transform.position.x), Mathf.Ceil(node.worldPosition.y - transform.position.y), 0);
                    movePoint = GetWorldGridPosition(transform.position) + lastDirection;
                    moveDone = true;
                }
                else
                {
                    lastDirection = Vector2.zero;
                    if (Vector3.Distance(transform.position, movePoint) <= 0.001f)
                        moveDone = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (pathfinderManager == null)
            pathfinderManager = PathfinderManager.instance;

        if (nodePath == null || nodePath.Count <= 0 || !pathfinderManager.DrawPlayerPath()) return;
        if (transform.position == (Vector3)nodePath[nodePath.Count - 1].worldPosition) return;

        foreach (Node node in nodePath)
        {
            if (node == null || !node.walkable) continue;
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(node.worldPosition.x, node.worldPosition.y, 0), Vector3.one * 0.5f);
        }
    }
}