using System;
using UnityEngine;

public class PathfinderManager : MonoBehaviour
{
    [SerializeField] private float playerSpeed;

    [SerializeField] private bool drawGrid;
    [SerializeField] private bool drawPlayerPath;

    public static PathfinderManager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetPlayerSpeed()
    {
        return playerSpeed;
    }

    public bool DrawGrid()
    {
        return drawGrid;
    }
    
    public bool DrawPlayerPath()
    {
        return drawPlayerPath;
    }

    public delegate void GenerateGrid();

    public static event GenerateGrid onGenerateGrid;

    public static void InvokeGenerateGrid()
    {
        onGenerateGrid?.Invoke();
    }
}