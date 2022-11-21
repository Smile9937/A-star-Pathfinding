using System;
using UnityEngine;

public class PathfinderManager : MonoBehaviour
{
    [SerializeField] private float playerSpeed;

    [SerializeField] private bool drawGrid;
    [SerializeField] private bool drawPlayerPath;

    private bool inEditMode;

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

    public void ToggleEditMode(bool edit) => inEditMode = edit;

    public bool InEditMode => inEditMode;

    public float PlayerSpeed => playerSpeed;

    public bool DrawGrid => drawGrid;

    public bool DrawPlayerPath => drawPlayerPath;

    public delegate void GenerateGrid();

    public static event GenerateGrid onGenerateGrid;

    public static void InvokeGenerateGrid() => onGenerateGrid?.Invoke();
}