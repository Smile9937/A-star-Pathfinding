using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private LineRenderer path;
    [SerializeField] private LineRenderer previousPath;
    private PathfinderManager pathfinderManager;
    private Vector3 lastDirection;
    private bool moveDone = false;
    private List<Vector2> movePoints = new List<Vector2>();
    private List<Vector2> reachedPoints = new List<Vector2>();
    private Grid grid;
    private Vector3 movePoint;
    private bool resetMovement;
    private Vector3 absoluteMousePosition;
    private Vector3 targetPosition;
    private bool reachedDestination;
    private Vector3 GetWorldGridPosition(Vector3 worldPosition) => new Vector3(Mathf.Floor(worldPosition.x) + 0.5f, Mathf.Floor(worldPosition.y) + 0.5f);
    private void Start()
    {
        pathfinderManager = PathfinderManager.instance;
        movePoint = targetPosition = transform.position;
        grid = FindObjectOfType<Grid>();
    }
    void Update()
    {
        if (pathfinderManager.InEditMode) return;
        MouseInput();
        MovementPerformed();
    }

    private void MouseInput()
    {
        if(Input.GetMouseButtonDown(0) && !resetMovement)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            absoluteMousePosition = GetWorldGridPosition(mousePos);

            if (grid.GetNodeByWorldPosition(GetWorldGridPosition(transform.position)) == null || grid.GetNodeByWorldPosition(absoluteMousePosition) == null)
                return;

            targetPosition = absoluteMousePosition;
            resetMovement = true;
            movePoints.Clear();
            reachedPoints.Clear();
            moveDone = false;
            reachedDestination = false;
            lastDirection = Vector3.zero;

            if (path.positionCount != 0 || !pathfinderManager.DrawPreviousPath)
            {
                previousPath.positionCount = 0;
            }

            if(path.positionCount != 0)
            {
                DrawPreviousPath();
            }
        }
    }

    public void SetPosition(Vector3 position)
    {
        RemovePath();
        resetMovement = true;
        movePoints.Clear();
        reachedPoints.Clear();
        moveDone = false;
        lastDirection = Vector3.zero;
        targetPosition = position;
        reachedDestination = false;
        transform.position = position;
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
                DrawPath();
            }
        }
        else
        {
            SetMovementVector();
        }

        transform.position = Vector3.MoveTowards(transform.position, movePoint, Time.deltaTime * pathfinderManager.PlayerSpeed);

        if (movePoints.Count <= 0) return;
        if (transform.position == (Vector3)movePoints[movePoints.Count - 1] && !reachedDestination)
        {
            reachedDestination = true;
            DrawPreviousPath();
            RemovePath();
        }
    }

    private void DrawPreviousPath()
    {
        if (!pathfinderManager.DrawPreviousPath) return;
        previousPath.positionCount = path.positionCount;

        Vector3[] newPos = new Vector3[path.positionCount];

        path.GetPositions(newPos);

        previousPath.SetPositions(newPos);
    }

    private void RemovePath() => path.positionCount = 0;

    private void FindPath(Vector3 startPosition, Vector3 endPosition)
    {
        Node startNode = grid.GetNodeByWorldPosition(startPosition);
        Node targetNode = grid.GetNodeByWorldPosition(endPosition);

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
                movePoints = SimplifyPath(RetracePath(startNode, targetNode));
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

    private List<Vector2> SimplifyPath(List<Vector2> points)
    {
        if(points == null || points.Count < 3)
            return points;

        List<Vector2> simplifiedPoints = new List<Vector2>{ points[0] };

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 pointA = points[i - 1];
            Vector2 pointB = points[i];
            Vector2 pointC = points[i + 1];

            float angle = Vector2.Angle(pointB - pointA, pointC - pointB);

            if(angle > 0.1f)
            {
                simplifiedPoints.Add(points[i]);
            }
        }

        simplifiedPoints.Add(points[points.Count - 1]);

        return simplifiedPoints;
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

    public List<Vector2> RetracePath(Node startNode, Node targetNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node currentNode = targetNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    void SetMovementVector()
    {
        if (movePoints != null)
        {
            if (movePoints.Count > 0)
            {
                if (!moveDone)
                {
                    for (int i = 0; i < movePoints.Count; i++)
                    {
                        if (reachedPoints.Contains(movePoints[i])) continue;
                        else reachedPoints.Add(movePoints[i]); break;
                    }
                    Vector2 position = reachedPoints[reachedPoints.Count - 1];
                    lastDirection = new Vector3(Mathf.Ceil(position.x - transform.position.x), Mathf.Ceil(position.y - transform.position.y), 0);
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

    private void DrawPath()
    {
        if (movePoints == null || movePoints.Count <= 0 || !pathfinderManager.DrawPlayerPath) return;
        if (transform.position == (Vector3)movePoints[movePoints.Count - 1]) return;

        path.positionCount = movePoints.Count + 1;
        path.SetPosition(0, new Vector3(transform.position.x, transform.position.y, 0));

        for (int i = 1; i < movePoints.Count + 1; i++)
        {
            Vector2 position = movePoints[i-1];

            path.SetPosition(i, new Vector3(position.x, position.y, 0));
        }
    }
}